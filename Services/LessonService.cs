using AutoMapper;
using Engineering_Hub.DTO;
using Engineering_Hub.DTO.Lesson;
using Engineering_Hub.DTO.LessonDTOs;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class LessonService : ILessonService
    {
        private readonly UnitWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IInstructorAuthorizationService _instructorAuthorization;

        public LessonService(
      UnitWork unitWork,
      IMapper mapper,
      IInstructorAuthorizationService instructorAuthorization)
        {
            _unitOfWork = unitWork;
            _mapper = mapper;
            _instructorAuthorization = instructorAuthorization;
        }

        public async Task<LessonResponseDTO?> CreateLessonAsync(
    CreateLessonDto dto,
    string instructorId)
        {
            // 1. Check Track
            if (!_instructorAuthorization.CanManageTrack(
                dto.TrackId,
                instructorId))
                    {
                        return null;
                    }

            // 3. Map DTO → Entity
            var lesson = _mapper.Map<Lesson>(dto);

            var lastLesson = _unitOfWork.Lessonrepo
                .GetByCondition(x => x.TrackId == dto.TrackId)
                .OrderByDescending(x => x.Order)
                .FirstOrDefault();

            lesson.Order = lastLesson == null
                ? 1
                : lastLesson.Order + 1;

            lesson.CreatedAt = DateTime.UtcNow;

            // 4. Add
            _unitOfWork.Lessonrepo.add(lesson);

            // 5. Save
            await _unitOfWork.SaveAsync();

            // 6. Map Entity → Response DTO
            return _mapper.Map<LessonResponseDTO>(lesson);
        }

        public async Task<List<LessonResponseDTO>> GetLessonsByTrackAsync(
            int trackId)
        {
            var lessons = _unitOfWork.Lessonrepo
                .GetByCondition(x =>
                    x.TrackId == trackId)
                .OrderBy(x => x.Order)
                .ToList();

            return _mapper.Map<List<LessonResponseDTO>>(lessons);
        }

        public async Task<LessonResponseDTO?> GetLessonByIdAsync(
            int id)
        {
            var lesson = _unitOfWork.Lessonrepo
                .GetById(id);

            if (lesson == null)
            {
                return null;
            }

            return _mapper.Map<LessonResponseDTO>(lesson);
        }

        public async Task<(bool Success, string Message)> UpdateLessonAsync(
            int id,
            LessonDTO dto,
            string instructorId)
        {
            // 1. Find lesson
            var lesson = _unitOfWork.Lessonrepo
                .GetById(id);

            if (lesson == null)
            {
                return (false, "Lesson not found.");
            }

            // 2. Check Track
            if (!_instructorAuthorization.CanManageTrack(
         lesson.TrackId,
         instructorId))
            {
                return (false, "You are not assigned to this Track.");
            }

            // 4. Map updated values
            _mapper.Map(dto, lesson);

            lesson.UpdatedAt = DateTime.UtcNow;

            // 5. Save
            _unitOfWork.Lessonrepo.Edit(lesson);

            await _unitOfWork.SaveAsync();

            return (true, "Lesson updated successfully.");
        }

        public async Task<(bool Success, string Message)> DeleteLessonAsync(
            int id,
            string instructorId)
        {
            // 1. Find lesson
            var lesson = _unitOfWork.Lessonrepo
                .GetById(id);

            if (lesson == null)
            {
                return (false, "Lesson not found.");
            }

            // 2. Check Track
            if (!_instructorAuthorization.CanManageTrack(
               lesson.TrackId,
               instructorId))
                    {
                        return (false, "You are not assigned to this Track.");
                    }

            // 4. Delete
            _unitOfWork.Lessonrepo.Delete(id);

            // 5. Save
            await _unitOfWork.SaveAsync();

            return (true, "Lesson deleted successfully.");
        }
    }
}