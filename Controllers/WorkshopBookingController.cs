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
    [Authorize]
    public class WorkshopBookingController : ControllerBase
    {
        private readonly IWorkshopBookingService _bookingService;

        public WorkshopBookingController(
            IWorkshopBookingService bookingService)
        {
            _bookingService = bookingService;
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

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> BookWorkshop(
            WorkshopBookingDto dto)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _bookingService
                .BookWorkshopAsync(userId, dto);

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
    }
}