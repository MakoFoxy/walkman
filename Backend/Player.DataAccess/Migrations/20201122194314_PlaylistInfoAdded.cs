using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class PlaylistInfoAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlaylistInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Info = table.Column<string>(nullable: false),
                    CreateDate = table.Column<DateTime>(nullable: false),
                    PlaylistId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaylistInfos_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistInfos_PlaylistId",
                table: "PlaylistInfos",
                column: "PlaylistId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaylistInfos");
        }
    }
}
