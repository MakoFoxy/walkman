using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class OrganizationAddedToSelection : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "Selections",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Selections_OrganizationId",
                table: "Selections",
                column: "OrganizationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Selections_Organizations_OrganizationId",
                table: "Selections",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Selections_Organizations_OrganizationId",
                table: "Selections");

            migrationBuilder.DropIndex(
                name: "IX_Selections_OrganizationId",
                table: "Selections");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "Selections");
        }
    }
}
