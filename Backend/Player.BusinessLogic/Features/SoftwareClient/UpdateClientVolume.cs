using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Player.DataAccess;
using Player.DTOs;
using Player.Helpers.ApiInterfaces.PublisherApiInterfaces;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class UpdateClientVolume
    {
        public class Handler : IRequestHandler<Request>
        {
            private readonly PlayerContext _context;
            private readonly IObjectApi _objectApi;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly ILogger<Handler> _logger;

            public Handler(PlayerContext context,
                IObjectApi objectApi,
                IHttpContextAccessor httpContextAccessor,
                ILogger<Handler> logger)
            {
                _context = context;
                _objectApi = objectApi;
                _httpContextAccessor = httpContextAccessor;
                _logger = logger;
                //Конструктор класса Handler, принимает контекст базы данных PlayerContext, интерфейс API для объектов IObjectApi, доступ к HTTP контексту через IHttpContextAccessor, и логгер ILogger<Handler> для логирования событий.
            }

            public async Task<Unit> Handle(Request request, CancellationToken cancellationToken)
            {
                var objectInfo = await _context.Objects.Where(o => o.Id == request.ObjectId)
                    .SingleOrDefaultAsync(cancellationToken);

                var json = JsonConvert.DeserializeObject<JObject>(objectInfo.ClientSettings);
                var advertVolume = json["advertVolume"]!.Value<JArray>();
                var musicVolume = json["musicVolume"]!.Value<JArray>();

                advertVolume[request.Hour].Remove();
                advertVolume[request.Hour].AddBeforeSelf(request.AdvertVolume);

                musicVolume[request.Hour].Remove();
                musicVolume[request.Hour].AddBeforeSelf(request.MusicVolume);

                objectInfo.ClientSettings = JsonConvert.SerializeObject(json);
                await _context.SaveChangesAsync(cancellationToken);

                try
                {
                    await _objectApi.ObjectVolumeChanged(request.ObjectId, new ObjectVolumeChangedDto
                    {
                        Hour = request.Hour,
                        AdvertVolume = request.AdvertVolume,
                        MusicVolume = request.MusicVolume,
                        ObjectId = request.ObjectId,
                    }, _httpContextAccessor.HttpContext!.Request.Headers["Authorization"]);
                }
                catch (Exception e)
                {
                    _logger.LogError("Online volume update failed. {Exception}", e);
                }


                return Unit.Value;
                //Это асинхронный метод, который обрабатывает запрос Request. Метод изменяет настройки громкости рекламы и музыки для конкретного объекта (клиента) на определенный час. После обновления настроек в базе данных, метод пытается отправить изменения через API к сервису или клиенту и логгирует результат.
            }
        }

        public class Request : IRequest<Unit>
        {
            public Guid ObjectId { get; set; }
            public int Hour { get; set; }
            public int AdvertVolume { get; set; }
            public int MusicVolume { get; set; }

            //Request содержит информацию о том, какие изменения необходимо выполнить: ObjectId (идентификатор объекта), Hour (час, на который производится изменение), AdvertVolume и MusicVolume (новые уровни громкости для рекламы и музыки соответственно).
        }
    }
    //Этот код может быть использован в системах, где требуется динамическое изменение громкости звука клиентских устройств, например, в заведениях общественного питания, магазинах или на производственных площадках. Позволяет централизованно управлять настройками звука, адаптируя их под различные условия или предпочтения пользователей.
}