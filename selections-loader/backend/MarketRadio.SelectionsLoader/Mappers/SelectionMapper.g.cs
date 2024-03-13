using System.Linq;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services.Abstractions.Mappers;

namespace MarketRadio.PlaylistLoader.Services.Abstractions.Mappers
{
    public partial class SelectionMapper : ISelectionMapper
    {
        public System.Linq.Expressions.Expression<System.Func<Selection, SelectionDto>> ProjectToDto => p1 => new SelectionDto()
        {
            IsPublic = p1.IsPublic,
            Created = p1.Created,
            DateBegin = p1.DateBegin,
            DateEnd = p1.DateEnd,
            Tracks = p1.Tracks.OrderBy<TrackInSelection, int>(t => t.Order).Select<TrackInSelection, TrackInSelectionDto>(p2 => new TrackInSelectionDto()
            {
                Length = p2.Track.Length,
                Order = p2.Order,
                Id = p2.Track.Id,
                Name = p2.Track.Name
            }).ToList<TrackInSelectionDto>(),
            Id = p1.Id,
            Name = p1.Name
        };
    }
}