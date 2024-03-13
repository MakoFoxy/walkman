using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.SignalR;
using Player.BusinessLogic.Hubs;
using Player.ClientIntegration;
using Player.ClientIntegration.MusicTrack;
using Player.DataAccess;
using Player.Domain;
using Player.Services;

namespace Player.BusinessLogic.Features.Objects
{
    public class BanMusic
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly UserManager _userManager;
            private readonly IHubContext<PlayerClientHub> _hubContext;

            public Handler(PlayerContext context, UserManager userManager, IHubContext<PlayerClientHub> hubContext)
            {
                _context = context;
                _userManager = userManager;
                _hubContext = hubContext;
                //В конструкторе класса Handler инъектируются контекст базы данных (PlayerContext), менеджер пользователей (UserManager) и контекст хаба SignalR (IHubContext<PlayerClientHub>), используемый для отправки сообщений клиентам.
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var user = await _userManager.GetCurrentUser(cancellationToken);
                _context.BannedMusicInObject.Add(new BannedMusicInObject
                {
                    ObjectId = command.ObjectId,
                    MusicTrackId = command.MusicId,
                    UserId = user.Id,
                });
                await _context.SaveChangesAsync(cancellationToken);

                await _hubContext.Clients.Group(command.ObjectId.ToString())
                    .SendAsync(OnlineEvents.MusicBanned, new BanMusicTrackDto
                    {
                        MusicId = command.MusicId,
                        ObjectId = command.ObjectId,
                    }, cancellationToken);

                return Unit.Value;
                //Метод Handle асинхронно обрабатывает команду бана музыкального трека. Внутри метода происходит:
                // Получение текущего пользователя через _userManager.
                // Добавление нового экземпляра BannedMusicInObject в контекст базы данных, указывающего на блокировку трека для определенного объекта пользователем.
                // Сохранение изменений в базе данных.
                // Оповещение всех клиентов, находящихся в группе с ID объекта (например, комнаты или места), через SignalR о том, что музыкальный трек был заблокирован, используя метод SendAsync.
            }
        }

        public class Command : IRequest<Unit>
        {
            public Guid MusicId { get; set; }
            public Guid ObjectId { get; set; }
        }
    }
    //В итоге, этот код позволяет блокировать музыкальные треки в определенных объектах системы и моментально уведомлять об этом всех соответствующих клиентов в реальном времени.
}