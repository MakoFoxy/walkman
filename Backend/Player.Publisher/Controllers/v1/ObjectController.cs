using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Player.BusinessLogic.Features.Objects;
using Player.BusinessLogic.Hubs;
using Player.ClientIntegration;
using Player.DTOs;
using Player.Helpers.ApiInterfaces.AppApiInterface;

namespace Player.Publisher.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class ObjectController : ControllerBase
    {
        private readonly IHubContext<PlayerClientHub> _clientHub;
        private readonly IObjectApi _objectApi;
        private readonly IMediator _mediator;

        public ObjectController(IHubContext<PlayerClientHub> clientHub,
            IObjectApi objectApi,
            IMediator mediator)
        {
            _clientHub = clientHub;
            _objectApi = objectApi;
            _mediator = mediator;
            //Объявление класса ObjectController и его конструктора. Конструктор принимает три параметра: контекст Hub для SignalR, API клиента, и медиатор для CQRS. Эти зависимости внедряются через конструктор.
            //    MediatR: Библиотека, реализующая шаблон "Посредник" (Mediator pattern), используется для декомпозиции и делегирования обработки бизнес-логики из контроллера в отдельные классы (команды и запросы).
            // SignalR: Платформа для добавления реального времени веб-функциональности. В контексте этого контроллера, SignalR используется для отправки сообщений клиентам, подключенным через веб-сокеты.
            // REST API: Контроллер обеспечивает RESTful API интерфейс для взаимодействия с объектами в системе. Он использует атрибуты маршрутизации и версионирования API для определения конечных точек.
        }


        [HttpPost("{objectId}/ban-music/{musicId}")]
        public async Task<IActionResult> BanMusic([FromRoute] Guid objectId, [FromRoute] Guid musicId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new BanMusic.Command
            {
                ObjectId = objectId,
                MusicId = musicId,
            }, cancellationToken);

            return Ok();
            //BanMusic: Позволяет запретить (заблокировать) определенную музыку для объекта. Метод принимает идентификаторы объекта и музыкального трека. Это действие обрабатывается с помощью MediatR, отправляя команду BanMusic.Command.
        }

        [HttpPost("object-info-changed/{id:Guid}")]
        public async Task<IActionResult> Post([FromRoute] Guid id, CancellationToken cancellationToken)
        {
            var objectInfo = await _objectApi.GetObject(id, HttpContext.Request.Headers["Authorization"]);
            await _clientHub.Clients.Groups(id.ToString()).SendAsync(OnlineEvents.ObjectInfoReceived, objectInfo, cancellationToken);
            return Ok();

            //Post (object-info-changed): Вызывается, когда информация об объекте изменилась. Метод получает обновленную информацию об объекте с использованием внешнего API и затем оповещает всех клиентов в соответствующей группе через SignalR. Этот механизм может использоваться для оповещения пользователей о изменениях в конфигурации или состоянии объекта.
        }

        [HttpPut("object-volume-changed/{id:Guid}")]
        public async Task<IActionResult> ObjectVolumeChanged([FromRoute] Guid id, [FromBody] ObjectVolumeChangedDto volumeData,
            CancellationToken cancellationToken)
        {
            await _clientHub.Clients.Group(id.ToString())
                .SendAsync(OnlineEvents.ObjectVolumeChanged, new Player.ClientIntegration.Object.ObjectVolumeChanged
                {
                    Hour = volumeData.Hour,
                    AdvertVolume = volumeData.AdvertVolume,
                    MusicVolume = volumeData.MusicVolume,
                    ObjectId = volumeData.ObjectId,
                }, cancellationToken);
            return Ok();
            //Используется для изменения громкости объекта. Этот метод принимает новые значения громкости для музыки и рекламы, а также идентификатор объекта. Подобно методу Post, он использует SignalR для оповещения всех клиентов о изменении громкости.
        }
    }
}