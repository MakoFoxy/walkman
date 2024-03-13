using System;
using System.Linq;
using System.Linq.Expressions;
using Mapster;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Models;

namespace MarketRadio.SelectionsLoader.Services.Abstractions.Mappers
{
    [Mapper]
    public interface ISelectionMapper
    {
        Expression<Func<Selection, SelectionDto>> ProjectToDto { get; }
    }

    public class SelectionMapperProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Selection, SelectionDto>()
                .Map(sd => sd.Tracks, s => s.Tracks.OrderBy(t => t.Order));
            
            config.NewConfig<TrackInSelection, TrackInSelectionDto>()
                .Map(td => td.Id, ts => ts.Track.Id)
                .Map(td => td.Name, ts => ts.Track.Name)
                .Map(td => td.Order, ts => ts.Order)
                .Map(td => td.Length, ts => ts.Track.Length);
        }
    }
}