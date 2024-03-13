using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Player.DataAccess;

namespace Player.BusinessLogic.Features.Music
{
    public class Download
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly PlayerContext _context;
            private readonly IConfiguration _configuration;

            public Handler(PlayerContext context, IConfiguration configuration)
            {
                _context = context;
                _configuration = configuration;
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                var basePath = _configuration.GetValue<string>("Player:SongsPath");
                var response = await _context.MusicTracks
                    .Where(mt => mt.Id == query.Id)
                    .Select(mt => new Response
                    {
                        TrackPath = mt.FilePath,
                        TrackName = mt.Name,
                        TrackType = mt.Extension.Substring(1)
                    })
                    .SingleAsync(cancellationToken);

                response.TrackStream = File.OpenRead(Path.Combine(basePath, response.TrackPath));
                return response;

                //Асинхронный метод Handle принимает запрос Query, содержащий идентификатор музыкального трека, и токен отмены. Он отвечает за извлечение информации о треке из базы данных и подготовку ответа Response.

                // Внутри метода:

                //     basePath получает путь к директории с музыкальными треками из конфигурации приложения.
                //     Запрос к базе данных извлекает информацию о треке по его идентификатору и формирует ответ.
                //     Файл трека открывается на чтение, и поток файла (TrackStream) включается в ответ.
            }
        }

        public class Query : IRequest<Response>
        {
            public Guid Id { get; set; }
        }

        public class Response
        {
            public Stream TrackStream { get; set; }
            public string TrackName { get; set; }
            public string TrackType { get; set; }
            public string TrackPath { get; set; }
        }
        //    Query содержит идентификатор музыкального трека, который пользователь хочет загрузить.
        // Response включает поток файла (TrackStream), название трека (TrackName), тип файла (TrackType), и путь к файлу на сервере (TrackPath).
    }
    //В итоге, этот код позволяет пользователям загружать музыкальные треки, обеспечивая доступ к файлам на сервере по идентификатору трека. Важно отметить, что в реальных приложениях следует также учитывать вопросы безопасности и авторизации при доступе к файлам.
}