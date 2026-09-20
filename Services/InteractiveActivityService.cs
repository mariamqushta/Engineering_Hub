using AutoMapper;
using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.InteractiveActivity;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class InteractiveActivityService : IInteractiveActivityService
    {
        private readonly UnitWork _unitWork;
        private readonly IMapper _mapper;

        public InteractiveActivityService(
            UnitWork unitWork,
            IMapper mapper)
        {
            _unitWork = unitWork;
            _mapper = mapper;
        }


        // CREATE
        public async Task<InteractiveActivityResponseDTO?> CreateActivityAsync(
            InteractiveActivityDTO dto,
            string instructorId)
        {
            // 1. Check Track
            var track = _unitWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return null;

            // 2. Check instructor assignment
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
            var activity =
                _mapper.Map<InteractiveActivity>(dto);

            // 8. Backend controls status
            activity.Status = SessionStatus.Scheduled;

            // 9. Add
            _unitWork.InteractiveActivityrepo.add(activity);

            // 10. Save
            await _unitWork.SaveAsync();

            // 11. Map Entity → Response DTO
            return _mapper.Map<InteractiveActivityResponseDTO>(
                activity);
        }


        // GET BY TRACK
        public async Task<List<InteractiveActivityResponseDTO>>
            GetActivitiesByTrackAsync(int trackId)
        {
            var activities = _unitWork.InteractiveActivityrepo
                .GetByCondition(x => x.TrackId == trackId);

            return _mapper.Map<List<InteractiveActivityResponseDTO>>(
                activities);
        }


        // UPDATE
        public async Task<(bool Success, string Message)>
            UpdateActivityAsync(
                int id,
                InteractiveActivityDTO dto,
                string instructorId)
        {
            // 1. Find activity
            var activity = _unitWork.InteractiveActivityrepo
                .GetById(id);

            if (activity == null)
                return (false, "Interactive activity not found.");

            // 2. Don't update cancelled activity
            if (activity.Status == SessionStatus.Cancelled)
                return (false,
                    "Cancelled activity cannot be updated.");

            // 3. Check Track
            var track = _unitWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return (false,
                    "Track not found or inactive.");

            // 4. Check instructor assignment
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == dto.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return (false,
                    "You are not assigned to this Track.");

            // 5. Validate price
            if (dto.Price < 0)
                return (false,
                    "Price cannot be negative.");

            // 6. Validate capacity
            if (dto.Capacity <= 0)
                return (false,
                    "Capacity must be greater than zero.");

            // 7. Validate time
            if (dto.EndTime <= dto.StartTime)
                return (false,
                    "End time must be after start time.");

            // 8. Validate date
            if (dto.Date.Date < DateTime.UtcNow.Date)
                return (false,
                    "Activity date cannot be in the past.");

            // 9. Map DTO → existing entity
            _mapper.Map(dto, activity);

            // Status remains unchanged

            // 10. Save
            await _unitWork.SaveAsync();

            return (true,
                "Interactive activity updated successfully.");
        }


        // CANCEL
        public async Task<(bool Success, string Message)>
            CancelActivityAsync(
                int id,
                string instructorId)
        {
            // 1. Find activity
            var activity = _unitWork.InteractiveActivityrepo
                .GetById(id);

            if (activity == null)
                return (false,
                    "Interactive activity not found.");

            // 2. Check instructor assignment
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == activity.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return (false,
                    "You are not assigned to this Track.");

            // 3. Check already cancelled
            if (activity.Status == SessionStatus.Cancelled)
                return (false,
                    "Interactive activity is already cancelled.");

            // 4. Cancel
            activity.Status = SessionStatus.Cancelled;

            // 5. Save
            await _unitWork.SaveAsync();

            return (true,
                "Interactive activity cancelled successfully.");
        }
    }
}