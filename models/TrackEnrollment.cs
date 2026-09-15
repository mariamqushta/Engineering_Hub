
using System;

namespace Engineering_Hub.models
{
    public class TrackEnrollment
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public int TrackId { get; set; }

        public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;

        public SessionStatus Status { get; set; }

        public DateTime? CompletedAt { get; set; }


        // Navigation properties

        public ApplicationUser Student { get; set; }

        public Track Track { get; set; }
    }
}
