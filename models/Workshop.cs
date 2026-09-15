using System;
using System.Collections.Generic;

namespace Engineering_Hub.models
{
    public class Workshop
    {
        public int Id { get; set; }

        public int TrackId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public string Place { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int Capacity { get; set; }

        public SessionStatus Status { get; set; }


        // Relationships

        public Track Track { get; set; }

        public ICollection<WorkshopBooking> Bookings { get; set; }
    }
}
