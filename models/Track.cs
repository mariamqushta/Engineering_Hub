using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class Track
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }
        public decimal Price { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;


        // Relationships

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        public ICollection<TrackInstructor> TrackInstructors { get; set; } = new List<TrackInstructor>();

        public ICollection<TrackEnrollment> TrackEnrollments { get; set; } = new List<TrackEnrollment>();

        public ICollection<Workshop> Workshops { get; set; } = new List<Workshop>();

        public ICollection<InteractiveActivity> InteractiveActivities { get; set; } = new List<InteractiveActivity>();


        public ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();
    }
}
