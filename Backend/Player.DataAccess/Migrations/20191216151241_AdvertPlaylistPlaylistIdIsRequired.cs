using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class AdvertPlaylistPlaylistIdIsRequired : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertPlaylists_Adverts_AdvertId",
                table: "AdvertPlaylists");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertPlaylists_Playlists_PlaylistId",
                table: "AdvertPlaylists");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlaylistId",
                table: "AdvertPlaylists",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AdvertId",
                table: "AdvertPlaylists",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertPlaylists_Adverts_AdvertId",
                table: "AdvertPlaylists",
                column: "AdvertId",
                principalTable: "Adverts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertPlaylists_Playlists_PlaylistId",
                table: "AdvertPlaylists",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertPlaylists_Adverts_AdvertId",
                table: "AdvertPlaylists");

            migrationBuilder.DropForeignKey(
                name: "FK_AdvertPlaylists_Playlists_PlaylistId",
                table: "AdvertPlaylists");

            migrationBuilder.AlterColumn<Guid>(
                name: "PlaylistId",
                table: "AdvertPlaylists",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "AdvertId",
                table: "AdvertPlaylists",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertPlaylists_Adverts_AdvertId",
                table: "AdvertPlaylists",
                column: "AdvertId",
                principalTable: "Adverts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertPlaylists_Playlists_PlaylistId",
                table: "AdvertPlaylists",
                column: "PlaylistId",
                principalTable: "Playlists",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
