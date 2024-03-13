using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketRadio.SelectionsLoader.Api;
using MarketRadio.SelectionsLoader.DataAccess;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Services.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MarketRadio.SelectionsLoader.Controllers
{
    [ApiController]
    [Route("/api/initialize")]
    public class InitializeController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ITrackApi _trackApi;
        private readonly ISelectionApi _selectionApi;
        private readonly ICurrentUserKeeper _currentUserKeeper;

        public InitializeController(
            DatabaseContext context, 
            ITrackApi trackApi,
            ISelectionApi selectionApi,
            ICurrentUserKeeper currentUserKeeper)
        {
            _context = context;
            _trackApi = trackApi;
            _selectionApi = selectionApi;
            _currentUserKeeper = currentUserKeeper;
        }


        [HttpGet]
        public async Task<bool> NeedInitialization()
        {
            return !await _context.Genres.AnyAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Initialize()
        {
            var token = _currentUserKeeper.Token;
            var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var genres = await ImportGenres(token);
                var musicTracks = await ImportTracks(genres, token);
                await ImportSelections(musicTracks, token);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                await transaction.RollbackAsync();
                return BadRequest(e.Message);
            }
            return Ok();
        }

        private async Task<List<Track>> ImportTracks(IReadOnlyCollection<Genre> genres, string token)
        {
            var raw = await _trackApi.GetMusicTracksRaw(token);
            var musicTrackDtos = await _trackApi.GetMusicTracks(token);

            var musicTracks = musicTrackDtos.Select(mt => new Track
            {
                Id = mt.Id,
                Name = mt.Name,
                Length = mt.Length.TotalSeconds,
                Uploaded = true,
                Genres = genres
                    .Where(d => mt.Genres.Select(g => g.Id).Contains(d.Id))
                    .ToList(),
            }).ToList();
            _context.Tracks.AddRange(musicTracks);
            return musicTracks;
        }

        private async Task<List<Selection>> ImportSelections(List<Track> musicTracks, string token)
        {
            var selectionDtos = await _selectionApi.GetSelections(token);
            var selections = new List<Selection>();

            foreach (var selectionDto in selectionDtos)
            {
                var fullSelectionDto = await _selectionApi.GetSelection(selectionDto.Id, token);

                var selection = new Selection
                {
                    Id = fullSelectionDto.Id,
                    Name = fullSelectionDto.Name,
                    DateBegin = fullSelectionDto.DateBegin,
                    DateEnd = fullSelectionDto.DateEnd,
                    IsPublic = fullSelectionDto.IsPublic,
                };

                var i = 0;
                selection.Tracks = fullSelectionDto.Tracks.Select(t => new TrackInSelection
                    {
                        Order = i++,
                        Selection = selection,
                        Track = musicTracks.Single(mt => mt.Id == t.Id),
                    })
                    .ToList();
                selections.Add(selection);
            }
            
            await _context.Database.ExecuteSqlRawAsync($"delete from {_context.GetTableName<Selection>()}");
            _context.Selections.AddRange(selections);
            
            return selections;
        }

        private async Task<List<Genre>> ImportGenres(string token)
        {
            var genreDtos = await _trackApi.GetGenres(token);

            await _context.Database.ExecuteSqlRawAsync($"delete from {_context.GetTableName<Genre>()}");
            var genres = genreDtos.Select(g => new Genre
            {
                Id = g.Id,
                Name = g.Name,
            }).ToList();
            _context.Genres.AddRange(genres);
            return genres;
        }
    }
}