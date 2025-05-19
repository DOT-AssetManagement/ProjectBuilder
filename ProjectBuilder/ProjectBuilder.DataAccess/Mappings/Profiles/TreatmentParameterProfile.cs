using AutoMapper;
using ProjectBuilder.Core.Models;
using ProjectBuilder.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess.Mappings.Profiles
{
    public class TreatmentParameterProfile : Profile
    {
        public TreatmentParameterProfile()
        {
            CreateMap<TreatmentParameterEntity, TreatmentParameterModel>()
        .ForMember(m => m.TreatmentParameterId, p => p.MapFrom(e => e.EntityId));
            CreateMap<TreatmentParameterModel, TreatmentParameterEntity>()
        .ForMember(m => m.EntityId, p => p.MapFrom(e => e.TreatmentParameterId));
        }
    }
}
