using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class AdvertMovedToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_Clients_ClientId",
                table: "Adverts");

            migrationBuilder.DropForeignKey(
                name: "FK_UserObjects_Managers_ManagerId",
                table: "UserObjects");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_UserObjects_ManagerId",
                table: "UserObjects");

            migrationBuilder.DropIndex(
                name: "IX_Adverts_ClientId",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "ManagerId",
                table: "UserObjects");

            migrationBuilder.DropColumn(
                name: "ClientId",
                table: "Adverts");           

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Adverts",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));
         
            migrationBuilder.CreateIndex(
                name: "IX_Adverts_OrganizationId",
                table: "Adverts",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_Organizations_OrganizationId",
                table: "Adverts",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_Organizations_OrganizationId",
                table: "Adverts");

            migrationBuilder.DropForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Adverts_OrganizationId",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Adverts");

            migrationBuilder.AlterColumn<Guid>(
                name: "RoleId",
                table: "Users",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid));

            migrationBuilder.AddColumn<Guid>(
                name: "ManagerId",
                table: "UserObjects",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClientId",
                table: "Adverts",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("07abf324-2fc7-44fd-ba41-ada305a33c9d"),
                column: "RoleId",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_UserObjects_ManagerId",
                table: "UserObjects",
                column: "ManagerId");

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
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserObjects_Managers_ManagerId",
                table: "UserObjects",
                column: "ManagerId",
                principalTable: "Managers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Roles_RoleId",
                table: "Users",
                column: "RoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
