using System;
using System.Linq;
using Innofactor.EfCoreJsonValueConverter;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class ObjectInfoConfiguration : IEntityTypeConfiguration<ObjectInfo>
    {
        private readonly DbContext _context;

        public ObjectInfoConfiguration(DbContext context)
        {
            _context = context;
        }

        public void Configure(EntityTypeBuilder<ObjectInfo> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Bin).IsRequired();
            builder.Property(e => e.Name).IsRequired();

            if (_context.Database.IsNpgsql())
            {
                builder.Property(oi => oi.FreeDays)
                    .IsRequired()
                    .HasDefaultValueSql("array[]::varchar[]")
                    .HasPostgresArrayConversion(week => week.ToString(), s => Enum.Parse<DayOfWeek>(s));
            }
            else
            {
                builder.Property(oi => oi.FreeDays)
                    .HasConversion(
                        v => string.Join(",", v.ToString()),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries)
                            .Select(d => Enum.Parse<DayOfWeek>(d)).ToArray()
                    );
            }
            builder.Property(oi => oi.ClientSettings).HasColumnType("jsonb").HasJsonValueConversion();
            builder.OwnsOne(oi => oi.ResponsiblePersonOne);
            builder.OwnsOne(oi => oi.ResponsiblePersonTwo);
        }
    }
}


//     ObjectInfoConfiguration: Принимает экземпляр DbContext, который позволяет вам определять конфигурацию в зависимости от типа используемой базы данных. Это полезно, если ваше приложение поддерживает несколько типов баз данных.

// Метод Configure:

//     Определяет e.Id как первичный ключ для сущности.
//     Устанавливает свойства Bin и Name как обязательные (IsRequired()).
//     Особая логика для поля FreeDays:
//         Если используется PostgreSQL (проверяется с помощью _context.Database.IsNpgsql()), поле настраивается как массив PostgreSQL с помощью специальной функции HasPostgresArrayConversion. Это позволяет сохранять дни недели как массив в базе данных.
//         В противном случае, для других типов баз данных, FreeDays преобразуется в строку и обратно, используя методы string.Join и string.Split для сериализации и десериализации массива.
//     Для ClientSettings устанавливает тип данных jsonb и использует конвертер для хранения JSON, что подходит для работы с неструктурированными или сложными данными.
//     Использует метод OwnsOne для настройки связанных объектов ResponsiblePersonOne и ResponsiblePersonTwo как Value Objects, что является частью паттерна DDD (Domain-Driven Design).

// Применение:

//     Эта конфигурация используется для точной настройки маппинга класса ObjectInfo на таблицу в базе данных. Такие конфигурации важны для обеспечения правильности схемы данных, особенно при работе с различными типами баз данных и при необходимости внедрения сложных типов данных (как JSON).
//     Зависимость от конкретного контекста базы данных делает возможным изменять поведение маппинга в зависимости от типа базы данных, что может быть полезно в многоцелевых или мультиплатформенных приложениях.

// Этот подход позволяет разработчикам управлять представлением и поведением данных в приложении на более детальном уровне, а также адаптировать модель данных под конкретные требования и особенности баз данных, с которыми они работают.

//     SQL (Structured Query Language): Это язык программирования, используемый для управления данными, хранящимися в системах управления реляционными базами данных (RDBMS). SQL позволяет выполнять различные операции, такие как создание, изменение, управление и запрос данных в таблицах баз данных. SQL сосредоточен на структурированных данных, где информация организована строго в таблицах с определенными отношениями между ними.

//     JSON (JavaScript Object Notation): Это легкий формат обмена данными, который легко читается и пишется для людей и легко разбирается и генерируется машинами. JSON основан на подмножестве языка программирования JavaScript, но его использование не ограничивается только JavaScript. JSON часто используется для обмена данными между серверами и веб-приложениями или как формат для сериализации сложных данных.

// Когда мы говорим о типах данных json и jsonb в контексте PostgreSQL (или других систем управления базами данных, поддерживающих JSON), мы имеем в виду, что база данных позволяет хранить данные в формате JSON непосредственно в таблицах SQL. Это позволяет сочетать гибкость JSON с строгой структурой реляционной базы данных. Однако сам SQL-код не является JSON, и эти технологии служат разным аспектам управления и хранения данных.

// Таким образом, хотя SQL и JSON могут использоваться вместе (например, для хранения JSON в реляционной базе данных), они представляют разные языки и форматы данных.