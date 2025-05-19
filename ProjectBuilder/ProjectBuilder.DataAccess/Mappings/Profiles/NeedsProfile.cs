using AutoMapper;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class NeedsProfile : Profile
    {
        public NeedsProfile()
        {
            CreateMap<AllNeedsEntity, AllNeedsModel>()
                .ForMember(m => m.TreatmentYear,opt => opt.MapFrom(e => e.EntityId));
            CreateMap<BridgeNeedsEntity, BridgeNeedsModel>()
                .ForMember(m => m.TreatmentYear, opt => opt.MapFrom(e => e.EntityId));
            CreateMap<PavementNeedsEntity, PavementNeedsModel>()
                .ForMember(m => m.TreatmentYear, opt => opt.MapFrom(e => e.EntityId));
        }
    }
}
