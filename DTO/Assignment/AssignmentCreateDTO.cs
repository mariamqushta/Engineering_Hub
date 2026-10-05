using System;

namespace Engineering_Hub.DTO
{
    public class AssignmentCreateDTO
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
    }
}