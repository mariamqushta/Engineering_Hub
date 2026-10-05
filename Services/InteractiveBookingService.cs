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
            var activity = _unitOfWork.InteractiveActivityrepo
                .GetById(dto.InteractiveActivityId);

            if (activity == null)
                return (false, "Interactive activity not found.");

            if (activity.Status == SessionStatus.Cancelled)
                return (false, "This interactive activity has been cancelled.");

            if (activity.Status == SessionStatus.Finished)
                return (false, "This interactive activity has already finished.");

            // Check if the user already has a booking
            // from either individual booking or package booking.
            var existingBooking = _unitOfWork.InteractiveBookingrepo
                .GetByCondition(b =>
                    b.UserId == userId &&
                    b.InteractiveActivityId == dto.InteractiveActivityId &&
                    b.Status != SessionStatus.Cancelled)
                .FirstOrDefault();

            if (existingBooking != null)
                return (false, "You are already booked for this interactive activity.");

            // Only individual bookings count toward capacity.
            var currentBookings = _unitOfWork.InteractiveBookingrepo
                .GetByCondition(b =>
                    b.InteractiveActivityId == dto.InteractiveActivityId &&
                    b.Status != SessionStatus.Cancelled &&
                    b.Source == BookingSource.Individual)
                .Count();

            if (currentBookings >= activity.Capacity)
                return (false, "This interactive activity is fully booked.");

            var booking = new InteractiveBooking
            {
                UserId = userId,
                InteractiveActivityId = dto.InteractiveActivityId,
                BookedAt = DateTime.UtcNow,
                Status = SessionStatus.Scheduled,
                Source = BookingSource.Individual
            };

            _unitOfWork.InteractiveBookingrepo.add(booking);

            await _unitOfWork.SaveAsync();

            return (true, "Interactive activity booked successfully.");
        }

        public async Task<List<InteractiveBookingResponseDTO>> GetMyBookingsAsync(
            string userId)
        {
            var bookings = _unitOfWork.InteractiveBookingrepo
                .GetByConditionWithInclude(
                    x => x.UserId == userId,
                    x => x.InteractiveActivity);

            var result = _mapper.Map<List<InteractiveBookingResponseDTO>>(bookings);

            return result;
        }
    }
}