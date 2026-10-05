using System;

namespace Engineering_Hub.DTO
{
    public class AssignmentSubmissionResponseDTO
    {
        public int Id { get; set; }

        public int AssignmentId { get; set; }

        public string StudentId { get; set; }

        public DateTime SubmittedAt { get; set; }

        public string? Content { get; set; }

        public string? FileUrl { get; set; }

        public decimal? Grade { get; set; }

        public string? Feedback { get; set; }
    }
}