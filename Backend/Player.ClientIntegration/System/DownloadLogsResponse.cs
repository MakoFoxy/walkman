namespace Player.ClientIntegration.System
{
    public class DownloadLogsResponse
    {
        public DownloadLogsRequest DownloadLogsRequest { get; set; }
        public ArchiveLogFile File { get; set; }
        //    Этот класс предназначен для предоставления ответа на запрос скачивания логов. Он включает в себя информацию о самом запросе и файле, содержащем запрошенные логи.
    }

    public class ArchiveLogFile
    {
        public string Body { get; set; }
        public string Name { get; set; }
        //    Этот класс представляет файл архива, содержащий логи. Он может использоваться для передачи файлов логов от сервера к клиенту.
    }
}

//Использование этих классов позволяет структурированно обрабатывать операции скачивания логов в системе. Клиент может отправить запрос на скачивание логов, используя DownloadLogsRequest, и сервер ответит через DownloadLogsResponse, предоставляя файл с логами в виде объекта ArchiveLogFile. Это облегчает обмен данными между клиентом и сервером и делает процесс скачивания логов более управляемым и безопасным.