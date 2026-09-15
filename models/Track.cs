using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class Track
    {
        public int Id { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;


        // Relationships

        public ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

        public ICollection<TrackInstructor> TrackInstructors { get; set; }

        public ICollection<TrackEnrollment> TrackEnrollments { get; set; }

        public ICollection<Workshop> Workshops { get; set; }

        public ICollection<InteractiveActivity> InteractiveActivities { get; set; }

        public ICollection<TrackPackage> TrackPackages { get; set; }

        public ICollection<Certificate> Certificates { get; set; }
    }
}
