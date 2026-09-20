using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.TrackPackage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ITrackPackageService
    {
        Task<TrackPackageResponseDTO?> CreatePackageAsync(
            TrackPackageDTO dto,
            string instructorId);

        Task<List<TrackPackageResponseDTO>> GetPackagesByTrackAsync(
            int trackId);

        Task<(bool Success, string Message)> UpdatePackageAsync(
            int id,
            TrackPackageDTO dto,
            string instructorId);
    }
}