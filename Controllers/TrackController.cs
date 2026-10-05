using Engineering_Hub.DTO;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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

        // =========================
        // CREATE TRACK
        // Admin only
        // =========================
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTrack(
            TrackDTO dto)
        {
            var track =
                await _trackService.CreateTrackAsync(dto);

            return Ok(track);
        }

        // =========================
        // GET ALL TRACKS
        // Public
        // =========================
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllTracks()
        {
            var tracks =
                await _trackService.GetAllTracksAsync();

            return Ok(tracks);
        }

        // =========================
        // GET TRACK BY ID
        // Public
        // =========================
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

        // =========================
        // UPDATE TRACK
        // Admin only
        // =========================
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTrack(
            int id,
            TrackDTO dto)
        {
            var result =
                await _trackService.UpdateTrackAsync(
                    id,
                    dto);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        // =========================
        // DEACTIVATE TRACK
        // Admin only
        // =========================
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTrack(
            int id)
        {
            var result =
                await _trackService.DeleteTrackAsync(id);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        // =========================
        // ASSIGN INSTRUCTOR
        // Admin only
        // =========================
        [HttpPost("{trackId}/instructors/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignInstructor(
       int trackId,
       string username)
        {
            var result =
                await _trackService.AssignInstructorAsync(
                    trackId,
                    username);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        [HttpDelete("{trackId}/instructors/{username}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveInstructor(
    int trackId,
    string username)
        {
            var result =
                await _trackService.RemoveInstructorAsync(
                    trackId,
                    username);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }
    }
}