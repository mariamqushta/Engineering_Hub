using AutoMapper;
using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitWorkClass = Engineering_Hub.UnitOfWork.UnitWork;

namespace Engineering_Hub.Services
{
    public class InteractiveBookingService : IInteractiveBookingService
    {
        private readonly UnitWorkClass _unitOfWork;
        private readonly IMapper _mapper;
        public InteractiveBookingService(
    UnitWorkClass unitWork,
    IMapper mapper)
        {
            _unitOfWork = unitWork;
            _mapper = mapper;
        }

        public async Task<(bool Success, string Message)> BookInteractiveAsync(
            string userId,
            InteractiveBookingDto dto)
        {
            // 1. Find Interactive Activity
            var activity = _unitOfWork.InteractiveActivityrepo
                .GetById(dto.InteractiveActivityId);

            if (activity == null)
            {
                return (false, "Interactive activity not found.");
            }

            // 2. Check status
            if (activity.Status == SessionStatus.Cancelled)
            {
                return (
                    false,
                    "This interactive activity has been cancelled."
                );
            }

            if (activity.Status == SessionStatus.Finished)
            {
                return (
                    false,
                    "This interactive activity has already finished."
                );
            }

            // 3. Check duplicate booking
            var existingBooking = _unitOfWork.InteractiveBookingrepo
                .GetByCondition(b =>
                    b.UserId == userId &&
                    b.InteractiveActivityId == dto.InteractiveActivityId &&
                    b.Status != SessionStatus.Cancelled)
                .FirstOrDefault();

            if (existingBooking != null)
            {
                return (
                    false,
                    "You are already booked for this interactive activity."
                );
            }

            // 4. Check capacity
            var currentBookings = _unitOfWork.InteractiveBookingrepo
                .GetByCondition(b =>
                    b.InteractiveActivityId ==
                    dto.InteractiveActivityId &&
                    b.Status != SessionStatus.Cancelled)
                .Count();

            if (currentBookings >= activity.Capacity)
            {
                return (
                    false,
                    "This interactive activity is fully booked."
                );
            }

            // 5. Create booking
            var booking = new InteractiveBooking
            {
                UserId = userId,
                InteractiveActivityId = dto.InteractiveActivityId,
                BookedAt = DateTime.UtcNow,
                Status = SessionStatus.Scheduled
            };

            _unitOfWork.InteractiveBookingrepo.add(booking);

            // 6. Save
            await _unitOfWork.SaveAsync();

            return (
                true,
                "Interactive activity booked successfully."
            );
        }
        public async Task<List<InteractiveBookingResponseDTO>> GetMyBookingsAsync(
       string userId)
        {
            var bookings = _unitOfWork.InteractiveBookingrepo
                .GetByConditionWithInclude(
                    x => x.UserId == userId,
                    x => x.InteractiveActivity);

            var result =
                _mapper.Map<List<InteractiveBookingResponseDTO>>(bookings);

            return result;
        }
    }
}