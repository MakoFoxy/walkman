using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Adverts
{
    public class ArchiveAdvert
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context; //PlayerContext _context: контекст Entity Framework DBContext, используемый для операций с базой данных.
            private readonly IPlaylistGenerator _playlistGenerator; //IPlaylistGenerator _playlistGenerator: интерфейс сервиса, предположительно для генерации плейлистов.

            public Handler(PlayerContext context, IPlaylistGenerator playlistGenerator)
            {
                _context = context;
                _playlistGenerator = playlistGenerator;
            }

            //TODO Написать логику архивации
            public async Task<Unit> Handle(Command request, CancellationToken cancellationToken)
            {
                throw new NotImplementedException();
                var adLifetimes = await _context.AdLifetimes
                    .Where(al => al.Advert.Id == request.Id && al.DateEnd > DateTime.Now)
                    .ToListAsync(cancellationToken);

                //Сначала он извлекает все времена жизни рекламы из базы данных, которые соответствуют данному ID рекламы и еще не закончились. Это делается с использованием методов LINQ Entity Framework Core.

                adLifetimes.ForEach(al => al.InArchive = true); //Пометка как архивированная: Каждый полученный AdLifetime затем помечается как архивированный (InArchive = true).

                await _context.SaveChangesAsync(cancellationToken); //Сохранение изменений: Контекст обновляется с этими изменениями через _context.SaveChangesAsync().

                var objects = await _context.Adverts.Where(a => a.Id == request.Id).SelectMany(a => a.AdTimes).Select(ah => ah.Object).Distinct()
                    .ToListAsync(cancellationToken);

                foreach (var objectInfo in objects)
                {
                    foreach (var adLifetime in adLifetimes)
                    {
                        for (var date = adLifetime.DateBegin; date < adLifetime.DateEnd; date = date.AddDays(1))
                        {
                            await _playlistGenerator.Generate(objectInfo, date);
                        }
                    }
                    //Обновление затронутых объектов: Затем он находит все уникальные объекты, связанные с временными слотами рекламы, и генерирует для них новые плейлисты на каждый день, когда реклама была запланирована к показу. Это включает:

                    // Извлечение всех уникальных объектов, связанных с рекламой.
                    // Итерирование по каждому объекту и каждому соответствующему времени жизни рекламы.
                    // Генерация нового плейлиста на каждый день периода жизни рекламы с использованием _playlistGenerator.
                }

                return Unit.Value;
            }
        }

        public class Command : IRequest<Unit>
        {
            public Guid Id { get; set; } ////Определяет тип запроса, с которым работает Handler. Он включает одно свойство, Id, указывая, что эта команда предназначена для архивации рекламы, идентифицируемой этим Guid.
        }
    }
}