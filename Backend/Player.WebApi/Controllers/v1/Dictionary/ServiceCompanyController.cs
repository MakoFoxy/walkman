using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.ServiceCompanies;
using Player.Domain;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ServiceCompanyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceCompanyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllServiceCompanies, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ICollection<List.ServiceCompanyModel>> Get(CancellationToken cancellationToken) => await _mediator.Send(new List.Query(), cancellationToken);
        //ServiceCompanyController - контроллер, отвечающий за запросы к сервисным компаниям (/api/v1/service-company). Возвращает список сервисных компаний.
        // Описание: Получение списка всех сервисных компаний. Возвращает данные о компаниях с их идентификаторами и названиями.
        // Возвращаемое значение: Возвращается HTTP 200 с JSON содержащим список сервисных компаний.

    }
}