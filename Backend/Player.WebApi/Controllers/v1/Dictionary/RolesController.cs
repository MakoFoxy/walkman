using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Roles;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class RolesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public RolesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<List.RoleModel>> Get([FromQuery] List.RoleFilter filter, CancellationToken cancellationToken) => await _mediator.Send(new List.Query { Filter = filter }, cancellationToken);
        //    Метод Get используется для получения списка ролей по фильтру, в данном случае фильтр на роли администраторов (Admin). Это происходит по маршруту /api/v1/roles?filter=Admin. Описание: Метод асинхронно возвращает список ролей, где IsAdminRole равно false, что указывает на неадминистративные роли. Возвращает список объектов RoleModel, включающий Id и Name ролей.     Метод обрабатывает HTTP GET запрос для получения списка ролей, исключая роли администраторов. Это осуществляется через фильтрацию параметра IsAdminRole. //http://localhost:8082/add-client
        //    Метод Get: Этот метод используется для получения списка ролей, которые не являются административными. Возвращается список объектов типа RoleModel, содержащих информацию о каждой роли, такую как ID и имя. Это асинхронный метод, который использует Entity Framework Core для запроса данных. Возвращаемые данные форматируются в JSON.
    }
}