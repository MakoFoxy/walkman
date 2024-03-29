using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MarketRadio.Player.Helpers;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Workers
{
    public class DeleteOldLogsWorker : PlayerBackgroundServiceBase
    //Этот код описывает фоновую службу DeleteOldLogsWorker, которая наследуется от базового класса PlayerBackgroundServiceBase. Цель службы — регулярно удалять старые лог-файлы приложения. Давайте рассмотрим каждую строку более детально:
    {
        private readonly ILogger<DeleteOldLogsWorker> _logger;
        //    Определяет логгер, который используется для регистрации информационных сообщений о процессе удаления старых лог-файлов.
        public DeleteOldLogsWorker(
            ILogger<DeleteOldLogsWorker> logger,
            PlayerStateManager stateManager
            ) : base(stateManager)
        {
            _logger = logger;
            //    Конструктор принимает два параметра: logger для логирования действий службы и stateManager, который передается в базовый класс PlayerBackgroundServiceBase. stateManager может использоваться для доступа к состоянию плеера или управления им, хотя в данном контексте он не используется непосредственно в ExecuteAsync.
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested) //    Цикл продолжается до тех пор, пока не будет запрошена отмена через stoppingToken. Это позволяет корректно останавливать службу при необходимости.
            {
                var oldLogFiles = Directory.GetFiles(DefaultLocations.AppLogsPath, "*.txt")
                    .Select(f => new FileInfo(f))
                    .Where(f => f.CreationTime < DateTime.Today.AddMonths(-1))
                    .ToList(); //    Получает список файлов логов старше одного месяца. Использует путь, заданный в DefaultLocations.AppLogsPath, и фильтрует файлы по расширению .txt. Для каждого файла создается объект FileInfo, что позволяет легко получить доступ к его свойствам, таким как время создания, и затем фильтрует файлы, чтобы оставить только те, что были созданы более месяца назад.

                foreach (var oldLogFile in oldLogFiles) //    Для каждого старого лог-файла логируется информация о его удалении, после чего файл удаляется.
                {
                    _logger.LogInformation("Deleting old log file {FileName}", oldLogFile.Name);
                    oldLogFile.Delete();
                }

                await Task.Delay(TimeSpan.FromHours(1), stoppingToken); //    После обработки всех старых лог-файлов служба засыпает на 1 час, после чего цикл повторяется. Task.Delay использует stoppingToken, что позволяет прервать задержку, если служба должна быть остановлена.
            }
        }
        //Этот код представляет собой типичный пример фоновой службы в .NET, которая выполняет регулярные задачи по очистке и поддержанию приложения. Использование CancellationToken обеспечивает возможность корректно прервать выполнение службы при необходимости.
    }
}