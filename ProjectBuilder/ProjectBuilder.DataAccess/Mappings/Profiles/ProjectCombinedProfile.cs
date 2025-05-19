using AutoMapper;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class ProjectCombinedProfile : Profile
    {
        public ProjectCombinedProfile()
        {
            CreateMap<CombinedProjectSummaryEntity, CombinedProjectModel>().
                ForMember(m => m.SelectedYear, opt => opt.MapFrom(e => e.EntityId));
        }
    }
}
