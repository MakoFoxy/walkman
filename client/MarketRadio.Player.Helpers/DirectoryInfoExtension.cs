using System.IO;
using System.Linq;

namespace MarketRadio.Player.Helpers
{
    public static class DirectoryInfoExtension
    { //Класс DirectoryInfoExtension в пространстве имен MarketRadio.Player.Helpers представляет собой статический класс расширения для System.IO.DirectoryInfo, который добавляет метод GetDirectorySize. Этот метод расширения позволяет легко получить общий размер файлов в директории, включая все поддиректории. Давайте разберемся, как он работает:
        public static long GetDirectorySize(this DirectoryInfo d) //Ключевое слово this перед первым параметром метода (DirectoryInfo d) указывает, что GetDirectorySize является методом расширения для типа DirectoryInfo.
        { //Метод возвращает суммарный размер файлов в директории (и всех её поддиректориях) в байтах.
            return Directory
                .EnumerateFiles(d.FullName, "*.*", SearchOption.AllDirectories)
                .Sum(f => new FileInfo(f).Length);
            //Directory.EnumerateFiles(d.FullName, "*.*", SearchOption.AllDirectories) использует статический метод EnumerateFiles класса Directory для получения перечислителя всех файлов в указанной директории d и её поддиректориях. Аргументы метода:    d.FullName указывает на полный путь к целевой директории.
            // "*.*" служит шаблоном для выбора всех файлов.
            // SearchOption.AllDirectories указывает на необходимость включения файлов из всех поддиректорий.
            //Результат перечисления файлов преобразуется в последовательность размеров файлов с помощью .Sum(f => new FileInfo(f).Length), где для каждого пути файла создается объект FileInfo, а его свойство Length (размер файла в байтах) используется для подсчета общей суммы размеров.
        }
        //Этот метод расширения упрощает процесс подсчета общего размера файлов в директории, делая код, который работает с файловой системой, более читаемым и лаконичным.
    }
}