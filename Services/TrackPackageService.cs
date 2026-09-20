using AutoMapper;
using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.TrackPackage;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class TrackPackageService : ITrackPackageService
    {
        private readonly UnitWork _unitWork;
        private readonly IMapper _mapper;

        public TrackPackageService(
            UnitWork unitWork,
            IMapper mapper)
        {
            _unitWork = unitWork;
            _mapper = mapper;
        }

        public async Task<TrackPackageResponseDTO?> CreatePackageAsync(
            TrackPackageDTO dto,
            string instructorId)
        {
            // 1. Find Track
            var track = _unitWork.Trackrepo.GetById(dto.TrackId);

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

            // 4. Map DTO → Entity
            var package = _mapper.Map<TrackPackage>(dto);

            // 5. Save
            _unitWork.TrackPackagerepo.add(package);

            await _unitWork.SaveAsync();

            // 6. Map Entity → Response DTO
            return _mapper.Map<TrackPackageResponseDTO>(package);
        }

        public async Task<List<TrackPackageResponseDTO>>
            GetPackagesByTrackAsync(int trackId)
        {
            var packages = _unitWork.TrackPackagerepo
                .GetByCondition(x => x.TrackId == trackId);

            return _mapper.Map<List<TrackPackageResponseDTO>>(packages);
        }

        public async Task<(bool Success, string Message)>
            UpdatePackageAsync(
                int id,
                TrackPackageDTO dto,
                string instructorId)
        {
            // 1. Find package
            var package = _unitWork.TrackPackagerepo.GetById(id);

            if (package == null)
                return (false, "Track package not found.");

            // 2. Find Track
            var track = _unitWork.Trackrepo.GetById(dto.TrackId);

            if (track == null || !track.IsActive)
                return (false, "Track not found or inactive.");

            // 3. Check instructor
            var instructor = _unitWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == dto.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
                return (
                    false,
                    "You are not assigned to this Track."
                );

            // 4. Validate price
            if (dto.Price < 0)
                return (false, "Price cannot be negative.");

            // 5. Update
            _mapper.Map(dto, package);

            await _unitWork.SaveAsync();

            return (
                true,
                "Track package updated successfully."
            );
        }
    }
}