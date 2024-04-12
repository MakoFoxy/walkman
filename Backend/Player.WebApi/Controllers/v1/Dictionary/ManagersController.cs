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

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllManagers, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<BaseFilterResult<List.ManagerShortInfoModel>> Get([FromQuery] BaseFilterModel model, CancellationToken cancellationToken = default) => await _mediator.Send(new List.Query { Filter = model }, cancellationToken);

        [HttpPost]
        [Authorize(Policy = Permission.CreateManager, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]

        //         Authorize применяется для обеспечения безопасности метода. Он гарантирует, что только аутентифицированные пользователи с определенными разрешениями могут вызывать этот метод.
        // Policy = Permission.CreateManager означает, что доступ к этому методу ограничен политикой, которая требует наличия разрешения CreateManager. Это разрешение должно быть предоставлено пользователю в процессе аутентификации и авторизации.
        // AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme уточняет, что для аутентификации должен использоваться JWT (JSON Web Token). Это стандартный способ для безопасной авторизации веб-запросов, особенно в REST API.
        public async Task<IActionResult> Post([FromBody] Create.Command command, CancellationToken cancellationToken)
        {///[FromBody] указывает, что данные для этого параметра должны быть извлечены из тела запроса. Это стандартный способ передачи сложных данных в POST-запросах.
            await _mediator.Send(command, cancellationToken);
            return Ok();
            //await _mediator.Send(command, cancellationToken);: Здесь используется шаблон Mediator для отправки command на обработку. Mediator упрощает разработку, разделяя логику создания объектов и их обработку. Это позволяет уменьшить зависимости между компонентами и облегчает тестирование. Метод Send асинхронно отправляет команду на обработку и ожидает её завершения.
        }

        [HttpPut]
        [Authorize(Policy = Permission.EditManager, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<IActionResult> Put([FromBody] Edit.Command command, CancellationToken cancellationToken)
        {
            await _mediator.Send(command, cancellationToken);
            return Ok();
        }
    }
}