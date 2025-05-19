using AutoMapper;
using ProjectBuilder.Core;

namespace ProjectBuilder.DataAccess
{
    public class ProjectSummaryProfile : Profile
    {
        public ProjectSummaryProfile()
        {
            CreateMap<ProjectSummaryEntity,ProjectSummaryModel>().
                ForMember(m => m.ProjectId,opt => opt.MapFrom(e => e.EntityId));
        }
    }
    public class TreatmentSummaryProfile : Profile
    {
        public TreatmentSummaryProfile()
        {
            CreateMap<TreatmentSummaryEntity, TreatmentSummaryModel>().
                ForMember(m => m.TreatmentId, opt => opt.MapFrom(e => e.EntityId));
        }
    }
}
