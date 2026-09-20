using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.Workshop;
using System.Collections.Generic;
using System.Threading.Tasks;

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
    }
}