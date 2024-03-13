using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.SoftwareClient;
using Player.ClientIntegration.Client;
using Player.ClientIntegration.System;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ClientController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("get-logs")]
        public async Task<DownloadLogs.Response> GetLogs([FromQuery]DownloadLogs.Query query, CancellationToken token)
        {
            return await _mediator.Send(query, token);
        }

        [HttpPost("send-logs")]
        public async Task<IActionResult> SendLogs([FromBody]DownloadLogsResponse downloadLogsResponse,
            CancellationToken token)
        {
            await _mediator.Send(new UploadLogs.Command {DownloadLogsResponse = downloadLogsResponse}, token);
            return Ok();
        }
        
        [HttpGet("time")]
        public ActionResult<CurrentTimeDto> Time()
        {
            return Ok(new CurrentTimeDto
            {
                CurrentTime = DateTimeOffset.Now,
            });
        }
    }
}