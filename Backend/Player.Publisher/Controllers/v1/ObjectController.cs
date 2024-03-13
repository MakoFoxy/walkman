using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Player.BusinessLogic.Features.Objects;
using Player.BusinessLogic.Hubs;
using Player.ClientIntegration;
using Player.DTOs;
using Player.Helpers.ApiInterfaces.AppApiInterface;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly IHubContext<PlayerClientHub> _clientHub;
        private readonly IObjectApi _objectApi;
        private readonly IMediator _mediator;

        public ObjectController(IHubContext<PlayerClientHub> clientHub,
            IObjectApi objectApi,
            IMediator mediator)
        {
            _clientHub = clientHub;
            _objectApi = objectApi;
            _mediator = mediator;
        }
        
        
        [HttpPost("{objectId}/ban-music/{musicId}")]
        public async Task<IActionResult> BanMusic([FromRoute] Guid objectId, [FromRoute] Guid musicId,  CancellationToken cancellationToken)
        {
            await _mediator.Send(new BanMusic.Command
            {
                ObjectId = objectId,
                MusicId = musicId,
            }, cancellationToken);

            return Ok();
        }
        
        [HttpPost("object-info-changed/{id:Guid}")]
        public async Task<IActionResult> Post([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var objectInfo = await _objectApi.GetObject(id, HttpContext.Request.Headers["Authorization"]);
            await _clientHub.Clients.Groups(id.ToString()).SendAsync(OnlineEvents.ObjectInfoReceived, objectInfo, cancellationToken);
            return Ok();
        }

        [HttpPut("object-volume-changed/{id:Guid}")]
        public async Task<IActionResult> ObjectVolumeChanged([FromRoute] Guid id, [FromBody]ObjectVolumeChangedDto volumeData,
            CancellationToken cancellationToken)
        {
            await _clientHub.Clients.Group(id.ToString())
                .SendAsync(OnlineEvents.ObjectVolumeChanged, new Player.ClientIntegration.Object.ObjectVolumeChanged
                {
                    Hour = volumeData.Hour,
                    AdvertVolume = volumeData.AdvertVolume,
                    MusicVolume = volumeData.MusicVolume,
                    ObjectId = volumeData.ObjectId,
                } , cancellationToken);
            return Ok();
        }
    }
}