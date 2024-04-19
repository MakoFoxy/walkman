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
        public async Task<ManagerModel> Get(Guid id, CancellationToken cancellationToken) => await _mediator.Send(new Details.Query { Id = id }, cancellationToken);
        // Метод: Get(Guid id, CancellationToken cancellationToken)
        // Описание: Получает информацию о конкретном менеджере по его идентификатору.
        // Возвращает: В этом случае, конкретная модель, вероятно, Player.BusinessLogic.Features.Managers.Models.ManagerModel не возвращается напрямую в ответе из-за ошибки авторизации.
        // HTTP ответ: 403 Forbidden из-за нехватки прав у пользователя для доступа к информации о менеджере.

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllManagers, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<List.ManagerShortInfoModel>> Get([FromQuery] BaseFilterModel model, CancellationToken cancellationToken = default) => await _mediator.Send(new List.Query { Filter = model }, cancellationToken);
        // GetManagers или аналогичный, для получения списка менеджеров. Описание: Этот метод предназначен для получения списка менеджеров с возможностью пагинации. Принимает параметры для фильтрации (например, номер страницы и количество элементов на страницу) и возвращает отфильтрованный список менеджеров.
        // Описание: Получает список менеджеров, с возможностью фильтрации и пагинации. Возвращает: BaseFilterResult<List.ManagerShortInfoModel> - список менеджеров с краткой информацией. Включает данные о каждом менеджере, такие как имя, фамилия, телефон, доступность телеграм-чата. HTTP ответ: 200 OK с данными о менеджерах в формате JSON. включает в себя пагинированный список менеджеров. Каждый элемент списка содержит информацию о менеджерах, такую как ID, имя, фамилия, и другие персональные данные, а также доступность телеграм-чата.
        [HttpPost]
        [Authorize(Policy = Permission.CreateManager, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        //         Authorize применяется для обеспечения безопасности метода. Он гарантирует, что только аутентифицированные пользователи с определенными разрешениями могут вызывать этот метод.
        // Policy = Permission.CreateManager означает, что доступ к этому методу ограничен политикой, которая требует наличия разрешения CreateManager. Это разрешение должно быть предоставлено пользователю в процессе аутентификации и авторизации.
        // AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme уточняет, что для аутентификации должен использоваться JWT (JSON Web Token). Это стандартный способ для безопасной авторизации веб-запросов, особенно в REST API.
        public async Task<IActionResult> Post([FromBody] Create.Command command, CancellationToken cancellationToken)
        {///[FromBody] указывает, что данные для этого параметра должны быть извлечены из тела запроса. Это стандартный способ передачи сложных данных в POST-запросах. Описание: Этот метод обрабатывает создание нового менеджера в системе. Он принимает данные команды для создания менеджера и сохраняет их в базу данных.
            await _mediator.Send(command, cancellationToken);
            return Ok();
            //await _mediator.Send(command, cancellationToken);: Здесь используется шаблон Mediator для отправки command на обработку. Mediator упрощает разработку, разделяя логику создания объектов и их обработку. Это позволяет уменьшить зависимости между компонентами и облегчает тестирование. Метод Send асинхронно отправляет команду на обработку и ожидает её завершения.
            //    Метод обрабатывает HTTP POST запрос для создания нового менеджера. Перед созданием проверяется наличие необходимых разрешений у пользователя. Возвращаемое значение: Возвращает статус код 200 (OK) после успешного создания менеджера, что подтверждает, что операция прошла успешно.
        }

        [HttpPut]
        [Authorize(Policy = Permission.EditManager, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] Edit.Command command, CancellationToken cancellationToken)
        { //Описание: Обновляет данные о менеджере в системе, включая обновление пользовательских объектов. Возвращает: HTTP статус 200 OK, указывающий на успешное выполнение операции обновления. Возвращаемое тело ответа не указано, предполагается стандартный ответ IActionResult.
            await _mediator.Send(command, cancellationToken);
            return Ok();
            // HTTP статус 200 OK, указывающий на успешное выполнение операции обновления. 
        }
    }
}