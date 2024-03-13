using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.DataAccess.Extensions;
using Player.DTOs;

namespace Player.BusinessLogic.Features.Managers
{
    public class List
    {
        public class Handler : IRequestHandler<Query, BaseFilterResult<ManagerShortInfoModel>>
        {
            //Handler обрабатывает запросы на получение списка менеджеров. Он реализует интерфейс IRequestHandler из MediatR, который в данном случае обрабатывает запросы Query и возвращает объект BaseFilterResult<ManagerShortInfoModel>, содержащий информацию для пагинации и список менеджеров.
            private readonly PlayerContext _context;

            public Handler(PlayerContext context)
            {
                _context = context;
                //Конструктор класса Handler, инициализирующий контекст базы данных, что позволяет взаимодействовать с данными в базе.
            }

            public async Task<BaseFilterResult<ManagerShortInfoModel>> Handle(Query request,
                CancellationToken cancellationToken)
            {
                var result = new BaseFilterResult<ManagerShortInfoModel>
                {
                    Page = request.Filter.Page,
                    ItemsPerPage = request.Filter.ItemsPerPage
                };

                var query = _context.GetValidManagers().OrderBy(m => m.User.Email);

                result.Result = await query
                    .GetPagedQuery(result.Page, result.ItemsPerPage)
                    .Select(m => new ManagerShortInfoModel
                    {
                        Id = m.Id,
                        LastName = m.User.LastName,
                        FirstName = m.User.FirstName,
                        SecondName = m.User.SecondName,
                        Role = new SimpleDto
                        {
                            Id = m.User.Role.Id,
                            Name = m.User.Role.Name
                        },
                        PhoneNumber = m.User.PhoneNumber,
                        TelegramChatIsAvailable = m.User.TelegramChatId.HasValue
                    })
                    .ToListAsync(cancellationToken);

                return result;
                //Асинхронный метод Handle обрабатывает запрос на получение списка менеджеров. Он использует информацию о фильтрации из request, чтобы получить данные из базы данных, и возвращает результаты, удовлетворяющие этим параметрам.

                // Внутри метода:

                //     Инициализируется объект result, который хранит информацию о странице и количестве элементов на странице.
                //     С помощью метода GetValidManagers извлекаются актуальные данные менеджеров.
                //     Применяется пагинация к запросу с помощью GetPagedQuery.
                //     Данные менеджеров проецируются в ManagerShortInfoModel и загружаются асинхронно.
            }
            //Этот код позволяет извлекать и представлять данные о менеджерах в удобном для пользователя виде, поддерживая функциональность пагинации и возможность фильтрации.
        }

        public class Query : IRequest<BaseFilterResult<ManagerShortInfoModel>>
        {
            public BaseFilterModel Filter { get; set; }
            //Query служит для передачи параметров фильтрации в запрос. Реализует IRequest с ожидаемым ответом в виде списка моделей менеджеров.
        }

        public class ManagerShortInfoModel
        {
            public Guid Id { get; set; }
            public string LastName { get; set; }
            public string FirstName { get; set; }
            public string SecondName { get; set; }
            public SimpleDto Role { get; set; }
            public string PhoneNumber { get; set; }
            public bool TelegramChatIsAvailable { get; set; }
            //ManagerShortInfoModel является моделью данных, используемой для передачи сокращенной информации о менеджере, включая его персональные данные, роль и информацию о доступности чата в Telegram.
        }
    }
}