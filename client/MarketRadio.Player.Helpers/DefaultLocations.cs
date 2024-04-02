using System;
using System.IO;

namespace MarketRadio.Player.Helpers
{
    public static class DefaultLocations
    {//В данном фрагменте кода представлен статический класс DefaultLocations, который принадлежит пространству имен MarketRadio.Player.Helpers. Класс предназначен для удобного получения путей к ключевым директориям и файлам приложения, таким как база данных, треки, журналы (логи) и другие. Вот разбор его основных компонентов:
        public static string DatabasePath => Path.Combine(BaseLocation, "db"); //DatabasePath: Путь к директории базы данных. Комбинируется из BaseLocation и поддиректории "db".
        public static string DatabaseFileName => "data_db.db"; //DatabaseFileName: Имя файла базы данных — "data_db.db". Это константа, определяющая имя файла, в котором будет храниться база данных приложения.
        
        public static string TracksPath => Path.Combine(BaseLocation, "tracks"); //TracksPath: Путь к директории, где будут храниться аудио треки. Состоит из BaseLocation и поддиректории "tracks".
        public static string LogsPath => Path.Combine(BaseLocation, "logs"); //LogsPath: Путь к директории логов. Формируется из BaseLocation и поддиректории "logs".
        public static string AppLogsPath => Path.Combine(LogsPath, "app"); //AppLogsPath: Путь к директории логов приложения. Определяется как поддиректория "app" внутри директории LogsPath.

        public static string BaseLocation => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "player-client"); //BaseLocation: Базовый путь к директории, где будет храниться вся информация приложения. Он формируется из специальной системной директории ApplicationData, к которой добавляется "player-client". Это обеспечивает универсальное место хранения данных приложения вне зависимости от операционной системы.
    }
    //Использование Path.Combine гарантирует корректное формирование путей в соответствии с особенностями файловой системы операционной системы. Это позволяет избегать ошибок, связанных с различиями в разделителях путей между ОС (например, \ в Windows и / в UNIX-подобных системах). Получение пути к системной директории ApplicationData через Environment.GetFolderPath обеспечивает независимость от конкретного пользователя и системы, так как ApplicationData автоматически указывает на правильное местоположение в зависимости от ОС и текущего пользователя. Этот класс значительно упрощает работу с файловой системой, предоставляя централизованное и безопасное место для доступа к важным директориям и файлам приложения.
}