using AutoMapper;
using ProjectBuilder.Core;
using ProjectBuilder.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectBuilder.DataAccess
{
    public class PAMSProfile : Profile
    {
        public PAMSProfile()
        {
            CreateMap<PAMSSectionSegment, PAMSSectionSegmentModel>().ReverseMap();
        }
    }
}
