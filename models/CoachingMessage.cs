using System;

namespace Engineering_Hub.models
{
    public class CoachingMessage
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        public string SenderId { get; set; }

        public string Message { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public bool IsRead { get; set; } = false;

        // Navigation properties
        public CoachingConversation Conversation { get; set; }

        public ApplicationUser Sender { get; set; }
    }
}