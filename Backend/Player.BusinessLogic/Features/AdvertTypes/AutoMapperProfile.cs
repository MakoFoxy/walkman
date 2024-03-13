using AutoMapper;
using Player.Domain;

namespace Player.BusinessLogic.Features.AdvertTypes
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<AdvertType, List.AdvertTypeModel>().ReverseMap();
        }
    }
}