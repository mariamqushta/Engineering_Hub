using Engineering_Hub.models;
using System;

namespace Engineering_Hub.DTO.BookingDTOs
{
    public class WorkshopBookingResponseDTO
    {
        public int BookingId { get; set; }
        public int WorkshopId { get; set; }
        public DateTime BookedAt { get; set; }
        public SessionStatus BookingStatus { get; set; }

        public WorkshopResponseDTO Workshop { get; set; }
    }

    public class WorkshopResponseDTO
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
        public decimal Price { get; set; }
    }
}