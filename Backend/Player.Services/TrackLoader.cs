using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Player.DataAccess;
using Player.Domain;
using Player.Helpers.Extensions;
using Player.Services.Abstractions;

namespace Player.Services
{
    public class TrackLoader : ITrackLoader
    //Класс TrackLoader, реализующий интерфейс ITrackLoader, предназначен для загрузки музыкальных треков для определенного объекта (ObjectInfo). Рассмотрим основные компоненты и методы этого класса:
    {
        private readonly PlayerContext _context;

        public TrackLoader(PlayerContext context)
        {
            _context = context;
            //Конструктор принимает контекст базы данных (PlayerContext), который используется для получения данных из базы.
        }

        public async Task<ICollection<MusicTrack>> LoadForObject(ObjectInfo objectInfo, CancellationToken cancellationToken = default)
        { //Метод LoadForObject(ObjectInfo objectInfo, CancellationToken cancellationToken = default):
            var now = DateTimeOffset.Now;
            // Получение списка запрещенных треков для объекта:
            var bannedTracks = await _context.BannedMusicInObject
                .Where(bm => bm.Object.Id == objectInfo.Id)
                .Select(bm => bm.MusicTrack)
                .ToListAsync(cancellationToken);
            // Получение подборок (селекций) для объекта, отфильтрованных по датам:
            var selections = await _context.Selections
                .Include(s => s.MusicTracks)
                .ThenInclude(mts => mts.MusicTrack)
                .ThenInclude(mt => mt.TrackType)
                .Where(s => s.Objects.Select(o => o.Object).Contains(objectInfo))
                .OrderByDescending(s => s.DateEnd)
                .Where(s => s.MusicTracks.Any())
                .ToListAsync(cancellationToken);
            // Если нет активных подборок, загружаем все доступные музыкальные треки, исключая запрещенные:      
            if (!selections.Any())
            {
                var musicTracks = await _context.MusicTracks
                    .Include(mt => mt.TrackType)
                    .Where(mt => mt.TrackType.Code == TrackType.Music)
                    .Where(mt => !bannedTracks.Contains(mt)) // Фильтрация запрещенных треков.
                    .ToListAsync(cancellationToken);

                return musicTracks.Randomize(); //Загружается список музыкальных треков, которые запрещены для воспроизведения в контексте заданного объекта. Получение подборок (селекций) для объекта
            }
            // Фильтрация селекций по актуальности даты:
            var filteredSelections = selections
                .Where(s => s.DateBegin <= now && (s.DateEnd == null || s.DateEnd > now))
                .ToList();
            // Обновление списка селекций активными подборками:
            if (filteredSelections.Any())
            {
                selections = filteredSelections;
            }
            // Инициализация списка результирующих селекций для обеспечения нужной длительности треков
            var resultedSelections = new List<Selection>(selections); //Обеспечение необходимой длительности музыкальных треков

            // Добавление селекций до достижения необходимой длительности треков:
            while (TracksLengthInSelection(resultedSelections, bannedTracks) < objectInfo.WorkTime.TotalSeconds)
            { //Если общая продолжительность треков в отобранных селекциях меньше рабочего времени объекта, селекции добавляются в список повторно, пока общая продолжительность не превысит необходимую.
                resultedSelections.AddRange(selections);
            }

            // Возвращение результирующего списка музыкальных треков из селекций, исключая запрещенные, в случайном порядке:
            return resultedSelections //Возвращение списка музыкальных треков из отобранных селекций, исключая запрещенные треки, в случайном порядке.
                .SelectMany(rs => rs.MusicTracks)
                .Select(mts => mts.MusicTrack)
                .Randomize()
                .ToList();
        }

        private static double TracksLengthInSelection(List<Selection> resultedSelections, List<MusicTrack> bannedTracks)
        { //Этот метод вычисляет общую длительность всех треков в переданных селекциях, исключая запрещенные треки.
            return resultedSelections.SelectMany(s => s.MusicTracks).Where(mt => !bannedTracks.Contains(mt.MusicTrack)).Sum(mts => mts.MusicTrack.Length.TotalSeconds);
        }

        public Task<ICollection<MusicTrack>> LoadForObject(ObjectInfo objectInfo, TimeSpan allTracksLength, CancellationToken cancellationToken = default)
        {//Эти перегрузки метода LoadForObject в данной реализации класса не реализованы (throw new NotImplementedException();), что указывает на то, что их реализация ожидается в будущем или они могут быть реализованы в зависимости от конкретных потребностей приложения.
            throw new NotImplementedException();
        }

        public Task<ICollection<MusicTrack>> LoadForObject(TimeSpan allTracksLength, CancellationToken cancellationToken = default)
        {//Эти перегрузки метода LoadForObject в данной реализации класса не реализованы (throw new NotImplementedException();), что указывает на то, что их реализация ожидается в будущем или они могут быть реализованы в зависимости от конкретных потребностей приложения.
            throw new NotImplementedException();
        }
    }
    //В целом, TrackLoader представляет собой компонент системы, отвечающий за выборку и подготовку музыкальных треков для воспроизведения в рамках определенного объекта, учитывая его индивидуальные настройки и ограничения.
}