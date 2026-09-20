using Engineering_Hub.models;
using System;

namespace Engineering_Hub.DTO.BookingDTOs
{
    public class InteractiveActivityResponseDTO
    {
        public int Id { get; set; }
        public int TrackId { get; set; }
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int Capacity { get; set; }
        public SessionStatus Status { get; set; }
        public decimal Price { get; set; }
        public string? MeetingUrl { get; set; }
    }
}