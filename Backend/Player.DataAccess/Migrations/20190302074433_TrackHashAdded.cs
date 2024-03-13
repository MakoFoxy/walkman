using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class TrackHashAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "MusicTracks",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Hash",
                table: "Adverts",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Hash",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "Hash",
                table: "Adverts");
        }
    }
}
