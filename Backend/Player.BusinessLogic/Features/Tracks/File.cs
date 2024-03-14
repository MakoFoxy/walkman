using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Player.DataAccess;
using Player.Domain;

namespace Player.BusinessLogic.Features.Tracks
{
    public class File
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly PlayerContext _context;
            private readonly IConfiguration _configuration;


            public Handler(PlayerContext context, IConfiguration configuration)
            {
                _context = context;
                _configuration = configuration;
                //             Принимает два параметра:

                // PlayerContext _context - контекст для доступа к базе данных.
                // IConfiguration _configuration - конфигурация приложения, из которой будет взято местоположение файлов треков.
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                string basePath;

                Track track;

                switch (query.TrackType)
                {
                    case TrackType.Advert:
                        {
                            track = await _context.Adverts.Where(a => a.Id == query.TrackId).SingleAsync(cancellationToken);
                            basePath = _configuration.GetValue<string>("Player:AdvertsPath");
                            break;
                        }
                    case TrackType.Music:
                        {
                            track = await _context.MusicTracks.Where(m => m.Id == query.TrackId).SingleAsync(cancellationToken);
                            basePath = _configuration.GetValue<string>("Player:SongsPath");
                            break;
                        }
                    default:
                        throw new ArgumentException(nameof(query.TrackType));
                }

                return new Response
                {
                    FileStream = System.IO.File.OpenRead(Path.Combine(basePath, track.FilePath)),
                    FileName = track.Name + track.Extension
                };
                //             Основная логика обработки запроса на получение файла трека:

                // Определение базового пути хранения файлов (basePath) в зависимости от типа трека (TrackType). В конфигурации приложения должны быть указаны пути для рекламных треков и музыкальных треков.
                // Поиск в базе данных трека по идентификатору (TrackId) в зависимости от его типа.
                // Составление полного пути к файлу, соединяя basePath и путь файла в базе данных (track.FilePath), и создание потока чтения файла.
                // Возвращение ответа с потоком файла и полным именем файла (имя + расширение).
            }
        }

        public class Query : IRequest<Response>
        {
            public string TrackType { get; set; }
            public Guid TrackId { get; set; }
            //Представляет запрос на получение файла трека. Содержит информацию о типе трека и его идентификаторе.
        }

        public class Response
        {
            public Stream FileStream { get; set; }
            public string FileName { get; set; }
            //Представляет ответ на запрос. Содержит поток файла (FileStream) и имя файла (FileName), которое клиент может использовать для сохранения или воспроизведения файла.
        }
    }
    //Этот функционал позволяет клиентской части приложения запросить и получить файлы аудиодорожек (например, для проигрывания или скачивания), используя информацию о треке, хранящуюся в базе данных и файловой системе сервера.
}
