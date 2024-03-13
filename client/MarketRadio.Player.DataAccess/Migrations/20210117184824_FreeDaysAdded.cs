using Microsoft.EntityFrameworkCore.Migrations;

namespace MarketRadio.Player.DataAccess.Migrations
{
    public partial class FreeDaysAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FreeDays",
                table: "ObjectInfos",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FreeDays",
                table: "ObjectInfos");
        }
    }
}
