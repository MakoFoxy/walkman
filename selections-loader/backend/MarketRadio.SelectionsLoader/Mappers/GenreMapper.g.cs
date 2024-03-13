using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services.Abstractions.Mappers;

namespace MarketRadio.PlaylistLoader.Services.Abstractions.Mappers
{
    public partial class GenreMapper : IGenreMapper
    {
        public System.Linq.Expressions.Expression<System.Func<Genre, GenreDto>> ProjectToDto => p1 => new GenreDto()
        {
            Id = p1.Id,
            Name = p1.Name
        };
    }
}