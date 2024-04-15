using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Cities;
using Player.Domain;

namespace Player.WebApi.Controllers.v1.Dictionary
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class CitiesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CitiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(Policy = Permission.ReadAllCities, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<List<List.CityModel>> Get(CancellationToken cancellationToken) => await _mediator.Send(new List.Query(), cancellationToken);

        //    Описание: Получение списка всех городов. Возвращает данные о городах, включая идентификаторы и названия.
        // Возвращаемое значение: Возвращается HTTP 200 с JSON содержащим список городов. Возвращаемое значение: Возвращает список объектов CityModel, который содержит идентификатор и название каждого города. Это список сериализован в JSON и отправлен клиенту. Описание: Получает список всех городов.
    }
}