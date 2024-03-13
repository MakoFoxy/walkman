using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Organizations;
using Player.BusinessLogic.Features.Organizations.Models;
using Player.DTOs;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class OrganizationsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrganizationsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:Guid}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<OrganizationModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query {Id = id}, cancellationToken);

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<List.OrganizationShortInfoModel>> Get([FromQuery]BaseFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query {Filter = model}, cancellationToken);

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] OrganizationModel model, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Create.Command {OrganizationModel = model}, cancellationToken);
            return Ok();
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] OrganizationModel model, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Edit.Command {OrganizationModel = model}, cancellationToken);
            return Ok();
        }
    }
}