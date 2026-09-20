using AutoMapper;
using Engineering_Hub.DTO;
using Engineering_Hub.DTO.Lesson;
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

        public LessonService(
            UnitWork unitWork,
            IMapper mapper)
        {
            _unitOfWork = unitWork;
            _mapper = mapper;
        }

        public async Task<LessonResponseDTO?> CreateLessonAsync(
            LessonDTO dto,
            string instructorId)
        {
            // 1. Check Track
            var track = _unitOfWork.Trackrepo
                .GetById(dto.TrackId);

            if (track == null || !track.IsActive)
            {
                return null;
            }

            // 2. Check Instructor is assigned to Track
            var instructor = _unitOfWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == dto.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
            {
                return null;
            }

            // 3. Map DTO → Entity
            var lesson = _mapper.Map<Lesson>(dto);

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
            var track = _unitOfWork.Trackrepo
                .GetById(lesson.TrackId);

            if (track == null || !track.IsActive)
            {
                return (false, "Track not found.");
            }

            // 3. Check Instructor
            var instructor = _unitOfWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == lesson.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
            {
                return (
                    false,
                    "You are not assigned to this Track."
                );
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
            var track = _unitOfWork.Trackrepo
                .GetById(lesson.TrackId);

            if (track == null || !track.IsActive)
            {
                return (false, "Track not found.");
            }

            // 3. Check Instructor
            var instructor = _unitOfWork.TrackInstructorrepo
                .GetByCondition(x =>
                    x.TrackId == lesson.TrackId &&
                    x.UserId == instructorId)
                .FirstOrDefault();

            if (instructor == null)
            {
                return (
                    false,
                    "You are not assigned to this Track."
                );
            }

            // 4. Delete
            _unitOfWork.Lessonrepo.Delete(id);

            // 5. Save
            await _unitOfWork.SaveAsync();

            return (true, "Lesson deleted successfully.");
        }
    }
}