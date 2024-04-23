using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Features.Adverts.Models;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Adverts
{
    public class List
    {
        public class Handler : IRequestHandler<Query, AdvertFilterResult>

        //Здесь определяется класс List и его внутренний класс Handler, который реализует интерфейс IRequestHandler из MediatR. Handler обрабатывает запросы типа Query и возвращает результат типа AdvertFilterResult.
        //Ваш код определяет класс Handler, который реализует интерфейс IRequestHandler из библиотеки MediatR. Этот класс предназначен для обработки запросов на получение фильтрованного списка рекламных объявлений, основанных на критериях, указанных в объекте Query. Вот более детальное объяснение каждого из его компонентов и шагов:
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;
            private readonly IUserManager _userManager;

            public Handler(PlayerContext context, IMapper mapper, IUserManager userManager)
            {
                _context = context; //PlayerContext: Используется для доступа к базе данных через Entity Framework.
                _mapper = mapper; //IMapper: Интерфейс AutoMapper используется для проецирования данных между типами.
                _userManager = userManager; //IUserManager: Сервис для работы с пользователями, например, для получения информации о текущем пользователе или его организации.
            }

            public async Task<AdvertFilterResult> Handle(Query listRequest, CancellationToken cancellationToken)
            //Этот асинхронный метод реализует логику получения фильтрованного списка рекламных объявлений на основе критериев поиска, заданных в listRequest.
            {
                var result = new AdvertFilterResult //Получение параметров запроса: Извлекает параметры пагинации и другие фильтры из объекта запроса.
                {
                    Page = listRequest.Filter.Page,
                    ItemsPerPage = listRequest.Filter.ItemsPerPage
                };

                var query = _context.Adverts.OrderByDescending(a => a.CreateDate) //Формирование базового запроса: Начинается с формирования LINQ-запроса к сущностям Adverts, который уже фильтрует активные (неархивированные и неистекшие) объявления.
                    .Where(a => a.AdLifetimes.Any(al => al.DateEnd > DateTime.Now && !al.InArchive));
                // Создает начальный результат фильтрации и формирует запрос к базе данных, выбирая объявления, которые не архивированы и срок действия которых не истек.
                if (listRequest.Filter.ObjectId.HasValue) //Применение дополнительных фильтров: Если в запросе указан ObjectId, добавляется фильтр по этому идентификатору. Для пагинации используется метод расширения GetPagedQuery, который применяет ограничения страницы и количество элементов на странице.
                {
                    result.ObjectId = listRequest.Filter.ObjectId;
                    query = query.Where(a => a.AdTimes.Any(at => at.ObjectId == listRequest.Filter.ObjectId));
                }

                if (listRequest.Filter.Page.HasValue && listRequest.Filter.ItemsPerPage.HasValue)
                {
                    query = query.GetPagedQuery(result.Page, result.ItemsPerPage);
                }
                //Применяет фильтры к запросу на основе заданных условий, например, по объекту показа рекламы и пагинации.
                var user = await _userManager.GetCurrentUser(cancellationToken);

                if (user.Role.RolePermissions.Select(rp => rp.Permission) //Обработка прав доступа: Проверяет права текущего пользователя на доступ к объектам и на основе этого корректирует запрос, чтобы включить только те объявления, которые связаны с организацией пользователя или доступными объектами.
                    .Any(p => p.Code == Permission.PartnerAccessToObject))
                {
                    //Получает текущего пользователя и проверяет его разрешения на доступ к объектам.
                    var organization = await _userManager.GetUserOrganization(cancellationToken);
                    var objects = await _userManager.GetUserObjects(cancellationToken);
                    query = query.Where(a => a.Organization == organization || //Проверка организации: Запрос проверяет, принадлежит ли объявление к организации текущего пользователя. Это осуществляется сравнением свойства Organization объявления с объектом organization, который представляет организацию текущего пользователя.
                                             _context.AdTimes.Any(at => at.Advert == a && //Сначала проверяется, существуют ли в таблице AdTimes записи, где Advert равно текущему объявлению (at.Advert == a). AdTimes представляет собой связь между объявлениями и объектами, где они должны быть показаны.
                                                                        objects.Any(o => o == at.Object)) //Затем для каждой найденной записи в AdTimes проверяется, содержится ли объект этой записи (at.Object) в списке объектов, доступных пользователю (objects). Этот список objects получен из базы данных и представляет объекты, к которым у пользователя есть доступ.
                    //Этот фрагмент кода устанавливает дополнительные условия для LINQ-запроса, который извлекает рекламные объявления из базы данных. Он направлен на обеспечение того, чтобы в результаты запроса попадали только те объявления, которые соответствуют определенным требованиям связанным с организацией пользователя и его доступом к объектам. Вот более детальное объяснение:
                    );
                }

                result.Result = await query
                    .ProjectTo<AdvertModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                //Выполняет запрос, преобразовывая его результаты в список моделей AdvertModel с помощью AutoMapper.
                //TODO Костыль, надо найти решение получше //Проекция и выполнение запроса: Использует AutoMapper для проецирования результатов запроса в модели AdvertModel и выполняет запрос асинхронно для получения результатов.
                foreach (var advertModel in result.Result)
                {
                    advertModel.Objects = advertModel.Objects.DistinctBy(o => o.Id);
                }
                result.TotalItems = await query.CountAsync(cancellationToken); //Обработка дубликатов и подсчёт общего количества: Удаляет дубликаты в полученных списках объектов для каждой рекламы и подсчитывает общее количество подходящих объявлений в базе данных.
                //Удаляет дубликаты в списках объектов для каждого рекламного объявления и подсчитывает общее количество объявлений по заданным фильтрам.
                return result; //    AdvertFilterResult: Возвращаемый объект содержит информацию о результате запроса, включая список объявлений, общее количество объявлений, текущую страницу и количество элементов на странице.
            }
            //Таким образом, этот код предоставляет детализированный и настраиваемый способ получения списка рекламных объявлений из базы данных с учетом различных параметров запроса и пользовательских разрешений.Этот подход обеспечивает гибкое и масштабируемое решение для управления фильтрацией рекламных объявлений, что позволяет удовлетворять различные бизнес-требования в контексте доступа и видимости рекламных материалов в зависимости от прав пользователей и структуры данных в системе.
        }

        public class Query : IRequest<AdvertFilterResult>
        {
            public AdvertFilterModel Filter { get; set; }
        }

        public class AdvertFilterModel : BaseFilterModel
        {
            public Guid? ObjectId { get; set; }
        }

        public class AdvertFilterResult : BaseFilterResult<AdvertModel>
        {
            public Guid? ObjectId { get; set; }
        }

        //Эти классы определяют структуру запроса, модель фильтра и результат фильтрации для операции списка объявлений. AdvertFilterModel содержит критерии фильтрации, Query используется для передачи этих критериев в обработчик, а AdvertFilterResult является контейнером для результата запроса.
    }
}