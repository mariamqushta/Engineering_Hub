using AutoMapper;
using Engineering_Hub.DTO;
using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.CoachingDTOs;
using Engineering_Hub.DTO.InteractiveActivity;
using Engineering_Hub.DTO.Lesson;
using Engineering_Hub.DTO.LessonDTOs;
using Engineering_Hub.DTO.LoginDTOs;
using Engineering_Hub.DTO.Workshop;
using Engineering_Hub.models;

namespace Engineering_Hub.AutoMapper
{
    public class MapingProfile : Profile
    {
        public MapingProfile()
        {
            CreateMap<Registerdto, ApplicationUser>();
            CreateMap<Lesson, LessonResponseDTO>();
            CreateMap<LessonDTO, Lesson>();
            CreateMap<CreateLessonDto, Lesson>();
            CreateMap<AssignmentCreateDTO, Assignment>();
            CreateMap<Assignment, AssignmentResponseDTO>();
            CreateMap<AssignmentSubmission, AssignmentSubmissionResponseDTO>();
            CreateMap<TrackBookingDto, TrackEnrollment>();
            CreateMap<TrackDTO, Track>();
            CreateMap<Track, TrackResponseDTO>();
            CreateMap<WorkshopDTO, Workshop>();
            CreateMap<Workshop, WorkshopResponseDTO>();
            CreateMap<WorkshopBooking, WorkshopBookingResponseDTO>();
            CreateMap<InteractiveActivityDTO, InteractiveActivity>();
            CreateMap<InteractiveActivity, InteractiveActivityResponseDTO>();
            CreateMap<InteractiveBooking, InteractiveBookingResponseDTO>();
            CreateMap<CoachingMessage, CoachingMessageResponseDto>();
            CreateMap<Certificate, CertificateResponseDTO>();

        }
    }
}
