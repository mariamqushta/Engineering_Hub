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
    public class TrackPackageBookingController : ControllerBase
    {
        private readonly ITrackPackageBookingService _trackPackageBookingService;

        public TrackPackageBookingController(
            ITrackPackageBookingService trackPackageBookingService)
        {
            _trackPackageBookingService = trackPackageBookingService;
        }

        [HttpPost]
        public async Task<IActionResult> BookTrackPackage(
            TrackPackageBookingDto dto)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var result = await _trackPackageBookingService
                .BookTrackPackageAsync(userId, dto);

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