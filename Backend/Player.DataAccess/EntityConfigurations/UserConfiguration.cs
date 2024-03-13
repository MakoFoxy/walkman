using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(e => e.Id);
            builder.HasData(new List<User>
            {
                new User
                {
                    Id = Guid.Parse("07ABF324-2FC7-44FD-BA41-ADA305A33C9D"),
                    Email = User.SystemUserEmail
                }
            });
        }
    }
}
