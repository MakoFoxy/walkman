using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class GenresAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Genres",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Genres", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MusicTrackGenres",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    MusicTrackId = table.Column<Guid>(nullable: true),
                    GenreId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MusicTrackGenres", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MusicTrackGenres_Genres_GenreId",
                        column: x => x.GenreId,
                        principalTable: "Genres",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MusicTrackGenres_MusicTracks_MusicTrackId",
                        column: x => x.MusicTrackId,
                        principalTable: "MusicTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrackGenres_GenreId",
                table: "MusicTrackGenres",
                column: "GenreId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTrackGenres_MusicTrackId",
                table: "MusicTrackGenres",
                column: "MusicTrackId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MusicTrackGenres");

            migrationBuilder.DropTable(
                name: "Genres");
        }
    }
}
