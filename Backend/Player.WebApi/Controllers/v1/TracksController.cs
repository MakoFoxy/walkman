using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Tracks;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TracksController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TracksController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet]
        [Produces("application/json")]
        public async Task<ActionResult<TracksInObjectList.TrackInObject>> Get([FromQuery]TracksInObjectList.Query query, CancellationToken token) => Ok(await _mediator.Send(query, token));
    }
}