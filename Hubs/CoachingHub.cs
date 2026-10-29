
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
            var userId = Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;

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

            // Student: must own the conversation AND have active
            // coaching access for the conversation's track.
            if (conversation.StudentId == userId)
            {
                var hasCoachingAccess =
                    await _context.TrackEnrollments.AnyAsync(e =>
                        e.StudentId == userId &&
                        e.TrackId == conversation.TrackId &&
                        e.Status == SessionStatus.Active &&
                        e.BookingType == TrackBookingType.TrackWithCoaching);

                if (!hasCoachingAccess)
                {
                    throw new HubException(
                        "You must have an active coaching booking to join this conversation.");
                }

                await Groups.AddToGroupAsync(
                    Context.ConnectionId,
                    $"conversation-{conversationId}");

                return;
            }

            // Instructor: must be assigned to the conversation's track.
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
