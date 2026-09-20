using Engineering_Hub.DTO.BookingDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface IInteractiveBookingService
    {
        Task<(bool Success, string Message)> BookInteractiveAsync(
            string userId,
            InteractiveBookingDto dto);

        Task<List<InteractiveBookingResponseDTO>> GetMyBookingsAsync(
    string userId);
    }
}