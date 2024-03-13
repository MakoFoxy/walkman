using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BrunoZell.ModelBinding;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Adverts;
using Player.BusinessLogic.Features.Objects;
using Player.BusinessLogic.Features.Objects.Models;
using Player.Domain;
using Player.DTOs;
using Create = Player.BusinessLogic.Features.Objects.Create;
using Details = Player.BusinessLogic.Features.Objects.Details;
using List = Player.BusinessLogic.Features.Objects.List;
using ObjectModel = Player.BusinessLogic.Features.Objects.Models.ObjectModel;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ObjectController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllObjects, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<ObjectModel>> Get([FromQuery]List.ObjectFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query{Filter = model}, cancellationToken);

        [HttpGet("user")]
        [Authorize(Policy = Permission.ReadAllObjects, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<GetUserObjects.Response> GetUserObjects(CancellationToken cancellationToken) => await _mediator.Send(new GetUserObjects.Query(), cancellationToken);
        
        [HttpGet("all")]
        //TODO Логику авторизации надо добавить на клиент
        //[Authorize(Policy = Permission.ReadAllObjectsForDropDown, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<SimpleListAll.ObjectDto>> GetAll(CancellationToken cancellationToken) => await _mediator.Send(new SimpleListAll.Query(), cancellationToken);

        [HttpGet("{id:Guid}")]
        //TODO Логику авторизации надо добавить на клиент
        // [Authorize(Policy = Permission.ReadObjectById, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ObjectInfoModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query {Id = id}, cancellationToken);

        [HttpGet("{objectId:Guid}/{date}/adverts")]
        public async Task<ListInObjectOnSelectedDate.Response> Get([FromRoute] Guid objectId, [FromRoute] DateTime date,
            CancellationToken cancellationToken) => await _mediator.Send(
            new ListInObjectOnSelectedDate.Query {Date = date, ObjectId = objectId}, cancellationToken);

        [HttpPut]
        [Authorize(Policy = Permission.EditObject, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] Edit.Command command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }

        [HttpPost]
        [Authorize(Policy = Permission.CreateObject, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([ModelBinder(BinderType = typeof(JsonModelBinder))]
            [FromBody]ObjectInfoModel objectInfoModel, IFormFileCollection images, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Create.Command {Model = objectInfoModel, Images = images}, cancellationToken);
            return Ok();
        }
    }
}