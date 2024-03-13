using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.Organizations
{
    public class List
    {
        public class Handler : IRequestHandler<Query, BaseFilterResult<OrganizationShortInfoModel>>
        {
            private readonly PlayerContext _context;
            private readonly IUserManager _userManager;

            public Handler(PlayerContext context, IUserManager userManager)
            {
                _context = context;
                _userManager = userManager;
                // Конструктор принимает контекст данных и менеджер пользователей, которые инъектируются через механизм внедрения зависимостей.
            }

            public async Task<BaseFilterResult<OrganizationShortInfoModel>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var result = new BaseFilterResult<OrganizationShortInfoModel>
                {
                    Page = request.Filter.Page,
                    ItemsPerPage = request.Filter.ItemsPerPage
                };

                var query = _context.Organizations.AsQueryable();

                var permissions = await _userManager.GetCurrentUserPermissions(cancellationToken);

                if (permissions.Any(p => p.Code == Permission.PartnerAccessToObject))
                {
                    var userOrganization = await _userManager.GetUserOrganization(cancellationToken);
                    query = query.Where(o => o == userOrganization);
                }

                result.Result = await query
                    .OrderBy(o => o.Name)
                    .GetPagedQuery(result.Page, result.ItemsPerPage)
                    .Select(o => new OrganizationShortInfoModel
                    {
                        Id = o.Id,
                        Name = o.Name,
                        Bin = o.Bin,
                        Address = o.Address,
                        Bank = o.Bank,
                        Iik = o.Iik,
                        Phone = o.Phone,
                        UserCount = o.Clients.Count,
                    })
                    .ToListAsync(cancellationToken);

                return result;

                //                 Этот асинхронный метод обрабатывает запрос на получение списка организаций. Он фильтрует организации в зависимости от разрешений текущего пользователя. Если у пользователя есть разрешение PartnerAccessToObject, он получит доступ только к своей организации.
                // Процесс фильтрации:

                //     Формируется начальный запрос к базе данных для выборки всех организаций.
                //     Если у пользователя есть соответствующие разрешения, запрос модифицируется для фильтрации организаций.
                //     Данные пагинируются и сортируются по имени организации.
                //     Результаты запроса проецируются в модель OrganizationShortInfoModel.
            }
        }

        public class Query : IRequest<BaseFilterResult<OrganizationShortInfoModel>>
        {
            public BaseFilterModel Filter { get; set; }
        }

        public class OrganizationShortInfoModel
        {
            public Guid Id { get; set; }
            public string Name { get; set; }
            public string Bin { get; set; }
            public string Address { get; set; }
            public string Bank { get; set; }
            public string Iik { get; set; }
            public string Phone { get; set; }
            public int UserCount { get; set; }
            // OrganizationShortInfoModel является моделью данных (DTO), используемой для передачи сокращенной информации об организациях, такой как идентификатор, имя, адрес, банковские данные и количество связанных пользователей.
        }
    }
    // Этот код позволяет пользователям системы получать список организаций в зависимости от их прав доступа и предоставляет основную информацию об этих организациях, что может быть использовано для отображения списка организаций или для выполнения других действий в пользовательском интерфейсе.
}