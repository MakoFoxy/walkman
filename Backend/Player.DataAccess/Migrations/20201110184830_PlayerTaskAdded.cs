using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class PlayerTaskAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdTimes_Adverts_AdvertId",
                table: "AdTimes");

            migrationBuilder.DropForeignKey(
                name: "FK_AdTimes_Objects_ObjectId",
                table: "AdTimes");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackSelections_MusicTracks_MusicTrackId",
                table: "MusicTrackSelections");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackSelections_Selections_SelectionId",
                table: "MusicTrackSelections");

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectionId",
                table: "MusicTrackSelections",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "MusicTrackSelections",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ObjectId",
                table: "AdTimes",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AdvertId",
                table: "AdTimes",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Tasks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Type = table.Column<string>(nullable: false),
                    RegisterDate = table.Column<DateTimeOffset>(nullable: false),
                    FinishDate = table.Column<DateTimeOffset>(nullable: true),
                    IsFinished = table.Column<bool>(nullable: false),
                    SubjectId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tasks", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AdTimes_Adverts_AdvertId",
                table: "AdTimes",
                column: "AdvertId",
                principalTable: "Adverts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdTimes_Objects_ObjectId",
                table: "AdTimes",
                column: "ObjectId",
                principalTable: "Objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackSelections_MusicTracks_MusicTrackId",
                table: "MusicTrackSelections",
                column: "MusicTrackId",
                principalTable: "MusicTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackSelections_Selections_SelectionId",
                table: "MusicTrackSelections",
                column: "SelectionId",
                principalTable: "Selections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdTimes_Adverts_AdvertId",
                table: "AdTimes");

            migrationBuilder.DropForeignKey(
                name: "FK_AdTimes_Objects_ObjectId",
                table: "AdTimes");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackSelections_MusicTracks_MusicTrackId",
                table: "MusicTrackSelections");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackSelections_Selections_SelectionId",
                table: "MusicTrackSelections");

            migrationBuilder.DropTable(
                name: "Tasks");

            migrationBuilder.AlterColumn<Guid>(
                name: "SelectionId",
                table: "MusicTrackSelections",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "MusicTrackSelections",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "ObjectId",
                table: "AdTimes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "AdvertId",
                table: "AdTimes",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_AdTimes_Adverts_AdvertId",
                table: "AdTimes",
                column: "AdvertId",
                principalTable: "Adverts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AdTimes_Objects_ObjectId",
                table: "AdTimes",
                column: "ObjectId",
                principalTable: "Objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackSelections_MusicTracks_MusicTrackId",
                table: "MusicTrackSelections",
                column: "MusicTrackId",
                principalTable: "MusicTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackSelections_Selections_SelectionId",
                table: "MusicTrackSelections",
                column: "SelectionId",
                principalTable: "Selections",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
