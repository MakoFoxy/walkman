using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class UserObjectAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objects_Users_UserId",
                table: "Objects");

            migrationBuilder.DropIndex(
                name: "IX_Objects_UserId",
                table: "Objects");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Objects");

            migrationBuilder.CreateTable(
                name: "UserObjects",
                columns: table => new
                {
                    UserId = table.Column<Guid>(nullable: false),
                    ObjectId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserObjects", x => new { x.UserId, x.ObjectId });
                    table.ForeignKey(
                        name: "FK_UserObjects_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserObjects_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserObjects_ObjectId",
                table: "UserObjects",
                column: "ObjectId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserObjects");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "Objects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Objects_UserId",
                table: "Objects",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Objects_Users_UserId",
                table: "Objects",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
