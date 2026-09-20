using Engineering_Hub.DTO.BookingDTOs;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ITrackPackageBookingService
    {
        Task<(bool Success, string Message)> BookTrackPackageAsync(
            string userId,
            TrackPackageBookingDto dto);
    }
}