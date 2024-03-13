using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class ObjectFreeDaysAdd : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string[]>(
                name: "FreeDays",
                table: "Objects",
                nullable: false,
                defaultValueSql: "array[]::varchar[]");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeDays",
                table: "Objects");
        }
    }
}
