using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.Workshop;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Authorize(Roles = "Student")]
    [Route("api/[controller]")]
    public class WorkshopController : ControllerBase
    {
        private readonly IWorkshopService _workshopService;

        public WorkshopController(
            IWorkshopService workshopService)
        {
            _workshopService = workshopService;
        }


        // CREATE
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateWorkshop(
            WorkshopDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var workshop =
                await _workshopService.CreateWorkshopAsync(
                    dto,
                    instructorId);

            if (workshop == null)
                return BadRequest(
                    "Track not found, invalid data, or you are not assigned to this Track.");

            return Ok(workshop);
        }


        // GET WORKSHOPS BY TRACK
        [HttpGet("Track/{trackId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetWorkshopsByTrack(
            int trackId)
        {
            var workshops =
                await _workshopService
                    .GetWorkshopsByTrackAsync(trackId);

            return Ok(workshops);
        }


        // UPDATE
        [HttpPut("{id}")]
 
        public async Task<IActionResult> UpdateWorkshop(
            int id,
            WorkshopDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var result =
                await _workshopService.UpdateWorkshopAsync(
                    id,
                    dto,
                    instructorId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }


        // CANCEL
        [HttpPut("{id}/cancel")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CancelWorkshop(
            int id)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var result =
                await _workshopService.CancelWorkshopAsync(
                    id,
                    instructorId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }
    }
}