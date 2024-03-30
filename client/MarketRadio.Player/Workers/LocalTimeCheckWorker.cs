using System;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.Helpers;
using MarketRadio.Player.Services.Http;
using Microsoft.Extensions.Logging;
using Player.ClientIntegration.Client;

namespace MarketRadio.Player.Workers
{
    public class LocalTimeCheckWorker : PlayerBackgroundServiceBase
    {//Этот код определяет класс LocalTimeCheckWorker, который является фоновой службой для проверки синхронизации локального времени с серверным временем. Давайте разберемся, что делает каждая часть этого класса:
        private readonly ISystemService _systemService;
        private readonly ILogger<LocalTimeCheckWorker> _logger;
        //Это приватные поля для хранения инжектированных зависимостей.
        public LocalTimeCheckWorker(
            PlayerStateManager stateManager,
            ISystemService systemService,
            ILogger<LocalTimeCheckWorker> logger
            ) : base(stateManager)
        {
            _systemService = systemService;
            _logger = logger;
            //Конструктор принимает три параметра: stateManager для управления состоянием плеера, systemService для получения времени с сервера, и logger для логирования. Эти зависимости инжектируются через механизм внедрения зависимостей.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {//Это основной метод фоновой службы, который выполняется в цикле до получения запроса на остановку через stoppingToken. Внутри цикла:
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await DoWork(true);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "");
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                //                 Вызывается метод DoWork, который выполняет логику проверки времени.
                // В случае возникновения исключения оно логируется с помощью _logger.
                // После каждой итерации выполнения задержка на час с помощью Task.Delay.
            }
        }

        private async Task DoWork(bool firstRequest)
        {
            var beforeTime = DateTimeOffset.Now;
            var serverTime = await _systemService.GetServerTime();
            var afterTime = DateTimeOffset.Now;

            if (afterTime - beforeTime < TimeSpan.FromSeconds(1))
            {
                CheckLocalTime(beforeTime, serverTime, afterTime);
            }
            else
            {
                await LogOrResendRequest(firstRequest, beforeTime, serverTime, afterTime);
            }
            //    Вызывается метод DoWork, который выполняет логику проверки времени.
            // В случае возникновения исключения оно логируется с помощью _logger.
            // После каждой итерации выполнения задержка на час с помощью Task.Delay.
        }

        private async Task LogOrResendRequest(bool firstRequest, DateTimeOffset beforeTime, CurrentTimeDto serverTime,
            DateTimeOffset afterTime)
        {
            if (firstRequest)
            {
                await DoWork(false);
            }
            else
            {
                _logger.LogWarning("Problems with server sync TimeBeforeRequest:{TimeBeforeRequest} ServerTime:{ServerTime} TimeAfterRequest:{TimeAfterRequest}",
                    beforeTime, serverTime.CurrentTime, afterTime);
            }
            //Если это первый запрос (firstRequest == true), метод пытается выполнить DoWork еще раз. В противном случае логируется предупреждение о задержке синхронизации времени с сервером.

        }

        private void CheckLocalTime(DateTimeOffset beforeTime, CurrentTimeDto serverTime, DateTimeOffset afterTime)
        {
            if (beforeTime.ResetToSeconds() <= serverTime.CurrentTime.ResetToSeconds() &&
                afterTime.ResetToSeconds() >= serverTime.CurrentTime.ResetToSeconds())
            {
                _logger.LogInformation("Local time is ok TimeAfterRequest:{TimeBeforeRequest} ServerTime:{ServerTime} TimeAfterRequest:{TimeAfterRequest}",
                    beforeTime, serverTime.CurrentTime, afterTime);
            }
            else
            {
                _logger.LogWarning("Problems with local time TimeBeforeRequest:{TimeBeforeRequest} ServerTime:{ServerTime} TimeAfterRequest:{TimeAfterRequest}",
                    beforeTime, serverTime.CurrentTime, afterTime);
            }
            //Проверяет, соответствует ли локальное время серверному времени в пределах замеренного интервала (beforeTime и afterTime). Если локальное время находится в пределах этого интервала, логируется информационное сообщение о корректности времени. В противном случае логируется предупреждение о несоответствии времени.
        }
        //Этот класс демонстрирует практику работы с асинхронными фоновыми службами в .NET, используя System.Threading.Tasks.Task для асинхронных операций и System.Threading.CancellationToken для управления остановкой службы. Особое внимание уделяется обработке исключений и логированию, что критически важно для диагностики и поддержки фоновых служб в продакшене.
    }
}