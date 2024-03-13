using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Player.ClientIntegration;
using Player.ClientIntegration.Object;
using Player.DataAccess;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Hubs
{
    public class PlayerClientHub : Hub // Класс, производный от Hub, который предоставляет функционал SignalR.
    //Hub в контексте SignalR — это основной компонент, который используется для управления коммуникацией между сервером и клиентами в реальном времени. 
    {
        // Зависимости, инжектируемые через конструктор.
        private readonly PlayerContext _context; // Контекст для доступа к базе данных.
        private readonly ITelegramMessageSender _telegramMessageSender; // Сервис для отправки сообщений в Telegram.
        private readonly ILogger<PlayerClientHub> _logger; // Логгер для регистрации событий.

        // Статические списки для отслеживания состояния подключенных клиентов.
        private static readonly List<OnlineClient> connectedClients = new();
        private static readonly List<MobileClient> mobileClients = new();

        // Для предоставления только для чтения списка подключенных клиентов.
        public static IReadOnlyList<OnlineClient> ConnectedClients => connectedClients;

        // Конструктор класса с инъекцией зависимостей.
        public PlayerClientHub(PlayerContext context, ITelegramMessageSender telegramMessageSender, ILogger<PlayerClientHub> logger)
        {
            _context = context;
            _telegramMessageSender = telegramMessageSender;
            _logger = logger;
        }

        // Обработчик событий при подключении нового клиента.
        public override async Task OnConnectedAsync()
        {
            if (IsMobileClient()) // Проверка, является ли клиент мобильным.
            {
                await MobileClientConnected(); // Обработка подключения мобильного клиента.
            }
            else
            {
                await ObjectConnected(); // Обработка подключения других типов клиентов.
            }
        }

        private async Task MobileClientConnected() // Логика подключения мобильного клиента.
        {
            var mobileClient = new MobileClient
            {
                UserId = Context.User.Identity.Name, // Идентификатор пользователя из контекста.
                ConnectionId = Context.ConnectionId, // Идентификатор подключения из контекста.
            };
            mobileClients.Add(mobileClient); // Добавление клиента в список.
            _logger.LogTrace("User {UserId} with version {Version} connected", mobileClient.UserId, Version()); // Логирование.
            await base.OnConnectedAsync(); // Вызов базового метода.
        }

        private async Task ObjectConnected() // Логика подключения не мобильного клиента.
        {
            var onlineClient = new OnlineClient
            {
                Id = ClientId(),
                Version = Version(),
                IpAddress = ClientIpAddress(),
            };
            connectedClients.Add(onlineClient); // Добавление клиента в список.
            _logger.LogTrace("Client {ClientId} with version {Version} connected", onlineClient.Id, onlineClient.Version); // Логирование.
            await Groups.AddToGroupAsync(Context.ConnectionId, onlineClient.Id.ToString()); // Добавление клиента в группу.
            await base.OnConnectedAsync(); // Вызов базового метода.

            // Обновление информации о клиенте в базе данных.
            var objectInfo = await _context.Objects.SingleAsync(o => o.Id == onlineClient.Id);
            objectInfo.IsOnline = true;
            objectInfo.LastOnlineTime = DateTime.Now;
            objectInfo.Version = onlineClient.Version;
            await _context.SaveChangesAsync(); // Сохранение изменений.
            await SendConnectionStatus(onlineClient.Id, true); // Отправка статуса подключения.
        }

        // Обработчик событий при отключении клиента.
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            if (IsMobileClient()) // Проверка, мобильный ли это клиент.
            {
                await MobileClientDisconnected(exception); // Обработка отключения мобильного клиента.
            }
            else
            {
                await ObjectDisconnected(exception); // Обработка отключения других типов клиентов.
            }
        }

        // Асинхронный метод, вызываемый при отключении мобильного клиента.
        private async Task MobileClientDisconnected(Exception exception)
        {
            // Создаем экземпляр MobileClient с информацией о текущем пользователе и его подключении.
            var mobileClient = new MobileClient
            {
                UserId = Context.User.Identity.Name, // Имя пользователя, полученное из контекста SignalR.
                ConnectionId = Context.ConnectionId, // ID подключения, также полученное из контекста SignalR.
            };

            // Удаляем этого мобильного клиента из списка подключенных мобильных клиентов.
            mobileClients.Remove(mobileClient);

            // Записываем в лог информацию об отключении клиента.
            // Заметьте, здесь ошибка в тексте: должно быть "disconnected" вместо "connected".
            _logger.LogTrace("User {UserId} with version {Version} disconnected", mobileClient.UserId, Version());

            // Вызываем базовый метод OnDisconnectedAsync, передавая исключение, если оно есть.
            await base.OnDisconnectedAsync(exception);
        }

        // Асинхронный метод, вызываемый при отключении объекта (не мобильного клиента).
        private async Task ObjectDisconnected(Exception exception)
        {
            // Создаем экземпляр OnlineClient с информацией о текущем подключении.
            var onlineClient = new OnlineClient
            {
                Id = ClientId(), // Получаем ID клиента из запроса.
                Version = Version(), // Получаем версию клиента из запроса.
                IpAddress = ClientIpAddress(), // Получаем IP-адрес клиента.
            };

            // Удаляем этот объект из списка подключенных клиентов.
            connectedClients.Remove(onlineClient);

            // Записываем в лог информацию об отключении клиента.
            _logger.LogTrace("Client {ClientId} with version {Version} disconnected", onlineClient.Id, onlineClient.Version);

            // Вызываем базовый метод OnDisconnectedAsync, передавая исключение.
            await base.OnDisconnectedAsync(exception);

            // Обновляем информацию о клиенте в базе данных.
            var objectInfo = await _context.Objects.SingleAsync(o => o.Id == onlineClient.Id);
            objectInfo.IsOnline = false; // Отмечаем, что клиент теперь не в сети.
            objectInfo.LastOnlineTime = DateTime.Now; // Записываем время последнего входа в сеть.
            await _context.SaveChangesAsync(); // Сохраняем изменения в базе данных.

            // Отправляем статус подключения другим клиентам.
            await SendConnectionStatus(onlineClient.Id, false);
        }

        // Асинхронный метод, вызываемый при начале воспроизведения плейлиста.
        public async Task PlaylistStarted()
        {
            var clientId = ClientId(); // Получаем ID текущего клиента.

            // Получаем информацию для связи между чатами и объектами для текущего подключения.
            var chatAndObjects = await GetChatsAndObjectForCurrentConnection(clientId);

            // Отправляем всем связанным чатам уведомление о начале воспроизведения плейлиста.
            await Task.WhenAll(chatAndObjects
                .Select(i =>
                    _telegramMessageSender.SendPlaylistStarted(i.ChatId, i.ObjectName, DateTime.Now))
                .ToList());
        }

        // Асинхронный метод, вызываемый при окончании воспроизведения плейлиста.
        public async Task PlaylistEnded()
        {
            var clientId = ClientId(); // Получаем ID текущего клиента.

            // Получаем информацию для связи между чатами и объектами для текущего подключения.
            var chatAndObjects = await GetChatsAndObjectForCurrentConnection(clientId);

            // Отправляем всем связанным чатам уведомление об окончании воспроизведения плейлиста.
            await Task.WhenAll(chatAndObjects
                .Select(i =>
                    _telegramMessageSender.SendPlaylistEnded(i.ChatId, i.ObjectName, DateTime.Now))
                .ToList());
        }


        // Метод для публикации изменения информации об объекте.
        public async Task PublishObjectInfoChanged(Guid id, CancellationToken cancellationToken)
        {
            // Получаем информацию об объекте по ID из базы данных.
            var objectInfo = await _context.Objects.Where(o => o.Id == id).SingleAsync(cancellationToken);

            // Отправляем обновленную информацию всем клиентам в группе, которая совпадает с ClientId текущего пользователя.
            await Clients.Groups(ClientId().ToString())
                .SendAsync(OnlineEvents.ObjectInfoReceived, JsonConvert.SerializeObject(objectInfo), cancellationToken);
            //Этот метод используется для оповещения всех клиентов в определенной группе об изменении информации об объекте. ClientId().ToString() определяет, какая группа клиентов получит сообщение, и обычно это идентификатор объекта или пользователя. Информация об объекте сериализуется в JSON и отправляется клиентам.
        }

        // Метод для отправки текущего трека выбранным мобильным клиентам.
        public async Task CurrentTrackResponse(OnlineObjectInfo onlineObjectInfo)
        {
            // Выбор всех мобильных клиентов, выбравших определенный объект.
            var clients = mobileClients
                .Where(mc => mc.SelectedObjectId == onlineObjectInfo.ObjectId)
                .ToList();

            // Отправка информации о текущем треке всем выбранным мобильным клиентам.
            await Clients.Clients(clients.Select(c => c.ConnectionId))
                .SendAsync(OnlineEvents.CurrentTrackResponse, JsonConvert.SerializeObject(onlineObjectInfo, new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                }));
            // Этот метод отвечает за отправку информации о текущем треке мобильным клиентам, которые выбрали определенный объект (например, устройство воспроизведения музыки). Сначала выбираются все мобильные клиенты, заинтересованные в данном объекте, затем им всем отправляется информация о текущем треке.
        }

        // Метод для установки выбранного объекта мобильным клиентом.
        public async Task SetSelectedObjectId(Guid objectId)
        {
            // Находим мобильного клиента, соответствующего текущему подключению.
            var mobileClient = mobileClients.Single(mc => mc.ConnectionId == Context.ConnectionId);

            // Устанавливаем выбранный объект для этого мобильного клиента.
            mobileClient.SelectedObjectId = objectId;

            // Вызываем метод отправки запроса на текущий трек для нового выбранного объекта.
            await SendCurrentTrackRequest(objectId);
            //Этот метод позволяет мобильному клиенту выбрать конкретный объект (например, плейлист или устройство). Выбор сохраняется, и затем выполняется запрос на текущий трек для этого объекта.
        }
        // Метод для отправки запроса на получение текущего трека.
        private async Task SendCurrentTrackRequest(Guid objectId)
        {
            // Проверяем, существует ли онлайн клиент с указанным ID.
            var onlineClient = connectedClients.SingleOrDefault(cc => cc.Id == objectId);

            // Если клиент онлайн, отправляем ему запрос на получение текущего трека.
            if (onlineClient != null)
            {
                await Clients.Groups(onlineClient.Id.ToString()).SendAsync(OnlineEvents.CurrentTrackRequest);
            }
            // Этот вспомогательный метод используется для отправки запроса на получение текущего трека от онлайн клиента. Если клиент находится в сети, он получит запрос через SignalR канал.
        }

        // Получение версии клиента из параметров запроса.
        private string Version()
        {
            var version = Context.GetHttpContext().Request.Query["version"]; // Получение параметра версии из строки запроса.
            return version.Any() ? version[0] : string.Empty; // Возвращаем первый элемент, если он есть, иначе пустую строку.

            //Этот метод возвращает версию клиента, переданную как параметр запроса, что может быть использовано для отслеживания или обработки разных версий клиентского приложения.
        }

        // Определение, является ли текущий клиент мобильным.
        private bool IsMobileClient()
        {
            var queryValues = Context.GetHttpContext().Request.Query["isMobile"]; // Получение параметра isMobile из строки запроса.

            // Возвращаем true, если параметр присутствует и равен true, иначе false.
            if (!queryValues.Any())
            {
                return false;
            }
            return Convert.ToBoolean(queryValues[0]);
        }

        private Guid ClientId()
        //// Получение идентификатора клиента из параметров запроса.
        {
            return Guid.Parse(Context.GetHttpContext().Request.Query["objectId"][0]);
            //Этот метод извлекает из запроса уникальный идентификатор клиента, который обычно используется для идентификации и взаимодействия с конкретным клиентом или устройством.
        }

        // Получение IP-адреса клиента из заголовков HTTP-запроса.
        private string ClientIpAddress()
        {
            var requestHeader = Context.GetHttpContext().Request.Headers["X-Forwarded-For"]; // Получаем заголовок X-Forwarded-For.

            // Возвращаем первый IP-адрес из списка, если он есть, иначе пустую строку.
            if (!requestHeader.Any())
            {
                return string.Empty;
            }
            return requestHeader[0];
            //Этот метод позволяет получить IP-адрес клиента, что может быть полезно для логирования, аудита или ограничения доступа.
        }

        // Отправка статуса соединения связанным чатам и объектам.
        private async Task SendConnectionStatus(Guid id, bool clientConnected)
        {
            // Получаем связанные чаты и объекты для текущего соединения.
            var chatAndObjects = await GetChatsAndObjectForCurrentConnection(id);

            // Асинхронно отправляем статус соединения всем связанным чатам.
            await Task.WhenAll(chatAndObjects
                .Select(i =>
                    _telegramMessageSender.SendClientConnectionStatusChanged(i.ChatId, i.ObjectName, DateTime.Now, clientConnected)
                )
                .ToList());
            //Этот метод используется для асинхронной отправки обновлений о статусе подключения клиента связанным с ним чатам в Telegram.
        }


        // Получение связанных чатов и объектов для текущего соединения.
        private Task<List<ChatAndObject>> GetChatsAndObjectForCurrentConnection(Guid id)
        {
            // Запрашиваем из контекста базы данных все связанные чаты и объекты.
            return _context.Managers
                .Where(m => m.User.Objects.Any(o => o.ObjectId == id)) // Фильтруем менеджеров по объектам пользователя.
                .Where(m => m.User.TelegramChatId.HasValue) // Убеждаемся, что у пользователя есть ID чата в Telegram.
                .Select(m => new ChatAndObject // Создаем новый объект для каждой связи чата и объекта.
                {
                    ChatId = m.User.TelegramChatId.Value,
                    ObjectName = m.User.Objects.Where(o => o.ObjectId == id).Select(o => o.Object.Name).Single()
                }).ToListAsync(); // Преобразуем результат в список асинхронно.

            //Этот метод извлекает информацию о связанных чатах Telegram и объектах для отправки им статусов соединений или других уведомлений.
        }


        // Определение вложенного класса для ассоциации данных чата и объекта.
        private class ChatAndObject
        {
            public long ChatId { get; set; } // Идентификатор чата в Telegram или другой системе мессенджеров.
            public string ObjectName { get; set; } // Название объекта, связанного с чатом.

            //Этот класс используется для создания связи между идентификатором чата и названием объекта. Такая модель может быть использована для передачи данных между различными частями приложения или для отправки сообщений в определенные чаты, связанные с объектами.
        }

        // Определение класса для представления мобильного клиента.
        public class MobileClient : IEquatable<MobileClient>
        {
            public string UserId { get; init; } // Уникальный идентификатор пользователя.
            public Guid SelectedObjectId { get; set; } // ID объекта, выбранного пользователем.
            public string ConnectionId { get; init; } // Идентификатор соединения SignalR.

            // Реализация интерфейса IEquatable для сравнения экземпляров MobileClient.
            public bool Equals(MobileClient other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return UserId == other.UserId && ConnectionId == other.ConnectionId;
            }

            // Переопределение метода Equals для объектного сравнения.
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((MobileClient)obj);
            }

            // Переопределение метода GetHashCode для генерации хеш-кода.
            public override int GetHashCode()
            {
                return HashCode.Combine(UserId, ConnectionId);
            }

            //MobileClient представляет собой класс для хранения информации о мобильном клиенте. Это включает в себя идентификатор пользователя, идентификатор текущего подключения и идентификатор выбранного объекта. Реализация IEquatable<T> позволяет сравнивать два экземпляра MobileClient, что может быть полезно для управления списками клиентов.
        }


        // Определение класса для представления онлайн-клиента.
        public class OnlineClient : IEquatable<OnlineClient>
        {
            public Guid Id { get; init; } // Уникальный идентификатор онлайн-клиента.
            public string Version { get; init; } // Версия клиентского приложения.
            public string IpAddress { get; init; } // IP-адрес клиента.

            // Реализация интерфейса IEquatable для сравнения экземпляров OnlineClient.
            public bool Equals(OnlineClient other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Id.Equals(other.Id) && Version == other.Version && IpAddress == other.IpAddress;
            }

            // Переопределение метода Equals для объектного сравнения.
            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((OnlineClient)obj);
            }

            // Переопределение метода GetHashCode для генерации хеш-кода.
            public override int GetHashCode()
            {
                return HashCode.Combine(Id, Version, IpAddress);
            }
            //OnlineClient представляет информацию об онлайн-клиенте, включая его идентификатор, версию и IP-адрес. Подобно MobileClient, этот класс реализует IEquatable<T>, чтобы позволить сравнение экземпляров между собой. Это полезно для отслеживания и управления подключенными пользователями в реальном времени.
        }

    }
}

//    Получение статуса о плейлисте: Когда происходит событие, связанное с плейлистом (например, его начало или окончание), сервер может быстро определить, какому чату следует отправить уведомление об этом событии, используя информацию из экземпляров ChatAndObject.

//Уведомления в Telegram: Когда плейлист начинается (PlaylistStarted) или заканчивается (PlaylistEnded), соответствующие методы в PlayerClientHub проходят через список объектов ChatAndObject, чтобы определить, куда отправить сообщение. Затем, используя сервис _telegramMessageSender, они отправляют уведомление в каждый соответствующий чат Telegram.

//  Например, если в системе есть плейлист с названием "Morning Playlist", который связан с чатом Telegram управляющего магазина, когда плейлист начинается, PlaylistStarted метод может отправить сообщение "Morning Playlist started" в этот чат Telegram, используя информацию из ChatAndObject.