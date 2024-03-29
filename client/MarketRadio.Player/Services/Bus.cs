using System;
using System.Threading.Tasks;
using ElectronNET.API;
using MarketRadio.Player.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace MarketRadio.Player.Services
{
    public class Bus : Hub<IBus>
    {//Класс Bus в пространстве имен MarketRadio.Player.Services наследует от Hub<IBus>, что делает его частью SignalR (технология в ASP.NET Core для добавления реального времени веб-функциональности в приложения). IBus - это интерфейс, определенный пользователем, который описывает методы, которые могут быть вызваны клиентом или сервером в контексте SignalR. Давайте подробно рассмотрим код:
        private readonly ILogger<Bus> _logger; //Объявляет зависимость на логгер, используемый для регистрации информации о процессах внутри Bus.

        private string? _connectionId; //Хранит идентификатор соединения клиента, который подключен к текущему экземпляру Hub.

        public Bus(ILogger<Bus> logger)
        {
            _logger = logger; //Конструктор класса, который инициализирует _logger с помощью внедрения зависимостей.
        }

        public override Task OnConnectedAsync()
        {
            _connectionId = Context.ConnectionId;
            _logger.LogInformation("Loading playlist on frontend with {Id} connected to bus", _connectionId);
            return base.OnConnectedAsync(); //Переопределяет метод OnConnectedAsync из базового класса Hub. В этом методе _connectionId устанавливается равным идентификатору соединения текущего клиента (Context.ConnectionId). Затем регистрируется информация о подключении с использованием _logger. Наконец, вызывается базовая реализация OnConnectedAsync.
        }
        //Оставшаяся часть класса содержит методы, которые вызываются сервером для отправки сообщений конкретному клиенту, идентифицируемому по _connectionId. Если _connectionId не установлен (то есть равен null), методы немедленно завершаются, возвращая Task.CompletedTask, что означает, что операция выполнена успешно, но фактически ничего не делалась.
        public Task ObjectUpdated(ObjectInfoDto @object) //Уведомляет клиента о том, что объект был обновлен.
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).ObjectUpdated(@object);
        }

        public Task PlaylistLoaded(PlaylistDto playlist) //Уведомляет клиента о том, что плейлист был загружен.
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).PlaylistLoaded(playlist);
        }

        public Task StopPlaying() //Уведомляет клиента остановить воспроизведение.
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).StopPlaying();
        }

        public Task PlaylistUpdating() //Уведомляет клиента о том, что плейлист обновляется.
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).PlaylistUpdating();
        }

        public Task PlaylistUpdateFinished() //Уведомляет клиента о том, что обновление плейлиста завершено.
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).PlaylistUpdateFinished();
        }

        public Task TrackAdded(Guid trackId) //Уведомляет клиента о добавлении трека в плейлист.
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).TrackAdded(trackId);
        }

        public Task CurrentTrackChanged(string trackUniqueId) //Уведомляет клиента о смене текущего трека.
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).CurrentTrackChanged(trackUniqueId);
        }

        public Task CurrentVolumeChanged(int volume) //Уведомляет клиента о изменении текущей громкости.
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).CurrentVolumeChanged(volume);
        }

        public Task PingCurrentVolume(int volume) //Посылает клиенту текущую громкость в качестве "пинга".
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).PingCurrentVolume(volume);
        }

        public Task OnlineStateChanged(bool isOnline) //Уведомляет клиента о изменении состояния подключения к Интернету.
        {
            if (_connectionId == null)
                return Task.CompletedTask;

            return Clients.Client(_connectionId).OnlineStateChanged(isOnline);
        }
    //Каждый из этих методов использует Clients.Client(_connectionId) для обращения к конкретному клиенту, идентификатор которого сохранен в _connectionId, и вызывает соответствующий метод на стороне клиента. Это позволяет серверу отправлять индивидуальные сообщения клиентам и управлять их состоянием в реальном времени.
    }
}