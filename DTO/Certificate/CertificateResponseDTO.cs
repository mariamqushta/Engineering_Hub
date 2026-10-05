using System;

namespace Engineering_Hub.DTO
{
    public class CertificateResponseDTO
    {
        public int Id { get; set; }

        public string StudentId { get; set; }

        public int TrackId { get; set; }

        public DateTime IssuedAt { get; set; }

        public string? CertificateUrl { get; set; }
    }
}