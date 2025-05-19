using AutoMapper;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess.Entities;
using System;

namespace ProjectBuilder.DataAccess
{
    public class TreatmentProfile : Profile
    {
        public TreatmentProfile()
        {
            CreateMap<Treatment, TreatmentModel>()
            .ForMember(t => t.County, opt => opt.MapFrom(tr => tr.County.CountyFullName))
            .ForMember(t => t.TreatmentId, opt => opt.MapFrom(tr => tr.EntityId));
            CreateMap<TreatmentModel, Treatment>()
                .ForMember(e => e.County, opt => opt.MapFrom(m => new CountyEntity(m.County)))
                .ForMember(e => e.EntityId, opt => opt.MapFrom(m => m.TreatmentId));
        }
    }
    public class UserTreatmentProfile : Profile
    {
        public UserTreatmentProfile()
        {
            CreateMap<UserTreatment, UserTreatmentModel>()
                .ForMember(t => t.ImportTimeGeneratedId, opt => opt.MapFrom(tr => tr.EntityId));

            CreateMap<UserTreatmentModel, UserTreatment>()
                .ForMember(t => t.EntityId, opt => opt.MapFrom(tr => tr.ImportTimeGeneratedId));
        }
    }

    public class ProjectTreatmentProfile : Profile
    {
        public ProjectTreatmentProfile()
        {
            CreateMap<ProjectTreatment, ProjectTreatmentModel>()
                .ForMember(t => t.ProjectTreatmentId, opt => opt.MapFrom(tr => tr.EntityId));

            CreateMap<ProjectTreatmentModel, ProjectTreatment>()
                .ForMember(t => t.EntityId, opt => opt.MapFrom(tr => tr.ProjectTreatmentId));
        }
    }
}
