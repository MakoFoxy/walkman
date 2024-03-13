using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Tracks;
using File = Player.BusinessLogic.Features.Tracks.File;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TrackController : ControllerBase
    {
        private readonly IMediator _mediator;
        
        public TrackController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<FileStreamResult> Get([FromQuery]File.Query query, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(query, cancellationToken);
            return File(response.FileStream, "audio/mpeg", response.FileName);
        }

        [HttpGet]
        [Route("check")]
        public async Task<ActionResult<HashCheck.Response>> Check([FromQuery]HashCheck.Query query, CancellationToken cancellationToken) => Ok(await _mediator.Send(query, cancellationToken));
    }
}