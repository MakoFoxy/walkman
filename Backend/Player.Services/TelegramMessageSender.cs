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
    { //Этот код определяет класс TelegramMessageSender, который реализует интерфейс ITelegramMessageSender. Этот класс предназначен для отправки сообщений и документов в Telegram из вашего приложения. Вот как он работает:
        private readonly ILogger<TelegramMessageSender> _logger;
        private readonly ITelegramBotClient _telegramBotClient;

        public TelegramMessageSender(ITelegramConfiguration telegramConfiguration, ILogger<TelegramMessageSender> logger)
        {
            _logger = logger;
            _telegramBotClient = telegramConfiguration.TelegramBotClient;
            //Конструктор принимает конфигурацию Telegram (ITelegramConfiguration) и логгер (ILogger<TelegramMessageSender>), инициализируя поля _telegramBotClient и _logger. Это позволяет использовать клиента Telegram Bot и функционал логирования во всех методах класса.
        }

        public async Task SendLogs(long chatId, byte[] archive, string logsName)
        {
            await using var stream = new MemoryStream(archive);
            await _telegramBotClient.SendDocumentAsync(chatId, new InputOnlineFile(stream, logsName));
            //SendLogs: Отправляет архив с логами как документ.
        }

        public Task SendSelectionExpired(long chatId, int day, string selectionName, string objectName)
        { //SendSelectionExpired: Уведомляет пользователя, что подборка будет отключена через заданное количество дней.
            return SendTextMessageAsync(chatId, $"Подборка {selectionName} на объекте {objectName} отключится через {day} дней");
        }

        public Task SendUserNotFound(long chatId, string message)
        { //SendUserNotFound и SendUserFound: Сообщают о том, был ли найден пользователь.
            return SendTextMessageAsync(chatId, $"Пользователь с почтой {message} не найден");
        }

        public Task SendUserFound(long chatId, string message)
        { //SendUserNotFound и SendUserFound: Сообщают о том, был ли найден пользователь.
            return SendTextMessageAsync(chatId, "Ваш аккаунт подключен к телеграмму");
        }

        public Task SendError(long chatId)
        {         //SendError: Отправляет сообщение об ошибке.
            return SendTextMessageAsync(chatId, "Для подключения телеграмма введите свою почту");
        }

        public async Task SendReport(long chatId, byte[] report, string reportName, string message)
        { //SendReport: Отправляет отчет как документ.
            await using var stream = new MemoryStream(report);
            await SendDocumentAsync(chatId,
                new InputOnlineFile(stream, reportName.Replace("\"", "")),
                message);
        }

        public Task SendClientConnectionStatusChanged(long chatId, string objectName, DateTime eventDate, bool clientConnected)
        { //SendClientConnectionStatusChanged: Уведомляет о изменении статуса подключения клиента.
            _logger.LogTrace("Client status clientConnected: {ClientConnected}, on {ObjectName}", clientConnected, objectName);
            return SendTextMessageAsync(chatId,
                clientConnected ? $"Клиент {objectName} подключен в {eventDate:dd.MM.yyyy HH:mm:ss}" : $"Клиент {objectName} отключен в {eventDate:dd.MM.yyyy HH:mm:ss}");
        }

        public Task SendPlaylistStarted(long chatId, string objectName, DateTime eventDate)
        { //SendPlaylistStarted и SendPlaylistEnded: Уведомляют о начале и конце эфира плейлиста.
            _logger.LogTrace("Playlist started at {EventDate} on {ObjectName}", eventDate, objectName);
            return SendTextMessageAsync(chatId, $"Клиент {objectName} запущен эфир в {eventDate:dd.MM.yyyy HH:mm:ss}");
        }

        public Task SendPlaylistEnded(long chatId, string objectName, DateTime eventDate)
        { //SendPlaylistStarted и SendPlaylistEnded: Уведомляют о начале и конце эфира плейлиста.
            _logger.LogTrace("Playlist ended at {EventDate} on {ObjectName}", eventDate, objectName);
            return SendTextMessageAsync(chatId, $"Клиент {objectName} закончил эфир в {eventDate:dd.MM.yyyy HH:mm:ss}");
        }

        public async Task SendTextMessageAsync(
            //Похож на SendTextMessageAsync, но предназначен для отправки документов, например, отчетов или архивов с логами.
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
            catch (Telegram.Bot.Exceptions.ApiRequestException e) when (e.ErrorCode == 403)
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
            //Оба метода SendTextMessageAsync и SendDocumentAsync содержат обработку исключений, чтобы управлять ошибками API Telegram и логировать их. Это помогает предотвратить сбои приложения при возникновении проблем с отправкой сообщений и обеспечивает документирование ошибок для дальнейшего анализа.
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
            //Оба метода SendTextMessageAsync и SendDocumentAsync содержат обработку исключений, чтобы управлять ошибками API Telegram и логировать их. Это помогает предотвратить сбои приложения при возникновении проблем с отправкой сообщений и обеспечивает документирование ошибок для дальнейшего анализа.
        }
    }
    //В целом, TelegramMessageSender представляет собой удобный инструмент для интеграции возможностей Telegram в приложения, позволяя автоматизировать отправку уведомлений и документов пользователям.
}
