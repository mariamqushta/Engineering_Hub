
using System;

namespace Engineering_Hub.models
{
    public class AssignmentSubmission
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public string StudentId { get; set; }

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public string? Content { get; set; }

        public string? FileUrl { get; set; }

        public decimal? Grade { get; set; }

        public string? Feedback { get; set; }

        // Student's uploaded file metadata
        public string? OriginalFileName { get; set; }
        public string? StoredFileName { get; set; }
        public string? ContentType { get; set; }
        public long? FileSize { get; set; }

        // Relationships

        public Assignment Assignment { get; set; }

        public ApplicationUser Student { get; set; }
    }
}
