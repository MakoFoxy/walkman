using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Player.ClientIntegration;
using Refit;

namespace MarketRadio.Player.Services
{
    public class PlaylistService
    {
        private readonly PlayerContext _context;
        private readonly IPlaylistApi _playlistApi;
        private readonly PlayerStateManager _stateManager;
        private readonly TrackService _trackService;
        private readonly Bus _bus;
        private readonly ILogger<PlaylistService> _logger;

        public PlaylistService(PlayerContext context, 
            IPlaylistApi playlistApi,
            PlayerStateManager stateManager,
            TrackService trackService,
            Bus bus,
            ILogger<PlaylistService> logger)
        {
            _context = context;
            _playlistApi = playlistApi;
            _stateManager = stateManager;
            _trackService = trackService;
            _bus = bus;
            _logger = logger;
        }

        public async Task<ReportSendingStatus> SendReport(PlaybackResultDto report)
        {
            var trackReport = new TrackReport
            {
                AdvertId = report.TrackId,
                ObjectId = _stateManager.Object!.Id,
                Start = report.StartTime?? DateTime.MinValue,
                End = report.EndTime ?? DateTime.MinValue,
            };
            
            try
            {
                _context.PlaybackResults.Add(new PlaybackResult
                {
                    PlaylistId = report.PlaylistId,
                    Status = report.Status,
                    AdditionalInfo = report.AdditionalInfo,
                    TrackId = report.TrackId,
                    StartTime = report.StartTime ?? DateTime.Now,
                    EndTime = report.EndTime
                });
                await _context.SaveChangesAsync();

                if (report.Status != PlaybackStatus.Ok)
                {
                    return ReportSendingStatus.TrackErrorNotSent;
                }

                var trackType = _stateManager.Playlist?.Tracks.FirstOrDefault(t => t.Id == report.TrackId)?.Type;
                
                if (string.IsNullOrWhiteSpace(trackType))
                {
                    trackType = await _context.Tracks.Where(t => t.Id == report.TrackId).Select(t => t.Type).SingleAsync();
                }

                if (trackType != Track.Advert)
                {
                    return ReportSendingStatus.TrackOkNotSent;
                }

                _logger.LogInformation("Sending report to server with report {@Report} started", report);
                
                await _playlistApi.SendTrackReport(trackReport);
                
                _logger.LogInformation("Sending report to server with report {@Report} success", report);
                return ReportSendingStatus.Sent;
            }
            catch (ApiException e)
            {
                _logger.LogInformation("Sending report to server with report {@Report} failed", report);
                _context.PendingRequest.Add(new PendingRequest
                {
                    Url = e.Uri!.ToString(),
                    HttpMethod = e.HttpMethod.Method,
                    Body = JsonConvert.SerializeObject(trackReport),
                    Date = DateTime.Now
                });
                await _context.SaveChangesAsync();
                return ReportSendingStatus.SendingError;
            }
        }

