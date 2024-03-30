using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.DataAccess;
using MarketRadio.Player.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class OpenAppSchedulerWorker : PlayerBackgroundServiceBase
    //Класс OpenAppSchedulerWorker представляет собой фоновую службу, цель которой — запланировать автоматический запуск приложения в определенное время. Эта служба использует PlayerBackgroundServiceBase в качестве базового класса, что указывает на то, что она предназначена для работы в контексте медиаплеера. Давайте подробно рассмотрим, как он работает:
    {
        private readonly WindowsTaskScheduler _windowsTaskScheduler;
        private readonly IServiceProvider _serviceProvider;
        private readonly IApp _app;
        private readonly IWebHostEnvironment _environment;

        public OpenAppSchedulerWorker(
            WindowsTaskScheduler windowsTaskScheduler,
            PlayerStateManager stateManager,
            IServiceProvider serviceProvider,
            IApp app,
            IWebHostEnvironment environment) : base(stateManager)
        {
            _windowsTaskScheduler = windowsTaskScheduler;
            _serviceProvider = serviceProvider;
            _app = app;
            _environment = environment;
            //         Конструктор принимает несколько зависимостей:

            // WindowsTaskScheduler windowsTaskScheduler: компонент, предназначенный для работы с планировщиком задач Windows.
            // PlayerStateManager stateManager: используется базовым классом для управления состоянием плеера.
            // IServiceProvider serviceProvider: позволяет получать сервисы (например, контекст базы данных) внутри метода ExecuteAsync.
            // IApp app: интерфейс, предоставляющий информацию о приложении, такую как его имя.
            // IWebHostEnvironment environment: используется для определения, находится ли приложение в разработке или в продуктивной среде.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (_environment.IsDevelopment())//Сначала проверяется, находится ли приложение в среде разработки. Если да, служба немедленно завершает работу.
            {
                return;
            }

            await WaitForObject(stoppingToken); //Вызывается метод WaitForObject, ожидающий некоторое событие или условие. Детали этого метода не показаны, но можно предположить, что он ожидает, пока приложение не будет готово к выполнению следующего шага.

            if (Environment.OSVersion.Platform == PlatformID.Win32NT) //Проверяется операционная система, на которой выполняется приложение. Если это Windows (PlatformID.Win32NT), выполняется создание задачи в планировщике задач Windows.
            { //Создается область сервисов для получения сервисов из контейнера DI.
                using var scope = _serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<PlayerContext>();
                var beginTime = await context.ObjectInfos.Select(o => o.BeginTime).SingleAsync(stoppingToken); //Из контекста базы данных извлекается beginTime — время начала какого-либо события.

                var when = beginTime.Subtract(TimeSpan.FromMinutes(30)); //Рассчитывается время, когда должна быть запланирована задача (when), как за 30 минут до beginTime. Если полученное время меньше текущего, к нему добавляется 24 часа.

                if (when < TimeSpan.Zero)
                {
                    when = when.Add(TimeSpan.FromHours(24));
                }

                var directoryName = AppContext.BaseDirectory; //Определяется путь к исполняемому файлу приложения.
                var launcherPath = Path.GetFullPath(Path.Combine(directoryName!, "../../", $"{_app.ProductName}.exe"));
                _windowsTaskScheduler.CreateTaskForStartup(when, launcherPath); //С помощью _windowsTaskScheduler создается задача на запуск приложения в запланированное время.
            }

            if (Environment.OSVersion.Platform == PlatformID.Unix) //Если операционная система — Unix, в текущей реализации ничего не происходит.
            {
            }

            //             Условие для среды разработки (_environment.IsDevelopment()) предотвращает выполнение планирования в неподходящей среде, что удобно для тестирования и разработки.
            // Использование IServiceProvider для создания области сервисов (scope) позволяет получить доступ к сервисам, таким как контекст базы данных, что демонстрирует расширенные возможности управления зависимостями в .NET.
            // Пример демонстрирует взаимодействие с нативными возможностями операционной системы (планировщик задач Windows) из .NET приложения.
            // Пустой блок для Unix указывает на потенциальное место для реализации аналогичной логики на системах Unix/Linux, возможно, с использованием cron.
        }
    }
    //Этот класс является примером продвинутого фонового сервиса, который может быть использован для автоматического запуска или предварительной подготовки приложения перед началом важного события, такого как запланированная трансляция или сеанс воспроизведения.
}