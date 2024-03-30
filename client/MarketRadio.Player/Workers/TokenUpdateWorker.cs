using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Services.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class TokenUpdateWorker : PlayerBackgroundServiceBase
    {//Код представляет собой фоновую службу TokenUpdateWorker, наследуемую от PlayerBackgroundServiceBase, цель которой — регулярное обновление токена пользователя, используемого в системе медиаплеера. Давайте разберем ключевые моменты его работы.
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<TokenUpdateWorker> _logger;

        public TokenUpdateWorker(
            PlayerStateManager stateManager,
            IServiceProvider serviceProvider,
            ILogger<TokenUpdateWorker> logger
        ) : base(stateManager)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            //    IServiceProvider _serviceProvider: Используется для доступа к сервисам и создания области видимости (scope), в которой работают сервисы.
            // ILogger<TokenUpdateWorker> _logger: Предоставляет функциональность логирования для регистрации информации о процессе работы и возникающих ошибках.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WaitForObject(stoppingToken);//Подготовка: Сначала выполняется ожидание готовности объекта (WaitForObject), что может быть необходимо для убеждения в доступности всех зависимостей и сервисов.
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWork();
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);//Цикл Обновления: Затем начинается бесконечный цикл, который продолжается до получения сигнала об остановке (stoppingToken.IsCancellationRequested). В этом цикле каждый час (await Task.Delay(TimeSpan.FromHours(1), stoppingToken)) вызывается метод DoWork, отвечающий за обновление токена.
            }
        }

        private async Task DoWork()
        {
            using var scope = _serviceProvider.CreateScope();//Создание Scope: Создается новая область видимости для запроса зависимых сервисов. Это важно для корректной работы с контекстами и другими сервисами, которые могут быть зарегистрированы с определенным временем жизни.
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();//Получение Настроек: Из контекста PlayerContext извлекается настройка пользователя с ключом UserSetting.Token, которая содержит текущее значение токена.
            var userSetting = await context.UserSettings.SingleOrDefaultAsync(us => us.Key == UserSetting.Token);

            if (userSetting == null)
            {
                return;
            }

            var userApi = scope.ServiceProvider.GetRequiredService<IUserApi>(); //Обновление Токена: Если настройка с токеном найдена, выполняется запрос к сервису IUserApi для обновления токена с помощью метода Renew.
            var response = await userApi.Renew($"Bearer {userSetting.Value}");

            switch (response.StatusCode)
            {
                case HttpStatusCode.OK: //В случае успеха (HttpStatusCode.OK), новое значение токена сохраняется в настройках пользователя, и изменения записываются в базу данных (context.SaveChangesAsync()).
                    {
                        userSetting.Value = await response.Content.ReadAsStringAsync();
                        await context.SaveChangesAsync();
                        break;
                    }
                case HttpStatusCode.Unauthorized://Если получен статус Unauthorized или InternalServerError, регистрируется соответствующая ошибка.
                    _logger.LogError("Unauthorized");
                    break;
                case HttpStatusCode.InternalServerError:
                    _logger.LogError("InternalServerError in Renew process");
                    break;
                default: //Для других статусов кодов также регистрируется предупреждение с содержимым ответа.
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        _logger.LogWarning("Renew body {Body}", content);
                        break;
                    }
            }
            //Служба TokenUpdateWorker играет критически важную роль в поддержании актуальности аутентификационных данных пользователя, регулярно обновляя токен для поддержания доступа к пользовательским API. Это обеспечивает непрерывную работоспособность системы и избегание проблем, связанных с истечением срока действия токена. Реализация подчеркивает важность безопасности и актуальности данных в современных приложениях, требующих аутентификации.
        }
    }
}