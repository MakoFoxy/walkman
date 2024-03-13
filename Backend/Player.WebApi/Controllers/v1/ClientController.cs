using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Player.BusinessLogic.Features.SoftwareClient;

namespace Player.WebApi.Controllers.v1
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
        
        [HttpGet("{objectId}/settings")]
        public async Task<string> GetClientSettings([FromRoute]Guid objectId, CancellationToken cancellationToken) => await _mediator.Send(new GetSettings.Request{ObjectId = objectId}, cancellationToken);
        
        [HttpGet("{objectId}/settings/volume/{hour}")]
        public async Task<CurrentClientVolume.Response> GetClientVolume([FromRoute]Guid objectId, [FromRoute]int hour, CancellationToken cancellationToken) => await _mediator.Send(new CurrentClientVolume.Request{ObjectId = objectId, Hour = hour}, cancellationToken);

        [HttpPost("{objectId}/settings/volume")]
        public async Task<IActionResult> UpdateClientVolume([FromRoute]Guid objectId, [FromBody] UpdateClientVolume.Request request, CancellationToken cancellationToken)
        {
            await _mediator.Send(request, cancellationToken);
            return Ok();
        }

        [HttpPost("{objectId}/settings")]
        public async Task<IActionResult> SaveClientSettings([FromRoute]Guid objectId, [FromBody]object settings, CancellationToken cancellationToken)
        {
            await _mediator.Send(new UpdateSettings.Command{ObjectId = objectId, Settings = JsonConvert.SerializeObject(settings)}, cancellationToken);
            return Ok();
        }
    }
}