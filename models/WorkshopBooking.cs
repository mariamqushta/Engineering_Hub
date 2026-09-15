
using System;

namespace Engineering_Hub.models
{
    public enum SessionStatus
    {
        Scheduled,
        Ongoing,
        Finished,
        Cancelled
    }
    public class WorkshopBooking
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int WorkshopId { get; set; }

        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        public SessionStatus Status { get; set; }


        // Relationships

        public ApplicationUser User { get; set; }

        public Workshop Workshop { get; set; }
    }
}
