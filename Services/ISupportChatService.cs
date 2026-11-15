using Engineering_Hub.DTO.SupportChatDTOs;
using Engineering_Hub.models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ISupportChatService
    {
        Task<SupportChatConversationDto> GetOrCreateConversationAsync(
            SupportChatType type,
            string participantId);

        Task<List<SupportChatConversationDto>> GetInboxAsync(
            SupportChatType type,
            string adminId);

        Task<List<SupportChatMessageDto>> GetMessagesAsync(
            SupportChatType type,
            int conversationId,
            string userId,
            bool isAdmin);

        Task<SupportChatMessageDto> SendMessageAsync(
            SupportChatType type,
            SendSupportChatMessageDto dto,
            string userId,
            bool isAdmin);

        Task<bool> UserCanAccessConversationAsync(
            SupportChatType type,
            int conversationId,
            string userId,
            bool isAdmin);
    }
}