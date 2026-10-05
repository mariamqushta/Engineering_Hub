using AutoMapper;
using Engineering_Hub.DTO;
using Engineering_Hub.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using UnitWorkClass = Engineering_Hub.UnitOfWork.UnitWork;

namespace Engineering_Hub.Services
{
    public class TrackService : ITrackService
    {
        private readonly UnitWorkClass _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public TrackService(
             UnitWorkClass unitWork,
             IMapper mapper,
             UserManager<ApplicationUser> userManager)
        {
            _unitOfWork = unitWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        // =========================
        // CREATE TRACK
        // Admin only
        // =========================
        public async Task<TrackResponseDTO> CreateTrackAsync(
            TrackDTO dto)
        {
            // DTO → Entity
            var track = _mapper.Map<Track>(dto);

            // System-generated values
            track.CreatedAt = DateTime.UtcNow;
            track.IsActive = true;

            // Add Track
            _unitOfWork.Trackrepo.add(track);

            // Save
            await _unitOfWork.SaveAsync();

            // Entity → Response DTO
            return _mapper.Map<TrackResponseDTO>(track);
        }

        // =========================
        // GET ALL TRACKS
        // =========================
        public async Task<List<TrackResponseDTO>> GetAllTracksAsync()
        {
            var tracks =
                _unitOfWork.Trackrepo
                    .GetByCondition(t => t.IsActive);

            return _mapper.Map<List<TrackResponseDTO>>(tracks);
        }

        // =========================
        // GET TRACK BY ID
        // =========================
        public async Task<TrackResponseDTO?> GetTrackByIdAsync(
            int id)
        {
            var track =
                _unitOfWork.Trackrepo
                    .GetByCondition(t =>
                        t.Id == id &&
                        t.IsActive)
                    .FirstOrDefault();

            if (track == null)
            {
                return null;
            }

            return _mapper.Map<TrackResponseDTO>(track);
        }

        // =========================
        // UPDATE TRACK
        // Admin only
        // =========================
        public async Task<(bool Success, string Message)> UpdateTrackAsync(
            int id,
            TrackDTO dto)
        {
            // Find Track
            var track =
                _unitOfWork.Trackrepo.GetById(id);

            if (track == null || !track.IsActive)
            {
                return (
                    false,
                    "Track not found."
                );
            }

            // Update Track from DTO
            _mapper.Map(dto, track);

            // Update modification date
            track.UpdatedAt = DateTime.UtcNow;

            // Save changes
            _unitOfWork.Trackrepo.Edit(track);

            await _unitOfWork.SaveAsync();

            return (
                true,
                "Track updated successfully."
            );
        }

        // =========================
        // DEACTIVATE TRACK
        // Admin only
        // =========================
        public async Task<(bool Success, string Message)> DeleteTrackAsync(
            int id)
        {
            // Find Track
            var track =
                _unitOfWork.Trackrepo.GetById(id);

            if (track == null || !track.IsActive)
            {
                return (
                    false,
                    "Track not found."
                );
            }

            // Soft delete
            track.IsActive = false;
            track.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Trackrepo.Edit(track);

            await _unitOfWork.SaveAsync();

            return (
                true,
                "Track deactivated successfully."
            );
        }

        // =========================
        // ASSIGN INSTRUCTOR
        // Admin only
        // =========================
        public async Task<(bool Success, string Message)> AssignInstructorAsync(
            int trackId,
            string username)
        {
            // 1. Check that the Track exists
            var track = _unitOfWork.Trackrepo
                .GetById(trackId);

            if (track == null || !track.IsActive)
            {
                return (
                    false,
                    "Track not found."
                );
            }

            // 2. Find the user
            var instructor = await _userManager
            .FindByNameAsync(username);

            if (instructor == null)
            {
                return (
                    false,
                    "Instructor not found."
                );
            }

            // 3. Make sure the user is actually an Instructor
            var roles = await _userManager
                .GetRolesAsync(instructor);

            if (!roles.Contains("Instructor"))
            {
                return (
                    false,
                    "The selected user is not an Instructor."
                );
            }

            // 4. Check if the Instructor is already assigned
            var existingAssignment =
                _unitOfWork.TrackInstructorrepo
                    .GetByCondition(x =>
                        x.TrackId == trackId &&
                        x.UserId == instructor.Id)
                    .FirstOrDefault();

            if (existingAssignment != null)
            {
                return (
                    false,
                    "This Instructor is already assigned to this Track."
                );
            }

            // 5. Create the assignment
            var trackInstructor = new TrackInstructor
            {
                TrackId = trackId,
                UserId = instructor.Id
            };

            _unitOfWork.TrackInstructorrepo.add(trackInstructor);

            // 6. Save
            await _unitOfWork.SaveAsync();

            return (
                true,
                "Instructor assigned to Track successfully."
            );
        }
        public async Task<(bool Success, string Message)> RemoveInstructorAsync(
    int trackId,
    string username)
        {
            // 1. Check that the Track exists
            var track = _unitOfWork.Trackrepo
                .GetById(trackId);

            if (track == null || !track.IsActive)
            {
                return (
                    false,
                    "Track not found."
                );
            }

            // 2. Find the user by username
            var instructor = await _userManager
                .FindByNameAsync(username);

            if (instructor == null)
            {
                return (
                    false,
                    "Instructor not found."
                );
            }

            // 3. Make sure the user is actually an Instructor
            var roles = await _userManager
                .GetRolesAsync(instructor);

            if (!roles.Contains("Instructor"))
            {
                return (
                    false,
                    "The selected user is not an Instructor."
                );
            }

            // 4. Find the assignment
            var assignment =
                _unitOfWork.TrackInstructorrepo
                    .GetByCondition(x =>
                        x.TrackId == trackId &&
                        x.UserId == instructor.Id)
                    .FirstOrDefault();

            if (assignment == null)
            {
                return (
                    false,
                    "This Instructor is not assigned to this Track."
                );
            }

            // 5. Remove the assignment
            _unitOfWork.DeleteTrackInstructor(assignment);

            // 6. Save
            await _unitOfWork.SaveAsync();

            return (
                true,
                "Instructor removed from Track successfully."
            );
        }
    }
}