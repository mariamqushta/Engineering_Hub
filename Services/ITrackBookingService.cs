using Engineering_Hub.DTO.BookingDTOs;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ITrackBookingService
    {
        Task<(bool Success, string Message)> BookTrackAsync(
            string studentId,
            TrackBookingDto dto);
    }
}
