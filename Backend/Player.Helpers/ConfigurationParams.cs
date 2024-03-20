using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Player.Helpers
{
    public static class ConfigurationParams
    {
        public static void Log(ILogger logger, IEnumerable<KeyValuePair<string, string>> configs, string prefix = "Player")
        {
            var skipConfigs = new[] { "PASSWORD", "TOKEN", "JWT", };

            var pairs = configs.Where(pair => !string.IsNullOrEmpty(pair.Value))
                .Where(pair => pair.Key != null && !skipConfigs.Any(sc => pair.Key.ToUpperInvariant().Contains(sc)))
                .Where(pair => pair.Value != null && !skipConfigs.Any(sc => pair.Value.ToUpperInvariant().Contains(sc)));
            //    Имеют не пустые значения.
            // Не содержат чувствительной информации в ключах.
            // Не содержат чувствительной информации в значениях.
            if (!string.IsNullOrWhiteSpace(prefix))
            {
                //Сначала проверяется, задан ли префикс и не является ли он пустым или строкой, состоящей только из пробельных символов. Если префикс не задан (пустой или состоит только из пробелов), дополнительная фильтрация не применяется.
                pairs = pairs.Where(pair => pair.Key.ToUpperInvariant().StartsWith(prefix.ToUpperInvariant()));
                //Если префикс задан, то из набора pairs выбираются только те пары ключ-значение, ключи которых начинаются с указанного префикса. Сравнение нечувствительно к регистру, так как как ключ (pair.Key), так и префикс (prefix) приводятся к верхнему регистру с помощью метода ToUpperInvariant() перед сравнением.
            }

            var serviceConfig = string.Join(Environment.NewLine, pairs.Select(p => $"{p.Key}: {p.Value}"));
            logger.LogInformation("App running with config with params: \n{ServiceConfig}", serviceConfig);
        }
    }
}

// Метод Log:

//     Параметры:
//         ILogger logger: Логгер, используемый для записи информации о конфигурации.
//         IEnumerable<KeyValuePair<string, string>> configs: Коллекция пар ключ-значение, представляющая параметры конфигурации.
//         string prefix: Префикс, который используется для фильтрации параметров конфигурации; по умолчанию "Player".
//     Логика работы:
//         skipConfigs: Массив ключевых слов, наличие которых в ключах или значениях конфигурации предотвращает логирование этих параметров. Это включает в себя слова, связанные с безопасностью, такие как "PASSWORD", "TOKEN" и "JWT", чтобы избежать случайного логирования конфиденциальной информации.
//         Последовательные фильтры применяются к исходной коллекции параметров:
//             Удаляются параметры, значения которых пустые или ключи содержат конфиденциальные данные, указанные в skipConfigs.
//             Если указан префикс, выбираются только те параметры, ключи которых начинаются с этого префикса (сравнение регистронезависимое).
//         После фильтрации создается строка serviceConfig, содержащая отфильтрованные и безопасные для логирования параметры.
//         Используя переданный ILogger, выводится информация о параметрах конфигурации приложения.

// Пример использования:

// Этот метод можно использовать для логирования безопасных параметров конфигурации при запуске приложения. Пример:

// csharp

// var configurationValues = new List<KeyValuePair<string, string>>
// {
//     // Примеры конфигурационных параметров
//     new KeyValuePair<string, string>("PlayerDatabaseEndpoint", "http://localhost:5000"),
//     new KeyValuePair<string, string>("PlayerToken", "some-secret-token")
// };

// ILogger logger = // Инициализация вашего логгера

// ConfigurationParams.Log(logger, configurationValues);

// В этом примере в журнал будет записан только параметр PlayerDatabaseEndpoint, так как PlayerToken содержит слово "TOKEN", которое присутствует в skipConfigs, и поэтому он будет исключен из логирования.
// Значение:

// Использование такого подхода помогает повысить безопасность приложения, предотвращая случайное логирование чувствительных данных. Это особенно важно в продакшн среде, где запись конфиденциальных данных в логи может привести к утечкам информации.
