using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.SoftwareClient;
using Player.ClientIntegration.Client;
using Player.ClientIntegration.System;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    //    Контроллер помечен атрибутами [ApiController], [ApiVersion("1.0")], и [Route("api/v{version:apiVersion}/[controller]")], что указывает на то, что это API контроллер с поддержкой версионирования.
    // В нем определен конструктор, который принимает IMediator от MediatR, что позволяет контроллеру отправлять команды и запросы.
    public class ClientController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("get-logs")]
        public async Task<DownloadLogs.Response> GetLogs([FromQuery] DownloadLogs.Query query, CancellationToken token)
        {
            return await _mediator.Send(query, token);
            //    Это HTTP GET метод, предназначенный для получения логов.
            // Метод принимает параметры запроса, которые собираются в объект DownloadLogs.Query из строки запроса ([FromQuery]).
            // Затем запрос делегируется через MediatR _mediator.Send(query, token);, где query - это объект запроса с параметрами, а token - токен отмены операции (CancellationToken), который можно использовать для отмены асинхронной операции, если это необходимо.
            // Возвращает объект ответа DownloadLogs.Response, который предположительно содержит информацию о запрошенных логах.
        }

        [HttpPost("send-logs")]
        public async Task<IActionResult> SendLogs([FromBody] DownloadLogsResponse downloadLogsResponse,
            CancellationToken token)
        {
            await _mediator.Send(new UploadLogs.Command { DownloadLogsResponse = downloadLogsResponse }, token);
            return Ok();
            //    Это HTTP POST метод, предназначенный для отправки логов на сервер.
            // Принимает в теле запроса объект DownloadLogsResponse ([FromBody]), который содержит данные логов, отправляемых клиентом.
            // Запрос на загрузку логов делегируется через MediatR с использованием команды UploadLogs.Command, которая инкапсулирует данные ответа логов.
            // После выполнения команды возвращает HTTP статус 200 OK, что указывает на успешное выполнение запроса.
        }

        [HttpGet("time")]
        public ActionResult<CurrentTimeDto> Time()
        {
            return Ok(new CurrentTimeDto
            {
                CurrentTime = DateTimeOffset.Now,
            });

            //             Простой HTTP GET метод, который возвращает текущее время сервера.
            // Возвращает объект CurrentTimeDto, который содержит текущее время. Этот метод может использоваться для проверки работоспособности API или для синхронизации времени между клиентом и сервером.
        }
    }

    //Этот контроллер демонстрирует типичное использование MediatR в ASP.NET Core API для обработки запросов CQRS и для интеграции различных бизнес-логик с внешним миром через HTTP интерфейсы. MediatR упрощает разделение бизнес-логики и веб-слой, делая код более чистым и легким для тестирования и поддержки.
}