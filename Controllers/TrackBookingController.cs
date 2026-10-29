using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.models;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Student")]
    public class TrackBookingController : ControllerBase
    {
        private readonly ITrackBookingService _bookingService;

        public TrackBookingController(ITrackBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> BookTrack(TrackBookingDto dto)
        {
            // Get the logged-in student's ID from JWT
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (studentId == null)
            {
                return Unauthorized();
            }

            var result = await _bookingService
                .BookTrackAsync(studentId, dto);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(new
            {
                message = result.Message,
                price = result.Price
            });
        }

        [HttpGet("price/{trackId}/{bookingType}")]
        public async Task<IActionResult> GetBookingPrice(
    int trackId,
    TrackBookingType bookingType)
        {
            var result = await _bookingService
                .GetBookingPriceAsync(trackId, bookingType);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(new
            {
                price = result.Price
            });
        }
        [HttpGet("MyBookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var studentId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (studentId == null)
                return Unauthorized();

            var requests = await _bookingService.GetMyRequestsAsync(studentId);

            return Ok(requests);
        }
    }
}