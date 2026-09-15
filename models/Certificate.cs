
using System;

namespace Engineering_Hub.models
{
    public class Certificate
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public int TrackId { get; set; }

        public DateTime IssuedAt { get; set; }

        public string? CertificateUrl { get; set; }


        // Relationships

        public ApplicationUser Student { get; set; }

        public Track Track { get; set; }
    }
}
