using System;

namespace Engineering_Hub.models
{
    public class SupportChatMessage
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        public string SenderId { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

        // False until the other participant reads the message.
        public bool IsRead { get; set; } = false;

        public SupportChatConversation Conversation { get; set; } = null!;

        public ApplicationUser Sender { get; set; } = null!;
    }
}