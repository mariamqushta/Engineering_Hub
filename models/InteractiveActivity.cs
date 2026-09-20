using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class InteractiveActivity
    {
        public int Id { get; set; }

        public int TrackId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }
        public decimal Price { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int Capacity { get; set; }

        public SessionStatus Status { get; set; }

        public string? MeetingUrl { get; set; }


        // Relationships

        public Track Track { get; set; }

        public ICollection<InteractiveBooking> Bookings { get; set; }
    }
}
