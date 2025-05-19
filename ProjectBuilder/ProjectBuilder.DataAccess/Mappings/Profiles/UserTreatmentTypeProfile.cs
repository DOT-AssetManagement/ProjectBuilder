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
    public class UserTreatmentTypeProfile : Profile
    {
        public UserTreatmentTypeProfile()
        {
            CreateMap<UserTreatmentType, UserTreatmentTypeModel>()
       .ForMember(m => m.UserTreatmentsId, p => p.MapFrom(e => e.EntityId));
            CreateMap<UserTreatmentTypeModel, UserTreatmentType>()
        .ForMember(m => m.EntityId, p => p.MapFrom(e => e.UserTreatmentsId));
        }
    }
}
