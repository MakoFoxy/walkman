using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Tracks;
using File = Player.BusinessLogic.Features.Tracks.File;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TrackController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TrackController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<FileStreamResult> Get([FromQuery] File.Query query, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(query, cancellationToken);
            return File(response.FileStream, "audio/mpeg", response.FileName);
            //    public async Task<FileStreamResult> Get([FromQuery]File.Query query, CancellationToken cancellationToken): Метод, обрабатывающий GET запросы. Он получает параметры запроса, представленные объектом File.Query, из строки запроса (query string). Затем отправляет этот запрос через MediatR и возвращает поток файла (FileStream) клиенту как FileStreamResult с типом MIME audio/mpeg и именем файла, предоставленным в ответе.

            //     [HttpGet("check")]: Определяет дополнительный маршрут для метода Check, который будет доступен по /api/v1/track/check. Это действие используется для проверки хеша трека.

            //     public async Task<ActionResult<HashCheck.Response>> Check([FromQuery]HashCheck.Query query, CancellationToken cancellationToken): Метод для проверки хеша трека. Принимает параметры в виде объекта HashCheck.Query, обрабатывает его с помощью MediatR и возвращает ответ типа HashCheck.Response. Используется стандартный HTTP ответ (Ok), оборачивающий результат.

            // Контроллер TrackController предоставляет два API: один для получения аудиофайла и второй для проверки хеша аудиофайла. Оба используют паттерн MediatR для обработки бизнес-логики, что делает код контроллера очень лаконичным и читаемым, отделяя бизнес-логику от логики транспортного уровня (HTTP).
        }

        [HttpGet]
        [Route("check")]
        public async Task<ActionResult<HashCheck.Response>> Check([FromQuery] HashCheck.Query query, CancellationToken cancellationToken) => Ok(await _mediator.Send(query, cancellationToken));
    }
}