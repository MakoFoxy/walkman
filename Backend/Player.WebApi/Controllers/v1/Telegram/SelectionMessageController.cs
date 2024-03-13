using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Player.DataAccess;
using Player.Services.Abstractions;
using Telegram.Bot.Types;

namespace Player.WebApi.Controllers.v1.Telegram
{
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
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            _logger.LogTrace("Init started");
            await _telegramConfiguration.Init();
            _logger.LogTrace("Init ended");
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody]Update update)
        {
            _logger.LogTrace("Message accepted");
            var message = update.Message?.Text;
            _logger.LogTrace("Message {Message}", message);

            if (string.IsNullOrWhiteSpace(message))
            {
                return Ok();
            }
            
            var isEmail = message.Contains("@");
            _logger.LogTrace("Message is email {IsEmail}", isEmail);

            if (isEmail)
            {
                var user = await _context.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == message.ToLower());
                _logger.LogTrace("User is not null {NotNull}", user != null);

                if (user == null)
                {
                    _logger.LogTrace("User not found");
                    await _telegramMessageSender.SendUserNotFound(update.Message.Chat.Id, message);
                    return Ok();
                }

                user.TelegramChatId = update.Message.Chat.Id;
                _logger.LogTrace("Updating user");
                await _context.SaveChangesAsync();
                _logger.LogTrace("User updated");

                await _telegramMessageSender.SendUserFound(update.Message.Chat.Id, message);
                _logger.LogTrace("User found send");
                return Ok();
            }

            _logger.LogTrace("User error sending");
            await _telegramMessageSender.SendError(update.Message.Chat.Id);
            _logger.LogTrace("User error send");
            return Ok();
        }
    }
}