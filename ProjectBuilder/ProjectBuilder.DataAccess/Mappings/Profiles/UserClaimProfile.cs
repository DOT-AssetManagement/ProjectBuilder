using AutoMapper;
using ProjectBuilder.Core;

namespace ProjectBuilder.DataAccess
{
    public class UserClaimProfile : Profile
    {
        public UserClaimProfile()
        {
            CreateMap<UserClaimEntity, UserClaimModel>().ReverseMap();
        }
    }
}
