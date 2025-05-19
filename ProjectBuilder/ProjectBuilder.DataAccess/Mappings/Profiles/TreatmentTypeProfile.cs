using AutoMapper;
using ProjectBuilder.Core;

namespace ProjectBuilder.DataAccess
{
    public class TreatmentTypeProfile : Profile
    {
        public TreatmentTypeProfile()
        {
            CreateMap<TreatmentType, TreatmentTypeModel>()
            .ForMember(m => m.TreatmentName, opt => opt.MapFrom(e => e.EntityId)).ReverseMap();
        }
    }

}
