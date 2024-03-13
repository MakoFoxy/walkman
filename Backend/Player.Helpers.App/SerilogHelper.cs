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
        }
    }
}