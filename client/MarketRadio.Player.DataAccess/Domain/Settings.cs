using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MarketRadio.Player.DataAccess.Domain.Base;
using Newtonsoft.Json;

namespace MarketRadio.Player.DataAccess.Domain
{
    public class Settings : Entity
    {
        public const int DefaultVolume = 80; 

        [NotMapped]
        [JsonProperty(PropertyName = "advertVolume")]
        public int[] AdvertVolumeComputed
        {
            get
            {
                if (string.IsNullOrWhiteSpace(AdvertVolume))
                {
                    return Enumerable.Repeat(DefaultVolume, 24).ToArray();
                }

                return AdvertVolume.Split(',').Select(int.Parse).ToArray();
            }
            set
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (value == null || !value.Any())
                {
                    AdvertVolume = "";
                    return;
                }

                AdvertVolume = string.Join(',', value);
            }
        }
        
        [NotMapped]
        [JsonProperty(PropertyName = "musicVolume")]
        public int[] MusicVolumeComputed
        {
            get
            {
                if (string.IsNullOrWhiteSpace(MusicVolume))
                {
                    return Enumerable.Repeat(DefaultVolume, 24).ToArray();
                }

                return MusicVolume.Split(',').Select(int.Parse).ToArray();
            }
            set
            {                
                // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
                if (value == null || !value.Any())
                {
                    MusicVolume = "";
                    return;
                }

                MusicVolume = string.Join(',', value);
            }
        }

        [JsonIgnore] 
        public string AdvertVolume { get; set; } = null!;

        [JsonIgnore]
        public string MusicVolume { get; set; } = null!;
        public bool IsOnTop { get; set; }
        public int SilentTime { get; set; }
    }
}