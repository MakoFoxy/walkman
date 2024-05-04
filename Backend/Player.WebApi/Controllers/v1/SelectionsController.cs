using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Selections;
using Player.Domain;
using UpdateSelectionModel = Player.BusinessLogic.Features.Selections.Models.UpdateSelectionModel;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SelectionsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SelectionsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:Guid}")]
        [Authorize(Policy = Permission.ReadSelectionById, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)] //Проверка разрешений: Перед обработкой запроса происходит проверка, имеет ли пользователь соответствующие разрешения для доступа к информации о подборке.
        public async Task<Details.SelectionModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query { Id = id }, cancellationToken); //    Используется Entity Framework Core для выполнения SQL-запроса в базу данных для получения деталей подборки по уникальному идентификатору (id). Запрос извлекает такие данные, как идентификатор подборки, имя, даты начала и окончания, статус публичности, а также связанные с ней музыкальные треки с их длительностью. Создается модель SelectionModel, которая заполняется данными из базы данных, включая информацию о музыкальных треках, связанных с подборкой. Ответ сериализуется в JSON и возвращается клиенту.

        // Модель данных возвращаемого ответа:

        //     SelectionModel (определен в пространстве имен Player.BusinessLogic.Features.Selections.Details):
        //         Id: GUID подборки.
        //         Name: Название подборки.
        //         DateBegin: Дата начала действия подборки.
        //         DateEnd: Дата окончания действия подборки.
        //         IsPublic: Булево значение, указывающее на публичность подборки.
        //         Tracks: Коллекция треков, каждый из которых содержит:
        //             Id: GUID трека.
        //             Name: Название трека.
        //             Length: Длительность трека в секундах.

        // Этот API-метод предназначен для получения подробной информации о конкретной музыкальной подборке, включая список треков, их длительность и другие связанные с подборкой данные.
        [HttpGet]
        [Authorize(Policy = Permission.ReadAllSelections, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List.SelectionFilterResult> Get([FromQuery] List.SelectionFilterModel model, CancellationToken cancellationToken) => await _mediator.Send(new List.Query { Filter = model }, cancellationToken); ////SelectionsController - контроллер для обработки запросов к музыкальным подборкам (/api/v1/selections). В частности, был выполнен запрос на получение актуальных музыкальных подборок.Описание: Получение списка выборок (selections) с возможностью фильтрации по актуальности. Возвращает отфильтрованный список с идентификаторами, названиями и другими данными. Возвращаемое значение: Возвращается HTTP 200 с JSON содержащим список выборок согласно фильтрам. Возвращает объект SelectionFilterResult, содержащий результаты выборки. Этот ответ также включает метаданные о количестве выборок и другую связанную информацию. Описание: Получает список подборок на основе фильтра актуальности, страницы и количества элементов на страницу. Возвращаемое значение: Возвращает SelectionFilterResult, содержащий отфильтрованный список подборок и информацию для пагинации.
        [HttpGet("all")]
        [Authorize(Policy = Permission.ReadAllSelections, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ICollection<List.SelectionListModel>> GetAll(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new List.Query(), cancellationToken);
            return response.Result;
        }


        [HttpPost]
        [Authorize(Policy = Permission.CreateSelection, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Post([FromBody] UpdateSelectionModel model, CancellationToken token)
        {
            await _mediator.Send(new Create.Command { Model = model }, token);
            return Ok();
            //     Метод Post:
            // Сигнатура: Task<IActionResult> Post(UpdateSelectionModel model, CancellationToken cancellationToken)
            // Описание: Создание или обновление сущности "Selection" в базе данных.
            // Модель данных: UpdateSelectionModel, которая может содержать данные для создания или обновления "Selection".
        }

        [HttpPut]
        [Authorize(Policy = Permission.EditSelection, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] UpdateSelectionModel model, CancellationToken token)
        {
            await _mediator.Send(new Edit.Command { Model = model }, token);
            return Ok();
            //         Метод PUT:
            // Путь: /api/v1/selections
            // Действие: Обновление существующей подборки.
            // Модель данных в запросе: UpdateSelectionModel, содержащая данные для обновления подборки.
            // Обработка:
            //     Проверка наличия подборки по ID.
            //     Удаление старых связей музыкальных треков и добавление новых.
            // Ответ: Возвращает статус 200 OK при успешном обновлении.
        }

        [HttpPost("from-archives")]
        public async Task<IActionResult> CreateFromArchives()
        {
            await _mediator.Send(new CreateFromArchives.Command());
            return Ok();
        }
    }
}