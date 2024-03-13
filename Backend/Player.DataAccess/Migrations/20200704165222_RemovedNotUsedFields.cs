using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class RemovedNotUsedFields : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_Managers_ManagerId",
                table: "Adverts");

            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_TrackTypes_TrackTypeId",
                table: "Adverts");

            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_Users_UploaderId",
                table: "Adverts");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackGenres_Genres_GenreId",
                table: "MusicTrackGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackGenres_MusicTracks_MusicTrackId",
                table: "MusicTrackGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_Managers_ManagerId",
                table: "MusicTracks");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_TrackTypes_TrackTypeId",
                table: "MusicTracks");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_Users_UploaderId",
                table: "MusicTracks");

            migrationBuilder.DropForeignKey(
                name: "FK_Objects_ActivityTypes_ActivityTypeId",
                table: "Objects");

            migrationBuilder.DropIndex(
                name: "IX_MusicTracks_ManagerId",
                table: "MusicTracks");

            migrationBuilder.DropIndex(
                name: "IX_Adverts_ManagerId",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "Adverts");

            migrationBuilder.AlterColumn<Guid>(
                name: "ActivityTypeId",
                table: "Objects",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UploaderId",
                table: "MusicTracks",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TrackTypeId",
                table: "MusicTracks",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "MusicTrackGenres",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "GenreId",
                table: "MusicTrackGenres",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "UploaderId",
                table: "Adverts",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "TrackTypeId",
                table: "Adverts",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_TrackTypes_TrackTypeId",
                table: "Adverts",
                column: "TrackTypeId",
                principalTable: "TrackTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_Users_UploaderId",
                table: "Adverts",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackGenres_Genres_GenreId",
                table: "MusicTrackGenres",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackGenres_MusicTracks_MusicTrackId",
                table: "MusicTrackGenres",
                column: "MusicTrackId",
                principalTable: "MusicTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_TrackTypes_TrackTypeId",
                table: "MusicTracks",
                column: "TrackTypeId",
                principalTable: "TrackTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_Users_UploaderId",
                table: "MusicTracks",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Objects_ActivityTypes_ActivityTypeId",
                table: "Objects",
                column: "ActivityTypeId",
                principalTable: "ActivityTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_TrackTypes_TrackTypeId",
                table: "Adverts");

            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_Users_UploaderId",
                table: "Adverts");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackGenres_Genres_GenreId",
                table: "MusicTrackGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTrackGenres_MusicTracks_MusicTrackId",
                table: "MusicTrackGenres");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_TrackTypes_TrackTypeId",
                table: "MusicTracks");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_Users_UploaderId",
                table: "MusicTracks");

            migrationBuilder.DropForeignKey(
                name: "FK_Objects_ActivityTypes_ActivityTypeId",
                table: "Objects");

            migrationBuilder.AlterColumn<Guid>(
                name: "ActivityTypeId",
                table: "Objects",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "UploaderId",
                table: "MusicTracks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "TrackTypeId",
                table: "MusicTracks",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "MusicTracks",
                type: "uuid",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "MusicTrackGenres",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "GenreId",
                table: "MusicTrackGenres",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "UploaderId",
                table: "Adverts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "TrackTypeId",
                table: "Adverts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "Adverts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_ManagerId",
                table: "MusicTracks",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_Adverts_ManagerId",
                table: "Adverts",
                column: "ManagerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_Managers_ManagerId",
                table: "Adverts",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_TrackTypes_TrackTypeId",
                table: "Adverts",
                column: "TrackTypeId",
                principalTable: "TrackTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_Users_UploaderId",
                table: "Adverts",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackGenres_Genres_GenreId",
                table: "MusicTrackGenres",
                column: "GenreId",
                principalTable: "Genres",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTrackGenres_MusicTracks_MusicTrackId",
                table: "MusicTrackGenres",
                column: "MusicTrackId",
                principalTable: "MusicTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_Managers_ManagerId",
                table: "MusicTracks",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_TrackTypes_TrackTypeId",
                table: "MusicTracks",
                column: "TrackTypeId",
                principalTable: "TrackTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_Users_UploaderId",
                table: "MusicTracks",
                column: "UploaderId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Objects_ActivityTypes_ActivityTypeId",
                table: "Objects",
                column: "ActivityTypeId",
                principalTable: "ActivityTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
