using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Player.BusinessLogic.Features.Objects.Models;
using Player.DataAccess;
using Player.Domain;
using Player.Helpers.ApiInterfaces.PublisherApiInterfaces;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Objects
{
    public class Edit
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly IObjectApi _objectApi;
            private readonly ILogger<Handler> _logger;
            private readonly IHttpContextAccessor _httpContextAccessor;
            private readonly IPlayerTaskCreator _playerTaskCreator;

            public Handler(PlayerContext context,
                IObjectApi objectApi,
                ILogger<Handler> logger,
                IHttpContextAccessor httpContextAccessor,
                IPlayerTaskCreator playerTaskCreator)
            {
                _context = context;
                _objectApi = objectApi;
                _logger = logger;
                _httpContextAccessor = httpContextAccessor;
                _playerTaskCreator = playerTaskCreator;
                //Конструктор инициализирует зависимости: контекст базы данных, API для взаимодействия с объектами, логгер, аксессор для доступа к HTTP-контексту и создателя задач для работы с плейлистами.
            }

            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                var objectInfoModel = request.Model;
                _logger.LogTrace("Object changing {ObjectId}", objectInfoModel.Id);
                var objectInfo = await _context.Objects
                    .Include(o => o.Selections)
                    .ThenInclude(s => s.Selection)
                    .SingleAsync(o => o.Id == objectInfoModel.Id, cancellationToken);
                var activityType = await _context.ActivityTypes
                    .SingleAsync(at => at.Id == objectInfoModel.ActivityType.Id, cancellationToken);
                var workTimeChanged = WorkTimeChanged(objectInfo, objectInfoModel);
                var playlistConfigurationChanged = PlaylistConfigurationChanged(objectInfo, objectInfoModel);

                objectInfo.BeginTime = objectInfoModel.BeginTime;
                objectInfo.EndTime = objectInfoModel.EndTime;
                objectInfo.Attendance = objectInfoModel.Attendance;
                objectInfo.ActualAddress = objectInfoModel.ActualAddress;
                objectInfo.LegalAddress = objectInfoModel.LegalAddress;
                objectInfo.ActivityType = activityType;
                objectInfo.Name = objectInfoModel.Name;
                objectInfo.Area = objectInfoModel.Area;
                objectInfo.RentersCount = objectInfoModel.RentersCount;
                objectInfo.Bin = objectInfoModel.Bin;
                objectInfo.CityId = objectInfoModel.City.Id;
                objectInfo.Priority = objectInfoModel.Priority;
                objectInfo.ResponsiblePersonOne = new ObjectInfo.ResponsiblePerson
                {
                    ComplexName = objectInfoModel.ResponsiblePersonOne.ComplexName,
                    Email = objectInfoModel.ResponsiblePersonOne.Email,
                    Phone = objectInfoModel.ResponsiblePersonOne.Phone,
                };
                objectInfo.ResponsiblePersonTwo = new ObjectInfo.ResponsiblePerson
                {
                    ComplexName = objectInfoModel.ResponsiblePersonTwo.ComplexName,
                    Email = objectInfoModel.ResponsiblePersonTwo.Email,
                    Phone = objectInfoModel.ResponsiblePersonTwo.Phone,
                };
                objectInfo.FreeDays = objectInfoModel.FreeDays;
                objectInfo.Selections = objectInfoModel.Selections.Select(s => new ObjectSelection
                {
                    Object = objectInfo,
                    SelectionId = s.Id
                }).ToList();
                objectInfo.MaxAdvertBlockInSeconds = objectInfoModel.MaxAdvertBlockInSeconds;

                await _context.SaveChangesAsync(cancellationToken);
                await _context.CommitTransactionAsync(cancellationToken);
                await _context.BeginTransactionAsync(cancellationToken);

                _logger.LogTrace("Object {ObjectId} changed", objectInfo.Id);

                try
                {
                    await _objectApi.ObjectInfoChanged(objectInfo.Id,
                        _httpContextAccessor.HttpContext.Request.Headers["Authorization"]);

                    //                         Отправка уведомления: Если информация об объекте успешно обновлена, вызывает метод ObjectInfoChanged у _objectApi для уведомления внешних систем.
                    // Проверка изменений: Определяет, были ли изменения в расписании работы или конфигурации плейлиста, что может потребовать дополнительных действий, таких как перегенерация плейлистов.
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Service unavailable"); //    В случае ошибок при вызове внешнего API, они логируются как "Service unavailable".
                }

                if (!workTimeChanged && !playlistConfigurationChanged)
                {
                    return Unit.Value; //    Метод возвращает Unit.Value, что является стандартным способом указать на успешное завершение операции в CQRS (Command Query Responsibility Segregation) паттернах, где не требуется возвращать результат.
                    //WorkTimeChanged и PlaylistConfigurationChanged проверяют, изменились ли определённые аспекты объекта, что может потребовать дополнительных действий, таких как обновление плейлистов.
                }

                _logger.LogTrace("Object {ObjectId} work time or playlist configuration changed", objectInfo.Id);
                var advertIds = await _context.Playlists //Извлечение идентификаторов реклам: Используется LINQ запрос для получения списка уникальных идентификаторов рекламных блоков из плейлистов, которые:
                    .Where(p => p.Object == objectInfo && p.PlayingDate > DateTime.Today) //Связаны с определенным объектом (objectInfo). Имеют дату воспроизведения больше текущей даты (DateTime.Today).
                    .SelectMany(p => p.Aderts).Select(ap => ap.Advert.Id).Distinct() //Для этого код фильтрует плейлисты по объекту и дате, извлекает рекламные блоки (Adverts), получает их идентификаторы и использует Distinct() для удаления дубликатов, прежде чем преобразовать итоговый список в список с помощью ToListAsync().
                    .ToListAsync(cancellationToken);

                foreach (var advertId in advertIds)
                {
                    await _playerTaskCreator.AddPlaylistGenerationTask(advertId, cancellationToken); //Создание задач генерации плейлистов: Для каждого уникального идентификатора рекламы вызывается метод AddPlaylistGenerationTask, который, вероятно, создает задачу для генерации нового плейлиста с учетом изменений, произошедших в объекте. Это может включать пересчет временных слотов или обновление конфигурации плейлиста.
                }

                await _context.SaveChangesAsync(cancellationToken); //Сохранение изменений в базе данных: После выполнения всех изменений вызывается SaveChangesAsync() для сохранения всех изменений в базе данных.
                return Unit.Value; //Возвращение результата: Возвращается значение Unit.Value, что является способом в C# указать, что метод возвращает пустой результат (void), но при этом поддерживает асинхронные операции.
            }

            private bool WorkTimeChanged(ObjectInfo objectInfo, ObjectInfoModel objectInfoModel)
            {//Эта функция проверяет, изменились ли времена начала или окончания работы объекта, а также список выходных дней. Она возвращает true, если были обнаружены изменения в следующих полях:     BeginTime: Время начала работы объекта.
             // EndTime: Время окончания работы объекта.
             // FreeDays: Список дней, когда объект не работает.
                return objectInfo.BeginTime != objectInfoModel.BeginTime ||
                       objectInfo.EndTime != objectInfoModel.EndTime ||
                       !objectInfo.FreeDays.OrderBy(fd => fd)
                           .SequenceEqual(objectInfoModel.FreeDays.OrderBy(fd => fd));
                //Функция сравнивает текущие значения в базе данных (objectInfo) с новыми значениями, предоставленными пользователем (objectInfoModel). Она также использует метод SequenceEqual после сортировки списка дней для точного сравнения порядка элементов, чтобы проверить, не изменился ли порядок или состав дней, в которые объект не работает.
            }

            private bool PlaylistConfigurationChanged(ObjectInfo objectInfo, ObjectInfoModel objectInfoModel)
            {
                return objectInfo.MaxAdvertBlockInSeconds != objectInfoModel.MaxAdvertBlockInSeconds;
                //Эта функция проверяет, были ли изменения в настройках конфигурации плейлиста, точнее в максимальной продолжительности рекламного блока в секундах (MaxAdvertBlockInSeconds). Она возвращает true, если значение максимальной продолжительности блока рекламы изменилось между текущим значением в базе данных и новым значением, предоставленным в objectInfoModel.
            }

            //Основная логика обработки команды редактирования объекта. Метод асинхронно обновляет данные объекта в базе данных и оповещает соответствующие сервисы или компоненты системы об изменениях.

            // Внутри метода происходит:

            //     Получение и обновление информации об объекте.
            //     Проверка, изменилось ли рабочее время или конфигурация плейлиста.
            //     Сохранение изменений в базе данных.
            //     Вызов внешнего API для оповещения о изменениях в объекте.
            //     При необходимости, создание задач на генерацию новых плейлистов.

            // Функции WorkTimeChanged и PlaylistConfigurationChanged:

            // Эти функции определяют, изменились ли рабочие часы или настройки плейлиста соответственно. Это необходимо для определения необходимости выполнения дополнительных действий, таких как перегенерация плейлистов.
        }

        public class Command : IRequest<Unit>
        {
            public ObjectInfoModel Model { get; set; }
            //Command содержит всю необходимую информацию для обновления объекта, инкапсулированную в модели ObjectInfoModel.
            //Когда пользователь или система инициируют редактирование объекта, создается и отправляется команда Command, которая обрабатывается экземпляром Handler. В процессе обработки проверяются изменения в объекте, обновляются данные в базе и, при необходимости, выполняются дополнительные действия, связанные с изменением рабочего времени или конфигурации плейлиста.

        }
    }
}