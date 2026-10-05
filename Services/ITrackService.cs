using Engineering_Hub.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ITrackService
    {
        Task<TrackResponseDTO> CreateTrackAsync(
            TrackDTO dto);

        Task<List<TrackResponseDTO>> GetAllTracksAsync();

        Task<TrackResponseDTO?> GetTrackByIdAsync(
            int id);

        Task<(bool Success, string Message)> UpdateTrackAsync(
            int id,
            TrackDTO dto);

        Task<(bool Success, string Message)> DeleteTrackAsync(
            int id);
        Task<(bool Success, string Message)> AssignInstructorAsync(
            int trackId,
            string username);

        Task<(bool Success, string Message)> RemoveInstructorAsync(
            int trackId,
            string username);
    }
}