using AutoMapper;
using ProjectBuilder.Core.Models;
using ProjectBuilder.DataAccess.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess.Mappings.Profiles
{
    public class ParameterProfile : Profile
    {
        public ParameterProfile()
        {
            CreateMap<ParameterEntity, ParameterModel>()
        .ForMember(m => m.ParameterId, p => p.MapFrom(e => e.EntityId));

            CreateMap<ParameterModel, ParameterEntity>()
       .ForMember(m => m.EntityId, p => p.MapFrom(e => e.ParameterId));
        }
    }
}
