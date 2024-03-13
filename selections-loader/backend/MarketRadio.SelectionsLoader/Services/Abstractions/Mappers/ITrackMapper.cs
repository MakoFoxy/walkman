using System;
using System.Linq.Expressions;
using Mapster;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Extensions;
using MarketRadio.SelectionsLoader.Models;

namespace MarketRadio.SelectionsLoader.Services.Abstractions.Mappers
{
    [Mapper]
    public interface ITrackMapper
    {
        Expression<Func<Track, TrackDto>> ProjectToDto { get; }
    }

    public class TrackMapperProfile : IRegister
    {
        public void Register(TypeAdapterConfig config)
        {
            config.NewConfig<Track, TrackDto>()
                .Map(td => td.Length, t => TimeSpan.FromSeconds(t.Length).Round());
        }
    }
}