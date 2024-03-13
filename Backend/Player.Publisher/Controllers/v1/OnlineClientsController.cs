using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.SoftwareClient;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OnlineClientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OnlineClientsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet]
        public async Task<List<OnlineList.OnlineClient>> Get(CancellationToken cancellationToken) => await _mediator.Send(new OnlineList.Query(), cancellationToken);
    }
}