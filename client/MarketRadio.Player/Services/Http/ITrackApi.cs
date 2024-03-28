using System;
using System.Net.Http;
using System.Threading.Tasks;
using MarketRadio.Player.Models;
using Refit;

namespace MarketRadio.Player.Services.Http
{
    public interface ITrackApi
    {
        [Get("/api/v1/track")]
        Task<HttpContent> DownloadTrack([Query] Guid trackId, [Query] string trackType); //Возвращаемое значение: Task<HttpContent> — асинхронный таск, который возвращает содержимое HTTP-ответа. HttpContent может быть использован для доступа к потоку данных трека, который можно затем сохранить в файле или проиграть напрямую.

        [Get("/api/v1/track/check")]
        Task<TrackIsCorrectWrapper> CheckTrack([Query] Guid trackId, [Query] string trackType, [Query] string hash); //Возвращаемое значение: Task<TrackIsCorrectWrapper> — асинхронный таск, возвращающий объект-обёртку, который содержит информацию о результате проверки трека. Эта информация может включать в себя статус проверки (например, прошёл ли трек проверку на корректность) и возможно дополнительные данные, связанные с проверкой.
    }
    //Использование ITrackApi позволяет абстрагироваться от низкоуровневых деталей HTTP-взаимодействия, предоставляя чистый и понятный интерфейс для работы с аудиотреками. Это может включать функции такие, как загрузка треков для последующего проигрывания или их проверка на соответствие определённым требованиям безопасности или качества. Благодаря интеграции с Refit, разработка клиентской части, взаимодействующей с внешними API, становится более удобной и безопасной.

}