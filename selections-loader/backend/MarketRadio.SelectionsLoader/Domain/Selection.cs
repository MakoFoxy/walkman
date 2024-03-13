using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MarketRadio.SelectionsLoader.Domain.Abstractions;

namespace MarketRadio.SelectionsLoader.Domain
{
    public class Selection : Entity
    {
        public string Name { get; set; }
        public bool IsPublic { get; set; }
        public bool Created { get; set; }
        public DateTime DateBegin { get; set; }
        public DateTime? DateEnd { get; set; }
        public List<TrackInSelection> Tracks { get; set; } = new();

        private long _tracksLength = 0;
        private long _uploadedTracksCount = 0;
        
        public double TracksLength
        {
            get
            {
                if (_tracksLength == 0)
                {
                    _tracksLength = Tracks.Select(t => t.Track.Path)
                        .Select(f => new FileInfo(f).Length)
                        .Sum();
                }

                return _tracksLength;
            }
        }
        
        public double UploadedTracksCount
        {
            get
            {
                var uploadedTracks = Tracks
                    .Where(t => t.Track.Uploaded)
                    .ToList();
                
                if (_uploadedTracksCount != uploadedTracks.Count)
                {
                    _uploadedTracksCount = uploadedTracks
                        .Select(t => t.Track.Path)
                        .Select(f => new FileInfo(f).Length)
                        .Sum();
                }

                return _uploadedTracksCount;
            }
        }
    }
}
