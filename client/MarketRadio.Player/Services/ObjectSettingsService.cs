using System;
using System.Linq;
using MarketRadio.Player.DataAccess.Domain;

namespace MarketRadio.Player.Services
{
    public class ObjectSettingsService
    {
        public Settings FillSettingsIfNeeded(ObjectInfo objectInfo)
        {
            if (!VolumeSettingsIsEmpty(objectInfo))
            {
                return objectInfo.Settings!;
            }

            return new Settings
            {
                AdvertVolumeComputed = FillSettings(objectInfo.BeginTime, objectInfo.EndTime),
                MusicVolumeComputed = FillSettings(objectInfo.BeginTime, objectInfo.EndTime),
                IsOnTop = false,
                SilentTime = 0
            };
        }

        private int[] FillSettings(TimeSpan beginTime, TimeSpan endTime)
        {
            if (Is24HourObject(beginTime, endTime))
            {
                return Enumerable.Repeat(Settings.DefaultVolume, 24).ToArray();
            }
            
            var volume = new int[24];
            
            for (var i = 0; i < 24; i++)
            {
                if (i >= beginTime.Hours && i <= endTime.Hours)
                {
                    volume[i] = Settings.DefaultVolume;
                }
                else
                {
                    volume[i] = 0;
                }
            }

            return volume;
        }

        private bool Is24HourObject(TimeSpan beginTime, TimeSpan endTime)
        {
            var length = endTime.Subtract(beginTime);
            return length <= TimeSpan.Zero;
        }

        private static bool VolumeSettingsIsEmpty(ObjectInfo objectInfo)
        {
            return
                objectInfo.Settings == null ||
                !objectInfo.Settings.AdvertVolume.Any() ||
                !objectInfo.Settings.MusicVolume.Any();
        }
    }
}