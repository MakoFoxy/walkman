using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Player.BusinessLogic.Features.Playlists;

namespace Player.Publisher.Controllers.v1
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
        }

        [HttpPost]
        public async Task<ActionResult<SendReport.Response>> Post([FromBody] SendReport.Command command, CancellationToken cancellationToken) => Ok(await _mediator.Send(command, cancellationToken));

        //     Метод Post:
        // [HttpPost] указывает, что этот метод отвечает на HTTP POST запросы.
        // public async Task<ActionResult<SendReport.Response>> Post([FromBody] SendReport.Command command, CancellationToken cancellationToken):
        //     Асинхронный метод, который принимает команду SendReport.Command из тела запроса (указывается [FromBody]).
        //     CancellationToken используется для отмены операции, если клиент прерывает запрос.
        //     Внутри метода вызывается _mediator.Send(command, cancellationToken) для отправки команды обработчику через медиатор.
        //     Возвращает результат выполнения команды в формате JSON.
    }
    //Метод Post в этом контроллере обрабатывает входящие POST-запросы для создания отчетов. Он использует MediatR для делегирования логики обработки команды SendReport.Command соответствующему обработчику, что позволяет разделять веб-слои приложения и бизнес-логику, упрощая тем самым тестирование и поддержку кода.
}