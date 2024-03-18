using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class AdHistoryConfiguration : IEntityTypeConfiguration<AdHistory>
    {
        public void Configure(EntityTypeBuilder<AdHistory> builder)
        {
            builder.HasKey(e => e.Id);
        }
    }
}

// Класс AdHistoryConfiguration:

//     Класс реализует интерфейс IEntityTypeConfiguration<AdHistory>, предоставляемый Entity Framework Core, который позволяет настраивать аспекты маппинга для сущности AdHistory.
//     В методе Configure(EntityTypeBuilder<AdHistory> builder) вы определяете конкретные настройки для сущности AdHistory.

// Настройка Сущности:

//     builder.HasKey(e => e.Id): Этот вызов устанавливает свойство Id сущности AdHistory в качестве первичного ключа таблицы в базе данных. Это стандартный подход для обозначения уникального идентификатора каждой записи.

// Применение:

//     Обычно, этот класс конфигурации будет использоваться в методе OnModelCreating контекста базы данных Entity Framework. Там вы можете добавить эту конфигурацию, чтобы EF Core знал, как интерпретировать и маппить сущность AdHistory на таблицу в вашей базе данных.

// Это базовая настройка, и в зависимости от структуры и требований вашей сущности AdHistory, вы можете добавить дополнительные настройки, такие как конфигурации связей (отношения между сущностями), индексы, ограничения и другие характеристики колонок. Если у AdHistory есть дополнительные свойства, которые вы хотите отобразить на колонки в базе данных, вы должны добавить соответствующие настройки в этот класс конфигурации.