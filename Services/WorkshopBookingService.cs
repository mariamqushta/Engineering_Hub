using AutoMapper;
using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class WorkshopBookingService : IWorkshopBookingService
    {
        private readonly UnitWork _unitOfWork;
        private readonly IMapper _mapper;

        public WorkshopBookingService(
            UnitWork unitWork,
            IMapper mapper)
        {
            _unitOfWork = unitWork;
            _mapper = mapper;
        }

        public async Task<(bool Success, string Message)> BookWorkshopAsync(
            string userId,
            WorkshopBookingDto dto)
        {
            var workshop = _unitOfWork.Workshoprepo
                .GetById(dto.WorkshopId);

            if (workshop == null)
                return (false, "Workshop not found.");

            if (workshop.Status == SessionStatus.Cancelled)
                return (false, "This workshop has been cancelled.");

            if (workshop.Status == SessionStatus.Finished)
                return (false, "This workshop has already finished.");

            var workshopStart = workshop.Date.Date
                .Add(workshop.StartTime);

            if (DateTime.UtcNow >= workshopStart)
                return (false, "This workshop has already started.");

            // Check whether the user already has an active booking
            // for this workshop, including a package booking.
            var existingBooking = _unitOfWork.WorkshopBookingrepo
                .GetByCondition(b =>
                    b.UserId == userId &&
                    b.WorkshopId == dto.WorkshopId &&
                    b.Status != SessionStatus.Cancelled &&
                    b.ApprovalStatus != BookingApprovalStatus.Rejected)
                .FirstOrDefault();

            if (existingBooking != null)
                return (false, "You are already booked for this workshop.");

            // Count reservations from all sources.
            // Pending requests also reserve a place.
            var currentBookings = _unitOfWork.WorkshopBookingrepo
                .GetByCondition(b =>
                    b.WorkshopId == dto.WorkshopId &&
                    b.Status != SessionStatus.Cancelled &&
                    b.ApprovalStatus != BookingApprovalStatus.Rejected)
                .Count();

            if (currentBookings >= workshop.Capacity)
                return (false, "This workshop is fully booked.");

            var booking = new WorkshopBooking
            {
                UserId = userId,
                WorkshopId = dto.WorkshopId,
                BookedAt = DateTime.UtcNow,
                Status = SessionStatus.Scheduled,
                Source = BookingSource.Individual,
                ApprovalStatus = BookingApprovalStatus.Pending
            };

            _unitOfWork.WorkshopBookingrepo.add(booking);

            await _unitOfWork.SaveAsync();

            return (
                true,
                "Workshop booking request submitted and awaiting admin approval."
            );
        }

        public async Task<List<WorkshopBookingResponseDTO>> GetMyBookingsAsync(
            string userId)
        {
            var bookings = _unitOfWork.WorkshopBookingrepo
                .GetByConditionWithInclude(
                    x => x.UserId == userId,
                    x => x.Workshop);

            var result = _mapper.Map<List<WorkshopBookingResponseDTO>>(bookings);

            return result;
        }
    }
}