using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class PendingRequestWorker : PlayerBackgroundServiceBase
    //Класс PendingRequestWorker является фоновой службой, предназначенной для обработки ожидающих запросов в базе данных. Эти запросы могут возникнуть, если приложение пыталось выполнить операцию, когда не было доступа в Интернет, и теперь необходимо выполнить их после восстановления подключения. Давайте детально рассмотрим, как работает этот класс:
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<PendingRequestWorker> _logger;
        private readonly PlayerStateManager _stateManager;

        public PendingRequestWorker(IServiceProvider provider,
            PlayerStateManager stateManager,
            ILogger<PendingRequestWorker> logger) : base(stateManager)
        {
            _provider = provider;
            _stateManager = stateManager;
            _logger = logger;
            //Конструктор получает три параметра: IServiceProvider для доступа к сервисам, PlayerStateManager для управления состоянием плеера и ILogger для логирования операций и ошибок.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await WaitForObject(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWork(stoppingToken);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
            //Это основной метод, который выполняется в фоне:
            // Сначала выполняется метод WaitForObject, ожидающий некоторое условие (не показан в коде).
            // Затем в цикле, который продолжается до запроса на отмену, выполняется метод DoWork, обрабатывающий ожидающие запросы. В случае ошибки она логируется.
            // После обработки всех запросов выполняется задержка на 1 минуту перед следующей итерацией цикла.
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using var scope = _provider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();//Создается область видимости для сервисов (IServiceScope), что позволяет получить PlayerContext и IHttpClientFactory из контейнера зависимостей.
            var httpClient = scope.ServiceProvider.GetRequiredService<IHttpClientFactory>() //v
                .CreateClient(nameof(PendingRequestWorker));

            var pendingRequests = await context
                .PendingRequest
                .OrderBy(pr => pr.Date)
                .ToListAsync(stoppingToken);

            if (!pendingRequests.Any()) //Если запросов нет, записывается соответствующая информация в лог и метод завершается.
            {
                _logger.LogInformation("No pending requests");
                return;
            }

            _logger.LogInformation("Pending requests count {Count}", pendingRequests.Count);

            var timeWithoutInternet = DateTime.Now - pendingRequests.First().Date;

            if (timeWithoutInternet >= TimeSpan.FromHours(1) && !_stateManager.IsOnline) //Если время без Интернета превысило 1 час и состояние плеера указывает на отсутствие подключения, выводится предупреждение.
            {
                _logger.LogWarning("Internet connection lost more then {TimeWithoutInternet} hour", timeWithoutInternet);
            }

            foreach (var request in pendingRequests)
            {//Для каждого запроса выполняется соответствующий HTTP-запрос с использованием HttpClient. В зависимости от метода запроса (GET, POST, PUT, DELETE) используется соответствующий метод HttpClient.
                HttpResponseMessage httpResponseMessage = null!;

                if (request.HttpMethod == HttpMethod.Get.Method)
                {
                    httpResponseMessage = await httpClient.GetAsync(request.Url, stoppingToken);
                }

                if (request.HttpMethod == HttpMethod.Post.Method)
                {
                    httpResponseMessage = await httpClient.PostAsync(request.Url, new StringContent(request.Body, Encoding.UTF8, "application/json"), stoppingToken);
                }

                if (request.HttpMethod == HttpMethod.Put.Method)
                {
                    httpResponseMessage = await httpClient.PutAsync(request.Url, new StringContent(request.Body, Encoding.UTF8, "application/json"), stoppingToken);
                }

                if (request.HttpMethod == HttpMethod.Delete.Method)
                {
                    httpResponseMessage = await httpClient.DeleteAsync(request.Url, stoppingToken);
                }

                if (httpResponseMessage.StatusCode == HttpStatusCode.InternalServerError)
                {
                    continue; //Если статус ответа сервера равен InternalServerError, запрос пропускается (возможно, предполагается повторная попытка позже).
                }

                httpResponseMessage.EnsureSuccessStatusCode();

                context.PendingRequest.Remove(request); //При успешном выполнении запроса он удаляется из базы данных.
                await context.SaveChangesAsync(stoppingToken);
            }
            //             Использование DI (IServiceProvider) для получения сервисов внутри фоновой задачи.
            // Логирование операций и ошибок для отслеживания состояния выполнения задачи и возникших проблем.
            // Взаимодействие с HTTP-сервисами через HttpClient, полученный из IHttpClientFactory, что является рекомендованным способом использования HttpClient в .NET.
            // Проверка состояния подключения к Интернету и соответствующая обработка ситуаций, когда подключение отсутствует.
        }
        //Эта служба показывает, как можно обрабатывать задачи, требующие восстановления после сбоев связи, и автоматически исполнять накопившиеся задачи, когда связь восстановлена.
    }
}