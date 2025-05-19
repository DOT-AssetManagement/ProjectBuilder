using AutoMapper;
using ProjectBuilder.Core;
using System.Diagnostics.CodeAnalysis;

namespace ProjectBuilder.DataAccess
{
    public class ProjectProfile : Profile
    {
        public ProjectProfile()
        {
            CreateMap<Project, ProjectModel>()
            .ForMember(m => m.ProjectId, opt => opt.MapFrom(e => e.EntityId));
            // .ForMember(m => m.County, opt => opt.MapFrom(e => e.County.CountyFullName))            
            // .ForMember(m => m.CountyId,opt => opt.MapFrom(e => e.County.EntityId))
            //  .ForMember(m => m.Section, opt => opt.MapFrom(e => e.SectionChain));
            CreateMap<ProjectModel, Project>()
             .ForMember(e => e.EntityId, opt => opt.MapFrom(m => m.ProjectId));
            // .ForMember(e => e.County, opt => opt.MapFrom(m => new CountyEntity(m.County)));
        }
    }
}
