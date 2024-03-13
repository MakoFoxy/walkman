using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Player.DataAccess;

// Определение пространства имен для вашего рабочего класса.
namespace Player.ArchiveWorker
{
    // Класс Worker наследуется от BackgroundService для создания фоновой службы.
    public class Worker : BackgroundService
    {
        // Объявление зависимостей, которые будут внедрены через конструктор.
        private readonly ILogger<Worker> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfiguration _configuration;

        // Конструктор для внедрения зависимостей.
        public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _configuration = configuration;
        }

        // Определение асинхронного метода, который будет выполняться при запуске фоновой службы.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Цикл, продолжающийся до тех пор, пока не будет запрошена остановка сервиса.
            while (!stoppingToken.IsCancellationRequested)
            {
                // Вычисление времени до следующего запуска сервиса.
                var timeLeft = DateTime.Today.Add(TimeSpan.Parse(_configuration.GetValue<string>("Player:WakeUpTime"))) - DateTime.Now;

                // Если до следующего времени запуска осталось отрицательное время, добавляем один день.
                if (timeLeft < TimeSpan.Zero)
                {
                    timeLeft = timeLeft.Add(TimeSpan.FromDays(1));
                }

                // Расчет следующего времени пробуждения.
                var nextWakeUpTime = DateTime.Now.Add(timeLeft);

                // Логирование времени следующего запуска.
                _logger.LogInformation("Worker wake up at {Time}", nextWakeUpTime);
                // Ожидание до следующего запуска или до момента отмены операции.
                await Task.Delay(timeLeft, stoppingToken);
                // Логирование факта пробуждения.
                _logger.LogInformation("Worker woke up");

                // Создание области для запроса сервисов.
                using var serviceScope = _serviceProvider.CreateScope();
                // Получение контекста базы данных.
                var context = serviceScope.ServiceProvider.GetRequiredService<PlayerContext>();

                // Получение объявлений, чей срок жизни истек и которые не помещены в архив.
                var adLifetimes = await context.Adverts
                    .Where(a => a.AdLifetimes.All(al => al.DateEnd < DateTime.Now))
                    .SelectMany(al => al.AdLifetimes)
                    .ToListAsync(stoppingToken);

                // Перемещение истекших объявлений в архив.
                foreach (var adLifetime in adLifetimes.Where(al => !al.InArchive))
                {
                    adLifetime.InArchive = true;
                }

                // Сохранение изменений в базе данных.
                await context.SaveChangesAsync(stoppingToken);
            }
        }
    }
}