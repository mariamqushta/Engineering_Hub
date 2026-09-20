using AutoMapper;
using Engineering_Hub.DTO;
using Engineering_Hub.DTO.BookingDTOs;
using Engineering_Hub.DTO.InteractiveActivity;
using Engineering_Hub.DTO.Lesson;
using Engineering_Hub.DTO.LoginDTOs;
using Engineering_Hub.DTO.TrackPackage;
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
            CreateMap<TrackBookingDto, TrackEnrollment>();
            CreateMap<TrackDTO, Track>();
            CreateMap<Track, TrackResponseDTO>();
            CreateMap<WorkshopDTO, Workshop>();
            CreateMap<Workshop, WorkshopResponseDTO>();
            CreateMap<WorkshopBooking, WorkshopBookingResponseDTO>();
            CreateMap<InteractiveActivityDTO, InteractiveActivity>();
            CreateMap<InteractiveActivity, InteractiveActivityResponseDTO>();
            CreateMap<InteractiveBooking, InteractiveBookingResponseDTO>();
            CreateMap<TrackPackageDTO, TrackPackage>();

            CreateMap<TrackPackage, TrackPackageResponseDTO>();
        }
    }
}
