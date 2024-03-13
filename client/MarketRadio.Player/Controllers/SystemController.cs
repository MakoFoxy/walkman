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
    public class SystemController : ControllerBase
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
        }

        [HttpGet]
        public SystemState GetSystemState() =>
            new SystemState
            {
                IsOnline = _stateManager.IsOnline,
                Version = _app.Version
            };

        [HttpPost("logs")]
        public IActionResult SendLogs(LogData logData)
        {
            _logger.Log(logData.Level, "Logs from frontend {@Data}", logData.Data);
            return Ok();
        }

        [HttpGet("open-url")]
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

        [HttpGet("open-window")]
        public async Task<IActionResult> OpenWindow([FromQuery] string url)
        {
            var window = await Electron.WindowManager.CreateWindowAsync(new BrowserWindowOptions
            {
                Center = true,
            }, url);
            window.Show();
            return Ok();
        }

        [HttpPut("master-volume")]
        public async Task<IActionResult> SetMasterVolume([FromBody] int volume)
        {
            await _stateManager.ChangeMasterVolume(volume);
            return Ok();
        }

        [HttpGet("master-volume")]
        public int GetMasterVolume()
        {
            return _stateManager.GetMasterVolume();
        }

        [HttpDelete("pending-requests")]
        public async Task<IActionResult> DeletePendingRequests()
        {
            var count = await _context.PendingRequest.ExecuteDeleteAsync();
            return Ok(count);
        }
    }
}