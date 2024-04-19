using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Player.Domain;
using Player.DTOs;
using Player.Services.Abstractions;
using Xabe.FFmpeg;
namespace Player.Services
{
    public class MusicTrackCreator : IMusicTrackCreator
    {
        //Этот C# код представляет класс MusicTrackCreator, который реализует интерфейс IMusicTrackCreator. Он предназначен для создания музыкальных треков на основе предоставленных данных. Давайте разберем основные части кода:
        private readonly TrackNormalizer _trackNormalizer; //Здесь MusicTrackCreator принимает экземпляр TrackNormalizer, который, предположительно, используется для нормализации звука треков.

        public MusicTrackCreator(TrackNormalizer trackNormalizer)
        {
            _trackNormalizer = trackNormalizer;
            //Здесь MusicTrackCreator принимает экземпляр TrackNormalizer, который, предположительно, используется для нормализации звука треков.
        }

        public async Task<List<MusicTrack>> CreateMusicTracks(MusicTrackCreatorData creatorData)
        {
            //Это асинхронный метод, который принимает данные для создания музыкальных треков (MusicTrackCreatorData) и возвращает список созданных музыкальных треков (List<MusicTrack>).
            var musicTracks = new List<MusicTrack>(creatorData.Tracks.Count);
            Console.WriteLine("musicTracks11 ", musicTracks);

            using var sha256 = SHA256.Create();
            Console.WriteLine("sha256 ", sha256);
            foreach (var file in creatorData.Tracks.Select(f => new SimpleDto
            {
                Id = f.Id,
                Name = Path.Combine(creatorData.BasePath, f.Name)

            }))

            {
                Console.WriteLine("Path.Combine(creatorData.BasePath, f.Name) ", file.Name);
                Console.WriteLine("Id = f.Id ", file.Id);
                try
                {
                    _trackNormalizer.Normalize(file.Name); //Каждый трек нормализуется для обеспечения согласованного уровня громкости.
                    Console.WriteLine("Normalization for {Path} started", file.Name);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error normalizing file.Name {file.Name}, ex.Message {ex.Message}");
                    continue; // Пропускаем текущий файл и продолжаем обработку следующего
                }
                var mediaInfo = await FFmpeg.GetMediaInfo(file.Name); //var mediaInfo = await FFmpeg.GetMediaInfo(file.Name); Используется FFmpeg для получения информации о медиафайле, включая длительность трека.
                Console.WriteLine("mediaInfo ", mediaInfo);

                var hashString = new StringBuilder(); //Для каждого файла вычисляется его хеш SHA-256, который затем преобразуется в строку.
                Console.WriteLine("hashString ", hashString);

                await using (var stream = File.OpenRead(file.Name))
                {
                    var hash = sha256.ComputeHash(stream);

                    foreach (var h in hash)
                    {
                        hashString.Append($"{h:x2}");
                    }
                }

                var musicTrack = new MusicTrack
                //Создается новый объект MusicTrack, в котором заполняются все необходимые поля, включая идентификатор, путь к файлу, расширение, индекс, признак популярности, валидность, длительность, имя, тип трека, идентификатор загрузчика и хеш.
                {
                    Id = file.Id,
                    FilePath = Path.GetRelativePath(creatorData.BasePath, file.Name),
                    Extension = Path.GetExtension(file.Name),
                    Index = ++creatorData.MaxIndex,
                    IsHit = false,
                    IsValid = true,
                    Length = mediaInfo.Duration,
                    Name = Path.GetFileNameWithoutExtension(file.Name),
                    TrackTypeId = creatorData.MusicTrackTypeId,
                    UploaderId = creatorData.UserId,
                    Hash = hashString.ToString(),
                };
                Console.WriteLine("musicTrack Id " + musicTrack.Id);
                Console.WriteLine("musicTrack FilePath " + musicTrack.FilePath);

                musicTrack.Genres.Add(new MusicTrackGenre
                {//К каждому музыкальному треку добавляется жанр.
                    GenreId = creatorData.GenreId,
                    MusicTrack = musicTrack,
                });

                musicTracks.Add(musicTrack); //Все созданные треки добавляются в список musicTracks, который в конце метода возвращается вызывающему коду.
            }

            return musicTracks; //Этот класс можно использовать в медиаплеере или системе управления контентом для создания и хранения информации о музыкальных треках, включая их обработку и каталогизацию.
        }
    }
}