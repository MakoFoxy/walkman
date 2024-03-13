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
            }
        }

        public class Request : IRequest<Unit>
        {
            public Guid ObjectId { get; set; }
            public int Hour { get; set; }
            public int AdvertVolume { get; set; }
            public int MusicVolume { get; set; }
        }
    }
}