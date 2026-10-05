
using Engineering_Hub.DTO.LessonContent;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LessonContentController : ControllerBase
    {
        private readonly ILessonContentService _lessonContentService;

        public LessonContentController(
            ILessonContentService lessonContentService)
        {
            _lessonContentService = lessonContentService;
        }

        // ==========================================
        // Add File Content
        // ==========================================

        [HttpPost("file")]
        public async Task<IActionResult> AddFileContent(
            [FromForm] AddFileContentDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            try
            {
                await _lessonContentService.AddFileContentAsync(
                    dto.LessonId,
                    dto.LessonTypeId,
                    dto.File,
                    dto.IsFree,
                    instructorId);

                return Ok(new
                {
                    message = "File content added successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }

        // ==========================================
        // Add Workshop Content
        // ==========================================

        [HttpPost("workshop")]
        public async Task<IActionResult> AddWorkshopContent(
            [FromBody] AddWorkshopContentDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            try
            {
                await _lessonContentService.AddWorkshopContentAsync(
                    dto.LessonId,
                    dto.LessonTypeId,
                    dto.WorkshopId,
                    instructorId);

                return Ok(new
                {
                    message = "Workshop content added successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // ==========================================
        // Add Interactive Content
        // ==========================================

        [HttpPost("interactive")]
        public async Task<IActionResult> AddInteractiveContent(
            [FromBody] AddInteractiveContentDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            try
            {
                await _lessonContentService.AddInteractiveContentAsync(
                    dto.LessonId,
                    dto.LessonTypeId,
                    dto.InteractiveActivityId,
                    instructorId);

                return Ok(new
                {
                    message = "Interactive content added successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // ==========================================
        // Get Lesson Contents
        // ==========================================

        [HttpGet("lesson/{lessonId}")]
        public async Task<IActionResult> GetLessonContents(
            int lessonId)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            try
            {
                var contents =
                    await _lessonContentService.GetLessonContentsAsync(
                        lessonId,
                        userId);

                return Ok(contents);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        // ==========================================
        // Update File Content
        // ==========================================


    [HttpPut("file/{contentId}")]
    public async Task<IActionResult> UpdateFileContent(
        int contentId,
    [FromForm] UpdateFileContentDTO dto)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            try
            {
                await _lessonContentService.UpdateFileContentAsync(
                    contentId,
                    dto.File,
                    dto.IsFree,
                    instructorId);

                return Ok(new
                {
                    message = "File content updated successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }



        // ==========================================
        // Delete Content
        // ==========================================

        [HttpDelete("{contentId}")]
        public async Task<IActionResult> DeleteContent(
            int contentId)
        {
            var instructorId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (instructorId == null)
            {
                return Unauthorized();
            }

            try
            {
                await _lessonContentService.DeleteContentAsync(
                    contentId,
                    instructorId);

                return Ok(new
                {
                    message = "Lesson content deleted successfully."
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}

