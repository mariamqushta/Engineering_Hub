
using System;

namespace Engineering_Hub.models
{
    public class TrackBookingRequest
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public int TrackId { get; set; }

        public TrackBookingType BookingType { get; set; }

        public BookingApprovalStatus ApprovalStatus { get; set; }
            = BookingApprovalStatus.Pending;

        public decimal RequestedPrice { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ApplicationUser Student { get; set; }

        public Track Track { get; set; }
    }
}
