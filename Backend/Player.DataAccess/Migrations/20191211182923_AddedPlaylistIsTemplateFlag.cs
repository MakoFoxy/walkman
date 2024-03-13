using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class AddedPlaylistIsTemplateFlag : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_Clients_ClientId",
                table: "Adverts");

            migrationBuilder.AddColumn<bool>(
                name: "IsTemplate",
                table: "Playlists",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClientId",
                table: "Adverts",
                nullable: false,
                oldClrType: typeof(Guid),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_Clients_ClientId",
                table: "Adverts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_Clients_ClientId",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "IsTemplate",
                table: "Playlists");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClientId",
                table: "Adverts",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_Clients_ClientId",
                table: "Adverts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
