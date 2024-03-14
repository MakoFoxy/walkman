using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using MediatR.Pipeline;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Player.DataAccess;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Songs
{
    public class CreateList
    {
        public class PreProcessor : IRequestPreProcessor<Command>
        {
            //Это класс предварительной обработки для команды Command. Он используется для сохранения потоков песен, полученных в команде, в файлы перед тем, как основной обработчик (Handler) начнет свою работу.
            private readonly IConfiguration _configuration;

            public PreProcessor(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public async Task Process(Command command, CancellationToken cancellationToken)
            {
                var basePath = _configuration.GetValue<string>("Player:SongsPath");

                foreach (var musicFile in command.SongStreams)
                {
                    await using var fileStream = File.Create(Path.Combine(basePath, musicFile.Name));
                    var memory = new byte[musicFile.Stream.Length];
                    await musicFile.Stream.ReadAsync(memory, 0, memory.Length, cancellationToken);
                    await fileStream.WriteAsync(memory, 0, memory.Length, cancellationToken);
                    //Метод загружает музыкальные файлы из потоков, содержащихся в команде, на сервер или в специфическое место на диске.
                }
            }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IConfiguration _configuration;
            private readonly IUserManager _userManager;
            private readonly IMusicTrackCreator _musicTrackCreator;

            public Handler(PlayerContext context,
                IConfiguration configuration,
                IUserManager userManager,
                IMusicTrackCreator musicTrackCreator)
            {
                _context = context;
                _configuration = configuration;
                _userManager = userManager;
                _musicTrackCreator = musicTrackCreator;
                //Конструктор принимает зависимости, необходимые для работы: контекст базы данных, конфигурацию приложения, менеджер пользователей и создателя музыкальных треков.
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var basePath = _configuration.GetValue<string>("Player:SongsPath");

                var maxIndex = await _context.MusicTracks.OrderByDescending(mt => mt.Index).Select(mt => mt.Index)
                    .FirstOrDefaultAsync(cancellationToken);
                var musicTrackTypeId =
                    await _context.TrackTypes.Where(tt => tt.Code == TrackType.Music)
                        .Select(tt => tt.Id)
                        .SingleAsync(cancellationToken);
                var currentUser = await _userManager.GetCurrentUser(cancellationToken);

                var genreId = command.SongStreams.First().Genre.Id;

                var musicTracks = command.SongStreams
                    .Select(ss => new SimpleDto
                    {
                        Id = ss.MusicTrackId ?? Guid.Empty,
                        Name = Path.Combine(basePath, ss.Name),
                    })
                    .ToList();
                var musicTracksForInsert = await _musicTrackCreator.CreateMusicTracks(new MusicTrackCreatorData
                {
                    Tracks = musicTracks,
                    MaxIndex = maxIndex,
                    BasePath = basePath,
                    MusicTrackTypeId = musicTrackTypeId,
                    GenreId = genreId,
                    UserId = currentUser.Id,
                });

                _context.MusicTracks.AddRange(musicTracksForInsert);
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
                //Основной метод обработчика. Он использует данные из команды для создания новых музыкальных треков и сохранения их в базе данных.
                //Метод Handle в обработчике сначала определяет базовый путь для музыкальных треков из конфигурации. Затем он собирает информацию о треках (включая метаданные и пути к файлам) и использует сервис IMusicTrackCreator для создания записей музыкальных треков в базе данных. Каждый новый музыкальный трек связывается с определенным жанром, пользователем (обычно автором или загрузившим) и типом трека (в данном случае музыка).
            }
        }

        public class Command : IRequest<Unit>
        {
            public ICollection<MusicFileModel> SongStreams { get; set; } = new List<MusicFileModel>();
            //Это класс команды, который передает информацию о музыкальных файлах, которые необходимо загрузить и добавить в базу данных.
        }

    }
    //Этот код предназначен для системы, в которой требуется добавление списка музыкальных треков пользователем или системой в базу данных. Может использоваться в музыкальных приложениях, системах управления контентом или мультимедийных платформах.
    //Этот набор классов и методов может использоваться в музыкальной платформе или приложении, где пользователи или администраторы могут загружать новые музыкальные треки для включения в каталог. Предварительная обработка обеспечивает сохранение файлов, а основная логика занимается созданием соответствующих записей в базе данных.
}
