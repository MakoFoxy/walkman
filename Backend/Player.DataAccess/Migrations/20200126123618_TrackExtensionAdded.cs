using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class TrackExtensionAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "MusicTracks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Extension",
                table: "Adverts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Extension",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "Extension",
                table: "Adverts");
        }
    }
}
