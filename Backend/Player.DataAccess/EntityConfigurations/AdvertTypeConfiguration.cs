using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class AdvertTypeConfiguration : IEntityTypeConfiguration<AdvertType>
    {
        public void Configure(EntityTypeBuilder<AdvertType> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Code).IsRequired();
            builder.Property(e => e.Name).IsRequired();
        }
    }
}

//     Player.DataAccess.EntityConfigurations: Это пространство имен, вероятно, используется для организации всех классов конфигурации сущностей вашего проекта. Оно помогает удерживать все конфигурации в одном месте, что упрощает управление и обслуживание.

// Класс AdvertTypeConfiguration:

//     Этот класс реализует интерфейс IEntityTypeConfiguration<AdvertType>, что позволяет настраивать детали маппинга для сущности AdvertType в контексте базы данных.
//     Метод Configure(EntityTypeBuilder<AdvertType> builder) используется для определения настроек сущности.

// Конфигурация Сущности:

//     builder.HasKey(e => e.Id): Задает свойство Id как первичный ключ в соответствующей таблице базы данных. Это означает, что каждый экземпляр AdvertType будет уникально идентифицироваться по своему Id.
//     builder.Property(e => e.Code).IsRequired(): Указывает, что свойство Code является обязательным (IsRequired), т.е. каждая запись в таблице должна иметь значение для этого поля, и оно не может быть NULL.
//     builder.Property(e => e.Name).IsRequired(): Аналогично, делает свойство Name обязательным, требуя, чтобы каждая запись содержала значение для этого поля.

// Использование:

// Как и другие классы конфигурации, AdvertTypeConfiguration обычно используется в методе OnModelCreating DbContext для настройки метаданных модели. Это включает в себя отображение сущностей на таблицы базы данных, настройку ключей, свойств и отношений между сущностями.

// Конфигурация сущности AdvertType помогает убедиться, что данные сохраняются правильно и что соблюдаются все необходимые ограничения целостности. Это особенно важно для сущностей, которые представляют ключевые бизнес-концепции в вашем приложении, как в данном случае типы рекламы.