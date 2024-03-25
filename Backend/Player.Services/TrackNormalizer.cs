using System.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Player.Services.Abstractions;

namespace Player.Services
{
    public class TrackNormalizer : ITrackNormalizer
    //Класс TrackNormalizer, реализующий интерфейс ITrackNormalizer, предназначен для нормализации аудиотреков, чтобы уровень звука во всех треках был одинаковым. Это важно для создания комфортного слушательского опыта, когда треки проигрываются подряд. Давайте рассмотрим основные моменты класса:
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TrackNormalizer> _logger;

        public TrackNormalizer(IConfiguration configuration, ILogger<TrackNormalizer> logger)
        {//Конструктор принимает объект конфигурации (IConfiguration) и объект логирования (ILogger<TrackNormalizer>). Это позволяет классу получать настройки из внешних файлов конфигурации и записывать информацию о процессе нормализации.
            _configuration = configuration;
            _logger = logger;
        }

        public void Normalize(string path)
        {//Этот метод запускает процесс нормализации для аудиотрека по заданному пути. Он использует внешнюю утилиту (обычно это MP3Gain или аналог), путь к которой определяется в файле конфигурации.
            _logger.LogTrace("Normalization for {Path} started", path);
            var mp3GainPath = _configuration.GetValue<string>("Player:Mp3GainPath");
            var mp3Gain = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = mp3GainPath,
                    Arguments = $"-r -c {path}", //Аргументы командной строки для MP3Gain (-r -c) означают выполнение рекурсивной нормализации (для всех файлов в папке, если путь указывает на директорию) и коррекцию без изменения общей громкости (сохранение изменений в тегах файлов без фактического изменения аудиоданных).
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };
            mp3Gain.Start();
            mp3Gain.WaitForExit();
            _logger.LogTrace("Normalization for {Path} ended", path);
            //             "Normalization for {Path} started": Логирование начала процесса.
            // mp3GainPath: Получение пути к исполняемому файлу нормализатора из конфигурации.
            // Создание и настройка процесса mp3Gain: Настраивается для запуска без создания окна консоли, перенаправления вывода и выполнения в фоновом режиме.
            // mp3Gain.Start(): Запуск процесса нормализации.
            // mp3Gain.WaitForExit(): Ожидание завершения процесса нормализации.
            // "Normalization for {Path} ended": Логирование окончания процесса.
        }
        //Этот класс позволяет автоматизировать процесс нормализации звука в аудиотреках, что является важной частью подготовки медиафайлов к воспроизведению.
    }
}

/*
 * curl -o  mp3gain.zip -sL https://sourceforge.net/projects/mp3gain/files/mp3gain/1.6.1/mp3gain-1_6_1-src.zip
*/