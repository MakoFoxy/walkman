using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class MaxAdvertBlockTimeOnObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_AdvertTypes_AdvertTypeId",
                table: "Adverts");

            migrationBuilder.AddColumn<int>(
                name: "MaxAdvertBlockInSeconds",
                table: "Objects",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<Guid>(
                name: "AdvertTypeId",
                table: "Adverts",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_AdvertTypes_AdvertTypeId",
                table: "Adverts",
                column: "AdvertTypeId",
                principalTable: "AdvertTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_AdvertTypes_AdvertTypeId",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "MaxAdvertBlockInSeconds",
                table: "Objects");

            migrationBuilder.AlterColumn<Guid>(
                name: "AdvertTypeId",
                table: "Adverts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_AdvertTypes_AdvertTypeId",
                table: "Adverts",
                column: "AdvertTypeId",
                principalTable: "AdvertTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
