using AutoMapper;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class BudgetProfile : Profile
    {
        public BudgetProfile()
        {
            CreateMap<BudgetEntity, BudgetModel>().
                ForMember(m => m.ScenarioYear, opt => opt.MapFrom(e => e.EntityId));
        }
    }
    public class BudgetSpentProfile : Profile
    {
        public BudgetSpentProfile()
        {
            CreateMap<BudgetSpentEntity,BudgetSpentModel>().
                ForMember(m => m.ScenarioYear, opt => opt.MapFrom(e => e.EntityId));
        }
    }
}
