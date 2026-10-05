using Engineering_Hub.DTO.CoachingDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ICoachingService
    {
        Task<(bool Success, string Message, int? ConversationId)>
            GetOrCreateConversationAsync(
                string studentId,
                int trackId);

        Task<(bool Success, string Message, CoachingMessageResponseDto? Data)>
            SendMessageAsync(
                string userId,
                SendCoachingMessageDto dto);

        Task<(bool Success, string Message, List<CoachingMessageResponseDto>? Messages)>
            GetMessagesAsync(
                string userId,
                int conversationId);
    }
}