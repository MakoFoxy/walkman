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
        {
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;
            private readonly IUserManager _userManager;

            public Handler(PlayerContext context, IMapper mapper, IUserManager userManager)
            {
                _context = context;
                _mapper = mapper;
                _userManager = userManager;
            }

            public async Task<AdvertFilterResult> Handle(Query listRequest, CancellationToken cancellationToken)
            //Этот асинхронный метод реализует логику получения фильтрованного списка рекламных объявлений на основе критериев поиска, заданных в listRequest.
            {
                var result = new AdvertFilterResult
                {
                    Page = listRequest.Filter.Page,
                    ItemsPerPage = listRequest.Filter.ItemsPerPage
                };

                var query = _context.Adverts.OrderByDescending(a => a.CreateDate)
                    .Where(a => a.AdLifetimes.Any(al => al.DateEnd > DateTime.Now && !al.InArchive));
                // Создает начальный результат фильтрации и формирует запрос к базе данных, выбирая объявления, которые не архивированы и срок действия которых не истек.
                if (listRequest.Filter.ObjectId.HasValue)
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

                if (user.Role.RolePermissions.Select(rp => rp.Permission)
                    .Any(p => p.Code == Permission.PartnerAccessToObject))
                {
                    //Получает текущего пользователя и проверяет его разрешения на доступ к объектам.
                    var organization = await _userManager.GetUserOrganization(cancellationToken);
                    var objects = await _userManager.GetUserObjects(cancellationToken);
                    query = query.Where(a => a.Organization == organization ||
                                             _context.AdTimes.Any(at => at.Advert == a &&
                                                                        objects.Any(o => o == at.Object))
                    );
                }

                result.Result = await query
                    .ProjectTo<AdvertModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);
                //Выполняет запрос, преобразовывая его результаты в список моделей AdvertModel с помощью AutoMapper.
                //TODO Костыль, надо найти решение получше
                foreach (var advertModel in result.Result)
                {
                    advertModel.Objects = advertModel.Objects.DistinctBy(o => o.Id);
                }
                result.TotalItems = await query.CountAsync(cancellationToken);
                //Удаляет дубликаты в списках объектов для каждого рекламного объявления и подсчитывает общее количество объявлений по заданным фильтрам.
                return result;
            }
            //Таким образом, этот код предоставляет детализированный и настраиваемый способ получения списка рекламных объявлений из базы данных с учетом различных параметров запроса и пользовательских разрешений.
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