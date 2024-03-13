using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace Player.Services.Abstractions
{
    public interface ITelegramMessageSender
    {
        Task SendLogs(long chatId, byte[] archive, string logsName);
        Task SendSelectionExpired(long chatId, int day, string selectionName, string objectName);
        Task SendUserNotFound(long chatId, string message);
        Task SendUserFound(long chatId, string message);
        Task SendError(long chatId);
        Task SendReport(long chatId, byte[] report, string reportName, string message);
        Task SendClientConnectionStatusChanged(long chatId, string objectName, DateTime eventDate, bool clientConnected);
        Task SendPlaylistStarted(long chatId, string objectName, DateTime eventDate);
        Task SendPlaylistEnded(long chatId, string objectName, DateTime eventDate);
        Task SendTextMessageAsync(ChatId chatId, string text, ParseMode parseMode = ParseMode.Default, bool disableWebPagePreview = false, bool disableNotification = false, int replyToMessageId = 0, IReplyMarkup replyMarkup = null, CancellationToken cancellationToken = default);
    }
}
