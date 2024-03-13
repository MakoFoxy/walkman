using AutoMapper;
using Player.Domain;

namespace Player.BusinessLogic.Features.Roles
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Role, List.RoleModel>();
        }
    }
}