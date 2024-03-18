using System;

namespace Player.ClientIntegration
{
    public class TrackDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public double Length { get; set; }

        public DateTime PlayingDateTime { get; set; }
        public string UniqueId => $"{Id}_{PlayingDateTime:HH:mm:ss_dd.MM.yyyy}";
        //UniqueId: Вычисляемое свойство, формирующее уникальный идентификатор трека на основе его Id и времени воспроизведения PlayingDateTime. Это может быть полезно для создания уникальных записей о воспроизведении трека.
        public string UniqueName => $"{Id}__{PathHelper.ToSafeName(Name)}";
        //UniqueName: Вычисляемое свойство, создающее уникальное имя трека, сочетая Id и безопасное имя (без запрещенных символов), полученное через метод PathHelper.ToSafeName(Name). Это позволяет создать уникальное и безопасное для файловой системы имя для трека, которое может быть использовано в качестве имени файла или для других целей.
    }
}
//Этот класс может использоваться в системах интеграции клиента, например, в музыкальных плеерах, аудио библиотеках или системах управления контентом, где требуется обрабатывать информацию о музыкальных треках, включая их идентификацию, классификацию, проверку целостности и учет времени воспроизведения.