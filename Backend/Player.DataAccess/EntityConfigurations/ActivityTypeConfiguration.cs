using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class ActivityTypeConfiguration : IEntityTypeConfiguration<ActivityType>
    {
        public void Configure(EntityTypeBuilder<ActivityType> builder)
        {
            builder.HasKey(e => e.Id);
            builder.Property(e => e.Code).IsRequired();
            builder.Property(e => e.Name).IsRequired();
        }
    }
}

//     Этот класс реализует интерфейс IEntityTypeConfiguration<ActivityType> из Entity Framework Core, что позволяет настроить аспекты сущности ActivityType в контексте базы данных.
//     Метод Configure(EntityTypeBuilder<ActivityType> builder): В этом методе определяется конфигурация для сущности ActivityType. Он принимает EntityTypeBuilder<ActivityType>, инструмент для настройки сущности в модели.

// Настройка Сущности:

//     builder.HasKey(e => e.Id): Эта строка указывает, что свойство Id сущности ActivityType должно использоваться в качестве первичного ключа в соответствующей таблице базы данных.
//     builder.Property(e => e.Code).IsRequired(): Здесь указывается, что свойство Code сущности является обязательным (IsRequired), что означает, что для этого поля не может быть установлено значение NULL в базе данных.
//     builder.Property(e => e.Name).IsRequired(): Аналогично, эта строка делает свойство Name обязательным для сущности.