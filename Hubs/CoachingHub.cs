using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Hubs
{
    [Authorize]
    public class CoachingHub : Hub
    {
        private readonly EngineeringHubContext _context;

        public CoachingHub(EngineeringHubContext context)
        {
            _context = context;
        }

        public async Task JoinConversation(int conversationId)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated.");
            }

            var conversation = await _context.CoachingConversations
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conversation == null)
            {
                throw new HubException("Conversation not found.");
            }

            // Student can join their own conversation
            if (conversation.StudentId == userId)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"conversation-{conversationId}");

                return;
            }

            // Instructor can join only if assigned to the Track
            var isInstructor = await _context.TrackInstructors
                .AnyAsync(ti =>
                    ti.UserId == userId &&
                    ti.TrackId == conversation.TrackId);

            if (isInstructor)
            {
                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"conversation-{conversationId}");

                return;
            }

            throw new HubException(
                "You are not allowed to join this conversation.");
        }
    }
}