using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MarketRadio.Player.DataAccess.Migrations
{
    public partial class PlaybackResultAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            CREATE TEMPORARY TABLE Tracks_Migration(Id, Type, Name, Hash, Length);
            INSERT INTO Tracks_Migration SELECT Id, Type, Name, Hash, Length FROM Tracks;
            DROP TABLE Tracks;
            CREATE TABLE Tracks
            (
                Id       TEXT not null constraint PK_Tracks primary key,
                Type     TEXT,
                Name     TEXT,
                Hash     TEXT,
                Length   REAL not null
                );

            INSERT INTO Tracks SELECT Id, Type, Name, Hash, Length FROM Tracks_Migration;
            DROP TABLE Tracks_Migration;");

            migrationBuilder.CreateTable(
                name: "PlaybackResults",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    PlaylistId = table.Column<Guid>(nullable: false),
                    TrackId = table.Column<Guid>(nullable: false),
                    StartTime = table.Column<DateTime>(nullable: false),
                    EndTime = table.Column<DateTime>(nullable: true),
                    Status = table.Column<int>(nullable: false),
                    AdditionalInfo = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaybackResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlaybackResults_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaybackResults_Tracks_TrackId",
                        column: x => x.TrackId,
                        principalTable: "Tracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackResults_PlaylistId",
                table: "PlaybackResults",
                column: "PlaylistId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaybackResults_TrackId",
                table: "PlaybackResults",
                column: "TrackId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlaybackResults");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Tracks",
                type: "TEXT",
                nullable: true);
        }
    }
}
