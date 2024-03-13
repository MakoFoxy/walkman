using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Player.ClientIntegration.System;
using Player.DataAccess;
using Player.Services.Abstractions;

namespace Player.BusinessLogic.Features.SoftwareClient
{
    public class UploadLogs
    {
        public class Handler : IRequestHandler<Command>
        {
            private readonly PlayerContext _context;
            private readonly ITelegramMessageSender _telegramMessageSender;

            public Handler(PlayerContext context, ITelegramMessageSender telegramMessageSender)
            {
                _context = context;
                _telegramMessageSender = telegramMessageSender;
            }

            public async Task<Unit> Handle(Command command, CancellationToken cancellationToken)
            {
                var chatId = await _context.Users
                    .Where(u => u.Id == command.DownloadLogsResponse.DownloadLogsRequest.UserId)
                    .Select(u => u.TelegramChatId)
                    .SingleAsync(cancellationToken);

                await _telegramMessageSender.SendLogs(chatId.Value,
                    Convert.FromBase64String(command.DownloadLogsResponse.File.Body),
                    command.DownloadLogsResponse.File.Name);
                
                return Unit.Value;
            }
        }

        public class Command : IRequest<Unit>
        {
            public DownloadLogsResponse DownloadLogsResponse { get; set; }
        }
    }
}