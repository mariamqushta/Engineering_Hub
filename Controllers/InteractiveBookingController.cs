using Engineering_Hub.DTO.BookingDTOs;
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
    public class InteractiveBookingController : ControllerBase
    {
        private readonly IInteractiveBookingService _bookingService;

        public InteractiveBookingController(
            IInteractiveBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        public async Task<IActionResult> BookInteractive(
            InteractiveBookingDto dto)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _bookingService
                .BookInteractiveAsync(userId, dto);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(new
            {
                message = result.Message
            });
        }
        [HttpGet("MyBookings")]
        public async Task<IActionResult> GetMyBookings()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var bookings =
                await _bookingService
                    .GetMyBookingsAsync(userId);

            return Ok(bookings);
        }
    }
}