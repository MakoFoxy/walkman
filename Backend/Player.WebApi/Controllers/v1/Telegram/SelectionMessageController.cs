using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Player.DataAccess;
using Player.Services.Abstractions;
using Telegram.Bot.Types;

namespace Player.WebApi.Controllers.v1.Telegram
{ //Этот код представляет собой контроллер в ASP.NET Core Web API, называемый SelectionMessageController, который предназначен для взаимодействия с Telegram ботом. Рассмотрим подробнее его методы и функциональность:
    [ApiController]
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    public class SelectionMessageController : ControllerBase
    {
        private readonly ITelegramConfiguration _telegramConfiguration;
        private readonly PlayerContext _context;
        private readonly ITelegramMessageSender _telegramMessageSender;
        private readonly ILogger<SelectionMessageController> _logger;

        public SelectionMessageController(ITelegramConfiguration telegramConfiguration,
            PlayerContext context,
            ITelegramMessageSender telegramMessageSender,
            ILogger<SelectionMessageController> logger)
        {
            _telegramConfiguration = telegramConfiguration;
            _context = context;
            _telegramMessageSender = telegramMessageSender;
            _logger = logger;

            //             ITelegramConfiguration: Конфигурация для Telegram, скорее всего, содержит данные для инициализации бота.
            // PlayerContext: Контекст Entity Framework для взаимодействия с базой данных.
            // ITelegramMessageSender: Сервис для отправки сообщений через Telegram.
            // ILogger<SelectionMessageController>: Логгер для записи событий в журнал.
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogTrace("Init started");
            await _telegramConfiguration.Init();
            _logger.LogTrace("Init ended");
            return Ok();
            //Этот метод, скорее всего, предназначен для инициализации конфигурации или подключения к Telegram боту. Он вызывает метод Init() из _telegramConfiguration, который может настраивать вебхук или выполнять другие необходимые настройки. Перед началом и после окончания инициализации в журнал записываются соответствующие сообщения.
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Update update)
        {
            _logger.LogTrace("Message accepted");
            var message = update.Message?.Text; //Получает текст сообщения из update.
            _logger.LogTrace("Message {Message}", message);

            if (string.IsNullOrWhiteSpace(message))
            {
                return Ok(); //Если сообщение является пустым, метод завершает выполнение и возвращает HTTP статус Ok.
            }

            var isEmail = message.Contains("@"); //Проверяет, содержит ли текст сообщения символ "@", чтобы определить, является ли это email адресом.
            _logger.LogTrace("Message is email {IsEmail}", isEmail);

            if (isEmail)
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == message.ToLower());
                _logger.LogTrace("User is not null {NotNull}", user != null);  //Ищет пользователя в базе данных по email.

                if (user == null)
                {
                    _logger.LogTrace("User not found");
                    await _telegramMessageSender.SendUserNotFound(update.Message.Chat.Id, message); //Если сообщение не содержит email, отправляет сообщение об ошибке в чат.
                    return Ok();
                }

                user.TelegramChatId = update.Message.Chat.Id; //Если пользователь найден, его TelegramChatId обновляется данными из входящего сообщения.
                _logger.LogTrace("Updating user");
                await _context.SaveChangesAsync();
                _logger.LogTrace("User updated");

                await _telegramMessageSender.SendUserFound(update.Message.Chat.Id, message); //В зависимости от результата поиска отправляет соответствующее сообщение обратно в чат Telegram через _telegramMessageSender.
                _logger.LogTrace("User found send");
                return Ok();
            }

            _logger.LogTrace("User error sending");
            await _telegramMessageSender.SendError(update.Message.Chat.Id);  //В зависимости от результата поиска отправляет соответствующее сообщение обратно в чат Telegram через _telegramMessageSender.
            _logger.LogTrace("User error send");
            return Ok();
        }
    }
}
// Замечания:
//     Метод Get не имеет явного использования в контексте Telegram, возможно, он предназначен для инициализации или тестирования.
//     Метод Post предполагает интерактивное взаимодействие с пользователями через Telegram, где можно привязать их аккаунт Telegram к пользователю системы, отправив email.
//     В текущем коде нет проверок авторизации для эндпоинтов, что может быть потенциальным риском безопасности, в зависимости от предполагаемого использования.
//     Необходимо убедиться, что логика определения, является ли сообщение email адресом, достаточно надежна и не пропускает валидные случаи.
//     Важно учитывать безопасность при сохранении TelegramChatId и другой личной информации пользователей.