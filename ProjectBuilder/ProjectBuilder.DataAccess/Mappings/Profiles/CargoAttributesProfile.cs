using AutoMapper;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public sealed class CargoAttributesProfile : Profile
    {
        public CargoAttributesProfile()
        {
            CreateMap<CargoAttributes, CargoAttributesModel>()
                .ForMember(m => m.AttributeNo, opt => opt.MapFrom(e => e.EntityId)).ReverseMap();
        }
    }
}
