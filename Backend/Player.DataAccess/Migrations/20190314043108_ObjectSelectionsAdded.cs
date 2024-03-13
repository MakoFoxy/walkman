using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class ObjectSelectionsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Selections",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ObjectSelections",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ObjectId = table.Column<Guid>(nullable: true),
                    SelectionId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObjectSelections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObjectSelections_Objects_ObjectId",
                        column: x => x.ObjectId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ObjectSelections_Selections_SelectionId",
                        column: x => x.SelectionId,
                        principalTable: "Selections",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ObjectSelections_ObjectId",
                table: "ObjectSelections",
                column: "ObjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ObjectSelections_SelectionId",
                table: "ObjectSelections",
                column: "SelectionId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ObjectSelections");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "Selections");
        }
    }
}
