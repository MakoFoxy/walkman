using System;
using System.Threading;
using System.Threading.Tasks;
using BrunoZell.ModelBinding;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Adverts;
using Player.BusinessLogic.Features.Adverts.Models;
using Player.Domain;
using Player.DTOs;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class AdvertsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdvertsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Policy = Permission.CreateAdvert, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([ModelBinder(BinderType = typeof(JsonModelBinder))][FromBody]Create.AdvertData advert, IFormFile advertFile)
        {
            await _mediator.Send(new Create.Command {Advert = advert, AdvertFile = advertFile});
            return Ok();
        }

        [HttpGet("check-possibility")]
        [Authorize(Policy = Permission.CreateAdvert, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> CheckPossibility(
            [FromQuery] CheckPossibility.Query query,
            CancellationToken token
            )
        {
            var response = await _mediator.Send(query, token);
            return Ok(response);
        }

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllAdverts, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<AdvertModel>> Get([FromQuery]List.AdvertFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query{Filter = model}, cancellationToken);

        [HttpGet("{id:Guid}")]
        [Authorize(Policy = Permission.ReadAdvertById, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseAdvertModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query {AdvertId = id}, cancellationToken);
    }
}