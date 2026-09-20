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
    [Authorize]
    public class LessonController : ControllerBase
    {
        private readonly ILessonAccessService _lessonAccessService;
        private readonly ILessonCompletionService _lessonCompletionService;
        private readonly ILessonService _lessonService;

        public LessonController(
            ILessonAccessService lessonAccessService,
            ILessonCompletionService lessonCompletionService,
            ILessonService lessonService)
        {
            _lessonAccessService = lessonAccessService;
            _lessonCompletionService = lessonCompletionService;
            _lessonService = lessonService;
        }

        // =========================
        // STUDENT
        // =========================

        [HttpGet("{lessonId}")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetLesson(int lessonId)
        {
            var studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (studentId == null)
            {
                return Unauthorized();
            }

            var result = await _lessonAccessService
                .GetLessonAsync(lessonId, studentId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Lesson);
        }

        [HttpPost("{lessonId}/complete")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CompleteLesson(int lessonId)
        {
            var studentId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (studentId == null)
            {
                return Unauthorized();
            }

            var result = await _lessonCompletionService
                .CompleteLessonAsync(lessonId, studentId);

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


        // =========================
        // INSTRUCTOR
        // =========================

        [HttpPost]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateLesson(
            LessonDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            var lesson =
                await _lessonService.CreateLessonAsync(
                    dto,
                    instructorId);

            if (lesson == null)
            {
                return BadRequest(
                    "Track not found or you are not assigned to this Track.");
            }

            return Ok(lesson);
        }

        [HttpGet("Track/{trackId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetLessonsByTrack(
            int trackId)
        {
            var lessons =
                await _lessonService
                    .GetLessonsByTrackAsync(trackId);

            return Ok(lessons);
        }



        [HttpPut("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateLesson(
            int id,
            LessonDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            var result =
                await _lessonService.UpdateLessonAsync(
                    id,
                    dto,
                    instructorId);

            if (!result.Success)
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteLesson(int id)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            var result =
                await _lessonService.DeleteLessonAsync(
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