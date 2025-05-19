using AutoMapper;
using ProjectBuilder.Core;

namespace ProjectBuilder.DataAccess
{
    public class ScenarioProfile : Profile
    {
        public ScenarioProfile()
        {
            CreateMap<Scenario, ScenarioModel>()
                .ForMember(m => m.ScenarioId,opt => opt.MapFrom(e => e.EntityId)).ReverseMap();
        }
    }
}
