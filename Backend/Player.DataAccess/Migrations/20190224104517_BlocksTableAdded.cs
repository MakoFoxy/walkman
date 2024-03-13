using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class BlocksTableAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "BlockId",
                table: "MusicTrackPlaylists",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BlockId",
                table: "AdvertPlaylists",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Blocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    LastMusicIndex = table.Column<int>(nullable: false),
                    LastAdvertIndex = table.Column<int>(nullable: false),
                    StartDateTime = table.Column<DateTime>(nullable: false),
                    BlockIndex = table.Column<int>(nullable: false),
                    PlaylistId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Blocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Blocks_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrackPlaylists_BlockId",
                table: "MusicTrackPlaylists",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_AdvertPlaylists_BlockId",
                table: "AdvertPlaylists",
                column: "BlockId");

            migrationBuilder.CreateIndex(
                name: "IX_Blocks_PlaylistId",
                table: "Blocks",
                column: "PlaylistId");

            migrationBuilder.AddForeignKey(
                name: "FK_AdvertPlaylists_Blocks_BlockId",
                table: "AdvertPlaylists",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackPlaylists_Blocks_BlockId",
                table: "MusicTrackPlaylists",
                column: "BlockId",
                principalTable: "Blocks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdvertPlaylists_Blocks_BlockId",
                table: "AdvertPlaylists");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackPlaylists_Blocks_BlockId",
                table: "MusicTrackPlaylists");

            migrationBuilder.DropTable(
                name: "Blocks");

            migrationBuilder.DropIndex(
                name: "IX_MusicTrackPlaylists_BlockId",
                table: "MusicTrackPlaylists");

            migrationBuilder.DropIndex(
                name: "IX_AdvertPlaylists_BlockId",
                table: "AdvertPlaylists");

            migrationBuilder.DropColumn(
                name: "BlockId",
                table: "MusicTrackPlaylists");

            migrationBuilder.DropColumn(
                name: "BlockId",
                table: "AdvertPlaylists");
        }
    }
}
