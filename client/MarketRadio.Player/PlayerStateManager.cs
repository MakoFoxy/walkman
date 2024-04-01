using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AudioSwitcher.AudioApi;
using AudioSwitcher.AudioApi.CoreAudio;
using ElectronNET.API;
using MarketRadio.Player.DataAccess.Domain;
using MarketRadio.Player.Models;
using MarketRadio.Player.Services;
using MarketRadio.Player.Services.LiveConnection;
using Microsoft.Extensions.DependencyInjection;
using Player.ClientIntegration;
using Player.ClientIntegration.Object;

namespace MarketRadio.Player
{
    public class PlayerStateManager
    {//Класс PlayerStateManager в вашем пространстве имен MarketRadio.Player является центральной частью для управления состоянием плеера, предоставляя интерфейсы для изменения текущего трека, плейлиста, объема и других параметров, а также взаимодействия с внешними системами через шину событий Bus. Рассмотрим добавленные методы и их функционал:
        private readonly Bus _bus;
        private readonly IServiceProvider _serviceProvider;
        private readonly IAudioController _audioController;

        public PlayerStateManager(Bus bus, IServiceProvider serviceProvider, IAudioController audioController)
        {
            _bus = bus;
            _serviceProvider = serviceProvider;
            _audioController = audioController;

            //          private readonly Bus _bus;: Шина событий для обмена сообщениями между разными частями приложения.
            // private readonly IServiceProvider _serviceProvider;: Провайдер служб для доступа к другим сервисам приложения.
            // private readonly IAudioController _audioController;: Контроллер для управления аудиоустройствами.
        }

        public PlaylistDto? Playlist { get; private set; } //Текущий плейлист.
        public bool PlaylistIsDownloading { get; set; } //Флаг, указывающий, идет ли скачивание плейлиста.
        public ObjectInfoDto? Object { get; set; } //Информация об объекте воспроизведения (например, о пользователе или устройстве).
        public TrackDto? CurrentTrack { get; private set; } // Текущий трек.
        public bool IsOnline { get; set; } //Состояние онлайн/оффлайн.
        public List<Guid> BannedTracks { get; private set; } = new(); //Список идентификаторов заблокированных треков.

        public TrackDto? NextTrack //    Вычисляет и возвращает следующий трек для воспроизведения, основываясь на времени воспроизведения текущего трека.
        {
            get
            {
                if (Playlist == null)
                {
                    return null;
                }

                return Playlist.Tracks
                    .Where(p => p.PlayingDateTime > CurrentTrack?.PlayingDateTime)
                    .MinBy(p => p.PlayingDateTime);
            }
        }

        public BrowserWindow? CurrentWindow { get; set; } //Свойство CurrentWindow:Хранит текущее окно браузера, если таковое используется (в контексте Electron или подобных технологий).

        public Task UpdateObject(ObjectInfoDto @object)
        {
            Object = @object;
            return _bus.ObjectUpdated(@object);//    Обновляет информацию об объекте воспроизведения и уведомляет систему через шину событий.
        }

        public Task ChangeMasterVolume(int volume) //    Изменяет главный уровень громкости воспроизведения.
        {
            _audioController.DefaultPlaybackDevice.Volume = volume;
            return Task.CompletedTask;
        }

        public int GetMasterVolume() //    Возвращает текущий главный уровень громкости воспроизведения.
        {
            return Convert.ToInt32(_audioController.DefaultPlaybackDevice.Volume);
        }

        public async Task ChangeCurrentTrack(TrackDto track)
        {
            CurrentTrack = track;
            await _bus.CurrentTrackChanged(track.UniqueId); //Изменяет текущий трек и отправляет соответствующее уведомление через шину событий.
            //TODO не хороший код, надо избавиться и изменить архитектуру
            using var scope = _serviceProvider.CreateScope();
            var serverLiveConnection = scope.ServiceProvider.GetRequiredService<ServerLiveConnection>(); //Создает новую область видимости для доступа к сервису ServerLiveConnection и отправляет информацию о смене трека.

            await serverLiveConnection.CurrentTrackResponse(new OnlineObjectInfo
            {
                Date = DateTime.Now,
                CurrentTrack = track.UniqueId,
                ObjectId = Object!.Id,
                SecondsFromStart = 0,
            });
        }

        public async Task ChangePlaylist(PlaylistDto playlist) //    Загружает новый плейлист и обновляет его в состоянии плеера, уведомляя систему через шину событий.
        {
            await _bus.PlaylistLoaded(playlist);
            Playlist = playlist;
        }

        public async Task ChangeOnlineState(bool isOnline) //    Изменяет онлайн-состояние плеера и уведомляет систему через шину событий.
        {
            await _bus.OnlineStateChanged(isOnline);
            IsOnline = isOnline;
        }

        public async Task ChangeObjectVolume(ObjectVolumeChanged volumeChanged) //    Изменяет уровень громкости в зависимости от типа трека (реклама или музыка) и отправляет соответствующее уведомление через шину событий.
        {
            if (CurrentTrack == null)
            {
                return;
            }

            if (CurrentTrack.Type == Track.Advert)
            {
                await _bus.CurrentVolumeChanged(volumeChanged.AdvertVolume);
            }
            else
            {
                await _bus.CurrentVolumeChanged(volumeChanged.MusicVolume);
            }
        }
        //Этот класс показывает, как централизованно управлять состоянием аудиоплеера, интегрируя различные аспекты управления медиа в единую систему.
    }
}