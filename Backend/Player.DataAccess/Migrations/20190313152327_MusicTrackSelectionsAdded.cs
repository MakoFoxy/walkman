using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class MusicTrackSelectionsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Selections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Selections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicTrackSelections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MusicTrackId = table.Column<Guid>(nullable: true),
                    SelectionId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicTrackSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicTrackSelections_MusicTracks_MusicTrackId",
                        column: x => x.MusicTrackId,
                        principalTable: "MusicTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MusicTrackSelections_Selections_SelectionId",
                        column: x => x.SelectionId,
                        principalTable: "Selections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrackSelections_MusicTrackId",
                table: "MusicTrackSelections",
                column: "MusicTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrackSelections_SelectionId",
                table: "MusicTrackSelections",
                column: "SelectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicTrackSelections");

            migrationBuilder.DropTable(
                name: "Selections");
        }
    }
}
