
using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class InteractiveSchedule
    {
        public int Id { get; set; }

        public int TrackId { get; set; }
        public Track Track { get; set; } = null!;

        public string InstructorId { get; set; } = null!;
        public ApplicationUser Instructor { get; set; } = null!;

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public string? MeetingUrl { get; set; }

        // Example: "Sunday,Tuesday"
        public string DaysOfWeek { get; set; } = string.Empty;

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<InteractiveActivity> Activities { get; set; }
            = new List<InteractiveActivity>();
    }
}
