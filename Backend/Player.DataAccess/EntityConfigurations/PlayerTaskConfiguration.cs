using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class PlayerTaskConfiguration : IEntityTypeConfiguration<PlayerTask>
    {
        public void Configure(EntityTypeBuilder<PlayerTask> builder)
        {
            builder.Property(pt => pt.Type)
                .HasConversion(d => d.ToString(),
                    d => (TaskType)Enum.Parse(typeof(TaskType), d))
                .IsRequired();
        }
    }
}