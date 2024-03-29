using System;
using System.Linq;
using MarketRadio.Player.DataAccess.Domain;

namespace MarketRadio.Player.Services
{
    public class ObjectSettingsService
    { //Этот код описывает сервис ObjectSettingsService в пространстве имен MarketRadio.Player.Services, предназначенный для управления настройками объекта, такими как настройки громкости объявлений и музыки, а также другие связанные настройки. Рассмотрим детально каждый метод и его функцию.
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
            //Этот метод принимает объект ObjectInfo, содержащий информацию о некотором объекте (скорее всего, медиаобъекте или устройстве воспроизведения), и проверяет, нужно ли заполнить его настройки громкости по умолчанию. Если настройки уже заданы, метод просто возвращает их. В противном случае он создает и возвращает новый объект настроек с дефолтными значениями громкости для рекламы и музыки, настройками отображения поверх других окон (IsOnTop) и временем беззвучного режима (SilentTime).
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
            //Этот метод генерирует массив громкости для каждого из 24 часов в сутках, основываясь на времени начала (beginTime) и времени окончания (endTime). Если объект работает круглосуточно (это определяется в методе Is24HourObject), то всем 24 часам присваивается дефолтная громкость. В противном случае, громкость вне заданного временного промежутка устанавливается в 0, а внутри промежутка — в дефолтную громкость.
        }

        private bool Is24HourObject(TimeSpan beginTime, TimeSpan endTime)
        {
            var length = endTime.Subtract(beginTime);
            return length <= TimeSpan.Zero;
            //Определяет, работает ли объект круглосуточно. Это проверяется путем вычисления продолжительности интервала между beginTime и endTime. Если продолжительность меньше или равна 0 (что может случиться, если endTime раньше beginTime), считается, что объект работает круглосуточно.
        }

        private static bool VolumeSettingsIsEmpty(ObjectInfo objectInfo)
        {
            return
                objectInfo.Settings == null ||
                !objectInfo.Settings.AdvertVolume.Any() ||
                !objectInfo.Settings.MusicVolume.Any();
                //Проверяет, не заданы ли настройки громкости в ObjectInfo. Если настройки не заданы (objectInfo.Settings == null), или массивы громкости для объявлений и музыки пусты (!objectInfo.Settings.AdvertVolume.Any() и !objectInfo.Settings.MusicVolume.Any()), возвращает true, указывая на то, что настройки необходимо заполнить.
        }
    }
    //Класс ObjectSettingsService играет важную роль в поддержке настройки звука для различных объектов (устройств или медиаконтента), управляя громкостью воспроизведения на основе времени суток. Это может быть особенно полезно в приложениях для воспроизведения медиа или рекламы, где нужно автоматически адаптировать уровень громкости в зависимости от времени дня, например, снижая громкость ночью.
}