using Engineering_Hub.models;
using System;

namespace Engineering_Hub.DTO.Calendar
{
    public class CalendarEventDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string EventType { get; set; } = string.Empty;

        public int TrackId { get; set; }

        public string TrackTitle { get; set; } = string.Empty;

        public DateTime StartDateTime { get; set; }

        public DateTime EndDateTime { get; set; }

        public SessionStatus Status { get; set; }
    }
}