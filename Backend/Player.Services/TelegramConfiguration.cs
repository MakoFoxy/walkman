using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Player.Services.Abstractions;
using Telegram.Bot;

namespace Player.Services
{
    public class TelegramConfiguration : ITelegramConfiguration
    {//Этот код описывает класс TelegramConfiguration, который реализует интерфейс ITelegramConfiguration. Этот класс предназначен для настройки и инициализации клиента Telegram Bot в контексте какой-то более крупной системы, вероятно, медиаплеера или сервиса, связанного с воспроизведением контента. Давайте разберем его подробнее:
        private readonly IConfiguration _configuration;

        public TelegramConfiguration(IConfiguration configuration)
        {
            _configuration = configuration;
            TelegramBotClient = new TelegramBotClient(configuration.GetValue<string>("Player:Telegram:Token"));
            //Конструктор принимает объект IConfiguration, который обычно используется в приложениях .NET для доступа к настройкам конфигурации, таким как appsettings.json. Здесь из конфигурации извлекается токен для Telegram Bot ("Player:Telegram:Token") и с его помощью создается новый экземпляр TelegramBotClient. Этот клиент сохраняется в свойстве TelegramBotClient для дальнейшего использования.
        }

        public ITelegramBotClient TelegramBotClient { get; }
        //Это автоматическое свойство только для чтения, которое предоставляет доступ к экземпляру TelegramBotClient, созданному в конструкторе. Оно позволяет использовать этот клиент в других частях системы для взаимодействия с Telegram API.
        public Task Init()
        {
            return TelegramBotClient.SetWebhookAsync(_configuration.GetValue<string>("Player:Telegram:WebHookUrl"));
            //Этот асинхронный метод инициализирует вебхук для клиента Telegram Bot. Вебхуки используются для того, чтобы Telegram мог отправлять сообщения (обновления) боту по определенному URL, что позволяет боту реагировать на сообщения от пользователей в режиме реального времени. URL вебхука также извлекается из конфигурации ("Player:Telegram:WebHookUrl").
        }
    }
    //Этот класс представляет собой ключевую часть интеграции сервиса (в данном случае, возможно, сервиса плеера или медиасервиса) с Telegram, позволяя обрабатывать команды или сообщения от пользователей Telegram через созданного бота.
}
