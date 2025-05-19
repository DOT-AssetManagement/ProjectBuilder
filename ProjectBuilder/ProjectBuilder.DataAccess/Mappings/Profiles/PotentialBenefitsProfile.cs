using AutoMapper;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class PotentialBenefitsProfile : Profile
    {
        public PotentialBenefitsProfile()
        {
            CreateMap<AllPotentialBenefitEntity, AllPotentialBenefitsModel>().
                ForMember(m => m.TreatmentYear, opt => opt.MapFrom(e => e.EntityId)).
                ForMember(m => m.InterstateBenefit, opt => opt.MapFrom(e => e.InterBenefit)).
                ForMember(m => m.NonInterstateBenefit, opt => opt.MapFrom(e => e.NonInterBenefit));
            CreateMap<BridgePotentialBenefitEntity, BridgePotentialBenefitsModel>().
                ForMember(m => m.TreatmentYear, opt => opt.MapFrom(e => e.EntityId)).
                ForMember(m => m.InterstateBenefit, opt => opt.MapFrom(e => e.InterBenefit)).
                ForMember(m => m.NonInterstateBenefit, opt => opt.MapFrom(e => e.NonInterBenefit));
            CreateMap<PavementPotentialBenefitEntity, PavementPotentialBenefitsModel>().
                ForMember(m => m.TreatmentYear, opt => opt.MapFrom(e => e.EntityId)).
                ForMember(m => m.InterstateBenefit, opt => opt.MapFrom(e => e.InterBenefit)).
                ForMember(m => m.NonInterstateBenefit, opt => opt.MapFrom(e => e.NonInterBenefit));
        } 
    }
    
}
