using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Adverts
{
    public class ListArchived
    //ListArchived - обертка для класса запроса. ArchivedAdvertListQuery реализует IRequestHandler, который обрабатывает запросы на получение архивных рекламных объявлений и возвращает результаты в структурированном виде (BaseFilterResult<ArchivedAdvertModel>).
    {
        public class ArchivedAdvertListQuery : IRequestHandler<ArchivedAdvertRequest, BaseFilterResult<ArchivedAdvertModel>>
        {
            private readonly PlayerContext _context;

            public ArchivedAdvertListQuery(PlayerContext context)
            {
                _context = context;
            }

            public async Task<BaseFilterResult<ArchivedAdvertModel>> Handle(ArchivedAdvertRequest request,
                CancellationToken cancellationToken)
            //Определяет асинхронный метод для обработки запроса на архивные объявления, принимая параметры фильтрации и токен отмены операции.
            {
                var result = new BaseFilterResult<ArchivedAdvertModel>
                {
                    Page = request.Filter.Page,
                    ItemsPerPage = request.Filter.ItemsPerPage
                };
                //Создает объект результата с информацией о странице и количестве элементов на странице на основе полученного запроса.
                var query = _context.Adverts.OrderByDescending(a => a.CreateDate);
                result.Result = await query
                    .Include(a => a.AdLifetimes)
                    .ThenInclude(al => al.Advert)
                    .ThenInclude(a => a.AdHistories)
                    .ThenInclude(ah => ah.Object)
                    .Where(a => a.AdLifetimes.Any(al => al.InArchive) &&
                                !a.AdLifetimes.Any(al => al.DateEnd > DateTime.Now && !al.InArchive))
                    .GetPagedQuery(result.Page, result.ItemsPerPage)
                    .Select(a => new ArchivedAdvertModel
                    {
                        Id = a.Id,
                        Name = a.Name,
                        CreateDate = a.CreateDate,
                        AdvertHistory = a.AdLifetimes.Select(al => new AdvertHistoryModel
                        {
                            DateBegin = al.DateBegin,
                            DateEnd = al.DateEnd,
                            ObjectNames =
                                _context.Playlists
                                    .Where(p => p.Aderts.Select(ad => ad.Advert).Contains(al.Advert) &&
                                                al.DateBegin <= p.PlayingDate && al.DateEnd >= p.PlayingDate)
                                    .Select(p => p.Object.Name)
                                    .Distinct()
                        })
                        //Формирует запрос к базе данных, выбирая архивные рекламные объявления, чьи сроки уже не активны.
                    })
                    .ToListAsync(cancellationToken);

                //TODO Костыль, надо найти решение получше
                foreach (var archivedAdvertModel in result.Result)
                {
                    foreach (var advertHistoryModel in archivedAdvertModel.AdvertHistory)
                    {
                        advertHistoryModel.ObjectNames = advertHistoryModel.ObjectNames.Distinct();
                    }

                    //Цель этого блока кода — гарантировать, что в информации о каждом периоде показа рекламы будут указаны только уникальные объекты показа, что делает данные более чистыми и предотвращает повторение одной и той же информации в пользовательском интерфейсе или в отчетах.
                }
                result.TotalItems = await query.CountAsync(cancellationToken);
                //Подсчитывает общее количество подходящих записей в базе данных для информации о пагинации.
                return result;
            }
        }

        public class ArchivedAdvertRequest : IRequest<BaseFilterResult<ArchivedAdvertModel>>
        {
            public BaseFilterModel Filter { get; set; }
        }

        public class ArchivedAdvertModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public IEnumerable<AdvertHistoryModel> AdvertHistory { get; set; }
            public DateTime CreateDate { get; set; }
        }

        public class AdvertHistoryModel
        {
            public DateTime DateBegin { get; set; }
            public DateTime DateEnd { get; set; }
            public IEnumerable<string> ObjectNames { get; set; } = new List<string>();
        }
        //Здесь определены классы для структурирования запроса и ответа. ArchivedAdvertRequest включает в себя параметры фильтрации, а ArchivedAdvertModel описывает структуру данных рекламного объявления для ответа.
    }
}
