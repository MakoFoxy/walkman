using AutoMapper;
using Player.Domain;

namespace Player.BusinessLogic.Features.Genres
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Genre, List.GenreModel>();
        }
    }
}