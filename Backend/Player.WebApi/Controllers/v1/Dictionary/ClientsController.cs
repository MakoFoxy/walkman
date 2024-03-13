using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Clients;
using Player.BusinessLogic.Features.Clients.Models;
using Player.Domain;
using Player.DTOs;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Policy = Permission.ReadClientById, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ClientModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query{Id =  id}, cancellationToken);

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllClients, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //TODO Убрать object в возвращаемом типе
        public async Task<object> Get([FromQuery]BaseFilterModel model, bool isDictionary = false, CancellationToken cancellationToken = default)
        {
            if (isDictionary)
                return await _mediator.Send(new SimpleListAll.Query(), cancellationToken);

            return await _mediator.Send(new List.Query {Filter = model}, cancellationToken);
        }

        [HttpPost]
        [Authorize(Policy = Permission.CreateClient, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] Create.Command model, CancellationToken cancellationToken)
        {
            await _mediator.Send(model, cancellationToken);
            return Ok();
        }

        [HttpPut]
        [Authorize(Policy = Permission.EditClient, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] Edit.Command model, CancellationToken cancellationToken)
        {
            await _mediator.Send(model, cancellationToken);
            return Ok();
        }
    }
}