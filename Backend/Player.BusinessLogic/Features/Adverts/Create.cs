using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;
using Xabe.FFmpeg;

namespace Player.BusinessLogic.Features.Adverts
{
    public class Create
    {
        public class AdvertValidator : AbstractValidator<AdvertData>
        //Это класс валидатора для данных рекламного объявления (AdvertData). Он проверяет корректность дат (начало до окончания), непустоту названия объявления, что количество показов больше нуля, и наличие информации о клиенте. Валидация необходима для обеспечения целостности данных перед их сохранением в базу данных или выполнением других действий в системе.
        {
            public AdvertValidator()
            {
                RuleFor(advert => advert.DateBegin).LessThan(a => a.DateEnd)
                    .WithMessage("Дата начала должна быть меньше даты окончания");
                RuleFor(advert => advert.Name).NotEmpty()
                    .WithMessage("Название обязятельно");
                RuleFor(advert => advert.RepeatCount).NotEqual(0)
                    .WithMessage("Количество выходов должно быть больше 0");
                RuleFor(advert => advert.Client).NotNull()
                    .WithMessage("Необходимо выбрать клиента");
            }
        }

        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IUserManager _userManager;
            private readonly IPlaylistGenerator _playlistGenerator;
            private readonly ITrackNormalizer _trackNormalizer;
            private readonly IPlayerTaskCreator _taskCreator;

            private static readonly Regex EscapeRegex = new(":|<|>|\"|\\/|\\\\|\\||\\?|\\*", RegexOptions.Compiled);

            public Handler(PlayerContext context,
                IUserManager userManager,
                IPlaylistGenerator playlistGenerator,
                ITrackNormalizer trackNormalizer,
                IPlayerTaskCreator taskCreator)
            {
                _context = context;
                _userManager = userManager;
                _playlistGenerator = playlistGenerator;
                _trackNormalizer = trackNormalizer;
                _taskCreator = taskCreator;
            }

            //Этот класс обрабатывает команду создания нового рекламного объявления. Он принимает входные данные объявления и файл объявления, создает новый экземпляр Advert с уникальным идентификатором и очищенным от недопустимых символов названием. Далее определяется тип рекламного трека, загружающий пользователь и устанавливается валидность объявления.

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var advertData = command.Advert;
                var advertFile = command.AdvertFile;

                var advert = new Advert
                // Сохранение файла объявления: Объявление сохраняется в определенной директории, а путь к файлу сохраняется в экземпляре Advert.
                {
                    Id = Guid.NewGuid(),
                    Name = EscapeRegex.Replace(advertData.Name, ""),
                    Extension = Path.GetExtension(advertFile.FileName),
                    AdvertTypeId = advertData.AdvertType?.Id,
                    TrackType = await _context.TrackTypes.SingleAsync(tt => tt.Code == TrackType.Advert, cancellationToken),
                    Uploader = await _userManager.GetCurrentUser(cancellationToken),
                    IsValid = true,
                    CreateDate = DateTime.Now,
                    OrganizationId = advertData.Client.Id
                    //Этот блок создает новый экземпляр рекламного объявления со всеми необходимыми полями, такими как уникальный идентификатор, очищенное имя, тип файла, тип трека, информация о пользователе, который загрузил объявление, дата создания и идентификатор организации клиента.
                };

                var directoryPath = Path.Combine(Directory.GetCurrentDirectory(), "adverts");
                Directory.CreateDirectory(directoryPath);
                //Определяется путь к директории, где будут храниться файлы объявлений, и создается данная директория, если она еще не существует.

                var path = Path.Combine(directoryPath, advert.Id + Path.GetExtension(advertFile.FileName));
                //Файл рекламного объявления сохраняется на сервере под уникальным именем, которое сочетает идентификатор объявления и расширение файла.
                await using (var fileStream = new FileStream(path, FileMode.Create))
                {
                    await advertFile.CopyToAsync(fileStream, cancellationToken);
                }

                _trackNormalizer.Normalize(path);
                //Аудиотрек объявления нормализуется (например, устанавливается стандартная громкость) с использованием предоставленного сервиса.

                var hashString = new StringBuilder();

