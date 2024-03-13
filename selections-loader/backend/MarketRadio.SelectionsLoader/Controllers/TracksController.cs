using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MarketRadio.SelectionsLoader.BackgroundServices;
using MarketRadio.SelectionsLoader.DataAccess;
using MarketRadio.SelectionsLoader.Domain;
using MarketRadio.SelectionsLoader.Models;
using MarketRadio.SelectionsLoader.Services.Abstractions.Mappers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Task = MarketRadio.SelectionsLoader.Domain.Task;
using TaskType = MarketRadio.SelectionsLoader.Domain.TaskType;

namespace MarketRadio.SelectionsLoader.Controllers
{
    [ApiController]
    [Route("/api/tracks")]
    public class TracksController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ITrackMapper _trackMapper;

        public TracksController(DatabaseContext context, ITrackMapper trackMapper)
        {
            _context = context;
            _trackMapper = trackMapper;
        }

        [HttpGet]
        public async Task<List<TrackDto>> Get()
        {
            return await _context.Tracks
                .OrderBy(t => t.Uploaded)
                .ThenBy(t => t.Name)
                .Select(_trackMapper.ProjectToDto).ToListAsync();
        }

        [HttpPost]
        public async Task<IActionResult> Upload([FromBody] TracksUploadRequest uploadRequest)
        {
            var genreIds = uploadRequest.Genres.Select(g => g.Id).ToList();
            var genres = await _context.Genres.Where(g => genreIds.Contains(g.Id)).ToListAsync();

            var priority = 0;
            
            foreach (var uploadRequestPath in uploadRequest.Paths)
            {
                var track = new Track
                {
                    Id = Guid.NewGuid(),
                    Genres = genres,
                    Length = 0,
                    Path = uploadRequestPath,
                    Name = Path.GetFileName(uploadRequestPath),
                };
                _context.Tracks.Add(track);
                _context.Tasks.Add(new Task
                {
                    Name = $"Загрузка трека {track.Name}",
                    Priority = priority++,
                    CreateDate = DateTime.Now,
                    TaskType = TaskType.Track,
                    TaskObjectId = track.Id
                });
            }

            await _context.SaveChangesAsync();

            TaskExecutorBackgroundService.StartService = true;
            return Ok();
        }
    }
}