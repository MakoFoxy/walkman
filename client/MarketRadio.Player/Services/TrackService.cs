using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Services.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Services
{
    public class TrackService
    {
        private readonly ITrackApi _trackApi;
        private readonly PlayerContext _context;
        private readonly ILogger<TrackService> _logger;

        public TrackService(ITrackApi trackApi, PlayerContext context, ILogger<TrackService> logger)
        {
            _trackApi = trackApi;
            _context = context;
            _logger = logger;
        }

        public async Task LoadTrackIfNeeded(Guid trackId)
        {
            var track = await _context.Tracks.SingleAsync(t => t.Id == trackId);
            await LoadTrackIfNeeded(track);
        } 
        
        public async Task LoadTrackIfNeeded(Track track)
        {
            if (!File.Exists(track.FilePath))
            {
                _logger.LogWarning("Track {@Track} not exists", track);
                await LoadTrack(track.Id, track.Type);
                return;
            }

            var currentHash = await CalculateTrackHash(track.FilePath);

            if (currentHash != track.Hash)
            {
                _logger.LogWarning("Track hash not equals {@Track}", track);
                await LoadTrack(track.Id, track.Type);
            }
            else
            {
                _logger.LogInformation("Track {@Track} ok", track);
            }
        }

        public async Task LoadTrack(Guid id, string trackType)
        {
            var trackIsLoaded = false;

            while (!trackIsLoaded)
            {
                try
                {
                    await LoadTrackInternal(id, trackType);
                    trackIsLoaded = true;

                }
                catch (Exception e)
                {
                    var waitTime = TimeSpan.FromSeconds(10);
                    _logger.LogError(e, "");
                    _logger.LogWarning(
                        "Track with id {Id} not loaded because internet connection was gone next attempt after {WaitTime}",
                        id, waitTime);
                    await Task.Delay(waitTime);
                }
            }
        }

        private async Task LoadTrackInternal(Guid id, string trackType)
        {
            _logger.LogInformation("Downloading track {TrackId} ...", id);

            var httpContent = await _trackApi.DownloadTrack(id, trackType);
            var trackFileBytes = await httpContent.ReadAsByteArrayAsync();

            var track = await _context.Tracks.SingleAsync(t => t.Id == id);

            var newHash = CalculateTrackHash(trackFileBytes);
            if (track.Hash != newHash)
            {
                _logger.LogWarning("Track {TrackId} with hash {Hash} not equal to downloaded track hash {NewHash}", track.Id,
                    track.Hash, newHash);
            }

            if (File.Exists(track.FilePath))
            {
                File.Delete(track.FilePath);
            }

            await using (var file = File.Create(track.FilePath))
            {
                file.Write(trackFileBytes);
            }

            await _context.SaveChangesAsync();
            _logger.LogInformation("Downloaded track {TrackId}", id);
        }

        public async Task<bool> CheckTrack(Guid id, string trackType, string hash)
        {
            var trackIsCorrect = await _trackApi.CheckTrack(id, trackType, hash);
            return trackIsCorrect.TrackIsCorrect;
        }
        
        public async Task<bool> CheckTrack(Guid id, string trackType)
        {
            var trackPath = await _context.Tracks
                                            .Where(t => t.Id == id).Select(t => t.FilePath)
                                            .SingleAsync();

            return await CheckTrack(id, trackType, await CalculateTrackHash(trackPath));
        }

        public async Task<bool> CheckTrack(Guid id)
        {
            var trackType = await _context.Tracks.Where(t => t.Id == id)
                                                        .Select(t => t.Type)
                                                        .SingleAsync();
            return await CheckTrack(id, trackType);
        }

        public string CalculateTrackHash(byte[] trackFileBytes)
        {
            var hashString = new StringBuilder();

            using (var sha256 = SHA256.Create())
            {
                var hash = sha256.ComputeHash(trackFileBytes);

                foreach (var h in hash)
                {
                    hashString.Append($"{h:x2}");
                }
            }

            return hashString.ToString();
        }

        public async Task<string> CalculateTrackHash(string trackFilePath)
        {
            if (string.IsNullOrWhiteSpace(trackFilePath) || !File.Exists(trackFilePath))
            {
                return string.Empty;
            }

            var trackFileBytes = await File.ReadAllBytesAsync(trackFilePath);
            return CalculateTrackHash(trackFileBytes);
        }
    }
}