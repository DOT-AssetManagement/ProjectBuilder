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
    public class ImportSessionsProfile:Profile
    {
        public ImportSessionsProfile() 
        {
            CreateMap<ImportSessions, ImportSessionsModel>()
                .ForMember(m => m.Id, opt => opt.MapFrom(e => e.EntityId));
        }
    }
}
