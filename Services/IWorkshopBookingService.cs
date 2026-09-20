using Engineering_Hub.DTO.BookingDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface IWorkshopBookingService
    {
        Task<(bool Success, string Message)> BookWorkshopAsync(
            string userId,
            WorkshopBookingDto dto);

        Task<List<WorkshopBookingResponseDTO>> GetMyBookingsAsync(
     string userId);
    }
}