        public async Task<PlaylistDto> LoadPlaylist(DateTime on)
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                var loadPlaylistInternal = await LoadPlaylistInternal(on);
                await tx.CommitAsync();
                return loadPlaylistInternal;
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                await tx.RollbackAsync();
                throw;
            }
        }

        private async Task<PlaylistDto> LoadPlaylistInternal(DateTime on, int retryCount = 0)
        {
            if (_stateManager.Object == null)
            {
                throw new InvalidOperationException(nameof(_stateManager.Object));
            }
            
            _logger.LogInformation("Loading playlist on {Date}", on);
            
            var playlistFromDb = await _context.Playlists
                .Include(p => p.PlaylistTracks)
                .ThenInclude(pt => pt.Playlist)
                .Include(p => p.PlaylistTracks)
                .ThenInclude(pt => pt.Track)
                .SingleOrDefaultAsync(p => p.Date == on);
            
            PlaylistWrapper playlistWrapper;
            try
            {
                playlistWrapper = await _playlistApi.GetPlaylist(_stateManager.Object.Id, on);

                if (!playlistWrapper.Playlist.Tracks.Any())
                {
                    return playlistWrapper.Playlist;
                }
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e, "Internet error");

                if (retryCount > 10)
                {
                    throw new Exception("Server error or internet connection error");
                }

                await Task.Delay(1000);
                return await LoadPlaylistInternal(on.AddDays(-1), retryCount + 1);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");

                if (playlistFromDb != null)
                {
                    var playlistToPlaylistDto = PlaylistToPlaylistDto(playlistFromDb);
                    await _stateManager.ChangePlaylist(playlistToPlaylistDto);
                    return playlistToPlaylistDto;
                }

                if (retryCount > 10)
                {
                    throw new Exception("Server error or internet connection error");
                }
                
                return await LoadPlaylistInternal(on.AddDays(-1), retryCount + 1);
            }

            PlaylistDto playlistDto;

            if (playlistFromDb == null)
            {
                playlistDto = await CreatePlaylist(playlistWrapper.Playlist, on);
            }
            else
            {
                playlistDto = await UpdatePlaylist(playlistWrapper.Playlist, playlistFromDb, on);
            }

            if (on == DateTime.Today)
            {
                await _stateManager.ChangePlaylist(playlistDto);
            }

            return playlistDto;
        }

        public async Task DownloadPlaylistTracks(Guid playlistId)
        {
            var tracks = await _context.Playlists
                .Include(p => p.PlaylistTracks)
                .ThenInclude(p => p.Track)
                .Where(p => p.Id == playlistId)
                .SelectMany(p => p.PlaylistTracks)
                .ToListAsync();

            await DownloadPlaylistTracks(tracks);
        }

        public async Task DownloadPlaylistTracks(IEnumerable<PlaylistTrack> tracks)
        {
            var orderedTracks = tracks.OrderBy(pt => pt.PlayingDateTime).ToList();
            var adverts = orderedTracks.Where(pt => pt.Track.Type == Track.Advert)
                                                        .Select(pt => pt.Track)
                                                        .DistinctBy(t => t.Id);

            var now = DateTime.Now;
            var actualMusic = orderedTracks
                .Where(pt => pt.PlayingDateTime >= now)
                .Where(pt => pt.Track.Type == Track.Music)
                .Select(pt => pt.Track)
                .DistinctBy(t => t.Id);
            
            var notActualMusic = orderedTracks
                .Where(pt => pt.PlayingDateTime < now)
                .Where(pt => pt.Track.Type == Track.Music)
                .Select(pt => pt.Track)
                .DistinctBy(t => t.Id);

            var formattedTracks = new List<Track>();
            formattedTracks.AddRange(adverts);
            formattedTracks.AddRange(actualMusic);
            formattedTracks.AddRange(notActualMusic);
            formattedTracks = formattedTracks.DistinctBy(t => t.Id).ToList();

            foreach (var track in formattedTracks)
            {
                await _trackService.LoadTrackIfNeeded(track);
                await _bus.TrackAdded(track.Id);
            }
        }

        private async Task<PlaylistDto> UpdatePlaylist(PlaylistDto playlist, Playlist playlistFromDb, DateTime on)
        {
            await InsertNewTracks(playlist);

            _context.Remove(playlistFromDb);

            var newPlaylist = new Playlist
            {
                Id = playlist.Id,
                Date = on,
                Overloaded = playlist.Overloaded,
                PlaylistTracks = playlist.Tracks.Select(t => new PlaylistTrack
                {
                    PlaylistId = playlist.Id,
                    TrackId = t.Id,
                    PlayingDateTime = t.PlayingDateTime
                }).ToList(),
            };

            _context.Playlists.Add(newPlaylist);
            await _context.SaveChangesAsync();
            return playlist;
        }

        private async Task<PlaylistDto> CreatePlaylist(PlaylistDto playlist, DateTime on)
        {
            await InsertNewTracks(playlist);
            _context.Playlists.Add(new Playlist
            {
                Id = playlist.Id,
                Date = on,
                Overloaded = playlist.Overloaded,
                PlaylistTracks = playlist.Tracks.Select(t => new PlaylistTrack
                {
                    PlaylistId = playlist.Id,
                    TrackId = t.Id,
                    PlayingDateTime = t.PlayingDateTime
                }).ToList()
            });
            await _context.SaveChangesAsync();

            return playlist;
        }

        private async Task InsertNewTracks(PlaylistDto playlist)
        {
            var newTracks = await GetNewTracks(playlist);
            _context.Tracks.AddRange(newTracks.DistinctBy(t => t.Id));
        }

        private async Task<List<Track>> GetNewTracks(PlaylistDto playlist)
        {
            var existedTracks = await _context.Tracks
                .Where(t => playlist.Tracks.Select(pt => pt.Id).Contains(t.Id))
                .ToListAsync();

            var newTracksId = playlist.Tracks.Select(t => t.Id)
                .Except(existedTracks.Select(et => et.Id))
                .ToList();

            return playlist.Tracks.Where(t => newTracksId.Contains(t.Id)).Select(t => new Track
                {
                    Id = t.Id,
                    Hash = t.Hash,
                    Length = t.Length,
                    Name = t.Name,
                    Type = t.Type
                }
            ).ToList();
        }

        private PlaylistDto PlaylistToPlaylistDto(Playlist playlist)
        {
            return new PlaylistDto
            {
                Id = playlist.Id,
                Date = playlist.Date,
                Overloaded = playlist.Overloaded,
                Tracks = playlist.PlaylistTracks.Select(pt => new TrackDto
                {
                    Id = pt.TrackId,
                    Name = pt.Track.Name,
                    Hash = pt.Track.Hash,
                    Length = pt.Track.Length,
                    Type = pt.Track.Type,
                    PlayingDateTime = pt.PlayingDateTime
                }).ToList()
            };
        }
    }
}