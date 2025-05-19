using AutoMapper;
using ProjectBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    internal class LibraryProfile : Profile
    {
        public LibraryProfile()
        {
            CreateMap<CandidatePool, CandidatePoolModel>()
                .ForMember(m => m.CandidatePoolId,p => p.MapFrom(e => e.EntityId)).ReverseMap();
        }
    }
}
