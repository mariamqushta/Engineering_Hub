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

            // 2. Check if the Track is active
            if (!track.IsActive)
            {
                return (false, "This Track is not available.");
            }

            // 3. Check if the student already has an active enrollment
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

            // 4. Create the enrollment
            var enrollment = new TrackEnrollment
            {
                StudentId = studentId,
                TrackId = dto.TrackId,
                EnrolledAt = DateTime.UtcNow,
                Status = SessionStatus.Active
            };

            _unitOfWork.TrackEnrollmentrepo.add(enrollment);

            // 5. Save
            await _unitOfWork.SaveAsync();

            return (true, "Track booked successfully.");
        }
    }
}