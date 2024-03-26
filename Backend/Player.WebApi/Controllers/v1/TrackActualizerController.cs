using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Player.BusinessLogic.Features.Songs;
using Player.DataAccess;
using Player.Domain;
using Player.DTOs;
using Player.Services;
using Xabe.FFmpeg;

namespace Player.WebApi.Controllers.v1
{
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class TrackActualizerController : ControllerBase
    {
        private readonly PlayerContext _context;
        private readonly IMediator _mediator;
        private readonly IConfiguration _configuration;

        public TrackActualizerController(PlayerContext context, IMediator mediator, IConfiguration configuration)
        {
            _context = context;
            _mediator = mediator;
            _configuration = configuration;
            //Принимает контекст базы данных PlayerContext, медиатор IMediator для CQRS-операций, и конфигурацию IConfiguration для доступа к настройкам приложения.
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] Guid genreId, CancellationToken cancellationToken)
        {
            await _mediator.Send(new ImportExisting.Command { GenreId = genreId }, cancellationToken);
            return Ok(); //Выполняет операцию импорта существующих треков по определенному жанру.
            //Принимает genreId и использует MediatR для отправки команды ImportExisting.Command, которая обрабатывается в соответствующем обработчике.
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SimpleDto genreModel)
        {
            var songsPath = _configuration.GetValue<string>("Player:SongsPath");

            var filePaths = Directory.GetFiles(songsPath).Select(s => (string)Path.GetFileName(s)).ToList();
            var musicTrackNames = await _context.MusicTracks.Select(mt => mt.FilePath).ToListAsync();

            var newTracks = filePaths.Where(fp => !musicTrackNames.Contains(fp)).ToList();

            var genre = await _context.Genres.FirstAsync(g => g.Id == genreModel.Id);

            var maxIndex = await _context.MusicTracks.OrderByDescending(mt => mt.Index).Select(mt => mt.Index).FirstOrDefaultAsync();
            var musicTrackType = await _context.TrackTypes.SingleAsync(tt => tt.Code == TrackType.Music);
            var systemUser = await _context.Users.SingleAsync(tt => tt.Email == Player.Domain.User.SystemUserEmail);

            using (var sha256 = SHA256.Create())
            {
                foreach (var file in newTracks.Select(t => Path.Combine(songsPath, t)))
                {
                    var mediaInfo = await FFmpeg.GetMediaInfo(file);
                    var hashString = new StringBuilder();

                    using (var stream = System.IO.File.OpenRead(file))
                    {
                        var hash = sha256.ComputeHash(stream);

                        foreach (var h in hash)
                        {
                            hashString.Append($"{h:x2}");
                        }
                    }

                    var musicTrack = new MusicTrack
                    {
                        FilePath = Path.GetRelativePath(songsPath, file),
                        Index = ++maxIndex,
                        IsHit = false,
                        IsValid = true,
                        Length = mediaInfo.Duration,
                        Name = Path.GetFileNameWithoutExtension(file),
                        Extension = Path.GetExtension(file),
                        TrackType = musicTrackType,
                        Uploader = systemUser,
                        Hash = hashString.ToString()
                    };

                    musicTrack.Genres.Add(new MusicTrackGenre
                    {
                        Genre = genre,
                        MusicTrack = musicTrack,
                    });

                    _context.MusicTracks.Add(musicTrack);
                    //    Импортирует новые музыкальные треки из заданной директории, указанной в настройках Player:SongsPath.
                    // Определяет новые треки, сравнивая их с уже существующими в базе данных, и добавляет их с соответствующим жанром.
                    // Вычисляет хэш каждого трека для последующей идентификации и проверки на уникальность.
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("music-track")]
        public async Task ActualizeMusicTracks(CancellationToken cancellationToken)
        {
            var musicTracks = await _context.MusicTracks.Where(mt => mt.TrackType.Code == TrackType.Music && mt.Hash == null).ToListAsync(cancellationToken);

            using (var sha256 = SHA256.Create())
            {
                foreach (var musicTrack in musicTracks)
                {
                    var hash = sha256.ComputeHash(System.IO.File.OpenRead(musicTrack.FilePath));
                    var hashString = new StringBuilder();

                    foreach (var h in hash)
                    {
                        hashString.Append($"{h:x2}");
                    }
                    musicTrack.Hash = hashString.ToString();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            //    Обновляет информацию для всех музыкальных треков, для которых не установлен хэш, вычисляя новый хэш и сохраняя его в базу данных.
        }

        [HttpPut("advert")]
        public async Task ActualizeAdverts(CancellationToken cancellationToken)
        {
            var adverts = await _context.Adverts.ToListAsync(cancellationToken);

            using (var sha256 = SHA256.Create())
            {
                foreach (var advert in adverts)
                {
                    var hash = sha256.ComputeHash(System.IO.File.OpenRead(advert.FilePath));
                    var hashString = new StringBuilder();

                    foreach (var h in hash)
                    {
                        hashString.Append($"{h:x2}");
                    }
                    advert.Hash = hashString.ToString();
                }
            }

            await _context.SaveChangesAsync(cancellationToken);
            //    Похож на ActualizeMusicTracks, но работает с рекламными треками.
        }

        [HttpPut("normalize")]
        public async Task<IActionResult> NormalizeTracks([FromServices] TrackNormalizer normalizer)
        {
            var adverts = await _context.Adverts.ToListAsync();
            var musicTracks = await _context.MusicTracks.Where(mt => mt.TrackType.Code == TrackType.Music && mt.Hash == null).ToListAsync();

            foreach (var advert in adverts)
            {
                normalizer.Normalize(advert.FilePath);
            }

            foreach (var musicTrack in musicTracks)
            {
                normalizer.Normalize(musicTrack.FilePath);
            }

            await ActualizeAdverts(CancellationToken.None);
            await ActualizeMusicTracks(CancellationToken.None);

            return Ok();
            //             Использует сервис TrackNormalizer для нормализации аудио треков и рекламы. Это может включать коррекцию громкости или другие аудио-процессинг операции.
            // После нормализации треков снова вызывает ActualizeAdverts и ActualizeMusicTracks для обновления хэшей нормализованных файлов.
        }
    }
}

// Безопасность: В контроллере не используются атрибуты [Authorize] для методов Post, ActualizeMusicTracks, ActualizeAdverts и NormalizeTracks, что потенциально может позволить нежелательные манипуляции с треками. В реальных приложениях важно ограничить доступ к таким операциям.

// Эффективность: При большом количестве треков операции вычисления хэшей и нормализации могут быть довольно ресурсоемкими. Рекомендуется обеспечить адекватное тестирование и, возможно, реализовать эти операции асинхронно или в фоновом режиме.

// Воспроизведение: Эти операции влияют на важные аспекты управления данными. Перед внедрением в продакшн убедитесь, что у вас есть резервные копии и стратегия восстановления на случай ошибок.