using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class ClientsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Adverts",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Clients",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Bin = table.Column<string>(nullable: true),
                    LegalAddress = table.Column<string>(nullable: true),
                    Bank = table.Column<string>(nullable: true),
                    Iik = table.Column<string>(nullable: true),
                    FirstPerson = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    Phone = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clients", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Adverts_ClientId",
                table: "Adverts",
                column: "ClientId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_Clients_ClientId",
                table: "Adverts",
                column: "ClientId",
                principalTable: "Clients",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_Clients_ClientId",
                table: "Adverts");

            migrationBuilder.DropTable(
                name: "Clients");

            migrationBuilder.DropIndex(
                name: "IX_Adverts_ClientId",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Adverts");
        }
    }
}
