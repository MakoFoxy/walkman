using System;
using System.IO;
using System.IO.Compression;
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

namespace Player.BusinessLogic.Features.Selections
{
    public class CreateFromArchives
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly IConfiguration _configuration;
            private readonly IMusicTrackCreator _musicTrackCreator;
            private readonly PlayerContext _context;

            public Handler(
                IConfiguration configuration,
                IMusicTrackCreator musicTrackCreator,
                PlayerContext context
                )
            {
                _configuration = configuration;
                _musicTrackCreator = musicTrackCreator;
                _context = context;
                //Конструктор принимает настройки конфигурации, сервис для создания музыкальных треков и контекст базы данных. Они используются для извлечения путей сохранения песен, создания музыкальных треков и доступа к данным соответственно.
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var basePath = _configuration.GetValue<string>("Player:SongsPath");

                var archives = Directory.GetFiles(basePath, "*.zip");

                var maxIndex = await _context.MusicTracks.OrderByDescending(mt => mt.Index).Select(mt => mt.Index)
                    .FirstOrDefaultAsync(cancellationToken);

                var musicTrackTypeId =
                    await _context.TrackTypes.Where(tt => tt.Code == TrackType.Music)
                        .Select(mt => mt.Id)
                        .SingleAsync(cancellationToken);

                var genreId = await _context.Genres.Select(g => g.Id).FirstAsync(cancellationToken);

                var currentUser = await _context.Users
                    .Where(u => u.Email == User.SystemUserEmail)
                    .SingleAsync(cancellationToken);

                foreach (var archive in archives)
                {
                    var selectionName = Path.GetFileNameWithoutExtension(archive);
                    ZipFile.ExtractToDirectory(archive, basePath);
                    var destinationDirectoryName = Directory.GetDirectories(basePath).Single();

                    var songFiles = Directory.GetFiles(destinationDirectoryName).OrderBy(sf => sf).ToList();
                    var songFileNames = songFiles.Select(Path.GetFileName).ToList();

                    foreach (var songFile in songFiles)
                    {
                        var resultDestination = Path.Combine(basePath, Path.GetFileName(songFile));

                        if (!File.Exists(resultDestination))
                        {
                            File.Move(songFile, resultDestination);
                        }
                    }

                    var newTracks = songFileNames.Select(f => new SimpleDto
                    {
                        Name = Path.GetFileName(f),
                    }).ToList();

                    var musicTracks = await _musicTrackCreator.CreateMusicTracks(new MusicTrackCreatorData
                    {
                        BasePath = basePath,
                        GenreId = genreId,
                        MaxIndex = maxIndex,
                        UserId = currentUser.Id,
                        MusicTrackTypeId = musicTrackTypeId,
                        Tracks = newTracks,
                    });

                    _context.MusicTracks.AddRange(musicTracks);
                    var selection = new Selection
                    {
                        Name = selectionName,
                        DateBegin = DateTimeOffset.Now.AddDays(-1),
                        IsPublic = true,
                    };
                    var index = 0;
                    selection.MusicTracks = musicTracks.Select(mt => new MusicTrackSelection
                    {
                        Index = index++,
                        Selection = selection,
                        MusicTrack = mt,
                    }).ToList();
                    _context.Selections.Add(selection);
                    Directory.Delete(destinationDirectoryName, true);
                }

                await _context.SaveChangesAsync(cancellationToken);
                return Unit.Value;

                //Асинхронный метод обработки команды, который выполняет следующие действия:

                // Читает базовый путь для сохранения песен из конфигурации.
                // Ищет все архивные файлы в этом каталоге.
                // Для каждого архива:
                //     Извлекает название подборки из названия архива.
                //     Распаковывает архив в указанный каталог.
                //     Определяет максимальный индекс среди уже существующих треков для правильного порядка следования.
                //     Определяет ID жанра и типа трека, используемых по умолчанию.
                //     Перемещает распакованные файлы в базовый каталог.
                //     Создает музыкальные треки из распакованных файлов.
                //     Собирает новые треки в подборку и связывает их с созданной подборкой.
                //     Добавляет созданную подборку в базу данных.
                // Удаляет временные каталоги после обработки.
                // Сохраняет изменения в базе данных.
            }
        }

        public class Command : IRequest<Unit>
        {
            //Это простой класс команды, который не содержит дополнительных данных и используется для сигнализации о необходимости выполнения операции создания подборок из архивов.
        }
    }
    //Особенности реализации:

    //     Предполагается, что каждый архив содержит коллекцию музыкальных треков, предназначенных для одной подборки.
    //     Метод обрабатывает все архивы, найденные в указанном каталоге, что позволяет автоматизировать процесс создания музыкальных подборок.
    //     Создание музыкальных треков и подборок в базе данных происходит в соответствии с предоставленными данными, что требует правильной организации файловой структуры и надежного механизма сохранения данных.

    // Этот код обеспечивает автоматическое создание подборок на основе музыкального контента, хранящегося в архивных файлах, что может значительно упростить и ускорить процесс пополнения медиатеки.
}