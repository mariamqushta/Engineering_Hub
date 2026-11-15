using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class SupportChatConversation
    {
        public int Id { get; set; }

        public SupportChatType Type { get; set; }

        // The student or instructor who owns this conversation.
        public string ParticipantId { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        public ApplicationUser Participant { get; set; } = null!;

        public ICollection<SupportChatMessage> Messages { get; set; }
            = new List<SupportChatMessage>();
    }
}