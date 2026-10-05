using Engineering_Hub.DTO.LessonContent;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace Engineering_Hub.Services
{
    public interface ILessonContentService
    {
        Task AddFileContentAsync(
            int lessonId,
            int lessonTypeId,
            IFormFile file,
            bool isFree,
            string instructorId);

        Task AddWorkshopContentAsync(
            int lessonId,
            int lessonTypeId,
            int workshopId,
            string instructorId);

        Task AddInteractiveContentAsync(
            int lessonId,
            int lessonTypeId,
            int interactiveActivityId,
            string instructorId);

        Task<List<LessonContentResponseDTO>> GetLessonContentsAsync(
            int lessonId,
            string userId);

        Task UpdateFileContentAsync(
            int contentId,
            IFormFile? file,
            bool? isFree,
            string instructorId);

        Task DeleteContentAsync(
            int contentId,
            string instructorId);
    }
}