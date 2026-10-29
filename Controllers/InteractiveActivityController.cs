using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.InteractiveActivity;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InteractiveActivityController : ControllerBase
    {
        private readonly IInteractiveActivityService _activityService;

        public InteractiveActivityController(
            IInteractiveActivityService activityService)
        {
            _activityService = activityService;
        }


        // CREATE
        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateActivity(
           InteractiveActivityDTO dto)
        {
            var instructorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var activity =
                await _activityService.CreateActivityAsync(
                    dto,
                    instructorId);

            if (activity == null)
                return BadRequest(
                    "Track not found, invalid data, or you are not assigned to this Track.");

            return Ok(activity);
        }


        // GET BY TRACK
        [HttpGet("Track/{trackId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActivitiesByTrack(
            int trackId)
        {
            var activities =
                await _activityService
                    .GetActivitiesByTrackAsync(trackId);

            return Ok(activities);
        }


        // UPDATE
        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateActivity(
            int id,
            InteractiveActivityDTO dto)
        {
            var instructorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var result =
                await _activityService.UpdateActivityAsync(
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
        public async Task<IActionResult> CancelActivity(
            int id)
        {
            var instructorId =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var result =
                await _activityService.CancelActivityAsync(
                    id,
                    instructorId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        // CREATE A RECURRING SCHEDULE
        [HttpPost("schedule")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateSchedule(
            [FromBody] InteractiveScheduleDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var schedule = await _activityService.CreateScheduleAsync(
                dto, instructorId);

            if (schedule == null)
            {
                return BadRequest(
                    "Invalid schedule, track, or instructor assignment.");
            }

            return Ok(schedule);
        }


        // GET SCHEDULES FOR A TRACK
        [HttpGet("Track/{trackId}/Schedules")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSchedulesByTrack(int trackId)
        {
            var schedules =
                await _activityService.GetSchedulesByTrackAsync(trackId);

            return Ok(schedules);
        }

        // UPDATE A RECURRING SCHEDULE
        [HttpPut("schedule/{scheduleId}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateSchedule(
            int scheduleId,
            [FromBody] InteractiveScheduleDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var result = await _activityService.UpdateScheduleAsync(
                scheduleId, dto, instructorId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }


        // END A RECURRING SCHEDULE
        [HttpPut("schedule/{scheduleId}/end")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> EndSchedule(int scheduleId)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
                return Unauthorized();

            var result = await _activityService.EndScheduleAsync(
                scheduleId, instructorId);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }


    }
}