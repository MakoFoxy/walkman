using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using MarketRadio.Player.DataAccess.Domain.Base;
using MarketRadio.Player.Helpers;

namespace MarketRadio.Player.DataAccess.Domain //Класс Track в пространстве имен MarketRadio.Player.DataAccess.Domain представляет собой модель данных для трека в приложении медиаплеера. Давайте рассмотрим его основные компоненты:
{
    public class Track : Entity//Наследование от Entity: Это указывает, что Track является сущностью данных, вероятно, с базовым идентификатором и другими общими свойствами, определенными в Entity.
    {
        public required string Type { get; set; } //public required string Type { get; set; }: Тип трека (например, реклама, музыка, тишина). Ключевое слово required (требуемое) предполагает, что это свойство не может быть null и должно быть задано. Это часть нововведений C# 10, связанных с nullable reference types.
        public required string Name { get; set; }   //public required string Name { get; set; }: Название трека. Также обязательно должно быть задано.     
        public required string Hash { get; set; } //public required string Hash { get; set; }: Хэш-значение трека, вероятно, используется для проверки целостности или для идентификации.
        public double Length { get; set; } //    public double Length { get; set; }: Продолжительность трека в какой-то единице измерения, скорее всего, в секундах.

        [NotMapped]
        public string UniqueName => $"{Id}__{PathHelper.ToSafeName(Name)}"; //[NotMapped] public string UniqueName => $"{Id}__{PathHelper.ToSafeName(Name)}";: Генерирует уникальное имя файла для трека, используя его Id и безопасное имя, полученное из Name. Атрибут [NotMapped] указывает, что это свойство не отображается на столбец в базе данных.

        [NotMapped]
        public string FilePath => Path.Combine(DefaultLocations.TracksPath, UniqueName); //[NotMapped] public string UniqueName => $"{Id}__{PathHelper.ToSafeName(Name)}";: Генерирует уникальное имя файла для трека, используя его Id и безопасное имя, полученное из Name. Атрибут [NotMapped] указывает, что это свойство не отображается на столбец в базе данных.
        //  Константы Advert, Music, Silent:  Определяют возможные значения для Type. Это удобно для обеспечения типобезопасности и улучшения читаемости кода при работе с различными типами треков.
        public const string Advert = nameof(Advert);
        public const string Music = nameof(Music);
        public const string Silent = nameof(Silent);
    }
    //Эта модель данных позволяет легко интегрировать и управлять аудиотреками в приложении, предоставляя необходимую гибкость для работы с различными типами контента. Вычисляемые свойства обеспечивают удобный доступ к часто используемым путям и именам файлов, что делает работу с файловой системой более безопасной и удобной.
}