using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.BusinessLogic.Hubs;
using Player.DataAccess;
using Player.DTOs;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class OnlineList
    {
        public class Handler : IRequestHandler<Query, List<OnlineClient>>
        {
            //Handler — это класс обработчика запросов, который реализует интерфейс IRequestHandler из MediatR. Этот обработчик предназначен для обработки запросов типа Query и возвращает список объектов OnlineClient.
            private readonly PlayerContext _context;
            private readonly IMapper _mapper;

            public Handler(PlayerContext context, IMapper mapper)
            {
                _context = context;
                _mapper = mapper;
                //Конструктор класса Handler, принимает контекст базы данных PlayerContext и объект маппера IMapper для выполнения запросов к базе данных и отображения результатов запросов.
            }

            public async Task<List<OnlineClient>> Handle(Query request, CancellationToken cancellationToken)
            {
                var connectedClients = PlayerClientHub.ConnectedClients.Select(cc => cc.Id).ToList();
                var onlineClients = await _context.Objects.Where(o => connectedClients.Contains(o.Id))
                    .Select(o => new OnlineClient
                    {
                        Id = o.Id,
                        Name = o.Name
                    })
                    .ToListAsync(cancellationToken);

                foreach (var onlineClient in onlineClients)
                {
                    var connectedClient = PlayerClientHub.ConnectedClients.First(cc => cc.Id == onlineClient.Id);
                    onlineClient.IpAddress = connectedClient.IpAddress;
                    onlineClient.Version = connectedClient.Version;
                }

                return onlineClients;
                //Асинхронный метод обработки запроса Query. Сначала получает список идентификаторов подключенных клиентов из статического списка ConnectedClients в PlayerClientHub, затем выполняет запрос к базе данных для получения объектов (клиентов), которые присутствуют в этом списке. Для каждого онлайн-клиента из базы данных добавляет IP-адрес и версию из соответствующего объекта в ConnectedClients.
            }
        }

        public class Query : IRequest<List<OnlineClient>>
        {
            //Query — это пустой класс запроса, который используется для активации операции получения списка онлайн-клиентов.
        }

        public class OnlineClient : SimpleDto
        {
            public string IpAddress { get; set; }
            public string Version { get; set; }
            //OnlineClient — это DTO (Data Transfer Object), который содержит информацию об онлайн-клиентах: идентификатор, имя, IP-адрес и версию.
        }
    }
    //     Этот код может использоваться для мониторинга активных клиентских подключений к системе в реальном времени, например, в административной панели управления. Это позволяет администраторам видеть, какие клиенты в настоящее время подключены, и получать информацию об их IP-адресах и версиях программного обеспечения.

    // Таким образом, администраторы системы могут использовать этот функционал для мониторинга и управления подключенными клиентами, а также для решения возникающих технических проблем или обновления клиентских приложений.
}