using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class AdHistoryStartEndDateTimeAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdHistories_Adverts_AdvertId",
                table: "AdHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_AdHistories_Objects_ObjectId",
                table: "AdHistories");
            
            migrationBuilder.AlterColumn<Guid>(
                name: "ObjectId",
                table: "AdHistories",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AdvertId",
                table: "AdHistories",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "End",
                table: "AdHistories",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddForeignKey(
                name: "FK_AdHistories_Adverts_AdvertId",
                table: "AdHistories",
                column: "AdvertId",
                principalTable: "Adverts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AdHistories_Objects_ObjectId",
                table: "AdHistories",
                column: "ObjectId",
                principalTable: "Objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AdHistories_Adverts_AdvertId",
                table: "AdHistories");

            migrationBuilder.DropForeignKey(
                name: "FK_AdHistories_Objects_ObjectId",
                table: "AdHistories");

            migrationBuilder.DropColumn(
                name: "End",
                table: "AdHistories");
            
            migrationBuilder.AlterColumn<Guid>(
                name: "ObjectId",
                table: "AdHistories",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AlterColumn<Guid>(
                name: "AdvertId",
                table: "AdHistories",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_AdHistories_Adverts_AdvertId",
                table: "AdHistories",
                column: "AdvertId",
                principalTable: "Adverts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AdHistories_Objects_ObjectId",
                table: "AdHistories",
                column: "ObjectId",
                principalTable: "Objects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