                using (var sha256 = SHA256.Create())
                {
                    await using var fileStream = new FileStream(path, FileMode.Open);
                    var hash = sha256.ComputeHash(fileStream);

                    foreach (var h in hash)
                    {
                        hashString.Append($"{h:x2}");
                    }
                }

                advert.Hash = hashString.ToString();
                //Для загруженного файла рекламы создается хеш-сумма с использованием алгоритма SHA256 для последующей верификации его подлинности и уникальности.
                var mediaInfo = await FFmpeg.GetMediaInfo(path);

                advert.Length = mediaInfo.Duration;
                advert.FilePath = path;
                //Используя библиотеку FFmpeg, извлекается информация о медиафайле, такая как его продолжительность. Эта информация сохраняется в свойствах экземпляра объявления.
                advert.AdLifetimes.Add(new AdLifetime
                {
                    Advert = advert,
                    DateBegin = advertData.DateBegin,
                    DateEnd = advertData.DateEnd
                });
                //Устанавливаются временные рамки действия рекламного объявления, создается и добавляется соответствующий объект AdLifetime.
                var objectIds = advertData.Objects.Select(o => o.Id);
                var objects = await _context.Objects.Where(o => objectIds.Contains(o.Id)).ToListAsync(cancellationToken);
                //Извлекается список идентификаторов объектов (плейлистов, каналов и т.д.), где должно быть показано объявление, и получаются соответствующие объекты из базы данных.
                if (advertData.PackageType == PackageType.Cheap)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    CreateGeneralPlaylistAdTimes(objects, advertData, advert);
                }
                //На основе типа пакета рекламы выбирается способ распределения объявления по времени и месту. Для экономных пакетов код еще не реализован (выбрасывается исключение).
                _context.Adverts.Add(advert);
                await _taskCreator.CreatePlaylistGenerationTask(advert.Id, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
                //Новое рекламное объявление добавляется в базу данных, создается задача на генерацию плейлистов с новым объявлением, и все изменения сохраняются.
                return Unit.Value;
                //Операция завершается успешно, возвращается специальное значение Unit.Value, представляющее отсутствие конкретного результата в контексте MediatR.
            }

            private void CreateCheapPlaylistAdTimes(List<CheapPlaylistTemplate> cheapPlaylistTemplates, AdvertData advertData, Advert advert)
            //Этот метод предназначен для распределения рекламных времен для "экономных" рекламных пакетов.
            {
                //TODO Добавить проверку cheapPlaylistTemplates.Max(cpt => cpt.Index)-cheapPlaylistTemplates.Min(cpt => cpt.Index)  == dateEnd - dateBegin
                var index = cheapPlaylistTemplates.Min(cpt => cpt.Index);

                for (var i = advertData.DateBegin; i <= advertData.DateEnd; i = i.AddDays(1))
                {
                    advert.AdTimes.Add(new AdTime
                    {
                        Advert = advert,
                        Object = cheapPlaylistTemplates.Single(pt => pt.Index == index).ObjectInfo,
                        PlayDate = i,
                        RepeatCount = advertData.RepeatCount
                    });
                    index++;
                }
            }

            private static void CreateGeneralPlaylistAdTimes(IEnumerable<ObjectInfo> objects, AdvertData advertData, Advert advert)
            {
                //Этот метод используется для создания временных слотов для стандартных рекламных пакетов.
                foreach (var objectInfo in objects)
                {
                    for (var i = advertData.DateBegin; i <= advertData.DateEnd; i = i.AddDays(1))
                    {
                        advert.AdTimes.Add(new AdTime
                        {
                            Advert = advert,
                            Object = objectInfo,
                            PlayDate = i,
                            RepeatCount = advertData.RepeatCount
                        });
                    }
                }
            }
        }

        public class Command : IRequest<Unit>
        {
            public AdvertData Advert { get; set; }
            public IFormFile AdvertFile { get; set; }
        }

        public class AdvertData : SimpleDto
        {
            public SimpleDto AdvertType { get; set; }
            public DateTime DateBegin { get; set; }
            public DateTime DateEnd { get; set; }
            public ICollection<SimpleDto> Objects { get; set; } = new List<SimpleDto>();
            public int RepeatCount { get; set; }
            public SimpleDto Client { get; set; }
            public PackageType PackageType { get; set; }
        }

        public enum PackageType
        {
            None,
            Regular,
            Cheap
        }
    }
}