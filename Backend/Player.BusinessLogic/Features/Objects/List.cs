using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;
using ObjectModel = Player.BusinessLogic.Features.Objects.Models.ObjectModel;

namespace Player.BusinessLogic.Features.Objects
{
    public class List
    {
        public class Handler : IRequestHandler<Query, BaseFilterResult<ObjectModel>>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;
            private readonly IUserManager _userManager;

            public Handler(PlayerContext context, IMapper mapper, IUserManager userManager)
            {
                _context = context;
                _mapper = mapper;
                _userManager = userManager;
                //Конструктор инициализирует зависимости класса: PlayerContext для доступа к базе данных, IMapper для преобразования сущностей базы данных в DTO и IUserManager для работы с информацией о текущем пользователе.
            }

            public async Task<BaseFilterResult<ObjectModel>> Handle(Query request,
                CancellationToken cancellationToken = default)
            {//Метод Handle возвращает объект BaseFilterResult<ObjectModel>, который содержит отфильтрованный список объектов (Result), а также информацию для пагинации (Page, ItemsPerPage, TotalItems), что позволяет клиенту удобно отображать данные с поддержкой разбивки на страницы. Результат: Возвращает BaseFilterResult<ObjectModel>, который содержит информацию о объектах, включая их идентификаторы, названия, адреса и другие свойства. В ответе также указывается общее количество объектов, которые соответствуют критериям фильтрации.
                var result = new BaseFilterResult<ObjectModel>
                {
                    Page = request.Filter.Page,
                    ItemsPerPage = request.Filter.ItemsPerPage
                };

                var query = _context.Objects.AsQueryable();

                if (!string.IsNullOrWhiteSpace(request.Filter.Name))
                {
                    query = query.Where(o => o.Name.ToLower().Contains(request.Filter.Name.ToLower()));
                    //В этом участке кода проверяется, указано ли в запросе имя объекта (request.Filter.Name). Если имя указано, запрос к базе данных модифицируется так, чтобы выбирать только те объекты, имя которых содержит указанную строку. Сравнение происходит без учета регистра, что обеспечивает более гибкий поиск.
                }

                if (request.Filter.IsOnline)
                {
                    query = query.Where(o => o.IsOnline);
                }

                var permissions = await _userManager.GetCurrentUserPermissions(cancellationToken);

                if (permissions.Any(p => p.Code == Permission.PartnerAccessToObject))
                {//Также в обработчике предусмотрена логика для работы с правами пользователя, что позволяет фильтровать объекты в зависимости от организации пользователя, если у пользователя есть соответствующие права (Permission.PartnerAccessToObject). Это делает систему гибкой и безопасной, ограничивая доступ к объектам на основе роли и организации пользователя.
                    var organization = await _userManager.GetUserOrganization(cancellationToken);

                    query = query.Where(o => _context.Organizations
                        .Where(org => org == organization)
                        .SelectMany(org => org.Clients)
                        .SelectMany(c => c.User.Objects)
                        .Select(uo => uo.Object)
                        .Contains(o));
                }

                result.Result = await query
                    .GetPagedQuery(result.Page, result.ItemsPerPage)
                    .OrderBy(o => o.Name)
                    .Select(info => new ObjectModel
                    {
                        Id = info.Id,
                        Name = info.Name,
                        ActualAddress = info.ActualAddress,
                        BeginTime = info.BeginTime,
                        EndTime = info.EndTime,
                        FirstPerson = null,
                        Loading = info.Playlists.Where(p => p.PlayingDate == request.Filter.Date.Date)
                            .Select(p => p.Loading).SingleOrDefault(),
                        UniqueAdvertCount = info.Playlists
                            .Where(p => p.PlayingDate == request.Filter.Date.Date)
                            .Select(p => p.UniqueAdvertsCount).SingleOrDefault(),
                        AllAdvertCount = info.Playlists
                            .Where(p => p.PlayingDate == request.Filter.Date.Date)
                            .Select(p => p.AdvertsCount).SingleOrDefault(),
                        Overloaded = info.Playlists
                            .Where(p => p.PlayingDate == request.Filter.Date.Date)
                            .Select(p => p.Overloaded).SingleOrDefault(),
                        PlaylistExist = info.Playlists.Any(p => p.PlayingDate == request.Filter.Date.Date),
                        IsOnline = info.IsOnline,
                    })
                    .ToListAsync(cancellationToken);

                result.TotalItems = await query.CountAsync(cancellationToken);

                return result;
                // Это основной метод обработчика, который асинхронно обрабатывает запрос на получение списка объектов, применяя фильтры, указанные в request.Filter, и возвращает отфильтрованный и упорядоченный список объектов вместе с информацией о пагинации.

                // В методе происходит следующее:

                //     Создается и инициализируется объект результата BaseFilterResult<ObjectModel>.
                //     Формируется начальный запрос к базе данных на выборку объектов.
                //     Применяются фильтры по имени и статусу IsOnline, если они указаны в запросе.
                //     Для пользователей с определенными правами фильтруются объекты, доступные для их организации.
                //     Результат запроса трансформируется в список объектов модели ObjectModel, включая расчетные значения, такие как количество уникальных рекламных блоков и проверка на перегрузку плейлистов.
                //     Вычисляется общее количество объектов, удовлетворяющих условиям фильтрации.

                // Классы Query, ObjectFilterModel и ObjectModel:

                //     Query содержит фильтры для выборки объектов.
                //     ObjectFilterModel определяет структуру фильтров, применяемых к запросу.
                //     ObjectModel определяет структуру данных, которая будет возвращена клиенту.
            }
        }

        public class Query : IRequest<BaseFilterResult<ObjectModel>>
        {
            public ObjectFilterModel Filter { get; set; }
        }

        public class ObjectFilterModel : BaseFilterModel
        {
            public DateTime Date { get; set; }
            public string Name { get; set; }
            public bool IsOnline { get; set; }
        }
        // Этот код позволяет пользователям системы эффективно фильтровать и просматривать список объектов в зависимости от своих прав и нужд, предоставляя актуальную информацию о состоянии объектов на текущую дату.
    }
}
