using AutoMapper;
using EventManagementApi.DTO;
using EventManagementApi.Entity;

namespace EventManagementApi.utils
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {

            CreateMap<Event, EventReadDto>()
                .ForMember(dest => dest.Organizer, opt => opt.MapFrom(src => src.Organizer));
            CreateMap<ApplicationUser, ApplicationUser>();
            CreateMap<EventCreateDto, Event>();
            CreateMap<EventUpdateDto, Event>().ForAllMembers(opt => opt.Condition((src, dest, member) => member != null));
        }
    }
}