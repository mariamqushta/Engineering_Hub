
using System;

namespace Engineering_Hub.models
{
    public class InteractiveBooking
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int InteractiveActivityId { get; set; }

        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        public SessionStatus Status { get; set; }


        // Relationships

        public ApplicationUser User { get; set; }

        public InteractiveActivity InteractiveActivity { get; set; }
    }
}
