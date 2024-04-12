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
        public async Task<BaseFilterResult<ObjectModel>> Get([FromQuery] List.ObjectFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query { Filter = model }, cancellationToken); //фильтрация используется для поиска объектов в базе данных по имени объекта, а также предоставляет возможность фильтрации объектов по их онлайн статусу и применения других критериев фильтрации, указанных в ObjectFilterModel. //Описание: Метод выполняет запрос к базе данных, используя фильтры из ObjectFilterModel, включая дату, наличие в сети и имя объекта. Возвращает подробную информацию об объектах включая ID, название, физический адрес, времена начала и окончания деятельности, загрузку по плейлисту, уникальное и общее количество реклам, признак перегрузки и статус наличия в сети. Возвращаемое значение: При успешном выполнении возвращает данные о объектах в виде BaseFilterResult содержащем список моделей ObjectModel и общее количество объектов. Возвращаемые данные включают статус ответа HTTP 200 и JSON с данными об объектах. //http://localhost:8082/home#objects

        [HttpGet("user")]
        [Authorize(Policy = Permission.ReadAllObjects, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<GetUserObjects.Response> GetUserObjects(CancellationToken cancellationToken) => await _mediator.Send(new GetUserObjects.Query(), cancellationToken);
        //Метод GetUserObjects вызывается для получения объектов, ассоциированных с пользователем. Это происходит по маршруту /api/v1/object/user.
        [HttpGet("all")]
        //TODO Логику авторизации надо добавить на клиент
        //[Authorize(Policy = Permission.ReadAllObjectsForDropDown, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<SimpleListAll.ObjectDto>> GetAll(CancellationToken cancellationToken) => await _mediator.Send(new SimpleListAll.Query(), cancellationToken); //Описание: Метод асинхронно возвращает список всех объектов в системе. Возвращаемый список содержит объекты ObjectDto, включающие информацию о каждом объекте, такую как Id, Name и Priority.

        //    Возвращает список всех объектов, упорядоченный по имени объекта. Каждый объект в списке содержит идентификатор, имя и приоритет. Ответ также сопровождается статусом HTTP 200.
        
        [HttpGet("{id:Guid}")]
        //TODO Логику авторизации надо добавить на клиент
        // [Authorize(Policy = Permission.ReadObjectById, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ObjectInfoModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query { Id = id }, cancellationToken);

        [HttpGet("{objectId:Guid}/{date}/adverts")]
        public async Task<ListInObjectOnSelectedDate.Response> Get([FromRoute] Guid objectId, [FromRoute] DateTime date,
            CancellationToken cancellationToken) => await _mediator.Send(
            new ListInObjectOnSelectedDate.Query { Date = date, ObjectId = objectId }, cancellationToken);

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
            await _mediator.Send(new Create.Command { Model = objectInfoModel, Images = images }, cancellationToken);
            return Ok();

            //Описание: Создание нового объекта с переданными данными и файлами. Принимает данные объекта и коллекцию файлов.
            // Возвращаемое значение: Возвращается HTTP 200, подтверждая успешное создание объекта.
        }
    }
}