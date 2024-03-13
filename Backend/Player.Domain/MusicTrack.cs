using System.Collections.Generic;

namespace Player.Domain
{
    public class MusicTrack : Track
    {
        public bool IsHit { get; set; }
        public int Index { get; set; }
        
        /// <summary>
        /// Список дат и времени воспроизведения музыкального трека на объекте
        /// </summary>
        public ICollection<MusicHistory> MusicHistories { get; set; } = new List<MusicHistory>();

        /// <summary>
        /// Плейлисты, в которых пристутсвует музыкальный трек
        /// </summary>
        public ICollection<MusicTrackPlaylist> Playlists { get; set; } = new List<MusicTrackPlaylist>();

        /// <summary>
        /// Подборки музыкальных треков
        /// </summary>
        public ICollection<MusicTrackSelection> Selections { get; set; }=new List<MusicTrackSelection>();

        /// <summary>
        /// Жанры музыкального трека
        /// </summary>
        public ICollection<MusicTrackGenre> Genres { get; set; } = new List<MusicTrackGenre>();
    }
}
