using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.Workshop;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;


namespace Engineering_Hub.Services
{
    public interface IWorkshopService
    {
        Task<WorkshopResponseDTO?> CreateWorkshopAsync(
            WorkshopDTO dto,
            string instructorId);

        Task<List<WorkshopResponseDTO>> GetWorkshopsByTrackAsync(
            int trackId);

        Task<(bool Success, string Message)> UpdateWorkshopAsync(
            int id,
            WorkshopDTO dto,
            string instructorId);

        Task<(bool Success, string Message)> CancelWorkshopAsync(
            int id,
            string instructorId);

        Task<(bool Success, string Message)> AdminApproveWorkshopAsync(int id);

        Task<(bool Success, string Message)> AdminRejectWorkshopAsync(int id);

        Task<(bool Success, string Message)> AdminRescheduleWorkshopAsync(
            int id,
            DateTime date,
            TimeSpan startTime,
            TimeSpan endTime);

        Task<(bool Success, string Message)> AdminCancelWorkshopAsync(int id);
        Task<List<WorkshopResponseDTO>>
    GetWorkshopsByInstructorAsync(string instructorId);
    }
}