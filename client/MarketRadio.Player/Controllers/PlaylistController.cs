using System;
using System.Threading.Tasks;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services;
using Microsoft.AspNetCore.Mvc;

namespace MarketRadio.Player.Controllers
{
    [ApiController] //[ApiController]: Декларирует класс как контроллер API.
    [Route("api/[controller]")] //[Route("api/[controller]")]: Устанавливает маршрут к API контроллера, где [controller] будет заменено на имя контроллера (в данном случае playlist).
    public class PlaylistController : ControllerBase
    //Этот код на C# представляет собой контроллер ASP.NET Core, названный PlaylistController, который управляет функциональностью, связанной с плейлистами в медиаплеере. Вот построчное объяснение его структуры и функционала:
    {
        private readonly PlaylistService _playlistService;
        private readonly PlayerStateManager _stateManager;

        public PlaylistController(PlaylistService playlistService,
            PlayerStateManager stateManager)
        {
            _playlistService = playlistService;
            _stateManager = stateManager; //    Объявляются приватные поля _playlistService и _stateManager для использования в методах контроллера.

            //Конструктор PlaylistController принимает сервисы PlaylistService и PlayerStateManager и инициализирует поля.
        }

        [HttpGet("current")] //[HttpGet("current")]: Обрабатывает GET запросы на api/playlist/current.
        public async Task<PlaylistDto> GetCurrentPlaylist()
        {
            return await GetPlaylistOn(DateTime.Today); //Возвращает текущий плейлист для сегодняшнего дня, используя сервис плейлистов.
        }

        [HttpGet]
        public async Task<PlaylistDto> GetPlaylistOn([FromQuery] DateTime on) //[HttpGet]: Основной метод GET, который обрабатывает запросы на api/playlist, ожидая дату в качестве параметра.
        {
            return await _playlistService.LoadPlaylist(on); //Загружает плейлист для указанной даты.
        }

        [HttpGet("tracks/current")] //[HttpGet("tracks/current")]: Обрабатывает запросы на api/playlist/tracks/current.
        public string? GetCurrentTrack()
        {
            return _stateManager.CurrentTrack?.UniqueId; //Возвращает идентификатор текущего трека, используя состояние плеера.
        }

        [HttpPost("report")] //[HttpPost("report")]: Обрабатывает POST запросы на api/playlist/report.
        public async Task<ReportSendingStatus> SendPlaylistReport([FromBody] PlaybackResultDto report)
        {
            return await _playlistService.SendReport(report); //Отправляет отчет о воспроизведении плейлиста, полученный в теле запроса.
        }

        [HttpPost("{playlistId}/tracks/download")] //[HttpPost("{playlistId}/tracks/download")]: Обрабатывает POST запросы на загрузку треков плейлиста.
        public async Task<IActionResult> DownloadPlaylistTracks([FromRoute] Guid playlistId)
        {
            if (_stateManager.PlaylistIsDownloading) //Если плейлист уже загружается, возвращает Ok() без повторного выполнения загрузки.
            {
                return Ok();
            }

            _stateManager.PlaylistIsDownloading = true; //В противном случае, устанавливает флаг загрузки, выполняет загрузку треков и сбрасывает флаг.
            await _playlistService.DownloadPlaylistTracks(playlistId);
            _stateManager.PlaylistIsDownloading = false;
            return Ok();
        }
        //Каждый метод обеспечивает часть функциональности API для управления плейлистами, включая получение текущего плейлиста, загрузку плейлистов на определенную дату, отслеживание текущего трека, отправку отчетов о воспроизведении и загрузку треков из плейлиста.
    }
}
//TODO  Дальше будем обыгрывать треки без пути на UI и показывать процесс их загрузки вытаскивать 