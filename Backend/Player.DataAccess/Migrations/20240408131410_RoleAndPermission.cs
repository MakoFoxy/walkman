using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.DataAccess.Migrations
{
    public partial class RoleAndPermission : Migration
    {
      protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("insert into \"Roles\" (\"Id\", \"Name\") VALUES ('AB5BC32A-BD5C-4390-9990-69FBCB1A2463', 'Администратор')");
            migrationBuilder.Sql("insert into \"RolePermissions\" (\"RoleId\", \"PermissionId\") SELECT 'AB5BC32A-BD5C-4390-9990-69FBCB1A2463', \"Id\" FROM \"Permissions\"");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("delete from \"Roles\" where  \"Id\" = 'AB5BC32A-BD5C-4390-9990-69FBCB1A2463'");
            migrationBuilder.Sql("delete from \"RolePermissions\" where  \"RoleId\" = 'AB5BC32A-BD5C-4390-9990-69FBCB1A2463'");
        }
    }
}
