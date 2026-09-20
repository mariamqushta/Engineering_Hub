using System;

namespace Engineering_Hub.DTO.Workshop
{
    public class WorkshopDTO
    {
        public int TrackId { get; set; }

        public string Title { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public string Place { get; set; }

        public DateTime Date { get; set; }

        public TimeSpan StartTime { get; set; }

        public TimeSpan EndTime { get; set; }

        public int Capacity { get; set; }
    }
}
