using Engineering_Hub.DTO.SupportChatDTOs;
using Engineering_Hub.models;
using Engineering_Hub.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SupportChatController : ControllerBase
    {
        private readonly ISupportChatService _chatService;

        public SupportChatController(ISupportChatService chatService)
        {
            _chatService = chatService;
        }

        private string? CurrentUserId =>
            User.FindFirstValue(ClaimTypes.NameIdentifier);

        private bool IsAdmin =>
            User.IsInRole("Admin");

        // =========================
        // STUDENT CHAT
        // =========================

        // Both Group and Private Students use the Student role.
        // No AccountType restriction is applied.
        [HttpGet("students/conversation")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetStudentConversation()
        {
            if (CurrentUserId == null)
                return Unauthorized();

            try
            {
                var conversation =
                    await _chatService.GetOrCreateConversationAsync(
                        SupportChatType.Student,
                        CurrentUserId);

                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("students/inbox")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetStudentsInbox()
        {
            if (CurrentUserId == null)
                return Unauthorized();

            var conversations = await _chatService.GetInboxAsync(
                SupportChatType.Student,
                CurrentUserId);

            return Ok(conversations);
        }

        [HttpGet("students/conversations/{conversationId}/messages")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> GetStudentMessages(
            int conversationId)
        {
            if (CurrentUserId == null)
                return Unauthorized();

            try
            {
                var messages = await _chatService.GetMessagesAsync(
                    SupportChatType.Student,
                    conversationId,
                    CurrentUserId,
                    IsAdmin);

                return Ok(messages);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("students/messages")]
        [Authorize(Roles = "Student,Admin")]
        public async Task<IActionResult> SendStudentMessage(
            [FromBody] SendSupportChatMessageDto dto)
        {
            if (CurrentUserId == null)
                return Unauthorized();

            try
            {
                var message = await _chatService.SendMessageAsync(
                    SupportChatType.Student,
                    dto,
                    CurrentUserId,
                    IsAdmin);

                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // INSTRUCTOR CHAT
        // =========================

        [HttpGet("instructors/conversation")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> GetInstructorConversation()
        {
            if (CurrentUserId == null)
                return Unauthorized();

            try
            {
                var conversation =
                    await _chatService.GetOrCreateConversationAsync(
                        SupportChatType.Instructor,
                        CurrentUserId);

                return Ok(conversation);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet("instructors/inbox")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetInstructorsInbox()
        {
            if (CurrentUserId == null)
                return Unauthorized();

            var conversations = await _chatService.GetInboxAsync(
                SupportChatType.Instructor,
                CurrentUserId);

            return Ok(conversations);
        }

        [HttpGet("instructors/conversations/{conversationId}/messages")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> GetInstructorMessages(
            int conversationId)
        {
            if (CurrentUserId == null)
                return Unauthorized();

            try
            {
                var messages = await _chatService.GetMessagesAsync(
                    SupportChatType.Instructor,
                    conversationId,
                    CurrentUserId,
                    IsAdmin);

                return Ok(messages);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("instructors/messages")]
        [Authorize(Roles = "Instructor,Admin")]
        public async Task<IActionResult> SendInstructorMessage(
            [FromBody] SendSupportChatMessageDto dto)
        {
            if (CurrentUserId == null)
                return Unauthorized();

            try
            {
                var message = await _chatService.SendMessageAsync(
                    SupportChatType.Instructor,
                    dto,
                    CurrentUserId,
                    IsAdmin);

                return Ok(message);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}