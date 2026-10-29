namespace Engineering_Hub.DTO.CoachingDTOs
{
    public class InstructorConversationDto
    {
        public int ConversationId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string TrackTitle { get; set; } = string.Empty;
        public string LastMessage { get; set; } = string.Empty;
    }
}