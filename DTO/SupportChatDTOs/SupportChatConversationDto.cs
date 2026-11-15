using System;

namespace Engineering_Hub.DTO.SupportChatDTOs
{
    public class SupportChatConversationDto
    {
        public int ConversationId { get; set; }

        public string ParticipantId { get; set; } = string.Empty;

        public string ParticipantName { get; set; } = string.Empty;

        // Useful to the Admin UI for distinguishing Group and Private Students.
        // It is informational only and is never used for authorization.
        public int? ParticipantAccountType { get; set; }

        public string LastMessage { get; set; } = string.Empty;

        public DateTime? LastMessageAtUtc { get; set; }

        public int UnreadCount { get; set; }
    }
}