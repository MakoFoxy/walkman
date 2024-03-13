using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Player.Services.Abstractions;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

namespace Player.Services
{
    public class TelegramMessageSender : ITelegramMessageSender
    {
        private readonly ILogger<TelegramMessageSender> _logger;
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramMessageSender(ITelegramConfiguration telegramConfiguration, ILogger<TelegramMessageSender> logger)
        {
            _logger = logger;
            _telegramBotClient = telegramConfiguration.TelegramBotClient;
        }

        public async Task SendLogs(long chatId, byte[] archive, string logsName)
        {
            await using var stream = new MemoryStream(archive);
            await _telegramBotClient.SendDocumentAsync(chatId, new InputOnlineFile(stream, logsName));
        }

        public Task SendSelectionExpired(long chatId, int day, string selectionName, string objectName)
        {
            return SendTextMessageAsync(chatId, $"Подборка {selectionName} на объекте {objectName} отключится через {day} дней");
        }

        public Task SendUserNotFound(long chatId, string message)
        {
            return SendTextMessageAsync(chatId, $"Пользователь с почтой {message} не найден");
        }

        public Task SendUserFound(long chatId, string message)
        {
            return SendTextMessageAsync(chatId, "Ваш аккаунт подключен к телеграмму");
        }

        public Task SendError(long chatId)
        {
            return SendTextMessageAsync(chatId, "Для подключения телеграмма введите свою почту");
        }

        public async Task SendReport(long chatId, byte[] report, string reportName, string message)
        {
            await using var stream = new MemoryStream(report);
            await SendDocumentAsync(chatId, 
                new InputOnlineFile(stream, reportName.Replace("\"", "")), 
                message);
        }

        public Task SendClientConnectionStatusChanged(long chatId, string objectName, DateTime eventDate, bool clientConnected)
        {
            _logger.LogTrace("Client status clientConnected: {ClientConnected}, on {ObjectName}", clientConnected, objectName);
            return SendTextMessageAsync(chatId,
                clientConnected ? $"Клиент {objectName} подключен в {eventDate:dd.MM.yyyy HH:mm:ss}" : $"Клиент {objectName} отключен в {eventDate:dd.MM.yyyy HH:mm:ss}");
        }

        public Task SendPlaylistStarted(long chatId, string objectName, DateTime eventDate)
        {
            _logger.LogTrace("Playlist started at {EventDate} on {ObjectName}", eventDate, objectName);
            return SendTextMessageAsync(chatId, $"Клиент {objectName} запущен эфир в {eventDate:dd.MM.yyyy HH:mm:ss}");
        }
        
        public Task SendPlaylistEnded(long chatId, string objectName, DateTime eventDate)
        {
            _logger.LogTrace("Playlist ended at {EventDate} on {ObjectName}", eventDate, objectName);
            return SendTextMessageAsync(chatId, $"Клиент {objectName} закончил эфир в {eventDate:dd.MM.yyyy HH:mm:ss}");
        }

        public async Task SendTextMessageAsync(
            ChatId chatId,
            string text,
            ParseMode parseMode = ParseMode.Default,
            bool disableWebPagePreview = false,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                await _telegramBotClient.SendTextMessageAsync(chatId, text, parseMode, disableWebPagePreview, disableNotification, replyToMessageId, replyMarkup, cancellationToken);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException e) when(e.ErrorCode == 403)
            {
                _logger.LogWarning(e, "Error with chat id = {ChatId}, chat was banned", chatId);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException e)
            {
                _logger.LogError(e, "Error with chat id = {ChatId}", chatId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when send text message");
            }
        }

        private async Task SendDocumentAsync(
            ChatId chatId,
            InputOnlineFile document,
            string caption = null,
            ParseMode parseMode = ParseMode.Default,
            bool disableNotification = false,
            int replyToMessageId = 0,
            IReplyMarkup replyMarkup = null,
            CancellationToken cancellationToken = default,
            InputMedia thumb = null)
        {
            try
            {
                await _telegramBotClient.SendDocumentAsync(chatId, document, caption, parseMode, disableNotification,
                    replyToMessageId, replyMarkup, cancellationToken, thumb);
            }
            catch (Telegram.Bot.Exceptions.ApiRequestException e)
            {
                _logger.LogError(e, "Error with chat id = {ChatId}", chatId);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error when send text document");
            }
        }
    }
}
