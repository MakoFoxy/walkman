using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using ElectronNET.API;
using ElectronNET.API.Entities;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class SystemController : ControllerBase //SystemController наследуется от ControllerBase и использует атрибуты [ApiController] и [Route("/api/[controller]")] для определения, что это API контроллер с маршрутом, базирующимся на его имени.
    {
        private readonly PlayerStateManager _stateManager;
        private readonly ILogger<SystemController> _logger;
        private readonly IApp _app;
        private readonly PlayerContext _context;

        public SystemController(PlayerStateManager stateManager, 
            ILogger<SystemController> logger,
            IApp app,
            PlayerContext context)
        {
            _stateManager = stateManager;
            _logger = logger;
            _app = app;
            _context = context;
            //В конструкторе класс принимает зависимости PlayerStateManager, ILogger<SystemController>, IApp, и PlayerContext, которые используются в различных методах API.
        }

        [HttpGet]
        public SystemState GetSystemState() =>
            new SystemState
            {
                IsOnline = _stateManager.IsOnline,
                Version = _app.Version
                //    [HttpGet]: Предоставляет информацию о текущем состоянии системы, включая статус онлайн и версию приложения.
            };

        [HttpPost("logs")] //    [HttpPost("logs")]: Принимает данные журнала (LogData) и записывает их в системный журнал. Используется для централизованного логирования событий из разных частей системы.
        public IActionResult SendLogs(LogData logData)
        {
            _logger.Log(logData.Level, "Logs from frontend {@Data}", logData.Data);
            return Ok();
        }

        [HttpGet("open-url")] //    [HttpGet("open-url")]: Открывает указанный URL в браузере пользователя. Обходит ограничения разных операционных систем на открытие URL.
        public IActionResult OpenUrl([FromQuery]string url)
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url); 
            }
            return Ok();
        }

        [HttpGet("open-window")] //    [HttpGet("open-window")]: Если используется Electron для создания настольного приложения, этот метод создает и открывает новое окно браузера с указанным URL.
        public async Task<IActionResult> OpenWindow([FromQuery] string url)
        {
            var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Center = true,
            }, url);
            window.Show();
            return Ok();
        }

        [HttpPut("master-volume")] //    [HttpPut("master-volume")]: Устанавливает главную громкость системы (или приложения), принимая значение громкости в теле запроса.
        public async Task<IActionResult> SetMasterVolume([FromBody] int volume)
        {
            await _stateManager.ChangeMasterVolume(volume);
            return Ok();
        }

        [HttpGet("master-volume")] //    [HttpGet("master-volume")]: Возвращает текущий уровень главной громкости системы (или приложения).
        public int GetMasterVolume()
        {
            return _stateManager.GetMasterVolume();
        }

        [HttpDelete("pending-requests")] //    [HttpDelete("pending-requests")]: Удаляет все ожидающие запросы из базы данных. Возвращает количество удаленных запросов.
        public async Task<IActionResult> DeletePendingRequests()
        {
            var count = await _context.PendingRequest.ExecuteDeleteAsync();
            return Ok(count);
        }
    }
    //Каждый метод выполняет конкретную функцию для управления или получения информации о системном состоянии и конфигурации. Важно отметить, что некоторые из этих методов, как OpenUrl и OpenWindow, могут использоваться только в определенных средах (например, в приложениях на базе Electron).
}