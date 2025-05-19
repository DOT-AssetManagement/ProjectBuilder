using AutoMapper;
using ProjectBuilder.Core;

namespace ProjectBuilder.DataAccess
{
    public class UserRoleProfile : Profile
    {
        public UserRoleProfile()
        {
            CreateMap<UserRoleEntity, UserRoleModel>().ReverseMap();
        }
    }
}
