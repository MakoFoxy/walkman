using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Player.DataAccess;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class CurrentClientVolume
    {
        public class Handler : IRequestHandler<Request, Response>
        {
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
            }

            public async Task<Response> Handle(Request request, CancellationToken cancellationToken)
            {
                var clientSettings = await _context.Objects.Where(o => o.Id == request.ObjectId)
                    .Select(o => o.ClientSettings)
                    .SingleOrDefaultAsync(cancellationToken);

                if (string.IsNullOrEmpty(clientSettings))
                {
                    return new Response
                    {
                        AdvertVolume = 80,
                        MusicVolume = 80,
                    };
                }

                var json = JsonConvert.DeserializeObject<JObject>(clientSettings);
                var advertVolume = json["advertVolume"]!.Value<JArray>();
                var musicVolume = json["musicVolume"]!.Value<JArray>();

                return new Response
                {
                    AdvertVolume = advertVolume[request.Hour].Value<int>(),
                    MusicVolume = musicVolume[request.Hour].Value<int>(),
                };

                //Этот асинхронный метод обрабатывает входящий запрос Request, содержащий идентификатор объекта и час, для которого требуется получить настройки громкости. Используя LINQ и Entity Framework, метод извлекает строку настроек клиента clientSettings из базы данных, а затем десериализует эту строку в объект JObject. Из этого объекта извлекаются значения громкости для музыки и рекламы в зависимости от заданного часа и возвращаются в виде объекта Response.
            }
        }

        public class Request : IRequest<Response>
        {
            public Guid ObjectId { get; set; }
            public int Hour { get; set; }
            //Request — класс запроса, содержащий идентификатор объекта (ObjectId) и час (Hour), для которого необходимо получить настройки громкости.
        }

        public class Response
        {
            public int MusicVolume { get; set; }
            public int AdvertVolume { get; set; }
            //Response — класс ответа, содержащий значения громкости для музыки (MusicVolume) и рекламы (AdvertVolume).
        }
    }
    //Логика обработки:

    //     Если настройки клиента (clientSettings) отсутствуют или пусты, метод возвращает значения громкости по умолчанию (80).
    //     Если настройки присутствуют, они десериализуются из JSON-формата, и извлекаются значения громкости для музыки и рекламы для конкретного часа.

    // Таким образом, этот код позволяет клиентам системы управлять настройками громкости в зависимости от времени суток, автоматически адаптируя звуковое сопровождение в своих объектах к текущим потребностям.
}