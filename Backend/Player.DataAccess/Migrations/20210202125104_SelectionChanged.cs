using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class SelectionChanged : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectSelections_Objects_ObjectId",
                table: "ObjectSelections");

            migrationBuilder.DropForeignKey(
                name: "FK_ObjectSelections_Selections_SelectionId",
                table: "ObjectSelections");

            migrationBuilder.AddColumn<bool>(
                name: "IsPublic",
                table: "Selections",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectionId",
                table: "ObjectSelections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ObjectId",
                table: "ObjectSelections",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "BannedMusicInObject",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObjectId = table.Column<Guid>(type: "uuid", nullable: false),
                    MusicTrackId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BannedMusicInObject", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BannedMusicInObject_MusicTracks_MusicTrackId",
                        column: x => x.MusicTrackId,
                        principalTable: "MusicTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BannedMusicInObject_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BannedMusicInObject_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BannedMusicInObject_MusicTrackId",
                table: "BannedMusicInObject",
                column: "MusicTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_BannedMusicInObject_ObjectId",
                table: "BannedMusicInObject",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_BannedMusicInObject_UserId",
                table: "BannedMusicInObject",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectSelections_Objects_ObjectId",
                table: "ObjectSelections",
                column: "ObjectId",
                principalTable: "Objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectSelections_Selections_SelectionId",
                table: "ObjectSelections",
                column: "SelectionId",
                principalTable: "Selections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ObjectSelections_Objects_ObjectId",
                table: "ObjectSelections");

            migrationBuilder.DropForeignKey(
                name: "FK_ObjectSelections_Selections_SelectionId",
                table: "ObjectSelections");

            migrationBuilder.DropTable(
                name: "BannedMusicInObject");

            migrationBuilder.DropColumn(
                name: "IsPublic",
                table: "Selections");

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectionId",
                table: "ObjectSelections",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ObjectId",
                table: "ObjectSelections",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectSelections_Objects_ObjectId",
                table: "ObjectSelections",
                column: "ObjectId",
                principalTable: "Objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ObjectSelections_Selections_SelectionId",
                table: "ObjectSelections",
                column: "SelectionId",
                principalTable: "Selections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
