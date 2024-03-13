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
        private readonly TrackNormalizer _trackNormalizer;

        public MusicTrackCreator(TrackNormalizer trackNormalizer)
        {
            _trackNormalizer = trackNormalizer;
        }

        public async Task<List<MusicTrack>> CreateMusicTracks(MusicTrackCreatorData creatorData)
        {
            var musicTracks = new List<MusicTrack>(creatorData.Tracks.Count);

            using var sha256 = SHA256.Create();
            foreach (var file in creatorData.Tracks.Select(f => new SimpleDto
            {
                Id = f.Id,
                Name = Path.Combine(creatorData.BasePath, f.Name) 
            }))
            {
                _trackNormalizer.Normalize(file.Name);
                var mediaInfo = await FFmpeg.GetMediaInfo(file.Name);
                var hashString = new StringBuilder();

                await using (var stream = File.OpenRead(file.Name))
                {
                    var hash = sha256.ComputeHash(stream);

                    foreach (var h in hash)
                    {
                        hashString.Append($"{h:x2}");
                    }
                }

                var musicTrack = new MusicTrack
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

                musicTrack.Genres.Add(new MusicTrackGenre
                {
                    GenreId = creatorData.GenreId,
                    MusicTrack = musicTrack,
                });
                        
                musicTracks.Add(musicTrack);
            }

            return musicTracks;
        }
    }
}