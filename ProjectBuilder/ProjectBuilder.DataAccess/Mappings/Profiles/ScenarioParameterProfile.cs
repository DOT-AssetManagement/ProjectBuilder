using AutoMapper;
using ProjectBuilder.Core;

namespace ProjectBuilder.DataAccess
{
    public class ScenarioParameterProfile : Profile
    {
        public ScenarioParameterProfile()
        {
            CreateMap<ScenarioParamater, ScenarioParameterModel>().
                ForMember(m => m.ParameterName, opt => opt.MapFrom(e => e.ParmName)).
                ForMember(m => m.ParameterId, opt => opt.MapFrom(e => e.EntityId)).
                ForMember(m => m.ParameterValue, opt => opt.MapFrom(e => e.ParameterValue)).
                ForMember(m => m.ParameterDescription, opt => opt.MapFrom(e => e.ParmDescription)).ReverseMap();
        }
    }
}
