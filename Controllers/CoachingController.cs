using Engineering_Hub.DTO.CoachingDTOs;
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
    public class CoachingController : ControllerBase
    {
        private readonly ICoachingService _coachingService;

        public CoachingController(ICoachingService coachingService)
        {
            _coachingService = coachingService;
        }

        [HttpPost("conversation/{trackId}")]
        public async Task<IActionResult> GetOrCreateConversation(
            int trackId)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var result =
                await _coachingService.GetOrCreateConversationAsync(
                    userId,
                    trackId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(new
            {
                message = result.Message,
                conversationId = result.ConversationId
            });
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage(
            SendCoachingMessageDto dto)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var result =
                await _coachingService.SendMessageAsync(
                    userId,
                    dto);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Data);
        }

        [HttpGet("conversation/{conversationId}/messages")]
        public async Task<IActionResult> GetMessages(
            int conversationId)
        {
            var userId =
                User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Unauthorized();
            }

            var result =
                await _coachingService.GetMessagesAsync(
                    userId,
                    conversationId);

            if (!result.Success)
            {
                return BadRequest(new
                {
                    message = result.Message
                });
            }

            return Ok(result.Messages);
        }
    }
}