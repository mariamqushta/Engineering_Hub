using System.ComponentModel.DataAnnotations;

namespace Engineering_Hub.DTO.SupportChatDTOs
{
    public class SendSupportChatMessageDto
    {
        [Required]
        public int ConversationId { get; set; }

        [Required]
        [StringLength(5000, MinimumLength = 1)]
        public string Message { get; set; } = string.Empty;
    }
}