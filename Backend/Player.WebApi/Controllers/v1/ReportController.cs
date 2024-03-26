using System;
using System.Linq;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Objects;
using Player.Domain;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportController(IMediator mediator)
        {
            _mediator = mediator;
            //    Принимает объект IMediator, который используется для отправки запросов (или команд) в системе. IMediator является частью библиотеки MediatR и позволяет реализовать шаблон CQRS.
        }

        [HttpGet]
        public async Task<IActionResult> Get(Guid objectId, DateTime date)
        {
            var response = await _mediator.Send(new PlaningMediaPlanReport.Query {ObjectId = objectId, Date = date});

            if (!response.IsSuccess)
            {
                return NotFound();
                //В случае неудачи (если response.IsSuccess ложно), метод возвращает статус NotFound. В случае успеха возвращает файл отчета пользователю.
            }

            return File(response.File, "application/octet-stream", response.FileName);
            //Этот метод HTTP GET возвращает отчет для заданного объекта и даты. Параметры objectId и date передаются в запрос PlaningMediaPlanReport.Query, который затем отправляется через медиатор.
        }

        [HttpGet("media-plan")]
        [Authorize(Policy = Permission.ReadMediaPlanReport, AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        //TODO Разобраться где используется
        public async Task<IActionResult> Get([FromQuery] MediaPlanReport.Query query)
        { //Это перегруженная версия метода HTTP GET, которая принимает параметры запроса из строки запроса для генерации отчета о медиапланировании.
            var response = await _mediator.Send(query);

            if (!response.File.Any())
            {
                return BadRequest();
                //В случае, если файл отчета пуст (не содержит данных), метод возвращает статус BadRequest. В противном случае возвращает файл отчета пользователю.
            }

            return File(response.File, "application/octet-stream", $"{response.FileName}.{response.Type}");
            //Метод требует авторизации на основе заданной политики ReadMediaPlanReport, используя схему аутентификации JwtBearerDefaults.AuthenticationScheme.
        }
    }
}
