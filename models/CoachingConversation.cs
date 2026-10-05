using System;

namespace Engineering_Hub.models
{
    public class CoachingConversation
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public int TrackId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser Student { get; set; }

        public Track Track { get; set; }
    }
}