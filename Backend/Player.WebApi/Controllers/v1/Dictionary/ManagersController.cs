using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Managers;
using Player.BusinessLogic.Features.Managers.Models;
using Player.Domain;
using Player.DTOs;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ManagersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ManagersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Policy = Permission.ReadManagerById, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ManagerModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query {Id = id}, cancellationToken);

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllManagers, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<List.ManagerShortInfoModel>> Get([FromQuery]BaseFilterModel model, CancellationToken cancellationToken = default) => await _mediator.Send(new List.Query {Filter = model}, cancellationToken);

        [HttpPost]
        [Authorize(Policy = Permission.CreateManager, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] Create.Command command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpPut]
        [Authorize(Policy = Permission.EditManager, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] Edit.Command command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
    }
}