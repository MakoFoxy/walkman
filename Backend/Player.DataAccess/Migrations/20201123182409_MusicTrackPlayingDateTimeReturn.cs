using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class MusicTrackPlayingDateTimeReturn : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackPlaylists_MusicTracks_MusicTrackId",
                table: "MusicTrackPlaylists");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackPlaylists_Playlists_PlaylistId",
                table: "MusicTrackPlaylists");

            migrationBuilder.DropColumn(
                name: "Index",
                table: "MusicTrackPlaylists");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlaylistId",
                table: "MusicTrackPlaylists",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "MusicTrackPlaylists",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlayingDateTime",
                table: "MusicTrackPlaylists",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackPlaylists_MusicTracks_MusicTrackId",
                table: "MusicTrackPlaylists",
                column: "MusicTrackId",
                principalTable: "MusicTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackPlaylists_Playlists_PlaylistId",
                table: "MusicTrackPlaylists",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackPlaylists_MusicTracks_MusicTrackId",
                table: "MusicTrackPlaylists");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackPlaylists_Playlists_PlaylistId",
                table: "MusicTrackPlaylists");

            migrationBuilder.DropColumn(
                name: "PlayingDateTime",
                table: "MusicTrackPlaylists");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlaylistId",
                table: "MusicTrackPlaylists",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "MusicTrackPlaylists",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<int>(
                name: "Index",
                table: "MusicTrackPlaylists",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackPlaylists_MusicTracks_MusicTrackId",
                table: "MusicTrackPlaylists",
                column: "MusicTrackId",
                principalTable: "MusicTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackPlaylists_Playlists_PlaylistId",
                table: "MusicTrackPlaylists",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
