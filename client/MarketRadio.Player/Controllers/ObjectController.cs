using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services;
using MarketRadio.Player.Services.Http;
using MarketRadio.Player.Services.LiveConnection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Player.ClientIntegration.Base;
using Object = MarketRadio.Player.DataAccess.Domain.Object;

namespace MarketRadio.Player.Controllers
{
    [ApiController] //[ApiController]: Атрибут, указывающий, что этот класс является контроллером API в ASP.NET Core.
    [Route("api/[controller]")] //[Route("api/[controller]")]: Устанавливает маршрут для API. [controller] будет заменено на имя контроллера, что делает базовый URL для этих эндпоинтов api/object.
    public class ObjectController : ControllerBase //public class ObjectController : ControllerBase: Объявляет класс ObjectController, наследующий функциональность от ControllerBase.
    {
        private readonly IObjectApi _objectApi;
        private readonly PlayerContext _context;
        private readonly ILogger<ObjectController> _logger;
        private readonly ServerLiveConnection _serverLiveConnection;
        private readonly PlayerStateManager _stateManager;
        private readonly ObjectSettingsService _objectSettingsService;

        public ObjectController(IObjectApi objectApi,
            PlayerContext context,
            ILogger<ObjectController> logger,
            ServerLiveConnection serverLiveConnection,
            PlayerStateManager stateManager,
            ObjectSettingsService objectSettingsService)
        {
            _objectApi = objectApi;
            _context = context;
            _logger = logger;
            _serverLiveConnection = serverLiveConnection;
            _stateManager = stateManager;
            _objectSettingsService = objectSettingsService;
            //    public ObjectController(...){...}: Конструктор класса, который принимает сервисы и контексты, используемые в методах контроллера, и присваивает их локальным переменным.
        }

        [HttpGet("all")] //[HttpGet("all")]: Атрибут, определяющий метод, обрабатывающий HTTP GET запросы на api/object/all.
        public async Task<ICollection<SimpleModel>> GetAll()
        {
            var token = await _context.UserSettings.Where(us => us.Key == UserSetting.Token)
                .Select(us => us.Value)
                .SingleAsync(); //Получение токена из настроек пользователя, выполнение запроса к внешнему API для получения всех объектов.
            var objects = await _objectApi.GetAll($"Bearer {token}");
            await using var tx = await _context.Database.BeginTransactionAsync(); //Начало транзакции БД
            try
            {
                await _context.Objects.ExecuteDeleteAsync(); //удаление существующих объектов
                _context.Objects.AddRange(objects.Objects.Select(o => new Object
                {
                    Id = o.Id,
                    Name = o.Name //добавление новых объектов из внешнего запроса
                }));
                await _context.SaveChangesAsync(); // затем сохранение изменений
                await tx.CommitAsync();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                await tx.RollbackAsync(); //Логирование ошибок и откат транзакции в случае исключения.
            }

            return objects.Objects;
        }

        [HttpGet("all/local")] //[HttpGet("all/local")]: Обрабатывает GET запросы на api/object/all/local.
        public async Task<List<Object>> GetAllLocal()
        {
            return await _context.Objects.AsNoTracking().ToListAsync();
            //Возвращает список всех объектов из локальной базы данных, используя AsNoTracking для повышения производительности, поскольку изменения сохраняться не будут.
        }

        [HttpPost("{objectId}")] //[HttpPost("{objectId}")]: Обрабатывает POST запросы на api/object/{objectId}, где {objectId} — это параметр маршрута.
        public async Task<ObjectInfo> SaveCurrentObject([FromRoute] Guid objectId)  //Получает полную информацию об объекте по его идентификатору из внешнего API.
        {
            var objectInfo = await _objectApi.GetFullInfo(objectId);
            var settingsRaw = await _objectApi.GetSettings(objectId);

            if (string.IsNullOrWhiteSpace(settingsRaw))
            {
                objectInfo.Settings = _objectSettingsService.FillSettingsIfNeeded(objectInfo);
            }
            else
            {
                objectInfo.Settings = JsonConvert.DeserializeObject<Settings>(settingsRaw)!;
            }
            //Запрашивает настройки для этого объекта. Если настройки не найдены (строка пуста), использует сервис для их создания. Если настройки найдены, десериализует их из строки в объект Settings.
            await using var tx = await _context.Database.BeginTransactionAsync(); //    await using var tx = await _context.Database.BeginTransactionAsync();: Запускает новую транзакцию базы данных. Использование await using гарантирует, что ресурсы транзакции будут корректно освобождены после завершения работы с ней.
            try
            { //    try { ... } catch (Exception e) { ... }: Окружает блок кода, чтобы отлавливать возникающие исключения. В случае ошибок выполнение переходит в блок catch.
                await _context.ObjectInfos.ExecuteDeleteAsync();
                _context.ObjectInfos.Add(objectInfo); //_context.ObjectInfos.Add(objectInfo);: Добавляет новую информацию об объекте (objectInfo) в контекст базы данных.
                await _context.SaveChangesAsync();//await _context.SaveChangesAsync();: Асинхронно сохраняет все изменения в базе данных.
                await tx.CommitAsync(); //    await tx.CommitAsync();: Если все операции в блоке try выполнены успешно, фиксирует (commit) транзакцию.

                var overrideObject = _stateManager.Object != null; //var overrideObject = _stateManager.Object != null;: Проверяет, существует ли уже текущий объект в менеджере состояний. Если да, переменная overrideObject становится true.

                _stateManager.Object = new ObjectInfoDto
                { // Создает и присваивает новый объект ObjectInfoDto в stateManager, используя данные из objectInfo. Это обновляет текущее состояние в плеере или системе.
                    Id = objectInfo.Id,
                    Bin = objectInfo.Bin,
                    Name = objectInfo.Name,
                    BeginTime = objectInfo.BeginTime,
                    EndTime = objectInfo.EndTime,
                    FreeDays = objectInfo.FreeDays,
                    Settings = new SettingsDto
                    {
                        MusicVolume = objectInfo.Settings!.MusicVolumeComputed,
                        SilentTime = objectInfo.Settings.SilentTime,
                        AdvertVolume = objectInfo.Settings.AdvertVolumeComputed,
                        IsOnTop = objectInfo.Settings.IsOnTop,
                    }
                };

                if (overrideObject)
                {
                    await _serverLiveConnection.ConnectToNewObject(objectId); //Если ранее был выбран другой объект (то есть overrideObject равно true), то происходит попытка асинхронного подключения к новому объекту через серверное соединение (_serverLiveConnection).
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, ""); //В случае возникновения исключения, информация об ошибке логируется.
                await tx.RollbackAsync(); //Транзакция откатывается (rollback), что гарантирует, что база данных останется в согласованном состоянии, несмотря на ошибку.
            }

            return objectInfo; // Возвращает информацию об объекте, который был обработан или добавлен.
        }
        //Этот код важен для обеспечения атомарности операций с базой данных (все изменения применяются вместе или не применяются вообще) и для управления состоянием медиаплеера или другого приложения, отслеживающего текущий активный объект.

