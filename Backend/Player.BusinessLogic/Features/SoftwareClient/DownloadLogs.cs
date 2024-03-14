using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Player.BusinessLogic.Hubs;
using Player.ClientIntegration;
using Player.ClientIntegration.System;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class DownloadLogs
    {
        public class Handler : IRequestHandler<Query, Response>
        {
            private readonly IHubContext<PlayerClientHub> _playerClientHub;
            private readonly IUserManager _userManager;

            public Handler(IHubContext<PlayerClientHub> playerClientHub, IUserManager userManager)
            {
                _playerClientHub = playerClientHub;
                _userManager = userManager;
                //Конструктор класса Handler принимает контекст хаба SignalR PlayerClientHub для взаимодействия с клиентами и сервис IUserManager для работы с информацией о пользователях.
            }

            public async Task<Response> Handle(Query query, CancellationToken cancellationToken)
            {
                if (!PlayerClientHub.ConnectedClients.Any(cc => cc.Id == query.Id))
                {
                    return new Response
                    {
                        IsOnline = false,
                        Message = "Object is offline",
                    };
                }

                var currentUser = await _userManager.GetCurrentUser(cancellationToken);
                await _playerClientHub.Clients.Group(query.Id.ToString()).SendAsync(OnlineEvents.DownloadLogs, new DownloadLogsRequest
                {
                    UserId = currentUser.Id,
                    From = query.From,
                    To = query.To,
                    DbLogs = query.DbLogs,
                }, cancellationToken);

                return new Response
                {
                    IsOnline = true,
                    Message = "Request has been sent",
                };
                //Асинхронно обрабатывает запрос Query, содержащий идентификатор клиентского устройства, диапазон дат для логов и флаг DbLogs, указывающий, нужно ли включать логи из базы данных. Если клиент в сети, метод отправляет запрос на клиентское устройство через SignalR хаб. Возвращает статус онлайн и сообщение об отправке запроса.
            }
        }

        public class Query : IRequest<Response>
        {
            public Guid Id { get; set; }
            public DateTime From { get; set; }
            public DateTime To { get; set; }
            public bool DbLogs { get; set; }
            //Query — класс запроса, содержащий идентификатор клиента (Id), диапазон дат (From, To) и флаг (DbLogs), определяющий, нужны ли логи базы данных.
        }

        public class Response
        {
            public bool IsOnline { get; set; }
            public string Message { get; set; }
            //Response — класс ответа, сообщающий о том, находится ли клиент в сети (IsOnline) и текстовое сообщение (Message) об результате операции.
        }
    }
    //Реализация и использование:

    //     Данный код может использоваться для создания веб-API или другого интерфейса системы, который позволяет администраторам отправлять запросы на загрузку логов с клиентских устройств для анализа или устранения неполадок.
    //     SignalR используется для мгновенного взаимодействия с клиентским устройством, что позволяет оперативно реагировать на запросы администратора.

    // Этот код обеспечивает инструменты для администрирования и мониторинга программных клиентов в системе, позволяя получать журналы событий для диагностики и анализа.
}