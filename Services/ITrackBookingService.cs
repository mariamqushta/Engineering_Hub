using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.models;
using System.Threading.Tasks;
using System.Collections.Generic;
namespace Engineering_Hub.Services
{
    public interface ITrackBookingService
    {
        Task<(bool Success, string Message, decimal? Price)> BookTrackAsync(
            string studentId,
            TrackBookingDto dto);

        Task<(bool Success, string Message, decimal? Price)> GetBookingPriceAsync(
            int trackId,
            TrackBookingType bookingType);

        Task<List<TrackBookingRequest>> GetPendingRequestsAsync();

        Task<(bool Success, string Message)> ApproveRequestAsync(int requestId);

        Task<(bool Success, string Message)> RejectRequestAsync(int requestId);

        Task<List<TrackBookingRequest>> GetMyRequestsAsync(string studentId);
    }
}