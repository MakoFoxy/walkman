using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.Domain;
using Player.DTOs;
using Player.Services;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Selections
{
    public class List
    {
        public class Handler : IRequestHandler<Query, SelectionFilterResult>
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;
            private readonly IUserManager _userManager;

            public Handler(PlayerContext context,
                IMapper mapper,
                IUserManager userManager)
            {
                _context = context;
                _mapper = mapper;
                _userManager = userManager;
                //Handler - класс обработчика, который реализует интерфейс IRequestHandler для обработки запроса Query и возврата результата SelectionFilterResult. В конструкторе класса используются контекст базы данных PlayerContext, маппер IMapper и менеджер пользователей IUserManager.
            }

            public async Task<SelectionFilterResult> Handle(Query request,
                CancellationToken cancellationToken = default)
            {
                var permissions = await _userManager.GetCurrentUserPermissions(cancellationToken); //Получение разрешений пользователя:
                // Извлекает разрешения текущего пользователя через _userManager.GetCurrentUserPermissions. Это необходимо, чтобы определить, какие данные доступны для просмотра и какие фильтры применят
                var query = _context.Selections.AsQueryable(); //Создаёт начальный запрос к таблице Selections в базе данных через контекст _context. Запрос пока не выполняется, так как IQueryable отложен по своей природе.

                if (permissions.Any(p => p.Code == Permission.PartnerAccessToObject)) //    Если у пользователя есть доступ к определённой организации (Permission.PartnerAccessToObject), метод дополнительно фильтрует подборки, принадлежащие этой организации или помеченные как публичные (IsPublic).
                {
                    var organization = await _userManager.GetUserOrganization(cancellationToken);
                    query = query.Where(s => s.Organization == organization || s.IsPublic);
                }

                var result = new SelectionFilterResult
                {
                    Page = request.Filter?.Page,
                    ItemsPerPage = request.Filter?.ItemsPerPage
                    //    Возвращает объект SelectionFilterResult, который содержит список отфильтрованных и отсортированных подборок и информацию для пагинации (номер страницы, количество элементов на странице, общее количество элементов).
                };

                if (request.Filter?.Actual != null) //Actual: Если установлен фильтр актуальности, запрос фильтруется по датам начала и окончания.
                {
                    if (request.Filter.Actual.Value)
                    {
                        var now = DateTimeOffset.Now;
                        query = query.Where(s => s.DateBegin <= now && (s.DateEnd == null || s.DateEnd >= now));
                    }
                }

                if (!string.IsNullOrWhiteSpace(request.Filter?.SearchText)) //SearchText: Если указан текст для поиска, фильтрует подборки по наличию этого текста в названии.
                    query = query.Where(s => s.Name.Contains(request.Filter.SearchText));

                if (request.Filter?.ObjectId != null) //ObjectId: Если указан идентификатор объекта, фильтрует подборки, связанные с этим объектом.
                {
                    query = query.Where(s =>
                        s.Objects.Select(so => so).Any(o => o.ObjectId == request.Filter.ObjectId));
                }

                result.Result = await query
                    .GetPagedQuery(result.Page, result.ItemsPerPage) //Применяет пагинацию (GetPagedQuery) и выборку данных, преобразуя их к модели SelectionListModel с использованием AutoMapper (_mapper).
                    .ProjectTo<SelectionListModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                result.TotalItems = await query.CountAsync(cancellationToken); //    Возвращает объект SelectionFilterResult, который содержит список отфильтрованных и отсортированных подборок и информацию для пагинации (номер страницы, количество элементов на странице, общее количество элементов).

                return result;

                //                 Этот асинхронный метод обрабатывает запрос на получение списка подборок. Он фильтрует подборки в зависимости от разрешений текущего пользователя, даты актуальности, текста поиска и ID объекта. Затем использует автомаппинг для преобразования результатов запроса в список моделей SelectionListModel и рассчитывает пагинацию.
                // Фильтрация и пагинация:

                // В зависимости от разрешений пользователя, даты, текста поиска и объекта формируется запрос к базе данных. Результат запроса затем пагинируется и преобразуется в список моделей подборок.
                // Модели:

                //     SelectionListModel - модель для представления музыкальной подборки в списке.
                //     SelectionFilterModel - модель фильтрации подборок, содержит критерии, по которым выполняется поиск и фильтрация подборок.
                //     SelectionFilterResult - результат выполнения запроса, содержит список подборок и информацию для пагинации.
            }
        }

        public class SelectionListModel : SimpleDto
        {
        }

        public class SelectionFilterModel : BaseFilterModel
        {
            public Guid? ObjectId { get; set; }
            public string SearchText { get; set; }
            public bool? Actual { get; set; }
        }

        public class Query : IRequest<SelectionFilterResult>
        {
            public SelectionFilterModel Filter { get; set; }
            //Класс запроса, который содержит модель фильтра SelectionFilterModel для применения фильтрации к списку подборок.
        }

        public class SelectionFilterResult : BaseFilterResult<SelectionListModel>
        {
        }
    }
    // Этот код обеспечивает получение фильтрованного и пагинированного списка музыкальных подборок, что может быть использовано для отображения на пользовательском интерфейсе или других частях системы. Пользователи могут искать подборки по различным критериям, включая актуальность, название и привязку к определенному объекту.
}
