using System;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Playlists;
using Player.DataAccess;
using Player.Domain;
using Player.DTOs;
using Player.Services;
using Player.Services.Abstractions;
using Player.Services.PlaylistServices;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class PlaylistController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly PlayerContext _context;
        private readonly IPlaylistGenerator _playlistGenerator;
        private readonly IMediaPlanExcelReportCreator _mediaPlanExcelReportCreator;

        public PlaylistController(IMediator mediator, 
            PlayerContext context,
            IPlaylistGenerator playlistGenerator,
            IMediaPlanExcelReportCreator mediaPlanExcelReportCreator)
        {
            _mediator = mediator;
            _context = context;
            _playlistGenerator = playlistGenerator;
            _mediaPlanExcelReportCreator = mediaPlanExcelReportCreator;
        }
        
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<GetPlaylistForObject.Response>> Get([FromQuery]GetPlaylistForObject.Query query, CancellationToken token) => Ok(await _mediator.Send(query, token));

        [HttpPost("run-generator")]
        public async Task<IActionResult> RunGenerator(Guid objectId, DateTime date, bool commit, bool onEmpty)
        {
            var dbContextTransaction = _context.Database.BeginTransaction();

            var objectInfo = _context.Objects.Single(o => o.Id == objectId);

            if (onEmpty)
            {
                var playlist = await _context.Playlists
                    .SingleOrDefaultAsync(p => p.Object.Id == objectId && p.PlayingDate == date);

                if (playlist != null)
                {
                    var deleted = await _context.Database.ExecuteSqlRawAsync($"select delete_playlist('{playlist.Id}')");

                    playlist = null;
                    playlist = await _context.Playlists
                        .SingleOrDefaultAsync(p => p.Object.Id == objectId && p.PlayingDate == date);
                }
            }

            var result = await _playlistGenerator.Generate(objectInfo, date);
            
            if (commit)
            {
                if (_context.Entry(result.Playlist).State == EntityState.Detached)
                {
                    _context.Playlists.Add(result.Playlist);
                }
                _context.SaveChanges();
                dbContextTransaction.Commit();
            }
            else
            {
                dbContextTransaction.Rollback();
            }

            return Ok(string.Join(Environment.NewLine, result.DebugInfo));
        }

        [HttpPost("run-temp-media-plan")]
        public async Task<IActionResult> RunTempMediaPlan(Guid objectId, DateTime date, bool onEmpty)
        {
            var dbContextTransaction = _context.Database.BeginTransaction();

            var objectInfo = _context.Objects.Single(o => o.Id == objectId);

            if (onEmpty)
            {
                var playlist = await _context.Playlists
                    .SingleOrDefaultAsync(p => p.Object.Id == objectId && p.PlayingDate == date);

                if (playlist != null)
                {
                    var deleted = await _context.Database.ExecuteSqlRawAsync($"select delete_playlist('{playlist.Id}')");

                    playlist = null;
                    playlist = await _context.Playlists
                        .SingleOrDefaultAsync(p => p.Object.Id == objectId && p.PlayingDate == date);
                }
            }

            var result = await _playlistGenerator.Generate(objectInfo, date);
            
            var model = new PlaylistModel
            { 
                Object = new ObjectModel
                {
                    Id = objectInfo.Id,
                    Name = result.Playlist.Object.Name,
                    WorkTime = result.Playlist.Object.WorkTime,
                    EndTime = result.Playlist.Object.EndTime,
                },
                Tracks = result.Playlist.MusicTracks.OrderBy(mt => mt.PlayingDateTime).Select(m=> new TrackModel
                {
                    Name = m.MusicTrack.Name,
                    TypeCode = m.MusicTrack.TrackType.Code,
                    Length = m.MusicTrack.Length,
                    StartTime = m.PlayingDateTime,
                }).ToList(),
                PlayDate = result.Playlist.PlayingDate
            };

            if (!model.Tracks.Any())
            {
                return NotFound();
            }

            var adverts = result.Playlist.Aderts.Select(m => new TrackModel
            {
                StartTime = m.PlayingDateTime,
                Name = m.Advert.Name,
                TypeCode = TrackType.Advert,
                Length = m.Advert.Length,
            }).ToList();

            model.Tracks.AddRange(adverts);
            model.Tracks = model.Tracks.OrderBy(t => t.StartTime).ToList();

            var report = _mediaPlanExcelReportCreator.Create(model);
            
            dbContextTransaction.Rollback();
            
            return File(report, "application/octet-stream", $"{objectInfo.Name}.xlsx");
        }
    }
}