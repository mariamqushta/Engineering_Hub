using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.InteractiveActivity;
using Engineering_Hub.models;
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

        Task<InteractiveSchedule?> CreateScheduleAsync(
            InteractiveScheduleDTO dto,
            string instructorId);

        Task<List<InteractiveSchedule>> GetSchedulesByTrackAsync(
            int trackId);
        Task<(bool Success, string Message)> UpdateScheduleAsync(
    int scheduleId,
    InteractiveScheduleDTO dto,
    string instructorId);

        Task<(bool Success, string Message)> EndScheduleAsync(
            int scheduleId,
            string instructorId);
    }
}