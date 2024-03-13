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
            }
        }

        public class Request : IRequest<Response>
        {
            public Guid ObjectId { get; set; }
            public int Hour { get; set; }
        }

        public class Response
        {
            public int MusicVolume { get; set; }
            public int AdvertVolume { get; set; }
        }
    }
}