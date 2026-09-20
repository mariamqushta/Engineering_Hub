using AutoMapper;
using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.Workshop;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class WorkshopService : IWorkshopService
    {
        private readonly UnitWork _unitWork;
        private readonly IMapper _mapper;

        public WorkshopService(
            UnitWork unitWork,
            IMapper mapper)
        {
            _unitWork = unitWork;
            _mapper = mapper;
        }

        // CREATE
        public async Task<WorkshopResponseDTO?> CreateWorkshopAsync(
            WorkshopDTO dto,
            string instructorId)
        {
            // 1. Check Track
            var track = _unitWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return null;

            // 2. Check instructor is assigned to Track
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == dto.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return null;

            // 3. Validate price
            if (dto.Price < 0)
                return null;

            // 4. Validate capacity
            if (dto.Capacity <= 0)
                return null;

            // 5. Validate time
            if (dto.EndTime <= dto.StartTime)
                return null;

            // 6. Validate date
            if (dto.Date.Date < DateTime.UtcNow.Date)
                return null;

            // 7. Map DTO → Entity
            var workshop = _mapper.Map<Workshop>(dto);

            // 8. Set backend-controlled values
            workshop.Status = SessionStatus.Scheduled;

            // 9. Add
            _unitWork.Workshoprepo.add(workshop);

            // 10. Save
            await _unitWork.SaveAsync();

            // 11. Map Entity → Response DTO
            return _mapper.Map<WorkshopResponseDTO>(workshop);
        }


        // GET WORKSHOPS BY TRACK
        public async Task<List<WorkshopResponseDTO>> GetWorkshopsByTrackAsync(
            int trackId)
        {
            var workshops = _unitWork.Workshoprepo
                .GetByCondition(w => w.TrackId == trackId);

            return _mapper.Map<List<WorkshopResponseDTO>>(workshops);
        }


        // UPDATE
        public async Task<(bool Success, string Message)> UpdateWorkshopAsync(
            int id,
            WorkshopDTO dto,
            string instructorId)
        {
            // 1. Find workshop
            var workshop = _unitWork.Workshoprepo
                .GetById(id);

            if (workshop == null)
                return (false, "Workshop not found.");

            // 2. Don't update cancelled workshop
            if (workshop.Status == SessionStatus.Cancelled)
                return (false, "Cancelled workshop cannot be updated.");

            // 3. Check Track
            var track = _unitWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return (false, "Track not found or inactive.");

            // 4. Check instructor assignment
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == dto.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return (false, "You are not assigned to this Track.");

            // 5. Validate price
            if (dto.Price < 0)
                return (false, "Price cannot be negative.");

            // 6. Validate capacity
            if (dto.Capacity <= 0)
                return (false, "Capacity must be greater than zero.");

            // 7. Validate time
            if (dto.EndTime <= dto.StartTime)
                return (false, "End time must be after start time.");

            // 8. Validate date
            if (dto.Date.Date < DateTime.UtcNow.Date)
                return (false, "Workshop date cannot be in the past.");

            // 9. Map DTO → existing entity
            _mapper.Map(dto, workshop);

            // Keep status controlled by backend
            // Don't allow update to change it.

            // 10. Save
            await _unitWork.SaveAsync();

            return (true, "Workshop updated successfully.");
        }


        // CANCEL
        public async Task<(bool Success, string Message)> CancelWorkshopAsync(
            int id,
            string instructorId)
        {
            // 1. Find workshop
            var workshop = _unitWork.Workshoprepo
                .GetById(id);

            if (workshop == null)
                return (false, "Workshop not found.");

            // 2. Check instructor assignment
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == workshop.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return (false, "You are not assigned to this Track.");

            // 3. Check already cancelled
            if (workshop.Status == SessionStatus.Cancelled)
                return (false, "Workshop is already cancelled.");

            // 4. Cancel
            workshop.Status = SessionStatus.Cancelled;

            // 5. Save
            await _unitWork.SaveAsync();

            return (true, "Workshop cancelled successfully.");
        }
    }
}