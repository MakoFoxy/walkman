using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Extensions.Hosting;
using NpgsqlTypes;
using Serilog;
using Serilog.Sinks.PostgreSQL;

namespace Player.Helpers.App
{
    public static class SerilogHelper
    {
        public static IHostBuilder UseSerilogConfigured(this IHostBuilder hostBuilder)
        {
            IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
            {
                {"message", new RenderedMessageColumnWriter(NpgsqlDbType.Text) },
                {"message_template", new MessageTemplateColumnWriter(NpgsqlDbType.Text) },
                {"level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                {"raise_date", new TimestampColumnWriter(NpgsqlDbType.Timestamp) },
                {"exception", new ExceptionColumnWriter(NpgsqlDbType.Text) },
                {"properties", new LogEventSerializedColumnWriter(NpgsqlDbType.Jsonb) },
            };

            return hostBuilder.UseSerilog((context, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .Enrich.FromLogContext()
                .Enrich.WithCorrelationId()
                .Enrich.WithProperty("Service", Assembly.GetEntryAssembly()!.GetName().Name)
                .Filter.ByExcluding(logEvent => logEvent.Exception?.GetType() == typeof(OperationCanceledException))
#if !DEBUG
                .WriteTo.PostgreSQL(
                    context.Configuration.GetSection("Player:WalkmanConnectionString").Value,
                    "walkman_system_log",
                    columnWriters,
                    schemaName: "walkman_log",
                    respectCase: true,
                    useCopy: false,
                    needAutoCreateTable: false
                )
#endif
                .WriteTo.Console(
                    outputTemplate:
                    "[{Timestamp:dd.MM.yyyy HH:mm:ss} {Level:u3}] {CorrelationId} {Message:lj}{NewLine}{Exception}")
            );
            //    Этот метод расширения для IHostBuilder позволяет интегрировать и настроить Serilog при построении хоста приложения.
            // В методе определяется словарь columnWriters, который задаёт, как различные части лог-сообщений (например, текст сообщения, шаблон сообщения, уровень логирования, дата, исключения и дополнительные свойства) будут записываться в базу данных PostgreSQL. Каждому ключу соответствует определённый обработчик столбца.
            // Внутри лямбда-выражения Serilog настраивается для чтения конфигурации из файла конфигурации (например, appsettings.json), обогащения логов дополнительными данными (например, идентификатором корреляции и названием сервиса) и фильтрации определенных типов исключений (в данном случае OperationCanceledException).
            // Есть условная компиляция, которая исключает запись в базу данных PostgreSQL в режиме DEBUG для предотвращения ненужного логирования во время разработки.
            // Наконец, логи выводятся на консоль с использованием определённого шаблона.
        }
    }
}

// Этот подход обеспечивает централизованную настройку логирования, позволяя легко изменять конфигурацию и способ логирования (например, изменять целевые назначения логов или форматы сообщений) без изменения остального кода приложения.

// Serilog предлагает гибкий и мощный способ логирования для .NET приложений, обеспечивая поддержку множества целевых назначений (так называемых "sinks") и возможности расширенной настройки.

// В вашем коде используется Serilog для записи логов в базу данных PostgreSQL. Конфигурация определяется следующим образом:

//     Словарь columnWriters: Определяет, как данные из логов будут записываться в определенные столбцы таблицы базы данных. Каждому ключу в словаре соответствует колонка в таблице базы данных, а ColumnWriterBase определяет, как тип данных лога будет обрабатываться и сохраняться в соответствующем столбце.

//     Конфигурация Serilog для PostgreSQL:
//         При использовании WriteTo.PostgreSQL(...), логи будут записываться в таблицу walkman_system_log в схеме walkman_log базы данных PostgreSQL.
//         Детали подключения к базе данных и другие параметры, такие как имя таблицы, схема, и нужно ли автоматически создавать таблицу (needAutoCreateTable), указываются в аргументах этого метода.
//         В данном случае, запись в базу данных происходит только в среде, отличной от DEBUG, что контролируется директивой предпроцессора #if !DEBUG. Это сделано для предотвращения записи логов во время разработки или отладки.

//     Фильтрация и обогащение логов:
//         С помощью .Enrich.FromLogContext(), .Enrich.WithCorrelationId() и .Enrich.WithProperty("Service", ...), логи обогащаются дополнительной информацией, что может помочь в диагностике и отладке при возникновении проблем.
//         .Filter.ByExcluding(logEvent => logEvent.Exception?.GetType() == typeof(OperationCanceledException)) исключает из логирования события, связанные с исключениями типа OperationCanceledException, которые могут быть несущественными для определенных приложений.

// Использование базы данных для логирования позволяет централизованно собирать, хранить и анализировать логи. Это может быть особенно полезно для распределенных приложений, приложений с большим объемом трафика или в ситуациях, когда нужно сохранять логи в течение длительного времени для аудита и анализа.