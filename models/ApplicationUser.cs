using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }

        public string? ProfileImageUrl { get; set; }

        public string? Bio { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // Relationships

        public ICollection<TrackInstructor> TrackInstructors { get; set; }

        public ICollection<TrackEnrollment> TrackEnrollments { get; set; }

        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; }

        public ICollection<WorkshopBooking> WorkshopBookings { get; set; }

        public ICollection<InteractiveBooking> InteractiveBookings { get; set; }

        public ICollection<TrackPackageBooking> TrackPackageBookings { get; set; }

        public ICollection<Certificate> Certificates { get; set; }

        public ICollection<LessonProgress> LessonProgresses { get; set; }
    }
}
