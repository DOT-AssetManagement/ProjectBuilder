using AutoMapper;
using ProjectBuilder.Core;

namespace ProjectBuilder.DataAccess
{
    public class DefaultSlackProfile : Profile
    {
        public DefaultSlackProfile()
        {
            CreateMap<DefaultSlack,DefaultSlackModel>()
                .ForMember(m => m.AssetType,opt => opt.MapFrom(e => e.EntityId));
        }
    }
}
