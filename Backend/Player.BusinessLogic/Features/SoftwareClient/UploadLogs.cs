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
                //Конструктор принимает контекст базы данных PlayerContext и сервис отправки сообщений в Telegram ITelegramMessageSender. Эти зависимости используются для выполнения основных задач обработчика.
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
                //Это основной метод класса обработчика, который асинхронно обрабатывает команду Command. В нем из базы данных извлекается Telegram chatId пользователя, который запрашивал логи, и затем через сервис ITelegramMessageSender логи отправляются этому пользователю.
            }
        }

        public class Command : IRequest<Unit>
        {
            public DownloadLogsResponse DownloadLogsResponse { get; set; }
            //Command — класс команды, который содержит все необходимые данные для выполнения запроса. В этом случае он содержит ответ на запрос логов DownloadLogsResponse, который включает в себя сам файл логов и дополнительные данные, такие как имя файла.
        }
    }
    //Этот код используется в системах, где требуется функционал отправки логов системы или ошибок напрямую пользователю через мессенджер (в данном случае Telegram). Это может быть полезно для быстрой диагностики и устранения проблем в системе путем анализа логов, полученных от клиентских устройств или программных компонентов.
}