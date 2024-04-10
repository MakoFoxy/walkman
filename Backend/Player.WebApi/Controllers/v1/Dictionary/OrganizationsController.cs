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
        public async Task<OrganizationModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query { Id = id }, cancellationToken);

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<List.OrganizationShortInfoModel>> Get([FromQuery] BaseFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query { Filter = model }, cancellationToken);

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] OrganizationModel model, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Create.Command { OrganizationModel = model }, cancellationToken);
            return Ok();
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] OrganizationModel model, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Edit.Command { OrganizationModel = model }, cancellationToken);
            return Ok();
        }
        //         OrganizationsController в вашем ASP.NET Core приложении предназначен для управления организациями. Он обеспечивает следующие HTTP эндпоинты:

        //     GET /organizations/{id}: Получает информацию об одной организации по её идентификатору (id). Этот запрос обрабатывается методом Get(Guid id, CancellationToken cancellationToken), который использует медиатор для отправки запроса Details.Query {Id = id}. Запрос обрабатывается обработчиком Details.Handler, который извлекает данные об организации из базы данных и возвращает их в виде OrganizationModel.

        //     GET /organizations: Получает список организаций с возможностью фильтрации и пагинации через параметры запроса. Этот запрос обрабатывается методом Get(BaseFilterModel model, CancellationToken cancellationToken), который также использует медиатор для отправки List.Query {Filter = model}.

        //     POST /organizations: Добавляет новую организацию. Принимает данные OrganizationModel в теле запроса и создаёт новую запись в базе данных.

        //     PUT /organizations: Обновляет существующую организацию. Принимает обновлённые данные OrganizationModel и модифицирует запись в базе данных.

        // Когда обработчик Details.Handler выполняет запрос, он возвращает OrganizationModel, который включает в себя:

        //     Id, Name, Bin, Address, Bank, Iik, Phone — базовая информация об организации.
        //     Clients — список клиентов организации, каждый из которых также представлен своей моделью ClientModel с детальной информацией, включая Id, FirstName, SecondName, LastName, Email, PhoneNumber, Role и Objects.
    }
}

//     Процесс Создания Организации (Create Команда):
//         Команда Create инициируется с моделью OrganizationModel, содержащей все необходимые данные для создания новой организации и связанных с ней клиентов.
//         Валидатор Validator (если правильно настроен) проверяет уникальность пользователей среди клиентов организации, используя UserUniqueValidator.
//         Обработчик Handler принимает команду Create и трансформирует OrganizationModel в сущность Organization, добавляя её в контекст базы данных. Для каждого клиента из OrganizationModel.Clients создаётся сущность User и связывается с Organization через Client. Если у клиента есть связанные объекты, они также добавляются через UserObjects.
//         После добавления организации и всех связанных сущностей в контекст, происходит сохранение изменений в базу данных.

//     Отправка данных через OrganizationsController.Post метод:
//         Post метод в OrganizationsController принимает OrganizationModel, создаёт команду Create и передаёт её в обработчик через MediatR. Это запускает процесс создания организации, описанный выше.

//     Получение данных через OrganizationsController.Get метод:
//         Get метод в OrganizationsController используется для получения списка организаций или информации об одной конкретной организации. Он также может использовать MediatR для отправки запросов на получение данных, которые затем обрабатываются соответствующими обработчиками. В зависимости от конкретной реализации запроса, может быть получена детализированная информация об организации, включая данные о связанных с ней клиентах и объектах.

// В завершение, данные, полученные через метод Get в OrganizationsController, представляют собой актуальное состояние организаций в базе данных, отражая все последние изменения, внесённые через Post метод.