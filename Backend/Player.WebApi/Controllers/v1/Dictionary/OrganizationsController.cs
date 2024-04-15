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
        public async Task<OrganizationModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query { Id = id }, cancellationToken); //Описание: Метод асинхронно возвращает детальную информацию об организации по её идентификатору. Возвращаемый объект OrganizationModel содержит данные организации, такие как Id, Name, Bin, Address, Bank, Iik, Phone, а также связанные данные о клиентах и объектах через вложенные запросы к другим таблицам. Этот метод обрабатывает запрос на получение информации об определённой организации по её идентификатору (System.Guid). Метод возвращает детализированную информацию об организации в виде объекта Player.BusinessLogic.Features.Organizations.Models.OrganizationModel, который содержит такие данные, как идентификатор, имя, BIN (Бизнес Идентификационный Номер), адрес, банковские данные, телефон и другие сопутствующие сведения.

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<List.OrganizationShortInfoModel>> Get([FromQuery] BaseFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query { Filter = model }, cancellationToken); //Описание: Метод асинхронно возвращает список организаций с пагинацией. Использует фильтры для лимитирования и смещения в запросе. Возвращает данные организаций включая Id, Name, Bin, Address, Bank, Iik, Phone, и количество пользователей (UserCount), ассоциированных с каждой организацией.Возвращает BaseFilterResult типа OrganizationShortInfoModel, который включает в себя базовую информацию о каждой организации, такую как ID, название, BIN, адрес, данные банковского счета и количество клиентов в каждой организации. Данные возвращаются в формате JSON.

        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] OrganizationModel model, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Create.Command { OrganizationModel = model }, cancellationToken);
            return Ok(); //Этот метод асинхронно обрабатывает POST запрос на создание новой организации, принимая модель данных организации и токен отмены операции. По завершении обработки запроса, если все операции прошли успешно, метод возвращает StatusCodeResult с HTTP статусом 200, что означает успешное выполнение запроса без возврата каких-либо данных в теле ответа.

            //Метод Post: Этот метод используется для создания новой организации. В запросе передаются данные новой организации, включая адрес, банк, БИН (бизнес-идентификатор), ИИК (индивидуальный идентификационный код), имя, телефон и пользовательские данные. Метод выполняет вставку данных организации и связанных с ней пользователей в базу данных. Ответ сервера указывает на успешное выполнение операции, возвращая статус код 200, что означает успешное создание данных.
        }

        [HttpPut]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] OrganizationModel model, CancellationToken cancellationToken)
        {
            await _mediator.Send(new Edit.Command { OrganizationModel = model }, cancellationToken);
            return Ok();
        } //Описание: Метод обрабатывает обновление данных организации на основе предоставленной модели OrganizationModel. Возвращает статусный код 200 (OK) после обновления данных, включая взаимодействие с таблицами UserObjects и Users.
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