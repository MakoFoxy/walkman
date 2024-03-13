using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Player.Domain;

namespace Player.Services.Abstractions
{
    public interface ITrackLoader
    {
        /// <summary>
        /// Отдает музыкальные треки для объекта <param name="objectInfo"></param> на весь его рабочий день
        /// </summary>
        /// <param name="objectInfo"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>Коллекция музыкальных треков</returns>
        Task<ICollection<MusicTrack>> LoadForObject(ObjectInfo objectInfo, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отдает музыкальные треки суммарная продолжительность которых равна <param name="allTracksLength"></param> или меньше, но не больше <param name="allTracksLength"></param>
        /// для объекта <param name="objectInfo"></param>
        /// </summary>
        /// <param name="objectInfo">Объект</param>
        /// <param name="allTracksLength">Максимальное время</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Коллекция музыкальных треков</returns>
        Task<ICollection<MusicTrack>> LoadForObject(ObjectInfo objectInfo, TimeSpan allTracksLength, CancellationToken cancellationToken = default);

        /// <summary>
        /// Отдает музыкальные треки суммарная продолжительность которых равна <param name="allTracksLength"></param> или меньше, но не больше <param name="allTracksLength"></param>
        /// </summary>
        /// <param name="allTracksLength">Максимальное время</param>
        /// <param name="cancellationToken"></param>
        /// <returns>Коллекция музыкальных треков</returns>
        Task<ICollection<MusicTrack>> LoadForObject(TimeSpan allTracksLength, CancellationToken cancellationToken = default);
    }
}