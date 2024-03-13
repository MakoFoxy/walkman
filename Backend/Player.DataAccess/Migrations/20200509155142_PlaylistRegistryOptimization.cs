using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class PlaylistRegistryOptimization : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AdvertsCount",
                table: "Playlists",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<double>(
                name: "Loading",
                table: "Playlists",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<bool>(
                name: "Overloaded",
                table: "Playlists",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "UniqueAdvertsCount",
                table: "Playlists",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdvertsCount",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "Loading",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "Overloaded",
                table: "Playlists");

            migrationBuilder.DropColumn(
                name: "UniqueAdvertsCount",
                table: "Playlists");
        }
    }
}
