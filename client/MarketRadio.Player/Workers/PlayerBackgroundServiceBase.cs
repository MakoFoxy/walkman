using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace MarketRadio.Player.Workers
{
    public abstract class PlayerBackgroundServiceBase : BackgroundService
    {//Класс PlayerBackgroundServiceBase представляет собой абстрактный базовый класс для фоновых служб в приложении медиаплеера. Он наследуется от BackgroundService, стандартного абстрактного класса в .NET для создания длительных фоновых задач. Этот базовый класс предоставляет общую функциональность и утилиты, которые могут быть использованы его потомками. Давайте рассмотрим его ключевые аспекты:
        private readonly PlayerStateManager _stateManager;

        public PlayerBackgroundServiceBase(PlayerStateManager stateManager)
        {
            _stateManager = stateManager;
            //    Конструктор принимает экземпляр PlayerStateManager, который, вероятно, является сервисом для управления состоянием медиаплеера, например, текущим воспроизводимым объектом, плейлистом и другими параметрами состояния.
        }

        protected bool NowTheWorkingTime
        {//Свойство NowTheWorkingTime предоставляет информацию о том, находится ли текущее время внутри рабочего интервала объекта, который управляется PlayerStateManager.
            get
            {
                if (_stateManager.Object == null)
                {
                    throw new ArgumentNullException($"Object in state manager is null, use {nameof(WaitForObject)} before access to object");
                }
                //Проверяет, соответствует ли текущее время рабочему времени объекта, учитывая его BeginTime и EndTime. Также учитывает FreeDays, что может указывать на особые дни недели, когда работа объекта разрешена вне обычного временного интервала.
                return _stateManager.Object.BeginTime <= DateTime.Now.TimeOfDay &&
                       _stateManager.Object.EndTime > DateTime.Now.TimeOfDay ||
                       _stateManager.Object.FreeDays.Contains(DateTime.Now.DayOfWeek);
            }
        }

        protected async Task WaitForObject(CancellationToken stoppingToken)
        {
            while (_stateManager.Object == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(.5), stoppingToken);
            }
            //    WaitForObject — это асинхронный метод, который ожидает, пока в PlayerStateManager не появится активный объект (_stateManager.Object != null). Если объект отсутствует, метод периодически "усыпляется" на короткий промежуток времени (0.5 секунды), позволяя другим задачам быть выполненными.
        }
        
        protected async Task WaitForPlaylist(CancellationToken stoppingToken)
        {//    Аналогично, WaitForPlaylist ожидает, пока не будет доступен плейлист (_stateManager.Playlist != null), также используя задержку для оптимизации использования ресурсов.
            while (_stateManager.Playlist == null)
            {
                await Task.Delay(TimeSpan.FromSeconds(.5), stoppingToken);
            }
        }
        //В целом, PlayerBackgroundServiceBase обеспечивает структурированную основу для создания специализированных фоновых служб, которые требуют взаимодействия со статусом медиаплеера. Он предоставляет механизмы для ожидания наличия ключевых элементов состояния (таких как активный объект или плейлист) и для проверки, находится ли текущее время в пределах определенного рабочего интервала.
    }
}

//Адаптивное управление ресурсами: Методы WaitForObject и WaitForPlaylist реализуют ожидание доступности данных с минимальным потреблением ресурсов, благодаря асинхронным задержкам. Это предотвращает ненужное потребление CPU и позволяет системе более эффективно распределять ресурсы между задачами.