        [HttpPost("settings")] //[HttpPost("settings")]: Этот метод обрабатывает POST запросы к api/object/settings. Он ожидает объект Settings в теле запроса.
        public async Task<Settings> SaveCurrentObjectSettings([FromBody] Settings settings)
        {
            var objectInfo = await _context.ObjectInfos.SingleAsync(); //Получает текущий объект информации из базы данных (SingleAsync бросит исключение, если в таблице более одной записи).

            await using var tx = await _context.Database.BeginTransactionAsync(); //Начинает транзакцию базы данных.
            try
            {
                objectInfo.Settings = settings;
                await _context.SaveChangesAsync(); //Присваивает новые настройки полученному объекту и сохраняет изменения (SaveChangesAsync).
                await tx.CommitAsync(); //Фиксирует транзакцию.
            }
            catch (Exception e)
            {
                _logger.LogError(e, ""); //В случае исключения логирует ошибку, откатывает транзакцию и перебрасывает исключение.
                await tx.RollbackAsync();
            }

            return settings; //Возвращает объект настроек, который был сохранен.
        }

        [HttpGet("current")] //[HttpGet("current")]: Этот метод обрабатывает GET запросы к api/object/current.
        public async Task<ObjectInfo?> GetCurrentObject()
        {
            var objectInfo = await _context.ObjectInfos.SingleOrDefaultAsync();

            if (objectInfo == null)
            {
                return null; //Получает текущую информацию об объекте из базы данных (если объекта нет, возвращает null).
            }

            _stateManager.Object = new ObjectInfoDto 
            {//Обновляет состояние объекта в StateManager, создавая и заполняя ObjectInfoDto данными из базы данных.
                Id = objectInfo.Id,
                Bin = objectInfo.Bin,
                Name = objectInfo.Name,
                BeginTime = objectInfo.BeginTime,
                EndTime = objectInfo.EndTime,
                FreeDays = objectInfo.FreeDays,
                Settings = new SettingsDto
                {
                    MusicVolume = objectInfo.Settings!.MusicVolumeComputed,
                    SilentTime = objectInfo.Settings.SilentTime,
                    AdvertVolume = objectInfo.Settings.AdvertVolumeComputed,
                    IsOnTop = objectInfo.Settings.IsOnTop,
                }
            };
            return objectInfo; //Возвращает найденный объект информации (ObjectInfo).
        }

        [HttpPost("{objectId}/connect")] //[HttpPost("{objectId}/connect")]: Обрабатывает POST запросы к api/object/{objectId}/connect, где {objectId} — это ID объекта, к которому нужно подключиться.
        public async Task<IActionResult> Connect([FromRoute] Guid objectId)
        {
            await _serverLiveConnection.Connect(objectId); //Использует сервис ServerLiveConnection для установления соединения с указанным объектом по его идентификатору.
            return Ok(); //Возвращает статус Ok, если подключение прошло успешно.
        }

        [HttpDelete] //[HttpDelete]: Этот метод обрабатывает DELETE запросы к api/object.
        public async Task<IActionResult> RemoveCurrentObject() //Получает текущую информацию об объекте из базы данных и удаляет её.
        {
            var objectInfo = await _context.ObjectInfos.SingleAsync(); 
            _context.Remove(objectInfo);
            await _context.SaveChangesAsync(); //Сохраняет изменения в базе данных.
            _stateManager.Object = null; //Очищает текущее состояние объекта в StateManager.
            return Ok(); //Возвращает статус Ok после успешного удаления объекта.
        }
    }
    //Каждый из этих методов выполняет конкретную функцию в рамках системы управления объектами: сохранение настроек, получение текущего объекта, установление соединения с объектом и удаление объекта. Эти операции являются ключевыми для администрирования и управления объектами в вашем приложении.
}