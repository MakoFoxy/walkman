using System;
using System.Threading.Tasks;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketRadio.Player.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlaylistController : ControllerBase
    {
        private readonly PlaylistService _playlistService;
        private readonly PlayerStateManager _stateManager;

        public PlaylistController(PlaylistService playlistService,
            PlayerStateManager stateManager)
        {
            _playlistService = playlistService;
            _stateManager = stateManager;
        }
        
        [HttpGet("current")]
        public async Task<PlaylistDto> GetCurrentPlaylist()
        {
            return await GetPlaylistOn(DateTime.Today);
        }
        
        [HttpGet]
        public async Task<PlaylistDto> GetPlaylistOn([FromQuery]DateTime on)
        {
            return await _playlistService.LoadPlaylist(on);
        }

        [HttpGet("tracks/current")]
        public string? GetCurrentTrack()
        {
            return _stateManager.CurrentTrack?.UniqueId;
        }
        
        [HttpPost("report")]
        public async Task<ReportSendingStatus> SendPlaylistReport([FromBody]PlaybackResultDto report)
        {
            return await _playlistService.SendReport(report);
        }

        [HttpPost("{playlistId}/tracks/download")]
        public async Task<IActionResult> DownloadPlaylistTracks([FromRoute] Guid playlistId)
        {
            if (_stateManager.PlaylistIsDownloading)
            {
                return Ok();
            }

            _stateManager.PlaylistIsDownloading = true;
            await _playlistService.DownloadPlaylistTracks(playlistId);
            _stateManager.PlaylistIsDownloading = false;
            return Ok();
        }
    }
}
//TODO  Дальше будем обыгрывать треки без пути на UI и показывать процесс их загрузки вытаскивать 