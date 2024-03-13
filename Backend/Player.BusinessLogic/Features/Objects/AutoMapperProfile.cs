using AutoMapper;
using Player.BusinessLogic.Features.Objects.Models;
using Player.Domain;

namespace Player.BusinessLogic.Features.Objects
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ObjectInfo, ObjectInfoModel>()
                .ForMember(oi => oi.Selections,
                    expression => expression.Ignore())
                .ReverseMap();

            CreateMap<ObjectInfo.ResponsiblePerson, ObjectInfoModel.ResponsiblePersonModel>().ReverseMap();
        }
    }
}