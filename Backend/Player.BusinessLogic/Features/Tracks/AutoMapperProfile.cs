using AutoMapper;
using Player.Domain;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Tracks
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<MusicTrack, SimpleDto>().ReverseMap();
        }
    }
}