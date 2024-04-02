using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using MarketRadio.Player.DataAccess.Domain.Base;
using Newtonsoft.Json;

namespace MarketRadio.Player.DataAccess.Domain
{
    public class Settings : Entity
    {//Класс Settings в пространстве имен MarketRadio.Player.DataAccess.Domain определяет настройки для доменной модели, вероятно, связанные с конфигурацией звука в медиаплеере или аудио-приложении. Рассмотрим подробности реализации этого класса: Наследование от Entity: Settings наследуется от базового класса Entity, что предполагает, что Settings является частью модели данных с определенным идентификатором или общими свойствами, наследуемыми от Entity.
        public const int DefaultVolume = 80;  //Константа DefaultVolume: Устанавливает значение громкости по умолчанию равное 80. Это значение используется, когда явные настройки громкости отсутствуют.

        //Свойства AdvertVolumeComputed и MusicVolumeComputed: предоставляют доступ к вычисляемым массивам значений громкости для рекламы и музыки соответственно. Если соответствующие строки (AdvertVolume, MusicVolume) пусты или содержат только пробельные символы, возвращается массив, содержащий значение громкости по умолчанию (DefaultVolume) для каждого часа суток (24 значения). Эти свойства предоставляют доступ к вычисляемым массивам значений громкости для рекламы и музыки соответственно. Если соответствующие строки (AdvertVolume, MusicVolume) пусты или содержат только пробельные символы, возвращается массив, содержащий значение громкости по умолчанию (DefaultVolume) для каждого часа суток (24 значения).При установке значения этих свойств массивы целых чисел преобразуются в строку, разделенную запятыми. Если массив пустой или null, соответствующая строка очищается.
        [NotMapped] //Атрибут [NotMapped] указывает, что эти свойства не должны отображаться на колонки в базе данных, возможно, потому что они вычисляются на основе других хранимых данных.
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
        //Свойства AdvertVolume и MusicVolume: строковые свойства хранят настройки громкости для рекламы и музыки в формате строки, разделенной запятыми. Они помечены атрибутом [JsonIgnore], чтобы исключить их из сериализации JSON, предполагая, что вместо них должны использоваться вычисляемые свойства AdvertVolumeComputed и MusicVolumeComputed. Свойства AdvertVolume и MusicVolume:   Эти строковые свойства хранят настройки громкости для рекламы и музыки в формате строки, разделенной запятыми. Они помечены атрибутом [JsonIgnore], чтобы исключить их из сериализации JSON, предполагая, что вместо них должны использоваться вычисляемые свойства AdvertVolumeComputed и MusicVolumeComputed.

        [JsonIgnore]
        public string AdvertVolume { get; set; } = null!;

        [JsonIgnore]
        public string MusicVolume { get; set; } = null!;
        public bool IsOnTop { get; set; } //Свойство IsOnTop: Булево свойство, которое может указывать, должно ли приложение оставаться "поверх" других окон.
        public int SilentTime { get; set; } //Свойство SilentTime: Целочисленное свойство, возможно, определяющее время в течение которого должно поддерживаться "тихий режим" (например, время в минутах или секундах).
    }
    //Класс Settings представляет собой сложную настройку конфигурации, которая может использоваться для управления аудио параметрами в приложении, позволяя легко изменять уровни громкости для различных типов контента и настраивать другие аспекты поведения приложения через хранение и обработку настроек.
}