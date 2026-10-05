
using Engineering_Hub.DTO.LessonContent;
using Engineering_Hub.models;
using Engineering_Hub.UnitOfWork;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public class LessonContentService : ILessonContentService
    {
        private readonly UnitWork _unitOfWork;
        private readonly IInstructorAuthorizationService _instructorAuthorization;
        private readonly IFileValidationService _fileValidation;
        private readonly IFileStorageService _fileStorage;

        public LessonContentService(
            UnitWork unitOfWork,
            IInstructorAuthorizationService instructorAuthorization,
            IFileValidationService fileValidation,
            IFileStorageService fileStorage)
        {
            _unitOfWork = unitOfWork;
            _instructorAuthorization = instructorAuthorization;
            _fileValidation = fileValidation;
            _fileStorage = fileStorage;
        }

        public async Task AddFileContentAsync(
            int lessonId,
            int lessonTypeId,
            IFormFile file,
            bool isFree,
            string instructorId)
        {
            // 1. Find the lesson
            var lesson = _unitOfWork.Lessonrepo
                .GetById(lessonId);

            if (lesson == null)
            {
                throw new ArgumentException("Lesson not found.");
            }

            // 2. Check that the instructor manages this Track
            if (!_instructorAuthorization.CanManageTrack(
                lesson.TrackId,
                instructorId))
            {
                throw new UnauthorizedAccessException(
                    "You are not assigned to this Track.");
            }

            // 3. Find the LessonType
            var lessonType = _unitOfWork.LessonTyperepo
                .GetById(lessonTypeId);

            if (lessonType == null)
            {
                throw new ArgumentException(
                    "Lesson type not found.");
            }

            // 4. Only file-based content is allowed here
            if (lessonType.Name != "Video" &&
                lessonType.Name != "PDF" &&
                lessonType.Name != "PowerPoint")
            {
                throw new ArgumentException(
                    "This lesson type does not support file uploads.");
            }

            // 5. Validate the uploaded file
            _fileValidation.ValidateFile(
                file,
                lessonType.Name);

            // 6. Decide which folder to use
            string folderName = lessonType.Name switch
            {
                "Video" => "videos",
                "PDF" => "pdfs",
                "PowerPoint" => "presentations",
                _ => throw new ArgumentException(
                    "Unsupported lesson type.")
            };

            // 7. Save the physical file
            string storedFileName =
    await _fileStorage.SaveFileAsync(
        file,
        folderName);

            try
            {
                // Create the database record
                var content = new LessonContent
                {
                    LessonId = lessonId,
                    LessonTypeId = lessonTypeId,

                    OriginalFileName = file.FileName,
                    StoredFileName = storedFileName,

                    ContentType = file.ContentType,
                    FileSize = file.Length,

                    ContentUrl =
                        $"/uploads/{folderName}/{storedFileName}",

                    IsFree = isFree
                };

                _unitOfWork.LessonContentrepo.add(content);

                // Save database changes
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                // Database save failed,
                // so remove the physical file we just created.
                await _fileStorage.DeleteFileAsync(
                    storedFileName,
                    folderName);

                throw;
            }
        }
  
        public async Task AddWorkshopContentAsync(
        int lessonId,
        int lessonTypeId,
        int workshopId,
        string instructorId)
        {
            // 1. Find the lesson
            var lesson = _unitOfWork.Lessonrepo
                .GetById(lessonId);

            if (lesson == null)
            {
                throw new ArgumentException("Lesson not found.");
            }

            // 2. Check that the instructor manages this Track
            if (!_instructorAuthorization.CanManageTrack(
                lesson.TrackId,
                instructorId))
            {
                throw new UnauthorizedAccessException(
                    "You are not assigned to this Track.");
            }

            // 3. Find the LessonType
            var lessonType = _unitOfWork.LessonTyperepo
                .GetById(lessonTypeId);

            if (lessonType == null)
            {
                throw new ArgumentException(
                    "Lesson type not found.");
            }

            // 4. Make sure this is actually a Workshop type
            if (lessonType.Name != "Workshop")
            {
                throw new ArgumentException(
                    "The selected lesson type is not Workshop.");
            }

            // 5. Find the Workshop
            var workshop = _unitOfWork.Workshoprepo
                .GetById(workshopId);

            if (workshop == null)
            {
                throw new ArgumentException(
                    "Workshop not found.");
            }

            // 6. Workshop and Lesson must belong to the same Track
            if (workshop.TrackId != lesson.TrackId)
            {
                throw new ArgumentException(
                    "The Workshop does not belong to the same Track as the Lesson.");
            }

            // 7. Create the LessonContent record
            var content = new LessonContent
            {
                LessonId = lessonId,
                LessonTypeId = lessonTypeId,
                WorkshopId = workshopId,

                ContentUrl = null,
                OriginalFileName = null,
                StoredFileName = null,
                ContentType = null,
                FileSize = null,

                // Workshop does not use Free/Paid
                IsFree = null
            };

            _unitOfWork.LessonContentrepo.add(content);

            // 8. Save database changes
            await _unitOfWork.SaveAsync();
        }


       
        public async Task AddInteractiveContentAsync(
        int lessonId,
        int lessonTypeId,
        int interactiveActivityId,
        string instructorId)
        {
            // 1. Find the lesson
            var lesson = _unitOfWork.Lessonrepo
                .GetById(lessonId);

            if (lesson == null)
            {
                throw new ArgumentException("Lesson not found.");
            }

            // 2. Check that the instructor manages this Track
            if (!_instructorAuthorization.CanManageTrack(
                lesson.TrackId,
                instructorId))
            {
                throw new UnauthorizedAccessException(
                    "You are not assigned to this Track.");
            }

            // 3. Find the LessonType
            var lessonType = _unitOfWork.LessonTyperepo
                .GetById(lessonTypeId);

            if (lessonType == null)
            {
                throw new ArgumentException(
                    "Lesson type not found.");
            }

            // 4. Make sure this is actually an Interactive type
            if (lessonType.Name != "Interactive")
            {
                throw new ArgumentException(
                    "The selected lesson type is not Interactive.");
            }

            // 5. Find the Interactive Activity
            var interactiveActivity = _unitOfWork.InteractiveActivityrepo
                .GetById(interactiveActivityId);

            if (interactiveActivity == null)
            {
                throw new ArgumentException(
                    "Interactive activity not found.");
            }

            // 6. Interactive Activity and Lesson
            // must belong to the same Track
            if (interactiveActivity.TrackId != lesson.TrackId)
            {
                throw new ArgumentException(
                    "The Interactive Activity does not belong to the same Track as the Lesson.");
            }

            // 7. Create the LessonContent record
            var content = new LessonContent
            {
                LessonId = lessonId,
                LessonTypeId = lessonTypeId,
                InteractiveActivityId = interactiveActivityId,

                // Interactive does not use Free/Paid
                IsFree = null
            };

            _unitOfWork.LessonContentrepo.add(content);

            // 8. Save database changes
            await _unitOfWork.SaveAsync();
        }



        public async Task<List<LessonContentResponseDTO>> GetLessonContentsAsync(
        int lessonId,
        string userId)
        {
            // 1. Find the lesson
            var lesson = _unitOfWork.Lessonrepo
                .GetById(lessonId);

            if (lesson == null)
            {
                throw new ArgumentException("Lesson not found.");
            }

            // 2. Check if this user is an instructor
            //    assigned to this Track
            bool isInstructor =
                _instructorAuthorization.CanManageTrack(
                    lesson.TrackId,
                    userId);

            // 3. Get all contents belonging to this lesson
            var contents = _unitOfWork.LessonContentrepo
                .GetByConditionWithInclude(
                    c => c.LessonId == lessonId,
                    c => c.LessonType)
                .ToList();

            // 4. If the user is the instructor responsible
            //    for this Track, they can see all contents
            if (isInstructor)
            {
                return contents.Select(c => new LessonContentResponseDTO
                {
                    Id = c.Id,
                    LessonId = c.LessonId,
                    LessonTypeId = c.LessonTypeId,
                    LessonTypeName = c.LessonType.Name,
                    ContentUrl = c.ContentUrl,
                    OriginalFileName = c.OriginalFileName,
                    ContentType = c.ContentType,
                    FileSize = c.FileSize,
                    IsFree = c.IsFree,
                    WorkshopId = c.WorkshopId,
                    InteractiveActivityId = c.InteractiveActivityId
                }).ToList();
            }

            // 5. Check if the student has an active enrollment
            var enrollment = _unitOfWork.TrackEnrollmentrepo
                .GetByCondition(e =>
                    e.StudentId == userId &&
                    e.TrackId == lesson.TrackId &&
                    e.Status == SessionStatus.Active)
                .FirstOrDefault();

            bool hasLessonAccess = enrollment != null;

            // 6. If enrolled and this is not the first lesson,
            //    check the previous lesson
            if (hasLessonAccess && lesson.Order > 1)
            {
                var previousLesson = _unitOfWork.Lessonrepo
                    .GetByCondition(l =>
                        l.TrackId == lesson.TrackId &&
                        l.Order == lesson.Order - 1)
                    .FirstOrDefault();

                if (previousLesson == null)
                {
                    hasLessonAccess = false;
                }
                else
                {
                    var previousProgress = _unitOfWork.LessonProgressrepo
                        .GetByCondition(p =>
                            p.StudentId == userId &&
                            p.LessonId == previousLesson.Id &&
                            p.IsCompleted)
                        .FirstOrDefault();

                    if (previousProgress == null)
                    {
                        hasLessonAccess = false;
                    }
                }
            }

            // 7. If the student has full lesson access,
            //    return all lesson contents
            if (hasLessonAccess)
            {
                return contents.Select(c => new LessonContentResponseDTO
                {
                    Id = c.Id,
                    LessonId = c.LessonId,
                    LessonTypeId = c.LessonTypeId,
                    LessonTypeName = c.LessonType.Name,
                    ContentUrl = c.ContentUrl,
                    OriginalFileName = c.OriginalFileName,
                    ContentType = c.ContentType,
                    FileSize = c.FileSize,
                    IsFree = c.IsFree,
                    WorkshopId = c.WorkshopId,
                    InteractiveActivityId = c.InteractiveActivityId
                }).ToList();
            }

            // 8. User does not have full lesson access.
            //    Return only free file content.
            return contents
                .Where(c => c.IsFree == true)
                .Select(c => new LessonContentResponseDTO
                {
                    Id = c.Id,
                    LessonId = c.LessonId,
                    LessonTypeId = c.LessonTypeId,
                    LessonTypeName = c.LessonType.Name,
                    ContentUrl = c.ContentUrl,
                    OriginalFileName = c.OriginalFileName,
                    ContentType = c.ContentType,
                    FileSize = c.FileSize,
                    IsFree = c.IsFree,
                    WorkshopId = c.WorkshopId,
                    InteractiveActivityId = c.InteractiveActivityId
                })
                .ToList();
        }



        public async Task UpdateFileContentAsync(
         int contentId,
         IFormFile? file,
         bool? isFree,
         string instructorId)
        {
            // 1. Find the content with its Lesson and LessonType
            var content = _unitOfWork.LessonContentrepo
                .GetByConditionWithInclude(
                    c => c.Id == contentId,
                    c => c.Lesson,
                    c => c.LessonType)
                .FirstOrDefault();

            if (content == null)
            {
                throw new ArgumentException(
                    "Lesson content not found.");
            }

            // 2. Make sure this is file-based content
            if (content.LessonType.Name != "Video" &&
                content.LessonType.Name != "PDF" &&
                content.LessonType.Name != "PowerPoint")
            {
                throw new ArgumentException(
                    "Only file content can be updated using this endpoint.");
            }

            // 3. Check that the instructor manages this Track
            if (!_instructorAuthorization.CanManageTrack(
                content.Lesson.TrackId,
                instructorId))
            {
                throw new UnauthorizedAccessException(
                    "You are not assigned to this Track.");
            }

            // 4. Make sure there is actually something to update
            if (file == null && isFree == null)
            {
                throw new ArgumentException(
                    "There is nothing to update.");
            }

            string? newStoredFileName = null;
            string? folderName = null;

            // 5. If a new file was provided
            if (file != null)
            {
                // Validate the new file
                _fileValidation.ValidateFile(
                    file,
                    content.LessonType.Name);

                // Decide where to store it
                folderName = content.LessonType.Name switch
                {
                    "Video" => "videos",
                    "PDF" => "pdfs",
                    "PowerPoint" => "presentations",
                    _ => throw new ArgumentException(
                        "Unsupported lesson type.")
                };

                // Save the NEW file first
                newStoredFileName =
                    await _fileStorage.SaveFileAsync(
                        file,
                        folderName);
            }

            // Keep the old file name
            string? oldStoredFileName = content.StoredFileName;

            try
            {
                // 6. Update file information if a new file was provided
                if (file != null)
                {
                    content.OriginalFileName = file.FileName;
                    content.StoredFileName = newStoredFileName;
                    content.ContentType = file.ContentType;
                    content.FileSize = file.Length;
                    content.ContentUrl =
                        $"/uploads/{folderName}/{newStoredFileName}";
                }

                // 7. Update IsFree if the user provided a value
                if (isFree.HasValue)
                {
                    content.IsFree = isFree.Value;
                }

                // 8. Update database
                _unitOfWork.LessonContentrepo.Edit(content);

                await _unitOfWork.SaveAsync();
            }
            catch
            {
                // 9. Database update failed
                // Delete the NEW file because it is no longer needed
                if (newStoredFileName != null && folderName != null)
                {
                    await _fileStorage.DeleteFileAsync(
                        newStoredFileName,
                        folderName);
                }

                // Keep old file + old database data
                throw;
            }

            // 10. Database update succeeded
            // Now it is safe to delete the OLD file
            if (file != null &&
                oldStoredFileName != null &&
                folderName != null)
            {
                await _fileStorage.DeleteFileAsync(
                    oldStoredFileName,
                    folderName);
            }
        }

        public async Task DeleteContentAsync(
         int contentId,
         string instructorId)
        {
            // 1. Find the content with its Lesson and LessonType
            var content = _unitOfWork.LessonContentrepo
                .GetByConditionWithInclude(
                    c => c.Id == contentId,
                    c => c.Lesson,
                    c => c.LessonType)
                .FirstOrDefault();

            if (content == null)
            {
                throw new ArgumentException(
                    "Lesson content not found.");
            }

            // 2. Check that the instructor manages this Track
            if (!_instructorAuthorization.CanManageTrack(
                content.Lesson.TrackId,
                instructorId))
            {
                throw new UnauthorizedAccessException(
                    "You are not assigned to this Track.");
            }

            // 3. Remember the file information before deleting
            string? storedFileName = content.StoredFileName;
            string? folderName = null;

            if (content.LessonType.Name == "Video")
            {
                folderName = "videos";
            }
            else if (content.LessonType.Name == "PDF")
            {
                folderName = "pdfs";
            }
            else if (content.LessonType.Name == "PowerPoint")
            {
                folderName = "presentations";
            }

            // 4. Delete the database record
            _unitOfWork.LessonContentrepo.Delete(contentId);

            try
            {
                await _unitOfWork.SaveAsync();
            }
            catch
            {
                // Database deletion failed.
                // Do NOT delete the physical file.
                throw;
            }

            // 5. Database deletion succeeded.
            // Now it is safe to delete the physical file.
            if (storedFileName != null && folderName != null)
            {
                await _fileStorage.DeleteFileAsync(
                    storedFileName,
                    folderName);
            }
        }
    }
}

