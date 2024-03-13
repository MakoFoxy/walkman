using System.Linq;
using AutoMapper;
using Player.BusinessLogic.Features.Clients.Models;
using Player.Domain;

namespace Player.BusinessLogic.Features.Clients
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Client, Player.BusinessLogic.Features.Organizations.Models.OrganizationModel.ClientModel>()
                .ForMember(cm => cm.Email, o => o.MapFrom(c => c.User.Email))
                .ForMember(cm => cm.Id, expression => expression.MapFrom(client => client.User.Id))
                .ForMember(cm => cm.Password, expression => expression.MapFrom(client => client.User.Password))
                .ForMember(cm => cm.Objects, expression => expression.MapFrom(client => client.User.Objects.Select(uo => uo.Object)))
                .ForMember(cm => cm.Role, expression => expression.MapFrom(client => client.User.Role))
                .ForMember(cm => cm.FirstName, expression => expression.MapFrom(client => client.User.FirstName))
                .ForMember(cm => cm.LastName, expression => expression.MapFrom(client => client.User.LastName))
                .ForMember(cm => cm.PhoneNumber, expression => expression.MapFrom(client => client.User.PhoneNumber))
                .ForMember(cm => cm.SecondName, expression => expression.MapFrom(client => client.User.SecondName))
                .ReverseMap();
            
            CreateMap<Client, ClientModel>()
                .ForMember(cm => cm.Name, expression => expression.MapFrom(client => client.Organization.Name))
                .ForMember(cm => cm.Bin, expression => expression.MapFrom(client => client.Organization.Bin))
                .ForMember(cm => cm.LegalAddress, expression => expression.MapFrom(client => client.Organization.Address))
                .ForMember(cm => cm.Iik, expression => expression.MapFrom(client => client.Organization.Iik))
                .ForMember(cm => cm.Bank, expression => expression.MapFrom(client => client.Organization.Bank))
                .ForMember(cm => cm.FirstPerson, expression => expression.MapFrom(client => ""))//TODO Подумать что и как тут ставить
                .ForMember(cm => cm.Email, expression => expression.MapFrom(client => client.User.Email))//TODO Подумать что и как тут ставить
                .ForMember(cm => cm.Phone, expression => expression.MapFrom(client => client.User.PhoneNumber))//TODO Подумать что и как тут ставить
                .ReverseMap();
            CreateMap<Client, SimpleListAll.ClientDictionaryModel>()
                .ForMember(cm => cm.Id, expression => expression.MapFrom(client => client.Id))
                .ForMember(cm => cm.Name, expression => expression.MapFrom(client => client.User.FullName))
                .ReverseMap();
        }
    }
}