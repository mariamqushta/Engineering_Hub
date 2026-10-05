using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class TrackBookingService : ITrackBookingService
    {
        private readonly UnitWork _unitOfWork;

        public TrackBookingService(UnitWork unitWork)
        {
            _unitOfWork = unitWork;
        }

        public async Task<(bool Success, string Message)> BookTrackAsync(
            string studentId,
            TrackBookingDto dto)
        {
            // 1. Find the Track
            var track = _unitOfWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null)
            {
                return (false, "Track not found.");
            }

            // 2. Check if Track is active
            if (!track.IsActive)
            {
                return (false, "This Track is not available.");
            }

            // 3. Check if student is already enrolled
            var existingEnrollment =
                _unitOfWork.TrackEnrollmentrepo
                    .GetByCondition(e =>
                        e.StudentId == studentId &&
                        e.TrackId == dto.TrackId &&
                        e.Status == SessionStatus.Active)
                    .FirstOrDefault();

            if (existingEnrollment != null)
            {
                return (false, "You are already enrolled in this Track.");
            }

            // 4. Get all Interactive Activities for this Track
            var interactiveActivities =
                _unitOfWork.InteractiveActivityrepo
                    .GetByCondition(x => x.TrackId == dto.TrackId)
                    .ToList();

            // 5. Get all Workshops for this Track
            var workshops =
                _unitOfWork.Workshoprepo
                    .GetByCondition(x => x.TrackId == dto.TrackId)
                    .ToList();

            // 6. Validate required Interactive Activities
            if (dto.BookingType == TrackBookingType.TrackWithInteractive ||
                dto.BookingType == TrackBookingType.TrackWithInteractiveAndWorkshop)
            {
                foreach (var activity in interactiveActivities)
                {
                    if (activity.Status == SessionStatus.Cancelled)
                    {
                        return (
                            false,
                            $"Interactive activity '{activity.Title}' has been cancelled."
                        );
                    }

                    //if (activity.Status == SessionStatus.Finished)
                    //{
                    //    return (
                    //        false,
                    //        $"Interactive activity '{activity.Title}' has already finished."
                    //    );
                    //}

                    //var activityStart =
                    //    activity.Date.Date.Add(activity.StartTime);

                    //if (DateTime.UtcNow >= activityStart)
                    //{
                    //    return (
                    //        false,
                    //        $"Interactive activity '{activity.Title}' has already started."
                    //    );
                    //}
                }
            }

            // 7. Validate required Workshops
            if (dto.BookingType ==
                TrackBookingType.TrackWithInteractiveAndWorkshop)
            {
                foreach (var workshop in workshops)
                {
                    if (workshop.Status == SessionStatus.Cancelled)
                    {
                        return (
                            false,
                            $"Workshop '{workshop.Title}' has been cancelled."
                        );
                    }

                    //if (workshop.Status == SessionStatus.Finished)
                    //{
                    //    return (
                    //        false,
                    //        $"Workshop '{workshop.Title}' has already finished."
                    //    );
                    //}

                    //var workshopStart =
                    //    workshop.Date.Date.Add(workshop.StartTime);

                    //if (DateTime.UtcNow >= workshopStart)
                    //{
                    //    return (
                    //        false,
                    //        $"Workshop '{workshop.Title}' has already started."
                    //    );
                    //}
                }
            }

            // 8. Create Track Enrollment
            var enrollment = new TrackEnrollment
            {
                StudentId = studentId,
                TrackId = dto.TrackId,
                BookingType = dto.BookingType,
                EnrolledAt = DateTime.UtcNow,
                Status = SessionStatus.Active
            };

            _unitOfWork.TrackEnrollmentrepo.add(enrollment);

            // 9. Book all Interactive Activities if selected
            if (dto.BookingType == TrackBookingType.TrackWithInteractive ||
                dto.BookingType == TrackBookingType.TrackWithInteractiveAndWorkshop)
            {
                foreach (var activity in interactiveActivities)
                {
                    var existingBooking =
                        _unitOfWork.InteractiveBookingrepo
                            .GetByCondition(b =>
                                b.UserId == studentId &&
                                b.InteractiveActivityId == activity.Id &&
                                b.Status != SessionStatus.Cancelled)
                            .FirstOrDefault();

                    if (existingBooking == null)
                    {
                        var booking = new InteractiveBooking
                        {
                            UserId = studentId,
                            InteractiveActivityId = activity.Id,
                            BookedAt = DateTime.UtcNow,
                            Status = SessionStatus.Scheduled,
                            Source = BookingSource.TrackBooking
                        };

                        _unitOfWork.InteractiveBookingrepo.add(booking);
                    }
                }
            }

            // 10. Book all Workshops if selected
            if (dto.BookingType ==
                TrackBookingType.TrackWithInteractiveAndWorkshop)
            {
                foreach (var workshop in workshops)
                {
                    var existingBooking =
                        _unitOfWork.WorkshopBookingrepo
                            .GetByCondition(b =>
                                b.UserId == studentId &&
                                b.WorkshopId == workshop.Id &&
                                b.Status != SessionStatus.Cancelled)
                            .FirstOrDefault();

                    if (existingBooking == null)
                    {
                        var booking = new WorkshopBooking
                        {
                            UserId = studentId,
                            WorkshopId = workshop.Id,
                            BookedAt = DateTime.UtcNow,
                            Status = SessionStatus.Scheduled,
                            Source = BookingSource.TrackBooking
                        };

                        _unitOfWork.WorkshopBookingrepo.add(booking);
                    }
                }
            }

            // 11. Save everything
            await _unitOfWork.SaveAsync();

            return (true, "Track booked successfully.");
        }
    }
}