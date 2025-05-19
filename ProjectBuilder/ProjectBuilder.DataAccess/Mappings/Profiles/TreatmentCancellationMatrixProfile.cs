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
    public class TreatmentCancellationMatrixProfile : Profile
    {
        public TreatmentCancellationMatrixProfile()
        {
            CreateMap<TreatmentCancellationMatrixEntity, TreatmentCancellationMatrixModel>()
        .ForMember(m => m.TreatmentA, p => p.MapFrom(e => e.EntityId));

            CreateMap<TreatmentCancellationMatrixModel, TreatmentCancellationMatrixEntity>()
       .ForMember(m => m.EntityId, p => p.MapFrom(e => e.TreatmentA));
        }
    }
}
