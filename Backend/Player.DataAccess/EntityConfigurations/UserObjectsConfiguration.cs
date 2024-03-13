using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Player.Domain;

namespace Player.DataAccess.EntityConfigurations
{
    public class UserObjectsConfiguration : IEntityTypeConfiguration<UserObjects>
    {
        public void Configure(EntityTypeBuilder<UserObjects> builder)
        {
            builder.HasKey(uo => new
            {
                uo.UserId,
                uo.ObjectId
            });
        }
    }
}