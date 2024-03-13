using AutoMapper;
using Player.Domain;

namespace Player.BusinessLogic.Features.TrackTypes
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<TrackType, List.TrackTypeModel>();
        }
    }
}