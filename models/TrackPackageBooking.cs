
using System;

namespace Engineering_Hub.models
{

    public class TrackPackageBooking
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public int TrackPackageId { get; set; }

        public DateTime BookedAt { get; set; } = DateTime.UtcNow;

        public SessionStatus Status { get; set; }


        // Relationships

        public ApplicationUser User { get; set; }

        public TrackPackage TrackPackage { get; set; }
    }
}
