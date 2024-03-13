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
    [Route("/api/selections")]
    public class SelectionsController : ControllerBase
    {
        private readonly DatabaseContext _context;
        private readonly ISelectionMapper _selectionMapper;

        public SelectionsController(DatabaseContext context, ISelectionMapper selectionMapper)
        {
            _context = context;
            _selectionMapper = selectionMapper;
        }

        [HttpGet]
        public async Task<ICollection<SelectionDto>> Get()
        {
            return await _context.Selections
                .Select(_selectionMapper.ProjectToDto)
                .ToListAsync();
        }

        [HttpPost("from-folder")]
        public async Task<IActionResult> CreateFromFolder([FromBody]SelectionFromFolderUploadRequest request)
        {
            var selectionName = Path.GetFileName(request.FullPath);
            var maxPriority = await _context.Tasks
                .Select(t => t.Priority)
                .OrderByDescending(p => p)
                .FirstOrDefaultAsync();
            
            var genre = await _context.Genres.Where(g => g.Id == request.Genre.Id).SingleAsync();
            var files = Directory.GetFiles(request.FullPath);
            var existedTracks = await _context.Tracks.Where(t => files.Contains(t.Name)).ToListAsync();

            files = files.Where(f => existedTracks.All(et => et.Name != f)).ToArray();
            
            var tracks = files
                .Select(f =>
                {
                    return new Track
                    {
                        Id = Guid.NewGuid(),
                        Name = Path.GetFileName(f),
                        Path = f,
                        Uploaded = false,
                        UploadInProgress = false,
                        Genres = new List<Genre>
                        {
                            genre,
                        },
                    };
                }).ToList();

            var selection = new Selection
            {
                Id = Guid.NewGuid(),
                Name = selectionName,
                DateBegin = request.DateBegin,
                DateEnd = request.DateEnd,
                IsPublic = true,
            };

            var tracksInSelection = tracks.Select((track, index) => new TrackInSelection
            {
                Id = Guid.NewGuid(),
                Selection = selection,
                Track = track,
                Order = index,
            }).ToList();

            selection.Tracks = tracksInSelection;

            _context.Selections.Add(selection);
            _context.Tasks.Add(new Task
            {
                Name = $"Загрузка подборки {selectionName}",
                Priority = maxPriority + 1,
                CreateDate = DateTime.Now,
                TaskType = TaskType.Selection,
                TaskObjectId = selection.Id,
            });

            await _context.SaveChangesAsync();
            
            TaskExecutorBackgroundService.StartService = true;
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Create(SelectionDto selectionDto)
        {
            var order = 0;
            var selection = new Selection
            {
                Name = selectionDto.Name,
                DateBegin = selectionDto.DateBegin,
                DateEnd = selectionDto.DateEnd,
                IsPublic = true,
            };
            selection.Tracks = selectionDto.Tracks.Select(t => new TrackInSelection
            {
                Order = order++,
                TrackId = t.Id,
                Selection = selection,
            }).ToList();
            _context.Selections.Add(selection);

            await _context.SaveChangesAsync();
            return Ok();
        }
        
        [HttpPut]
        public async Task<IActionResult> Update(SelectionDto selectionDto)
        {
            var selection = await _context.Selections
                .Include(s => s.Tracks)
                .Where(s => s.Id == selectionDto.Id)
                .SingleAsync();
            
            var order = 0;
            
            selection.Name = selectionDto.Name;
            selection.DateBegin = selectionDto.DateBegin;
            selection.DateEnd = selectionDto.DateEnd;
            selection.IsPublic = true;
            selection.Tracks = selectionDto.Tracks.Select(t => new TrackInSelection
            {
                Order = order++,
                TrackId = t.Id,
                Selection = selection,
            }).ToList();

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}