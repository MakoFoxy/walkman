using System; // Импорт базового пространства имен .NET
using System.Linq; // Импорт для использования LINQ-запросов
using System.Threading; // Импорт для работы с потоками и токенами отмены
using System.Threading.Tasks; // Импорт для асинхронного программирования
using MediatR; // Импорт MediatR для реализации шаблона CQRS
using Microsoft.EntityFrameworkCore; // Импорт Entity Framework Core для взаимодействия с базой данных
using Player.DataAccess; // Импорт контекста данных вашего приложения
using Player.Services.Abstractions; // Импорт абстракций сервисов

// Определение пространства имен для логики бизнес-функционала, связанного с рекламой
namespace Player.BusinessLogic.Features.Adverts
{
    // Определение класса для актуализации информации о рекламе
    public class ActualizeAdvert
    {
        // Внутренний класс, реализующий интерфейс IRequestHandler из MediatR для обработки команд
        public class Handler : IRequestHandler<Command>
        {
            // Зависимости, необходимые для работы обработчика
            private readonly PlayerContext _context; // Контекст базы данных
            private readonly IPlaylistGenerator _playlistGenerator; // Сервис для генерации плейлистов
            private readonly IPlayerTaskCreator _playerTaskCreator; // Сервис для создания задач

            // Конструктор класса обработчика с инъекцией зависимостей
            public Handler(PlayerContext context, IPlaylistGenerator playlistGenerator, IPlayerTaskCreator playerTaskCreator)
            {
                _context = context;
                _playlistGenerator = playlistGenerator;
                _playerTaskCreator = playerTaskCreator;
                //В конструкторе Handler принимаются зависимости: контекст базы данных и сервисы для работы с плейлистами и задачами.
            }

            // Асинхронный метод для обработки команды актуализации рекламы
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                // Получение списка рекламных кампаний, которые нужно актуализировать
                var adLifetimes = await _context.AdLifetimes
                    .Where(al => al.Advert.Id == request.Id && al.DateEnd > DateTime.Now && al.InArchive)
                    .ToListAsync(cancellationToken);

                // Установка статуса "не в архиве" для всех полученных кампаний
                adLifetimes.ForEach(al => al.InArchive = false);

                // Для каждой рекламной кампании и каждого дня её действия создание задачи на генерацию плейлиста
                foreach (var adLifetime in adLifetimes)
                {
                    for (var date = adLifetime.DateBegin; date < adLifetime.DateEnd; date = date.AddDays(1))
                    {
                        await _playerTaskCreator.AddPlaylistGenerationTask(request.Id, cancellationToken);
                    }
                }

                // Сохранение изменений в базе данных
                await _context.SaveChangesAsync(cancellationToken);

                // Возврат пустого результата, так как возвращаемый тип Unit является аналогом void для MediatR
                return Unit.Value;
                //Этот метод асинхронно обрабатывает команду актуализации рекламы. Он извлекает из базы данных все сроки действия рекламы (AdLifetimes), которые удовлетворяют определенным условиям (связаны с заданной рекламой, еще не истекли и находятся в архиве). Затем для всех найденных записей меняет статус на неархивный и для каждой даты в диапазоне действия рекламы создает задачи на генерацию плейлистов. В конце происходит сохранение изменений в базе данных.
            }
        }

        // Класс, представляющий команду для актуализации рекламы, реализующий интерфейс IRequest из MediatR
        public class Command : IRequest<Unit>
        {
            public Guid Id { get; set; } // Идентификатор рекламной кампании для актуализации
        }
    }
    //в целом, данный код представляет собой обработчик команды в архитектуре, основанной на CQRS и MediatR. Его задача — обновить информацию о рекламных акциях, снять их с архива при необходимости и инициировать создание соответствующих задач для системы.
}
