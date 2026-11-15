using Engineering_Hub.DTO.SupportChatDTOs;
using Engineering_Hub.Hubs;
using Engineering_Hub.models;
using Engineering_Hub.models.context;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class SupportChatService : ISupportChatService
    {
        private readonly EngineeringHubContext _context;
        private readonly IHubContext<SupportChatHub> _hubContext;

        public SupportChatService(
            EngineeringHubContext context,
            IHubContext<SupportChatHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
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

        private static void ValidateType(SupportChatType type)
        {
            if (type != SupportChatType.Student &&
                type != SupportChatType.Instructor)
            {
                throw new ArgumentException("Invalid chat type.");
            }
        }

        private async Task<SupportChatConversation?> FindAccessibleConversationAsync(
            SupportChatType type,
            int conversationId,
            string userId,
            bool isAdmin)
        {
            var conversation = await _context.SupportChatConversations
                .FirstOrDefaultAsync(c =>
                    c.Id == conversationId &&
                    c.Type == type);

            if (conversation == null)
                return null;

            if (isAdmin || conversation.ParticipantId == userId)
                return conversation;

            return null;
        }

        public async Task<bool> UserCanAccessConversationAsync(
            SupportChatType type,
            int conversationId,
            string userId,
            bool isAdmin)
        {
            ValidateType(type);

            return await _context.SupportChatConversations.AnyAsync(c =>
                c.Id == conversationId &&
                c.Type == type &&
                (isAdmin || c.ParticipantId == userId));
        }

        public async Task<SupportChatConversationDto>
            GetOrCreateConversationAsync(
                SupportChatType type,
                string participantId)
        {
            ValidateType(type);

            if (string.IsNullOrWhiteSpace(participantId))
                throw new ArgumentException("Participant is required.");

            var participant = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == participantId);

            if (participant == null)
                throw new InvalidOperationException("User not found.");

            var conversation = await _context.SupportChatConversations
                .FirstOrDefaultAsync(c =>
                    c.Type == type &&
                    c.ParticipantId == participantId);

            if (conversation == null)
            {
                conversation = new SupportChatConversation
                {
                    Type = type,
                    ParticipantId = participantId,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _context.SupportChatConversations.Add(conversation);

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    // Another request may have created the same conversation
                    // at the same time. The unique index prevents duplicates.
                    _context.Entry(conversation).State =
                        EntityState.Detached;

                    conversation = await _context.SupportChatConversations
                        .FirstOrDefaultAsync(c =>
                            c.Type == type &&
                            c.ParticipantId == participantId);

                    if (conversation == null)
                        throw;
                }
            }

            return await GetConversationSummaryAsync(
             conversation.Id,
             type,
             false);
        }

        public async Task<List<SupportChatConversationDto>> GetInboxAsync(
            SupportChatType type,
            string adminId)
        {
            ValidateType(type);

            return await _context.SupportChatConversations
                .AsNoTracking()
                .Where(c => c.Type == type)
                .Select(c => new SupportChatConversationDto
                {
                    ConversationId = c.Id,
                    ParticipantId = c.ParticipantId,

                    ParticipantName =
                        c.Participant.FullName,

                    ParticipantAccountType =
                        type == SupportChatType.Student
                            ? (int?)c.Participant.AccountType
                            : null,

                    LastMessage =
                        c.Messages
                            .OrderByDescending(m => m.SentAtUtc)
                            .Select(m => m.Message)
                            .FirstOrDefault() ?? string.Empty,

                    LastMessageAtUtc =
                        c.Messages
                            .OrderByDescending(m => m.SentAtUtc)
                            .Select(m => (DateTime?)m.SentAtUtc)
                            .FirstOrDefault(),

                    UnreadCount =
                        c.Messages.Count(m =>
                            m.SenderId != adminId &&
                            !m.IsRead)
                })
                .OrderByDescending(c => c.LastMessageAtUtc)
                .ThenByDescending(c => c.ConversationId)
                .ToListAsync();
        }

        public async Task<List<SupportChatMessageDto>> GetMessagesAsync(
            SupportChatType type,
            int conversationId,
            string userId,
            bool isAdmin)
        {
            ValidateType(type);

            var conversation = await FindAccessibleConversationAsync(
                type,
                conversationId,
                userId,
                isAdmin);

            if (conversation == null)
                throw new UnauthorizedAccessException(
                    "You do not have access to this conversation.");

            // Mark messages sent by the other participant as read.
            var unreadMessages = await _context.SupportChatMessages
                .Where(m =>
                    m.ConversationId == conversationId &&
                    m.SenderId != userId &&
                    !m.IsRead)
                .ToListAsync();

            if (unreadMessages.Count > 0)
            {
                foreach (var message in unreadMessages)
                {
                    message.IsRead = true;
                }

                await _context.SaveChangesAsync();

                await _hubContext.Clients
                    .Group(ConversationGroup(type, conversationId))
                    .SendAsync("MessagesRead", new
                    {
                        ConversationId = conversationId,
                        ReaderId = userId
                    });

                if (isAdmin)
                {
                    await _hubContext.Clients
                    .Group(InboxGroup(type))
                    .SendAsync("ConversationUpdated",
                        await GetConversationSummaryAsync(
                            conversationId,
                            type,
                            true));
                }
            }

            return await _context.SupportChatMessages
                .AsNoTracking()
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAtUtc)
                .Select(m => new SupportChatMessageDto
                {
                    Id = m.Id,
                    ConversationId = m.ConversationId,
                    SenderId = m.SenderId,
                    SenderName = m.Sender.FullName,
                    Message = m.Message,
                    SentAtUtc = m.SentAtUtc,
                    IsRead = m.IsRead
                })
                .ToListAsync();
        }

        public async Task<SupportChatMessageDto> SendMessageAsync(
            SupportChatType type,
            SendSupportChatMessageDto dto,
            string userId,
            bool isAdmin)
        {
            ValidateType(type);

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.Message))
                throw new ArgumentException("Message cannot be empty.");

            if (dto.Message.Length > 5000)
                throw new ArgumentException(
                    "Message cannot exceed 5000 characters.");

            var conversation = await FindAccessibleConversationAsync(
                type,
                dto.ConversationId,
                userId,
                isAdmin);

            if (conversation == null)
                throw new UnauthorizedAccessException(
                    "You do not have access to this conversation.");

            var sender = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (sender == null)
                throw new InvalidOperationException("Sender not found.");

            var message = new SupportChatMessage
            {
                ConversationId = conversation.Id,
                SenderId = userId,
                Message = dto.Message.Trim(),
                SentAtUtc = DateTime.UtcNow,
                IsRead = false
            };

            _context.SupportChatMessages.Add(message);

            await _context.SaveChangesAsync();

            var response = new SupportChatMessageDto
            {
                Id = message.Id,
                ConversationId = message.ConversationId,
                SenderId = message.SenderId,
                SenderName = sender.FullName,
                Message = message.Message,
                SentAtUtc = message.SentAtUtc,
                IsRead = message.IsRead
            };

            // Notify everyone currently inside this conversation.
            await _hubContext.Clients
                .Group(ConversationGroup(type, conversation.Id))
                .SendAsync("ReceiveMessage", response);

            // Notify Admin's inbox so the conversation preview and unread
            // count can update even when the Admin is viewing another chat.
            var summary = await GetConversationSummaryAsync(
                 conversation.Id,
                 type,
                 true);

            await _hubContext.Clients
                .Group(InboxGroup(type))
                .SendAsync("ConversationUpdated", summary);

            return response;
        }

        private async Task<SupportChatConversationDto>
         GetConversationSummaryAsync(
             int conversationId,
             SupportChatType type,
             bool forAdminInbox)
        {
            return await _context.SupportChatConversations
                .AsNoTracking()
                .Where(c =>
                    c.Id == conversationId &&
                    c.Type == type)
                .Select(c => new SupportChatConversationDto
                {
                    ConversationId = c.Id,
                    ParticipantId = c.ParticipantId,
                    ParticipantName = c.Participant.FullName,

                    ParticipantAccountType =
                        type == SupportChatType.Student
                            ? (int?)c.Participant.AccountType
                            : null,

                    LastMessage =
                        c.Messages
                            .OrderByDescending(m => m.SentAtUtc)
                            .Select(m => m.Message)
                            .FirstOrDefault() ?? string.Empty,

                    LastMessageAtUtc =
                        c.Messages
                            .OrderByDescending(m => m.SentAtUtc)
                            .Select(m => (DateTime?)m.SentAtUtc)
                            .FirstOrDefault(),

                    UnreadCount = forAdminInbox
                        ? c.Messages.Count(m =>
                            m.SenderId == c.ParticipantId &&
                            !m.IsRead)
                        : c.Messages.Count(m =>
                            m.SenderId != c.ParticipantId &&
                            !m.IsRead)
                })
                .FirstAsync();
        }
    }
}