using AutoMapper;
using Player.Domain;
using Player.DTOs;

namespace Player.BusinessLogic.Features.ServiceCompanies
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ServiceCompany, SimpleDto>().ReverseMap();//TODO Зачем этот маппинг
            CreateMap<List.ServiceCompanyModel, ServiceCompany>().ReverseMap();
        }
    }
}