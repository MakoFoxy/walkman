using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Player.ClientIntegration;
using Player.ClientIntegration.Object;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace MarketRadio.Player.Services.LiveConnection
{
    public class ServerLiveConnection
    //Класс ServerLiveConnection представляет собой компонент для управления подключением к серверу в реальном времени, возможно, через SignalR или аналогичную технологию, которая позволяет устанавливать постоянные соединения для двустороннего обмена сообщениями между клиентом и сервером. Этот класс интегрируется с другими сервисами и компонентами приложения для обеспечения живого соединения с сервером, обработки команд от сервера и поддержания состояния плеера. Давайте подробнее разберём его ключевые части и функционал:
    {
        private readonly IConfiguration _configuration;
        private readonly PlayerStateManager _stateManager;
        private readonly ILogger<ServerLiveConnection> _logger;
        private readonly IApp _app;
        private readonly ServerLiveConnectionCommandProcessor _serverLiveConnectionCommandProcessor;
        private HubConnection? _connection;

        public ServerLiveConnection(IConfiguration configuration,
            PlayerStateManager stateManager,
            ILogger<ServerLiveConnection> logger,
            IApp app,
            ServerLiveConnectionCommandProcessor serverLiveConnectionCommandProcessor)
        {
            _configuration = configuration;
            _stateManager = stateManager;
            _logger = logger;
            _app = app;
            _serverLiveConnectionCommandProcessor = serverLiveConnectionCommandProcessor;

            //         Принимает зависимости, необходимые для работы:

            // IConfiguration: Для доступа к настройкам приложения.
            // PlayerStateManager: Управляет состоянием плеера, например, текущим треком, состоянием воспроизведения и т.д.
            // ILogger<ServerLiveConnection>: Для логирования.
            // IApp: Общая информация и состояние приложения.
            // ServerLiveConnectionCommandProcessor: Обработчик команд, полученных от сервера через живое соединение.
        }

        public async Task Connect(Guid objectId)
        {//Цель: Установить соединение с сервером для заданного объекта (objectId), если оно ещё не установлено.
            if (_connection != null)
            { //В случае, если соединение уже установлено (_connection не равно null), метод просто возвращает управление, не выполняя новое подключение.
                return;
            }

            await ConnectInternal(objectId); //В противном случае вызывает внутренний метод ConnectInternal, который фактически устанавливает соединение.
        }

        public async Task ConnectToNewObject(Guid objectId) //Сменить объект подключения, отключившись от текущего соединения и установив новое соединение для другого объекта (objectId).
        {
            if (_connection != null)
            {
                await _connection.StopAsync();
                await _connection.DisposeAsync();
            }

            await ConnectInternal(objectId); //Если текущее соединение активно, метод останавливает и освобождает его, после чего устанавливает новое соединение, вызывая ConnectInternal с новым objectId.
        }

        private async Task ConnectInternal(Guid objectId)
        { //Метод ConnectInternal отвечает за установление и настройку внутреннего соединения с сервером в реальном времени для объекта с определённым идентификатором (objectId). Вот ключевые моменты его работы:
            var publisherUrl = _configuration.GetValue<string>("Endpoints:PublisherUrl"); //Извлечение URL сервера: URL для подключения извлекается из конфигурации приложения (_configuration), что позволяет легко изменять адрес сервера без необходимости перекомпиляции кода.
            _connection = new HubConnectionBuilder() //Использует HubConnectionBuilder для создания и конфигурирования HubConnection, что является частью SignalR (технология Microsoft для добавления реального времени веб-функциональности в приложения).
                .AddNewtonsoftJsonProtocol() //Добавляет поддержку протокола NewtonsoftJson, который используется для сериализации и десериализации сообщений между клиентом и сервером.
                .WithUrl($"{publisherUrl}/ws/player-client-hub?objectId={objectId}&version={_app.Version}") //Настраивает URL соединения с использованием извлеченного адреса сервера, идентификатора объекта и версии приложения.
                .WithAutomaticReconnect(new PlayerRetryPolicy(_logger)) //Включает автоматическое переподключение с использованием пользовательской политики повторных попыток (PlayerRetryPolicy).
                .ConfigureLogging(logging =>
                {
                    logging.AddSerilog(); //Настраивает логирование через Serilog.
                })
                .Build();

            _connection.Reconnected += async _ =>
            {
                _logger.LogWarning("Reconnected");
                await _stateManager.ChangeOnlineState(true);
                //Обработка событий соединения:

                // При переподключении (Reconnected) логируется предупреждение и изменяется онлайн-состояние приложения на true (онлайн).
            };

            _connection.Closed += async _ =>
            {
                await _stateManager.ChangeOnlineState(false);
                // При закрытии соединения (Closed) изменяется онлайн-состояние на false (оффлайн).

            };

            _serverLiveConnectionCommandProcessor.Subscribe(_connection);
            //Подписка на команды: Использует ServerLiveConnectionCommandProcessor для подписки на команды от сервера через текущее соединение.
            if (_connection.State == HubConnectionState.Disconnected)
            {//Проверяет, что текущее состояние соединения — отключено (Disconnected).
                try
                {//Пытается начать соединение с использованием StartAsync. В случае ошибки логирует её.
                    await _connection.StartAsync();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }

                if (_connection.State == HubConnectionState.Connected)
                {//Если соединение успешно установлено (состояние — Connected), изменяет онлайн-состояние на true.
                    await _stateManager.ChangeOnlineState(true);
                }
            }
            //Этот метод является центральным элементом для управления живым соединением с сервером, обеспечивая стабильность и надёжность коммуникации благодаря механизму автоматического переподключения и интеграции с системой логирования и управления состоянием приложения.
        }

        public Task SendPlaylistStarted() //Сообщить серверу о начале проигрывания плейлиста.
        {//Если соединение с сервером (_connection) не установлено (null), метод немедленно завершается, возвращая Task.CompletedTask, что означает, что операция завершена и не требует асинхронного ожидания.
            if (_connection == null)
            {
                return Task.CompletedTask;
            }

            return _connection.SendAsync(OnlineEvents.PlaylistStarted); //Если соединение активно, метод отправляет асинхронно событие OnlineEvents.PlaylistStarted без дополнительных данных. SendAsync используется для асинхронной отправки сообщений на сервер через открытое соединение.
        }

        public Task CurrentTrackResponse(OnlineObjectInfo onlineObjectInfo) //Отправить на сервер информацию о текущем треке, воспроизводимом в плеере. //Принимает параметр onlineObjectInfo, который содержит данные о текущем треке, такие как его идентификатор, имя, продолжительность и прочую информацию. Аналогично методу SendPlaylistStarted, если соединение не установлено, метод завершается, возвращая Task.CompletedTask.
        {
            if (_connection == null)
            {
                return Task.CompletedTask;
            }

            return _connection.SendAsync(OnlineEvents.CurrentTrackResponse, onlineObjectInfo); //Если соединение с сервером активно, метод асинхронно отправляет событие OnlineEvents.CurrentTrackResponse на сервер, передавая в качестве аргумента объект onlineObjectInfo с информацией о треке.
        }
        //Эти методы позволяют приложению активно участвовать в обмене данными с сервером, сообщая о важных событиях в жизненном цикле плеера. Использование асинхронной отправки сообщений (SendAsync) обеспечивает неблокирующую операцию, позволяя приложению продолжать свою работу без ожидания подтверждения от сервера.
    }

    public class PlayerRetryPolicy : IRetryPolicy
    {//Класс PlayerRetryPolicy реализует интерфейс IRetryPolicy, предоставляемый библиотекой SignalR для настройки поведения автоматического переподключения. Этот класс используется для определения логики и задержки между попытками переподключения к серверу, когда соединение прерывается. Вот ключевые аспекты его работы:
        private readonly ILogger _logger;
        private readonly Random _random = new();

        public PlayerRetryPolicy(ILogger logger)
        {
            _logger = logger;//    Принимает объект ILogger, который используется для логирования попыток переподключения. Это позволяет отслеживать процесс переподключения и облегчает диагностику проблем соединения.
        }

        public TimeSpan? NextRetryDelay(RetryContext retryContext) //Вызывается автоматически SignalR клиентом, когда требуется определить задержку перед следующей попыткой переподключения после потери соединения.В качестве параметра принимает RetryContext, который содержит информацию о контексте переподключения, такую как количество предыдущих попыток переподключения и последняя ошибка, приведшая к необходимости переподключения.
        {
            _logger.LogWarning("Reconnecting...");
            return TimeSpan.FromSeconds(_random.NextDouble() * 10);
            //Метод логирует предупреждение о попытке переподключения и возвращает случайную задержку перед следующей попыткой. Эта задержка вычисляется как случайное значение в диапазоне от 0 до 10 секунд, что помогает избежать ситуации "громового стада" (thundering herd problem), когда множество клиентов пытаются переподключиться одновременно.
        }
    }
    //Использование случайной задержки является распространённой практикой при реализации механизмов автоматического переподключения, поскольку это помогает равномерно распределить нагрузку на сервер при массовых попытках переподключения от большого числа клиентов. Такой подход также может помочь уменьшить вероятность возникновения проблем с производительностью сервера и сети.
}