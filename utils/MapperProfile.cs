using AutoMapper;
using EventManagementApi.DTO;
using EventManagementApi.Entity;

namespace EventManagementApi.utils
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<EventCreateDto, Event>();
            CreateMap<EventUpdateDto, Event>().ForAllMembers(opt => opt.Condition((src, dest, member) => member != null));
        }
    }
}