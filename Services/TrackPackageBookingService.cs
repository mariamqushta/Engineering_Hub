using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;
using UnitWorkClass = Engineering_Hub.UnitOfWork.UnitWork;

namespace Engineering_Hub.Services
{
    public class TrackPackageBookingService : ITrackPackageBookingService
    {
        private readonly UnitWorkClass _unitOfWork;

        public TrackPackageBookingService(UnitWorkClass unitWork)
        {
            _unitOfWork = unitWork;
        }

        public async Task<(bool Success, string Message)> BookTrackPackageAsync(
            string userId,
            TrackPackageBookingDto dto)
        {
            // 1. Find the package
            var package = _unitOfWork.TrackPackagerepo
                .GetById(dto.TrackPackageId);

            if (package == null)
            {
                return (
                    false,
                    "Track package not found."
                );
            }

            // 2. Find the Track associated with the package
            var track = _unitOfWork.Trackrepo
                .GetById(package.TrackId);

            if (track == null)
            {
                return (
                    false,
                    "The Track associated with this package was not found."
                );
            }

            // 3. Check if Track is active
            if (!track.IsActive)
            {
                return (
                    false,
                    "The Track associated with this package is not active."
                );
            }

            // 4. Check if the user already booked this package
            var existingBooking = _unitOfWork.TrackPackageBookingrepo
                .GetByCondition(b =>
                    b.UserId == userId &&
                    b.TrackPackageId == package.Id &&
                    b.Status != SessionStatus.Cancelled)
                .FirstOrDefault();

            if (existingBooking != null)
            {
                return (
                    false,
                    "You have already booked this package."
                );
            }

            // 5. Check if the user already has access to the Track
            var existingEnrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == userId &&
                    e.TrackId == package.TrackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            // If the user is not already enrolled,
            // create a new Track enrollment.
            if (existingEnrollment == null)
            {
                var enrollment = new TrackEnrollment
                {
                    StudentId = userId,
                    TrackId = package.TrackId,
                    EnrolledAt = DateTime.UtcNow,
                    Status = SessionStatus.Active
                };

                _unitOfWork.TrackEnrollmentrepo.add(enrollment);
            }

            // 6. Create package booking
            var packageBooking = new TrackPackageBooking
            {
                UserId = userId,
                TrackPackageId = package.Id,
                BookedAt = DateTime.UtcNow,
                Status = SessionStatus.Active
            };

            _unitOfWork.TrackPackageBookingrepo.add(packageBooking);

            // 7. Get workshops included in the package
            var packageWorkshops =
                _unitOfWork.TrackPackageWorkshoprepo
                    .GetByCondition(x =>
                        x.TrackPackageId == package.Id);

            // 8. Book the included workshops
            foreach (var packageWorkshop in packageWorkshops)
            {
                var workshop = _unitOfWork.Workshoprepo
                    .GetById(packageWorkshop.WorkshopId);

                // Check that workshop exists
                if (workshop == null)
                {
                    return (
                        false,
                        "One of the workshops in the package was not found."
                    );
                }

                // Check workshop status
                if (workshop.Status == SessionStatus.Cancelled)
                {
                    return (
                        false,
                        "One of the workshops in the package has been cancelled."
                    );
                }

                if (workshop.Status == SessionStatus.Finished)
                {
                    return (
                        false,
                        "One of the workshops in the package has already finished."
                    );
                }

                // Check that workshop belongs to the same Track
                if (workshop.TrackId != package.TrackId)
                {
                    return (
                        false,
                        "One of the workshops does not belong to this Track."
                    );
                }

                // Check if this user already booked the workshop
                var existingWorkshopBooking =
                    _unitOfWork.WorkshopBookingrepo
                        .GetByCondition(b =>
                            b.UserId == userId &&
                            b.WorkshopId == workshop.Id &&
                            b.Status != SessionStatus.Cancelled)
                        .FirstOrDefault();

                // Count all active workshop bookings
                var workshopBookings =
                    _unitOfWork.WorkshopBookingrepo
                        .GetByCondition(b =>
                            b.WorkshopId == workshop.Id &&
                            b.Status != SessionStatus.Cancelled);

                // Check workshop capacity
                if (workshopBookings.Count >= workshop.Capacity &&
                    existingWorkshopBooking == null)
                {
                    return (
                        false,
                        $"Workshop '{workshop.Title}' is full."
                    );
                }

                // Create workshop booking only if
                // the user does not already have one
                if (existingWorkshopBooking == null)
                {
                    var workshopBooking = new WorkshopBooking
                    {
                        UserId = userId,
                        WorkshopId = workshop.Id,
                        BookedAt = DateTime.UtcNow,
                        Status = SessionStatus.Scheduled
                    };

                    _unitOfWork.WorkshopBookingrepo
                        .add(workshopBooking);
                }
            }

            // 9. Get interactive activities included in the package
            var packageInteractives =
                _unitOfWork.TrackPackageInteractiverepo
                    .GetByCondition(x =>
                        x.TrackPackageId == package.Id);

            // 10. Book the included interactive activities
            foreach (var packageInteractive in packageInteractives)
            {
                var activity =
                    _unitOfWork.InteractiveActivityrepo
                        .GetById(
                            packageInteractive.InteractiveActivityId);

                // Check that activity exists
                if (activity == null)
                {
                    return (
                        false,
                        "One of the interactive activities in the package was not found."
                    );
                }

                // Check activity status
                if (activity.Status == SessionStatus.Cancelled)
                {
                    return (
                        false,
                        "One of the interactive activities in the package has been cancelled."
                    );
                }

                if (activity.Status == SessionStatus.Finished)
                {
                    return (
                        false,
                        "One of the interactive activities in the package has already finished."
                    );
                }

                // Check that activity belongs to the same Track
                if (activity.TrackId != package.TrackId)
                {
                    return (
                        false,
                        "One of the interactive activities does not belong to this Track."
                    );
                }

                // Check if this user already booked the activity
                var existingInteractiveBooking =
                    _unitOfWork.InteractiveBookingrepo
                        .GetByCondition(b =>
                            b.UserId == userId &&
                            b.InteractiveActivityId == activity.Id &&
                            b.Status != SessionStatus.Cancelled)
                        .FirstOrDefault();

                // Count all active interactive bookings
                var activityBookings =
                    _unitOfWork.InteractiveBookingrepo
                        .GetByCondition(b =>
                            b.InteractiveActivityId == activity.Id &&
                            b.Status != SessionStatus.Cancelled);

                // Check activity capacity
                if (activityBookings.Count >= activity.Capacity &&
                    existingInteractiveBooking == null)
                {
                    return (
                        false,
                        $"Interactive activity '{activity.Title}' is full."
                    );
                }

                // Create booking only if
                // the user does not already have one
                if (existingInteractiveBooking == null)
                {
                    var interactiveBooking = new InteractiveBooking
                    {
                        UserId = userId,
                        InteractiveActivityId = activity.Id,
                        BookedAt = DateTime.UtcNow,
                        Status = SessionStatus.Scheduled
                    };

                    _unitOfWork.InteractiveBookingrepo
                        .add(interactiveBooking);
                }
            }

            // 11. Save everything
            await _unitOfWork.SaveAsync();

            return (
                true,
                "Track package booked successfully."
            );
        }
    }
}