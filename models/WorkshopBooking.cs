
using System;

namespace Engineering_Hub.models
{
    public class WorkshopBooking
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int WorkshopId { get; set; }

        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        public SessionStatus Status { get; set; }

        public BookingSource Source { get; set; }

        public BookingApprovalStatus ApprovalStatus { get; set; }
            = BookingApprovalStatus.Pending;

        // Relationships
        public ApplicationUser User { get; set; }

        public Workshop Workshop { get; set; }
    }
}
