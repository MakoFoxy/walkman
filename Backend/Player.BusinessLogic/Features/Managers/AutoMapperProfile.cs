using System.Linq;
using AutoMapper;
using Player.BusinessLogic.Features.Managers.Models;
using Player.Domain;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Managers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Role, SimpleDto>().ReverseMap();
            //TODO Найти более оптимальную конфигурацию
            CreateMap<Manager, ManagerModel>()
                .ForMember(mm => mm.Password, m => m.MapFrom(manager => manager.User.Password))
                .ForMember(mm => mm.FirstName, m => m.MapFrom(manager => manager.User.FirstName))
                .ForMember(mm => mm.LastName, m => m.MapFrom(manager => manager.User.LastName))
                .ForMember(mm => mm.SecondName, m => m.MapFrom(manager => manager.User.SecondName))
                .ForMember(mm => mm.Email, m => m.MapFrom(manager => manager.User.Email))
                .ForMember(mm => mm.PhoneNumber, m => m.MapFrom(manager => manager.User.PhoneNumber))
                .ForMember(mm => mm.Role, m => m.MapFrom(manager => manager.User.Role))
                .ForMember(mm => mm.Objects, m => m.MapFrom(manager => manager.User.Objects.Select(uo => uo.Object)
                    .Select(o => new SimpleDto
                    {
                        Id = o.Id,
                        Name = o.Name
                    })))
                .ReverseMap();
        }
    }
}