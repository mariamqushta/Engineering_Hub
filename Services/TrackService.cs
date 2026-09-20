using AutoMapper;
using Engineering_Hub.DTO;
using Engineering_Hub.models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnitWorkClass = Engineering_Hub.UnitOfWork.UnitWork;

namespace Engineering_Hub.Services
{
    public class TrackService : ITrackService
    {
        private readonly UnitWorkClass _unitOfWork;
        private readonly IMapper _mapper;

        public TrackService(
            UnitWorkClass unitWork,
            IMapper mapper)
        {
            _unitOfWork = unitWork;
            _mapper = mapper;
        }

        // CREATE
        public async Task<TrackResponseDTO> CreateTrackAsync(
            TrackDTO dto,
            string instructorId)
        {
            // DTO → Entity
            var track = _mapper.Map<Track>(dto);

            track.CreatedAt = DateTime.UtcNow;
            track.IsActive = true;

            _unitOfWork.Trackrepo.add(track);

            // Connect the instructor to the Track
            var trackInstructor = new TrackInstructor
            {
                UserId = instructorId,
                Track = track
            };

            _unitOfWork.TrackInstructorrepo.add(trackInstructor);

            await _unitOfWork.SaveAsync();

            // Entity → Response DTO
            return _mapper.Map<TrackResponseDTO>(track);
        }

        // GET ALL
        public async Task<List<TrackResponseDTO>> GetAllTracksAsync()
        {
            var tracks = _unitOfWork.Trackrepo
                .GetByCondition(t => t.IsActive);

            return _mapper.Map<List<TrackResponseDTO>>(tracks);
        }

        // GET BY ID
        public async Task<TrackResponseDTO?> GetTrackByIdAsync(int id)
        {
            var track = _unitOfWork.Trackrepo
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

        // UPDATE
        public async Task<(bool Success, string Message)> UpdateTrackAsync(
            int id,
            TrackDTO dto,
            string instructorId)
        {
            var track = _unitOfWork.Trackrepo
                .GetById(id);

            if (track == null || !track.IsActive)
            {
                return (
                    false,
                    "Track not found."
                );
            }

            // Check that this instructor belongs to the Track
            var instructor = _unitOfWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == id &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
            {
                return (
                    false,
                    "You are not assigned to this Track."
                );
            }

            // Update Entity from DTO
            _mapper.Map(dto, track);

            track.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Trackrepo.Edit(track);

            await _unitOfWork.SaveAsync();

            return (
                true,
                "Track updated successfully."
            );
        }

        // DELETE / DEACTIVATE
        public async Task<(bool Success, string Message)> DeleteTrackAsync(
            int id,
            string instructorId)
        {
            var track = _unitOfWork.Trackrepo
                .GetById(id);

            if (track == null || !track.IsActive)
            {
                return (
                    false,
                    "Track not found."
                );
            }

            // Check that this instructor belongs to the Track
            var instructor = _unitOfWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == id &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
            {
                return (
                    false,
                    "You are not assigned to this Track."
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
    }
}