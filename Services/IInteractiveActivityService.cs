using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.InteractiveActivity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface IInteractiveActivityService
    {
        Task<InteractiveActivityResponseDTO?> CreateActivityAsync(
            InteractiveActivityDTO dto,
            string instructorId);

        Task<List<InteractiveActivityResponseDTO>>
            GetActivitiesByTrackAsync(int trackId);

        Task<(bool Success, string Message)> UpdateActivityAsync(
            int id,
            InteractiveActivityDTO dto,
            string instructorId);

        Task<(bool Success, string Message)> CancelActivityAsync(
            int id,
            string instructorId);
    }
}