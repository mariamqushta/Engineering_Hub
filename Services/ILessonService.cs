using Engineering_Hub.DTO;
using Engineering_Hub.DTO.Lesson;
using Engineering_Hub.DTO.LessonDTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface ILessonService
    {
        Task<LessonResponseDTO?> CreateLessonAsync(
            CreateLessonDto dto,
            string instructorId);

        Task<List<LessonResponseDTO>> GetLessonsByTrackAsync(
            int trackId);

        Task<LessonResponseDTO?> GetLessonByIdAsync(
            int id);

        Task<(bool Success, string Message)> UpdateLessonAsync(
            int id,
            LessonDTO dto,
            string instructorId);

        Task<(bool Success, string Message)> DeleteLessonAsync(
            int id,
            string instructorId);
    }
}