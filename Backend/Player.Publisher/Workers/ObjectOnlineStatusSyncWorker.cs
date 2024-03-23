using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.BusinessLogic.Hubs;
using Player.DataAccess;
using Player.Domain;

namespace Player.Publisher.Workers
{
    public class ObjectOnlineStatusSyncWorker : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        //IServiceProvider serviceProvider: Используется для создания областей сервисов, чтобы получить доступ к зарегистрированным сервисам, таким как контекст базы данных.
        private readonly ILogger<ObjectOnlineStatusSyncWorker> _logger;
        //ILogger<ObjectOnlineStatusSyncWorker> logger: Предоставляет механизм логирования для записи информации о выполнении и ошибках.
        private readonly IWebHostEnvironment _env;
        //ILogger<ObjectOnlineStatusSyncWorker> logger: Предоставляет механизм логирования для записи информации о выполнении и ошибках.

        public ObjectOnlineStatusSyncWorker(
            IServiceProvider serviceProvider,
            ILogger<ObjectOnlineStatusSyncWorker> logger,
            IWebHostEnvironment env)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _env = env;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_env.IsDevelopment())
            {
                return;
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                await DoWork(stoppingToken);
            }
            //Этот метод переопределяет базовую реализацию BackgroundService. В нем:

            // Проверяется, находится ли приложение в режиме разработки. Если да, то служба завершает работу.
            // В противном случае начинается бесконечный цикл, который каждую минуту вызывает метод DoWork, передавая ему токен отмены.
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using var provider = _serviceProvider.CreateScope();

            var context = provider.ServiceProvider.GetRequiredService<PlayerContext>();

            try
            {
                await context.BeginTransactionAsync(stoppingToken);
                //    Начинает новую транзакцию базы данных. Это позволяет группировать несколько операций с базой данных в одну единую транзакцию, чтобы все изменения были применены вместе или ни одно (атомарность).
                var onlineObjects = await context.Objects
                    .Where(o => o.IsOnline)
                    .Select(o => o.Id)
                    .ToListAsync(stoppingToken);
                //    Получает список идентификаторов объектов из базы данных, которые в настоящее время помечены как онлайн. Это делается путем выполнения асинхронного запроса к базе данных с использованием LINQ.
                var onlineClientIds = PlayerClientHub.ConnectedClients.Select(cc => cc.Id).ToList();
                //    Получает список идентификаторов клиентов, которые в настоящее время подключены к хабу SignalR (PlayerClientHub). Это позволяет сравнить, какие клиенты онлайн с точки зрения хаба, и отличаются ли они от тех, что хранятся в базе данных.
                _logger.LogTrace("Online objects in database {@OnlineObjects}", onlineObjects);
                _logger.LogTrace("Online objects in memory {@OnlineObjects}", onlineClientIds);
                //    Записывает в лог список объектов, которые в базе данных помечены как онлайн. Используется для отладки и мониторинга.
                if (onlineObjects.Intersect(onlineClientIds).Count() == onlineObjects.Count)
                {
                    return;
                }
                //    Проверяет, совпадает ли список онлайн-объектов из базы данных с активными подключениями в хабе. Если все объекты, которые должны быть онлайн, уже отмечены онлайн и количество совпадает, дальнейшие действия не требуются.
                _logger.LogWarning("Need object online status sync");
                //    Если списки не совпадают, выводит предупреждение, что требуется синхронизация статусов онлайн-объектов.
                var tableName = context.GetTableName<ObjectInfo>();
                //    Получает имя таблицы, соответствующей объектам в базе данных.
                var isOnlineColumnName = context.GetColumnName<ObjectInfo>(info => info.IsOnline);
                //    Получает имя столбца, соответствующего свойству IsOnline в объектах.
                var idColumnName = context.GetColumnName<ObjectInfo>(info => info.Id);
                //    Получает имя столбца для идентификатора объектов.
                await context.Database.ExecuteSqlRawAsync($"UPDATE \"{tableName}\" set \"{isOnlineColumnName}\" = false");
                //    Выполняет SQL-запрос, который устанавливает статус всех объектов в базе данных как оффлайн.
                await context.Database.ExecuteSqlRawAsync(
                    $"UPDATE \"{tableName}\" set \"{isOnlineColumnName}\" = true WHERE \"{idColumnName}\" = ANY (@p0)",
                    onlineClientIds);
                //    Выполняет SQL-запрос, который обновляет статус объектов на онлайн для тех, чьи идентификаторы совпадают с идентификаторами подключенных клиентов.
                await context.SaveChangesAsync(stoppingToken);
                await context.CommitTransactionAsync(stoppingToken);
                //    Подтверждает транзакцию, фиксируя все изменения в базе данных.
                //             Здесь происходит основная работа службы:

                // Создается область сервисов для изоляции зависимостей.
                // Из контейнера сервисов извлекается контекст базы данных PlayerContext.
                // Внутри блока try-catch:
                //     Начинается новая транзакция.
                //     Из базы данных извлекается список ID объектов, которые в данный момент онлайн.
                //     Из PlayerClientHub (статического списка) извлекается список ID подключенных клиентов.
                //     Записываются логи о найденных онлайн объектах.
                //     Если пересечение списков из базы данных и хаба не соответствует полному списку из базы, выводится предупреждение о необходимости синхронизации.
                //     Обновляется статус онлайн всех объектов в базе данных: сначала все объекты помечаются как оффлайн, затем статус обновляется на онлайн для тех, что есть в списке подключенных клиентов.
                //     Сохраняются изменения и подтверждается транзакция.
                // В случае возникновения исключения, логируется ошибка, и транзакция откатывается.
            }
            catch (Exception e)
            {
                _logger.LogError(e, "");
                await context.RollbackTransactionAsync(stoppingToken);
            }
        }
    }
    //Эта служба полезна в ситуациях, где важно поддерживать актуальный статус онлайн для объектов, например, для управления медиа-устройствами или иными ресурсами, подключенными к вашему приложению.
    //Код выполняет синхронизацию между фактическим состоянием подключенных клиентов (которые активны в PlayerClientHub) и записями в базе данных (которые помечены как онлайн). Если список онлайн клиентов, хранящихся в памяти, и список онлайн объектов в базе данных не совпадают, код обновляет статусы в базе данных, чтобы они отражали актуальное состояние. Это обеспечивает, что информация об онлайн-статусе объектов в базе данных всегда актуальна и соответствует тому, кто действительно подключен к системе через SignalR хаб.
}
//Да, именно так. Этот фоновый процесс (worker) проверяет и синхронизирует два списка:

//     Список онлайн-клиентов в клиентской части (памяти): Это список идентификаторов клиентов, которые в данный момент подключены к вашему серверу через SignalR хаб (в вашем случае через PlayerClientHub). Этот список отражает текущее состояние подключений в реальном времени.

//     Список онлайн-объектов из базы данных: Это список объектов (например, пользователей или устройств), которые в базе данных помечены как онлайн. Это состояние может быть устаревшим, если объекты были отключены, но информация в базе данных не обновилась.

// Фоновый сервис выполняет следующие действия:

//     Сравнивает эти два списка. Если они полностью совпадают, то дополнительные действия не требуются – это означает, что текущий онлайн-статус объектов в базе данных актуален.

//     Если списки не совпадают (например, некоторые объекты, которые должны быть онлайн, отсутствуют в списке онлайн-объектов из базы данных, или наоборот), фоновый сервис обновляет записи в базе данных, чтобы они соответствовали фактическому онлайн-статусу клиентов в хабе SignalR.

// Это обеспечивает, что информация об онлайн-статусе в базе данных всегда точно отражает реальное состояние системы, что особенно важно для функций, зависящих от актуальности данных о статусе подключения, например, для отображения списка активных пользователей или для управления устройствами в реальном времени.
