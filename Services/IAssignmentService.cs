using Engineering_Hub.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Engineering_Hub.Services
{
    public interface IAssignmentService
    {
        Task<(bool Success, string Message, AssignmentResponseDTO? Data)>
            CreateAssignmentAsync(
                int lessonId,
                AssignmentCreateDTO dto,
                string instructorId);

        Task<List<AssignmentResponseDTO>>
            GetAssignmentsForLessonAsync(
                int lessonId,
                string studentId);
        Task<(bool Success, string Message)>
       SubmitAssignmentAsync(
        int assignmentId,
        AssignmentSubmissionDTO dto,
        string studentId);

        Task<List<AssignmentSubmissionResponseDTO>>
            GetSubmissionsAsync(
                int assignmentId,
                string instructorId);

        Task<(bool Success, string Message)>
            GradeSubmissionAsync(
                int submissionId,
                decimal grade,
                string? feedback,
                string instructorId);
        Task<AssignmentSubmissionResponseDTO?> GetMySubmissionAsync(
    int assignmentId,
    string studentId);
    }
}