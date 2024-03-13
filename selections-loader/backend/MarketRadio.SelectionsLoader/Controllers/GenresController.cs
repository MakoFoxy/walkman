using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketRadio.SelectionsLoader.Api;
using MarketRadio.SelectionsLoader.DataAccess;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services.Abstractions;
using MarketRadio.SelectionsLoader.Services.Abstractions.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketRadio.SelectionsLoader.Controllers
{
    [ApiController]
    [Route("/api/genres")]
    public class GenresController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly IGenreMapper _genreMapper;
        private readonly ITrackApi _trackApi;
        private readonly ICurrentUserKeeper _currentUserKeeper;

        public GenresController(
            DatabaseContext context, 
            IGenreMapper genreMapper, 
            ITrackApi trackApi,
            ICurrentUserKeeper currentUserKeeper
            )
        {
            _context = context;
            _genreMapper = genreMapper;
            _trackApi = trackApi;
            _currentUserKeeper = currentUserKeeper;
        }

        [HttpGet]
        public async Task<ICollection<GenreDto>> Get()
        {
            return await _context.Genres
                .Select(_genreMapper.ProjectToDto)
                .ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Create(SimpleDto genre)
        {
            var token = _currentUserKeeper.Token;
            var genreId = await _trackApi.CreateGenre(genre, token);
            _context.Genres.Add(new Genre
            {
                Id = genreId,
                Name = genre.Name,
            });
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}