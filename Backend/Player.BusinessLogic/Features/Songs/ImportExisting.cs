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
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Songs
{
    public class ImportExisting
    {
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
                //Принимает четыре параметра:

                // PlayerContext _context: контекст Entity Framework для доступа к базе данных.
                // IConfiguration _configuration: конфигурация приложения, откуда берется путь к папке с песнями.
                // IUserManager _userManager: сервис для работы с информацией о пользователях.
                // IMusicTrackCreator _musicTrackCreator: сервис для создания новых записей музыкальных треков.
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var basePath = _configuration.GetValue<string>("Player:SongsPath");

                var files = Directory.GetFiles(basePath, "*.mp3", SearchOption.AllDirectories);
                var fileNames = files.Select(f => (string)Path.GetFileNameWithoutExtension(f));

                var tracksInDb = await _context.MusicTracks
                    .Select(mt => mt.Name)
                    .ToListAsync(cancellationToken);

                var newTracks = fileNames
                    .Where(fn => !tracksInDb.Contains(Path.GetFileNameWithoutExtension(fn)))
                    .Select(t => new SimpleDto
                    {
                        Name = t,
                    })
                    .ToList();

                var maxIndex = await _context.MusicTracks.OrderByDescending(mt => mt.Index).Select(mt => mt.Index)
                    .FirstOrDefaultAsync(cancellationToken);

                var musicTrackTypeId =
                    await _context.TrackTypes.Where(tt => tt.Code == TrackType.Music)
                        .Select(mt => mt.Id)
                        .SingleAsync(cancellationToken);

                var currentUser = await _userManager.GetCurrentUser(cancellationToken);

                var musicTracks = await _musicTrackCreator.CreateMusicTracks(new MusicTrackCreatorData
                {
                    Tracks = newTracks,
                    MaxIndex = maxIndex,
                    BasePath = basePath,
                    MusicTrackTypeId = musicTrackTypeId,
                    GenreId = command.GenreId,
                    UserId = currentUser.Id,
                });

                _context.MusicTracks.AddRange(musicTracks);
                await _context.SaveChangesAsync(cancellationToken);

                return Unit.Value;
                //             Асинхронный метод, который исполняет логику команды:

                // Получает базовый путь к файлам музыки из конфигурации.
                // Считывает все файлы формата .mp3 из этого каталога и подкаталогов.
                // Из базы данных выбирает названия всех существующих музыкальных треков.
                // Вычисляет, какие из найденных файлов еще не добавлены в базу данных.
                // Для новых треков создает объекты SimpleDto, содержащие только названия треков.
                // Получает максимальный индекс существующих треков для определения порядка новых треков.
                // Вызывает сервис IMusicTrackCreator для создания записей новых треков в базе данных, передавая информацию о треках, их индексах, жанре и пользователе.
                // Добавляет новые треки в базу данных и сохраняет изменения.
            }
        }

        public class Command : IRequest<Unit>
        {
            public Guid GenreId { get; set; }
            //Этот класс содержит данные, необходимые для выполнения команды. В данном случае это идентификатор жанра музыкальных треков, которые будут импортированы.
        }
        //Этот процесс позволяет автоматически добавить в систему новые музыкальные треки, которые были размещены в соответствующей папке на сервере, но еще не занесены в базу данных, упрощая управление музыкальной коллекцией.
    }
}