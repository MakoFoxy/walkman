using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.SoftwareClient;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/software-client")]
    public class SoftwareClientController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SoftwareClientController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestVersion(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new LatestClientExe.Query(), cancellationToken);
            return Ok(response);
            //    [HttpGet("latest")]: Атрибут, указывающий, что следующий метод отвечает на HTTP GET запросы к пути /latest. Это означает, что если кто-то отправит GET запрос к /api/v1/software-client/latest, будет вызван метод GetLatestVersion.

            //     public async Task<IActionResult> GetLatestVersion(CancellationToken cancellationToken): Асинхронный метод, который обрабатывает запрос на получение последней версии клиентского программного обеспечения. Использует токен отмены CancellationToken для правильной обработки отмены запроса.

            //     var response = await _mediator.Send(new LatestClientExe.Query(), cancellationToken);: Строка, в которой используется MediatR для отправки запроса LatestClientExe.Query(). Здесь происходит запрос к системе на получение информации о последней версии программного обеспечения. await указывает на ожидание завершения операции перед продолжением.

            //     return Ok(response);: Возвращает результат выполнения запроса клиенту. Ok() оборачивает response в стандартный HTTP ответ с кодом 200, что указывает на успешное выполнение запроса.

            // В целом, SoftwareClientController служит для управления запросами к вашему API, связанными с клиентским программным обеспечением, и демонстрирует, как можно использовать MediatR для организации кода в стиле CQRS в приложении ASP.NET Core.
        }
    }
}