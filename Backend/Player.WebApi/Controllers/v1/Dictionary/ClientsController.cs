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
//Этот код описывает ASP.NET Core Web API контроллер ClientsController, который управляет операциями над клиентами (или заказчиками) в системе. Контроллер использует шаблон CQRS (Command Query Responsibility Segregation) с помощью библиотеки MediatR для отправки запросов и команд, соответствующих действиям над клиентами. Давайте подробнее рассмотрим функциональность, предоставляемую этим контроллером:
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientsController(IMediator mediator)
        {
            _mediator = mediator;//Принимает экземпляр IMediator, который используется для отправки запросов и команд в соответствии с паттерном CQRS.
        }

        [HttpGet("{id:Guid}")] //Маршрут для получения данных одного клиента по его идентификатору (GUID).
        [Authorize(Policy = Permission.ReadClientById, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //Ограничивает доступ к методу, требуя наличие определенного разрешения (ReadClientById).
        public async Task<ClientModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query { Id = id }, cancellationToken); //Возвращает модель клиента (ClientModel) для указанного ID.

        [HttpGet] //[HttpGet]: Обрабатывает запросы GET без указания конкретного ID, используется для получения списка клиентов.
        [Authorize(Policy = Permission.ReadAllClients, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //[Authorize]: Требует наличие разрешения ReadAllClients.
        //TODO Убрать object в возвращаемом типе
        public async Task<object> Get([FromQuery] BaseFilterModel model, bool isDictionary = false, CancellationToken cancellationToken = default) //Принимает дополнительные параметры для фильтрации и пагинации (через BaseFilterModel) и флаг isDictionary для определения типа возвращаемых данных.
        {
            if (isDictionary)
                return await _mediator.Send(new SimpleListAll.Query(), cancellationToken);

            return await _mediator.Send(new List.Query { Filter = model }, cancellationToken);
            //В зависимости от значения isDictionary, отправляет либо запрос на получение простого списка всех клиентов, либо полного списка с учетом фильтрации.
        }

        [HttpPost] //[HttpPost]: Определяет метод как обработчик HTTP POST запросов для создания нового клиента.
        [Authorize(Policy = Permission.CreateClient, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]  //[Authorize]: Требует разрешения CreateClient.
        public async Task<IActionResult> Post([FromBody] Create.Command model, CancellationToken cancellationToken)
        { //Принимает модель команды для создания клиента (Create.Command), передает ее в медиатор и возвращает результат выполнения.
            await _mediator.Send(model, cancellationToken);
            return Ok();
        }

        [HttpPut] //[HttpPut]: Определяет метод как обработчик HTTP PUT запросов для редактирования существующего клиента.
        [Authorize(Policy = Permission.EditClient, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //[Authorize]: Требует разрешения EditClient.
        public async Task<IActionResult> Put([FromBody] Edit.Command model, CancellationToken cancellationToken)
        { //Принимает модель команды для редактирования клиента (Edit.Command), передает ее в медиатор и возвращает результат.
            await _mediator.Send(model, cancellationToken);
            return Ok();
        }
    }
    //    Комментарий //TODO Убрать object в возвращаемом типе указывает на необходимость улучшения типизации возвращаемого значения в методе Get для списка клиентов. Вместо возврата object стоит определить конкретный тип или интерфейс, который лучше описывает структуру возвращаемых данных, что улучшит читаемость кода и облегчит его поддержку.
}