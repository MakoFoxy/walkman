using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Adverts;
using Player.DTOs;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ArchivedAdvertsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ArchivedAdvertsController(IMediator mediator)
        {
            _mediator = mediator;
        }
        
        [HttpGet]
        public async Task<BaseFilterResult<ListArchived.ArchivedAdvertModel>> Get([FromQuery]BaseFilterModel model, CancellationToken cancellationToken)
        {
            return await _mediator.Send(new ListArchived.ArchivedAdvertRequest {Filter = model}, cancellationToken);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ArchiveAdvert.Command {Id = id}, cancellationToken);
            return Ok();
        }

        [HttpDelete]
        public async Task<IActionResult> Delete([FromBody]Guid id, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ActualizeAdvert.Command {Id = id}, cancellationToken);
            return Ok();
        }
    }
}