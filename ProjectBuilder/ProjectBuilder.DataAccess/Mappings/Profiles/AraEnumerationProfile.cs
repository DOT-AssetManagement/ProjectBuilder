using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using ProjectBuilder.Core;
using ProjectBuilder.Core.Models;
using ProjectBuilder.DataAccess.Entities;

namespace ProjectBuilder.DataAccess.Mappings.Profiles
{
    public class ARAEnumerationProfile:Profile
    {
        public ARAEnumerationProfile()
        {
            CreateMap<AraEnumerations, AraEnumerationsModel>().
                ForMember(m=>m.EnumFamily, opt=>opt.MapFrom(e=>e.EntityId));
               
        }
    }
}
