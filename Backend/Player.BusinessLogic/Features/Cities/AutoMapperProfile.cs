using AutoMapper;
using Player.Domain;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Cities
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<City, SimpleDto>().ReverseMap();
            CreateMap<City, List.CityModel>().ReverseMap();
        }
    }
}