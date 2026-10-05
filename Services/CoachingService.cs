using AutoMapper;
using Engineering_Hub.DTO.CoachingDTOs;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Engineering_Hub.Hubs;

namespace Engineering_Hub.Services
{
    public class CoachingService : ICoachingService
    {
        private readonly UnitWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<CoachingHub> _hubContext;
        public CoachingService(
       UnitWork unitWork,
       IMapper mapper,
       IHubContext<CoachingHub> hubContext)
        {
            _unitOfWork = unitWork;
            _mapper = mapper;
            _hubContext = hubContext;
        }
        public async Task<(bool Success, string Message, int? ConversationId)>
            GetOrCreateConversationAsync(
                string studentId,
                int trackId)
        {
            // 1. Check Track
            var track = _unitOfWork.Trackrepo.GetById(trackId);

            if (track == null)
            {
                return (false, "Track not found.", null);
            }

            // 2. Check student's active enrollment
            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == studentId &&
                    e.TrackId == trackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            if (enrollment == null)
            {
                return (
                    false,
                    "You are not enrolled in this Track.",
                    null
                );
            }

            // 3. Check Coaching access
            if (enrollment.BookingType != TrackBookingType.TrackWithCoaching)
            {
                return (
                    false,
                    "You do not have Coaching access for this Track.",
                    null
                );
            }

            // 4. Find existing conversation
            var conversation = _unitOfWork.CoachingConversationrepo
                .GetByCondition(c =>
                    c.StudentId == studentId &&
                    c.TrackId == trackId)
                .FirstOrDefault();

            // 5. Create conversation if it doesn't exist
            if (conversation == null)
            {
                conversation = new CoachingConversation
                {
                    StudentId = studentId,
                    TrackId = trackId,
                    CreatedAt = DateTime.UtcNow
                };

                _unitOfWork.CoachingConversationrepo.add(conversation);

                await _unitOfWork.SaveAsync();
            }

            return (
                true,
                "Coaching conversation ready.",
                conversation.Id
            );
        }

        public async Task<(bool Success, string Message, CoachingMessageResponseDto? Data)>
            SendMessageAsync(
                string userId,
                SendCoachingMessageDto dto)
        {
            // 1. Find conversation
            var conversation = _unitOfWork.CoachingConversationrepo
                .GetByCondition(c => c.Id == dto.ConversationId)
                .FirstOrDefault();

            if (conversation == null)
            {
                return (
                    false,
                    "Conversation not found.",
                    null
                );
            }

            // 2. Check whether user can access this conversation
            var hasAccess = await UserHasAccessAsync(
                userId,
                conversation);

            if (!hasAccess)
            {
                return (
                    false,
                    "You do not have access to this conversation.",
                    null
                );
            }

            // 3. Validate message
            if (string.IsNullOrWhiteSpace(dto.Message))
            {
                return (
                    false,
                    "Message cannot be empty.",
                    null
                );
            }

            // 4. Create message
            var message = new CoachingMessage
            {
                ConversationId = dto.ConversationId,
                SenderId = userId,
                Message = dto.Message.Trim(),
                SentAt = DateTime.UtcNow,
                IsRead = false
            };

            _unitOfWork.CoachingMessagerepo.add(message);

            await _unitOfWork.SaveAsync();

            // 5. Return response
            var response = _mapper.Map<CoachingMessageResponseDto>(message);

            await _hubContext.Clients
                .Group($"conversation-{dto.ConversationId}")
                .SendAsync("ReceiveMessage", response);

            return (
                true,
                "Message sent successfully.",
                response
            );
        }

        public async Task<(bool Success, string Message, List<CoachingMessageResponseDto>? Messages)>
            GetMessagesAsync(
                string userId,
                int conversationId)
        {
            // 1. Find conversation
            var conversation = _unitOfWork.CoachingConversationrepo
                .GetByCondition(c => c.Id == conversationId)
                .FirstOrDefault();

            if (conversation == null)
            {
                return (
                    false,
                    "Conversation not found.",
                    null
                );
            }

            // 2. Check access
            var hasAccess = await UserHasAccessAsync(
                userId,
                conversation);

            if (!hasAccess)
            {
                return (
                    false,
                    "You do not have access to this conversation.",
                    null
                );
            }

            // 3. Get messages
            var messages = _unitOfWork.CoachingMessagerepo
                .GetByCondition(m =>
                    m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
              .Select(m => _mapper.Map<CoachingMessageResponseDto>(m))
                .ToList();

            return (
                true,
                "Messages retrieved successfully.",
                messages
            );
        }

        private async Task<bool> UserHasAccessAsync(
            string userId,
            CoachingConversation conversation)
        {
            // Student access
            if (conversation.StudentId == userId)
            {
                var enrollment = _unitOfWork.TrackEnrollmentrepo
                    .GetByCondition(e =>
                        e.StudentId == userId &&
                        e.TrackId == conversation.TrackId &&
                        e.Status == SessionStatus.Active)
                    .FirstOrDefault();

                return enrollment != null &&
                       enrollment.BookingType ==
                       TrackBookingType.TrackWithCoaching;
            }

            // Instructor access
            var instructor = _unitOfWork.TrackInstructorrepo
              .GetByCondition(ti =>
                  ti.TrackId == conversation.TrackId &&
                  ti.UserId == userId)
              .FirstOrDefault();
               

            return instructor != null;
        }
    }
}