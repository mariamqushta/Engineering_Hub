using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Engineering_Hub.Hubs
{
    [Authorize]
    public class SupportChatHub : Hub
    {
        private readonly EngineeringHubContext _context;

        public SupportChatHub(EngineeringHubContext context)
        {
            _context = context;
        }

        private string? GetUserId()
        {
            return Context.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private static bool TryParseType(
            string chatType,
            out SupportChatType type)
        {
            return Enum.TryParse(chatType, true, out type) &&
                   (type == SupportChatType.Student ||
                    type == SupportChatType.Instructor);
        }

        private static string ConversationGroup(
            SupportChatType type,
            int conversationId)
        {
            return $"support-conversation-{type}-{conversationId}";
        }

        private static string InboxGroup(SupportChatType type)
        {
            return $"support-inbox-{type}";
        }

        public async Task JoinInbox(string chatType)
        {
            if (Context.User == null ||
                !Context.User.IsInRole("Admin"))
            {
                throw new HubException(
                    "Only Admin can access chat inboxes.");
            }

            if (!TryParseType(chatType, out var type))
            {
                throw new HubException("Invalid chat type.");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                InboxGroup(type));
        }

        public async Task JoinConversation(
            string chatType,
            int conversationId)
        {
            var userId = GetUserId();

            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated.");
            }

            if (!TryParseType(chatType, out var type))
            {
                throw new HubException("Invalid chat type.");
            }

            var isAdmin = Context.User!.IsInRole("Admin");

            if (!isAdmin)
            {
                if (type == SupportChatType.Student &&
                    !Context.User.IsInRole("Student"))
                {
                    throw new HubException(
                        "Only students can join student conversations.");
                }

                if (type == SupportChatType.Instructor &&
                    !Context.User.IsInRole("Instructor"))
                {
                    throw new HubException(
                        "Only instructors can join instructor conversations.");
                }
            }

            var conversation =
                await _context.SupportChatConversations
                    .FirstOrDefaultAsync(c =>
                        c.Id == conversationId &&
                        c.Type == type);

            if (conversation == null)
            {
                throw new HubException("Conversation not found.");
            }

            // Admin can access all conversations of either type.
            // Other users can access only their own conversation.
            if (!isAdmin && conversation.ParticipantId != userId)
            {
                throw new HubException(
                    "You are not allowed to join this conversation.");
            }

            await Groups.AddToGroupAsync(
                Context.ConnectionId,
                ConversationGroup(type, conversationId));
        }
    }
}