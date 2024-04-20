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
            private readonly IConfiguration _configuration; //В классе используется зависимость IConfiguration, которая инжектируется через конструктор. _configuration позволяет доступ к конфигурационным данным приложения.

            public PreProcessor(IConfiguration configuration)
            {
                _configuration = configuration; //В конструкторе _configuration присваивается полученный экземпляр конфигурации.
            }

            public async Task Process(Command command, CancellationToken cancellationToken)
            { //Этот метод асинхронный, что позволяет выполнять операции ввода-вывода без блокировки основного потока выполнения. Метод принимает параметр command, который содержит потоки музыкальных файлов для загрузки, и cancellationToken для управления отменой операции.
                var basePath = _configuration.GetValue<string>("Player:SongsPath");
                SemaphoreSlim streamLock = new SemaphoreSlim(1, 1);  // Создаем семафор для асинхронной блокировки

                foreach (var musicFile in command.SongStreams)
                {
                    await streamLock.WaitAsync();  // Блокируем поток до получения доступа
                    var fullPath = Path.Combine(basePath, musicFile.Name);
                    if (File.Exists(fullPath))
                    {
                        streamLock.Release();  // Освобождаем блокировку если файл существует
                        continue;
                    }

                    try
                    {
                        using (var buffer = new MemoryStream())
                        {
                            await musicFile.Stream.CopyToAsync(buffer, cancellationToken);  // Копирование в буфер
                            buffer.Seek(0, SeekOrigin.Begin);  // Переходим в начало потока буфера

                            using (var targetStream = File.Create(fullPath))  // Создаем файл
                            {
                                await buffer.CopyToAsync(targetStream, cancellationToken);  // Копирование буфера в файл
                            }
                            Console.WriteLine($"File {musicFile.Name} processed and saved successfully.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error processing file {musicFile.Name}: {ex.Message}");
                        Console.WriteLine($"Exception type: {ex.GetType()}");
                        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                    }
                    finally
                    {
                        streamLock.Release();  // Освобождаем блокировку после обработки файла
                    }
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
                var basePath = _configuration.GetValue<string>("Player:SongsPath"); //    basePath получает путь к директории, где будут сохраняться музыкальные файлы, из конфигурации приложения.

                var maxIndex = await _context.MusicTracks.OrderByDescending(mt => mt.Index).Select(mt => mt.Index)
                    .FirstOrDefaultAsync(cancellationToken); //    Из базы данных извлекается максимальный индекс существующих музыкальных треков для определения индекса новых треков.
                var musicTrackTypeId =
                    await _context.TrackTypes.Where(tt => tt.Code == TrackType.Music)
                        .Select(tt => tt.Id)
                        .SingleAsync(cancellationToken); //    Получение идентификатора типа трека, который соответствует коду 'Music', для правильного категоризирования добавляемых треков.
                var currentUser = await _userManager.GetCurrentUser(cancellationToken); //    Используя менеджер пользователей (_userManager), извлекается информация о текущем пользователе, который выполняет операцию.

                var genreId = command.SongStreams.First().Genre.Id; //    Из первого элемента в списке потоков музыкальных файлов (SongStreams) извлекается Id жанра.

                var musicTracks = command.SongStreams
                    .Select(ss => new SimpleDto //    Создание списка объектов SimpleDto, который включает идентификатор и путь к файлу для каждого музыкального трека из команды.
                    {
                        Id = ss.MusicTrackId ?? Guid.Empty,
                        Name = Path.Combine(basePath, ss.Name),
                    })
                    .ToList();
                var musicTracksForInsert = await _musicTrackCreator.CreateMusicTracks(new MusicTrackCreatorData
                { //    Вызывается сервис _musicTrackCreator для создания объектов музыкальных треков, передавая данные, включая полученные треки, максимальный индекс, базовый путь, тип трека, жанр и идентификатор пользователя. 
                    Tracks = musicTracks, //    Для каждого трека из списка Tracks создается новый объект MusicTrack. Поля этого объекта инициализируются соответствующими значениями из MusicTrackCreatorData.
                    MaxIndex = maxIndex, //Index: Устанавливается индекс для музыкального трека, начиная с maxIndex + 1 для первого трека, увеличиваясь для каждого следующего.
                    BasePath = basePath, //BasePath: Базовый путь, который может использоваться для указания местоположения файла музыкального трека.
                    MusicTrackTypeId = musicTrackTypeId, //MusicTrackTypeId: Тип трека, установленный как ID для музыкальных треков.
                    GenreId = genreId, //GenreId: Идентификатор жанра, который связывается с каждым треком, возможно, через отношение многие-ко-многим с использованием MusicTrackGenre.
                    UserId = currentUser.Id, //UserId: Идентификатор пользователя, который создает эти треки.

                    //В данном контексте MusicTrackCreatorData используется как DTO (Data Transfer Object), который предоставляет все необходимые данные для создания объектов MusicTrack. Этот объект передается в сервис _musicTrackCreator, который отвечает за создание и инициализацию экземпляров MusicTrack в базе данных.
                    //Когда MusicTrackCreatorData передается в _musicTrackCreator.CreateMusicTracks, этот метод обрабатывает следующие действия:     Для каждого трека из списка Tracks создается новый объект MusicTrack. Поля этого объекта инициализируются соответствующими значениями из MusicTrackCreatorData.
                    //Index: Устанавливается индекс для музыкального трека, начиная с maxIndex + 1 для первого трека, увеличиваясь для каждого следующего.     Genres, MusicHistories, Playlists, и Selections являются коллекциями, которые могут быть инициализированы или обновлены в этом процессе, в зависимости от бизнес-логики, определенной в _musicTrackCreator.
                });

                _context.MusicTracks.AddRange(musicTracksForInsert); //    В контекст базы данных добавляются созданные музыкальные треки.
                await _context.SaveChangesAsync(cancellationToken); //    Вызывается SaveChangesAsync для сохранения изменений в базе данных.

                return Unit.Value; //    Метод возвращает Unit.Value, что является стандартным возвращаемым значением для обработчиков MediatR, которые не возвращают результат.
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
