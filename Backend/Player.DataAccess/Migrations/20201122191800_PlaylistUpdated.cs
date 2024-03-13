using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class PlaylistUpdated : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BlockIndex",
                table: "MusicTrackPlaylists");

            migrationBuilder.DropColumn(
                name: "PlayingDateTime",
                table: "MusicTrackPlaylists");

            migrationBuilder.DropColumn(
                name: "BlockIndex",
                table: "AdvertPlaylists");

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "MusicTrackPlaylists",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Index",
                table: "MusicTrackPlaylists");

            migrationBuilder.AddColumn<int>(
                name: "BlockIndex",
                table: "MusicTrackPlaylists",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlayingDateTime",
                table: "MusicTrackPlaylists",
                type: "timestamp without time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "BlockIndex",
                table: "AdvertPlaylists",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}
