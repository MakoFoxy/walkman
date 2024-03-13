using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.SoftwareClient;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/software-client")]
    public class SoftwareClientController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SoftwareClientController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestVersion(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new LatestClientExe.Query(), cancellationToken);
            return Ok(response);
        }
    }
}