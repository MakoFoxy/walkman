using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Player.Services.Abstractions;
using Telegram.Bot;

namespace Player.Services
{
    public class TelegramConfiguration : ITelegramConfiguration
    {
        private readonly IConfiguration _configuration;

        public TelegramConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
            TelegramBotClient = new TelegramBotClient(configuration.GetValue<string>("Player:Telegram:Token"));
        }

        public ITelegramBotClient TelegramBotClient { get; }

        public Task Init()
        {
            return TelegramBotClient.SetWebhookAsync(_configuration.GetValue<string>("Player:Telegram:WebHookUrl"));
        }
    }
}
