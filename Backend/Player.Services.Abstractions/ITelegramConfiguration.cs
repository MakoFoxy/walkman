using System.Threading.Tasks;
using Telegram.Bot;

namespace Player.Services.Abstractions
{
    public interface ITelegramConfiguration
    {
        ITelegramBotClient TelegramBotClient { get; }
        Task Init();
    }
}
