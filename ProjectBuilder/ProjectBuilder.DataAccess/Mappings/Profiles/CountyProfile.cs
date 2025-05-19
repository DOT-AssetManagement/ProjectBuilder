using AutoMapper;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;

namespace ProjectBuilder.DataAccess
{
    public sealed class CountyProfile : Profile
    {
        public CountyProfile()
        {
            CreateMap<CountyEntity, CountyModel>()
                .ForMember(m => m.CountyId, opt => opt.MapFrom(e => e.EntityId)).ReverseMap();
        }
    }
}
