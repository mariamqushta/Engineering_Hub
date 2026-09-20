using Engineering_Hub.DTO;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrackController : ControllerBase
    {
        private readonly ITrackService _trackService;

        public TrackController(ITrackService trackService)
        {
            _trackService = trackService;
        }

        // CREATE
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateTrack(
            TrackDTO dto)
        {
            var instructorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            var track =
                await _trackService.CreateTrackAsync(
                    dto,
                    instructorId);

            return Ok(track);
        }

        // GET ALL
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTracks()
        {
            var tracks =
                await _trackService.GetAllTracksAsync();

            return Ok(tracks);
        }

        // GET BY ID
        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetTrackById(
            int id)
        {
            var track =
                await _trackService.GetTrackByIdAsync(id);

            if (track == null)
            {
                return NotFound("Track not found.");
            }

            return Ok(track);
        }

        // UPDATE
        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateTrack(
            int id,
            TrackDTO dto)
        {
            var instructorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            var result =
                await _trackService.UpdateTrackAsync(
                    id,
                    dto,
                    instructorId);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        // DELETE / DEACTIVATE
        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteTrack(
            int id)
        {
            var instructorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            var result =
                await _trackService.DeleteTrackAsync(
                    id,
                    instructorId);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
    }
}