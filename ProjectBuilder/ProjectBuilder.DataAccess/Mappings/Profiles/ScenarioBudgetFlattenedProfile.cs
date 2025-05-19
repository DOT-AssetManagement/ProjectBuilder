using AutoMapper;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class ScenarioBudgetFlattenedProfile : Profile 
    {
        public ScenarioBudgetFlattenedProfile()
        {
            CreateMap<ScenarioBudgetFlat, ScenarioBudgetFlatModel>()
                .ForMember(m => m.YearWork, opt => opt.MapFrom(e => e.EntityId));
        }
    }
}
