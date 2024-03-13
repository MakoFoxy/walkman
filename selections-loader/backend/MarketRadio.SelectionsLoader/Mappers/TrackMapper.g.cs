using System.Linq;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Extensions;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services.Abstractions.Mappers;

namespace MarketRadio.PlaylistLoader.Services.Abstractions.Mappers
{
    public partial class TrackMapper : ITrackMapper
    {
        public System.Linq.Expressions.Expression<System.Func<Track, TrackDto>> ProjectToDto => p1 => new TrackDto()
        {
            Length = System.TimeSpan.FromSeconds(p1.Length).Round(),
            Uploaded = p1.Uploaded,
            UploadInProgress = p1.UploadInProgress,
            Genres = p1.Genres.Select<Genre, SimpleDto>(p2 => new SimpleDto()
            {
                Id = p2.Id,
                Name = p2.Name
            }).ToList<SimpleDto>(),
            Id = p1.Id,
            Name = p1.Name
        };
    }
}