using Engineering_Hub.DTO;
using Engineering_Hub.models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ITrackService
    {
        Task<TrackResponseDTO> CreateTrackAsync(
     TrackDTO dto,
     string instructorId);

        Task<List<TrackResponseDTO>> GetAllTracksAsync();

        Task<TrackResponseDTO?> GetTrackByIdAsync(int id);

        Task<(bool Success, string Message)> UpdateTrackAsync(
            int id,
            TrackDTO dto,
            string instructorId);

        Task<(bool Success, string Message)> DeleteTrackAsync(
            int id,
            string instructorId);
    }
}