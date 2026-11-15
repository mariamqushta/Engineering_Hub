using System;

namespace Engineering_Hub.DTO.SupportChatDTOs
{
    public class SupportChatMessageDto
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        public string SenderId { get; set; } = string.Empty;

        public string SenderName { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime SentAtUtc { get; set; }

        public bool IsRead { get; set; }
    }
}