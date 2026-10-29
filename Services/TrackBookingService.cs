using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Collections.Generic;
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



        private bool IsUpgrade(
    TrackBookingType currentType,
    TrackBookingType requestedType)
        {
            if (currentType == requestedType)
                return false;

            return currentType switch
            {
                TrackBookingType.TrackOnly =>
                    requestedType == TrackBookingType.TrackWithInteractive ||
                    requestedType == TrackBookingType.TrackWithInteractiveAndWorkshop ||
                    requestedType == TrackBookingType.TrackWithCoaching,

                TrackBookingType.TrackWithInteractive =>
                    requestedType == TrackBookingType.TrackWithInteractiveAndWorkshop,

                _ => false
            };
        }
        public async Task<(bool Success, string Message, decimal? Price)> BookTrackAsync(
      string studentId,
      TrackBookingDto dto)
        {
            // 1. Find the track
            var track = _unitOfWork.Trackrepo.GetById(dto.TrackId);

            if (track == null)
                return (false, "Track not found.", null);

            if (!track.IsActive)
                return (false, "This Track is not available.", null);

            if (!Enum.IsDefined(typeof(TrackBookingType), dto.BookingType))
                return (false, "Invalid booking type.", null);

            // 2. Prevent another pending request for this track
            //var pendingRequest = _unitOfWork.TrackBookingRequestrepo
            //    .GetByCondition(r =>
            //        r.StudentId == studentId &&
            //        r.TrackId == dto.TrackId &&
            //        r.ApprovalStatus == BookingApprovalStatus.Pending)
            //    .FirstOrDefault();

            //if (pendingRequest != null)
            //{
            //    return (
            //        false,
            //        "You already have a pending request for this Track.",
            //        null
            //    );
            //}

            // 3. Check whether the student already has an active enrollment
            var existingEnrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == studentId &&
                    e.TrackId == dto.TrackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            // 4. If already enrolled, allow only a valid upgrade
            if (existingEnrollment != null &&
                !IsUpgrade(existingEnrollment.BookingType, dto.BookingType))
            {
                return (
                    false,
                    "You are already enrolled in this Track. Choose an eligible upgrade.",
                    null
                );
            }

            // 5. Get the activities and workshops
            var interactiveActivities = _unitOfWork.InteractiveActivityrepo
                .GetByCondition(x => x.TrackId == dto.TrackId)
                .ToList();

            var workshops = _unitOfWork.Workshoprepo
                .GetByCondition(x => x.TrackId == dto.TrackId)
                .ToList();

            // 6. Validate the activities included in the requested package
            bool includesInteractive =
                dto.BookingType == TrackBookingType.TrackWithInteractive ||
                dto.BookingType == TrackBookingType.TrackWithInteractiveAndWorkshop;

            bool includesWorkshops =
                dto.BookingType == TrackBookingType.TrackWithInteractiveAndWorkshop;

            if (includesInteractive &&
                interactiveActivities.Any(x => x.Status == SessionStatus.Cancelled))
            {
                return (
                    false,
                    "This package includes a cancelled interactive activity.",
                    null
                );
            }

            if (includesWorkshops &&
                workshops.Any(x => x.Status == SessionStatus.Cancelled))
            {
                return (
                    false,
                    "This package includes a cancelled workshop.",
                    null
                );
            }

            // 7. Calculate the full requested package price
            decimal requestedPrice = track.Price;

            if (dto.BookingType == TrackBookingType.TrackWithInteractive)
            {
                decimal interactiveTotal = interactiveActivities
                    .Where(x => x.Status != SessionStatus.Cancelled)
                    .Sum(x => x.Price);

                requestedPrice = (track.Price + interactiveTotal) * 0.90m;
            }
            else if (
                dto.BookingType ==
                TrackBookingType.TrackWithInteractiveAndWorkshop)
            {
                decimal interactiveTotal = interactiveActivities
                    .Where(x => x.Status != SessionStatus.Cancelled)
                    .Sum(x => x.Price);

                decimal workshopTotal = workshops
                    .Where(x => x.Status != SessionStatus.Cancelled)
                    .Sum(x => x.Price);

                requestedPrice =
                    (track.Price + interactiveTotal + workshopTotal) * 0.85m;
            }
            else if (dto.BookingType == TrackBookingType.TrackWithCoaching)
            {
                requestedPrice = track.Price + 100m;
            }

            // 8. Check whether the student already has a pending request.
            var pendingRequest = _unitOfWork.TrackBookingRequestrepo
                .GetByCondition(r =>
                    r.StudentId == studentId &&
                    r.TrackId == dto.TrackId &&
                    r.ApprovalStatus == BookingApprovalStatus.Pending)
                .FirstOrDefault();

            if (pendingRequest != null)
            {
                // Do not create a duplicate request for the same package.
                if (pendingRequest.BookingType == dto.BookingType)
                {
                    return (
                        false,
                        "You already have a pending request for this package.",
                        null
                    );
                }

                // Only allow an eligible upgrade.
                if (!IsUpgrade(pendingRequest.BookingType, dto.BookingType))
                {
                    return (
                        false,
                        "You can only change your pending request to an eligible upgrade.",
                        null
                    );
                }

                // Update the existing pending request.
                pendingRequest.BookingType = dto.BookingType;
                pendingRequest.RequestedPrice = requestedPrice;

                await _unitOfWork.SaveAsync();

                return (
                    true,
                    "Your pending booking request has been updated successfully.",
                    requestedPrice
                );
            }

            // 8. Calculate the amount due for this request
            decimal amountDue = existingEnrollment == null
                ? requestedPrice
                : Math.Max(
                    0m,
                    requestedPrice - existingEnrollment.ApprovedPrice
                );

            // 9. Create a pending request only.
            // Do NOT create an enrollment or activity/workshop bookings here.
            var request = new TrackBookingRequest
            {
                StudentId = studentId,
                TrackId = dto.TrackId,
                BookingType = dto.BookingType,
                RequestedPrice = requestedPrice,
                RequestedAt = DateTime.UtcNow,
                ApprovalStatus = BookingApprovalStatus.Pending
            };

            _unitOfWork.TrackBookingRequestrepo.add(request);

            await _unitOfWork.SaveAsync();

            return (
                true,
                existingEnrollment == null
                    ? "Track booking request submitted and awaiting admin approval."
                    : "Upgrade request submitted and awaiting admin approval.",
                amountDue
            );
        }
        public async Task<(bool Success, string Message, decimal? Price)> GetBookingPriceAsync(
    int trackId,
    TrackBookingType bookingType)
        {
            // Find the Track
            var track = _unitOfWork.Trackrepo.GetById(trackId);

            if (track == null)
            {
                return (false, "Track not found.", null);
            }

            // Check if Track is active
            if (!track.IsActive)
            {
                return (false, "This Track is not available.", null);
            }

            // Check if BookingType is valid
            if (!Enum.IsDefined(typeof(TrackBookingType), bookingType))
            {
                return (false, "Invalid booking type.", null);
            }

            // Get Interactive Activities
            var interactiveActivities =
                _unitOfWork.InteractiveActivityrepo
                    .GetByCondition(x => x.TrackId == trackId)
                    .ToList();

            // Get Workshops
            var workshops =
                _unitOfWork.Workshoprepo
                    .GetByCondition(x => x.TrackId == trackId)
                    .ToList();

            // Start with Track price
            decimal finalPrice = track.Price;

            // Track + Interactive
            if (bookingType == TrackBookingType.TrackWithInteractive)
            {
                decimal interactiveTotal = interactiveActivities
                    .Where(x => x.Status != SessionStatus.Cancelled)
                    .Sum(x => x.Price);

                decimal subtotal = track.Price + interactiveTotal;

                finalPrice = subtotal * 0.90m;
            }

            // Track + Interactive + Workshop
            if (bookingType == TrackBookingType.TrackWithInteractiveAndWorkshop)
            {
                decimal interactiveTotal = interactiveActivities
                    .Where(x => x.Status != SessionStatus.Cancelled)
                    .Sum(x => x.Price);

                decimal workshopTotal = workshops
                    .Where(x => x.Status != SessionStatus.Cancelled)
                    .Sum(x => x.Price);

                decimal subtotal =
                    track.Price +
                    interactiveTotal +
                    workshopTotal;

                finalPrice = subtotal * 0.85m;
            }

            // Track + Coaching
            if (bookingType == TrackBookingType.TrackWithCoaching)
            {
                finalPrice = track.Price + 100m;
            }

            return (
                true,
                "Price calculated successfully.",
                finalPrice
            );
        }

        public Task<List<TrackBookingRequest>> GetPendingRequestsAsync()
        {
            var requests = _unitOfWork.TrackBookingRequestrepo
                .GetByCondition(r =>
                    r.ApprovalStatus == BookingApprovalStatus.Pending)
                .OrderBy(r => r.RequestedAt)
                .ToList();

            return Task.FromResult(requests);
        }


        public async Task<(bool Success, string Message)> ApproveRequestAsync(
    int requestId)
        {
            var request = _unitOfWork.TrackBookingRequestrepo
                .GetById(requestId);

            if (request == null)
                return (false, "Booking request not found.");

            if (request.ApprovalStatus != BookingApprovalStatus.Pending)
                return (false, "This request has already been processed.");

            var track = _unitOfWork.Trackrepo.GetById(request.TrackId);

            if (track == null)
                return (false, "Track not found.");

            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == request.StudentId &&
                    e.TrackId == request.TrackId)
                .FirstOrDefault();

            // A new booking creates an enrollment.
            if (enrollment == null)
            {
                enrollment = new TrackEnrollment
                {
                    StudentId = request.StudentId,
                    TrackId = request.TrackId,
                    BookingType = request.BookingType,
                    EnrolledAt = DateTime.UtcNow,
                    Status = SessionStatus.Active,
                    ApprovedPrice = request.RequestedPrice
                };

                _unitOfWork.TrackEnrollmentrepo.add(enrollment);
            }
            else
            {
                // An existing enrollment is updated for an upgrade.
                if (!IsUpgrade(enrollment.BookingType, request.BookingType))
                {
                    return (
                        false,
                        "The requested package is no longer a valid upgrade."
                    );
                }

                enrollment.BookingType = request.BookingType;
                enrollment.ApprovedPrice = request.RequestedPrice;
                enrollment.Status = SessionStatus.Active;
            }

            // Include interactive activities when selected.
            bool includesInteractive =
                request.BookingType == TrackBookingType.TrackWithInteractive ||
                request.BookingType ==
                    TrackBookingType.TrackWithInteractiveAndWorkshop;

            if (includesInteractive)
            {
                var activities = _unitOfWork.InteractiveActivityrepo
                    .GetByCondition(a => a.TrackId == request.TrackId)
                    .ToList();

                foreach (var activity in activities)
                {
                    if (activity.Status == SessionStatus.Cancelled)
                        continue;

                    var booking = _unitOfWork.InteractiveBookingrepo
                        .GetByCondition(b =>
                            b.UserId == request.StudentId &&
                            b.InteractiveActivityId == activity.Id &&
                            b.Status != SessionStatus.Cancelled)
                        .FirstOrDefault();

                    if (booking == null)
                    {
                        booking = new InteractiveBooking
                        {
                            UserId = request.StudentId,
                            InteractiveActivityId = activity.Id,
                            BookedAt = DateTime.UtcNow,
                            Status = SessionStatus.Scheduled,
                            Source = BookingSource.TrackBooking,
                            ApprovalStatus = BookingApprovalStatus.Approved
                        };

                        _unitOfWork.InteractiveBookingrepo.add(booking);
                    }
                    else
                    {
                        booking.ApprovalStatus = BookingApprovalStatus.Approved;
                    }
                }
            }

            // Include workshops only in the complete package.
            if (request.BookingType ==
                TrackBookingType.TrackWithInteractiveAndWorkshop)
            {
                var workshops = _unitOfWork.Workshoprepo
                    .GetByCondition(w => w.TrackId == request.TrackId)
                    .ToList();

                foreach (var workshop in workshops)
                {
                    if (workshop.Status == SessionStatus.Cancelled)
                        continue;

                    var booking = _unitOfWork.WorkshopBookingrepo
                        .GetByCondition(b =>
                            b.UserId == request.StudentId &&
                            b.WorkshopId == workshop.Id &&
                            b.Status != SessionStatus.Cancelled)
                        .FirstOrDefault();

                    if (booking == null)
                    {
                        booking = new WorkshopBooking
                        {
                            UserId = request.StudentId,
                            WorkshopId = workshop.Id,
                            BookedAt = DateTime.UtcNow,
                            Status = SessionStatus.Scheduled,
                            Source = BookingSource.TrackBooking,
                            ApprovalStatus = BookingApprovalStatus.Approved
                        };

                        _unitOfWork.WorkshopBookingrepo.add(booking);
                    }
                    else
                    {
                        booking.ApprovalStatus = BookingApprovalStatus.Approved;
                    }
                }
            }

            request.ApprovalStatus = BookingApprovalStatus.Approved;

            await _unitOfWork.SaveAsync();

            return (true, "Booking request approved successfully.");
        }


        public async Task<(bool Success, string Message)> RejectRequestAsync(
    int requestId)
        {
            var request = _unitOfWork.TrackBookingRequestrepo
                .GetById(requestId);

            if (request == null)
                return (false, "Booking request not found.");

            if (request.ApprovalStatus != BookingApprovalStatus.Pending)
                return (false, "This request has already been processed.");

            request.ApprovalStatus = BookingApprovalStatus.Rejected;

            await _unitOfWork.SaveAsync();

            return (true, "Booking request rejected successfully.");
        }
        public Task<List<TrackBookingRequest>> GetMyRequestsAsync(string studentId)
        {
            var requests = _unitOfWork.TrackBookingRequestrepo
                .GetByCondition(r => r.StudentId == studentId)
                .OrderByDescending(r => r.RequestedAt)
                .ToList();

            return Task.FromResult(requests);
        }
    }
}