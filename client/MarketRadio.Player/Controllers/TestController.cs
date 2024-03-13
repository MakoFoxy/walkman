using System;
using System.Threading.Tasks;
using MarketRadio.Player.Services.System;
using Microsoft.AspNetCore.Mvc;
using Player.ClientIntegration.System;

namespace MarketRadio.Player.Controllers
{
#if DEBUG
    [ApiController]
    [Route("/api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly LogsUploader _logsUploader;

        public TestController(LogsUploader logsUploader)
        {
            _logsUploader = logsUploader;
        }
        
        [HttpPost]
        public async Task<IActionResult> Upload()
        {
            await _logsUploader.UploadLogs(new DownloadLogsRequest
            {
                From = DateTime.Now.AddYears(-1),
                To = DateTime.Now,
            });
            return Ok();
        }
    }
#endif
}