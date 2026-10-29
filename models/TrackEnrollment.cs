
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
        public decimal ApprovedPrice { get; set; }
        public TrackBookingType BookingType { get; set; }

        public BookingApprovalStatus ApprovalStatus { get; set; }
            = BookingApprovalStatus.Pending;

        // Navigation properties
        public ApplicationUser Student { get; set; }

        public Track Track { get; set; }
    }
}
