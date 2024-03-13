using System;
using System.Linq.Expressions;
using Mapster;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Models;

namespace MarketRadio.SelectionsLoader.Services.Abstractions.Mappers
{
    [Mapper]
    public interface IGenreMapper
    {
        Expression<Func<Genre, GenreDto>> ProjectToDto { get; }
    }
}