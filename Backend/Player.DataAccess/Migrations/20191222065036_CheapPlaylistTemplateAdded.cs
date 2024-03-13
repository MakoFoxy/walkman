using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class CheapPlaylistTemplateAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CheapPlaylistTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Index = table.Column<int>(nullable: false),
                    ObjectInfoId = table.Column<Guid>(nullable: false),
                    DateBegin = table.Column<DateTime>(nullable: false),
                    DateEnd = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CheapPlaylistTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CheapPlaylistTemplates_Objects_ObjectInfoId",
                        column: x => x.ObjectInfoId,
                        principalTable: "Objects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CheapPlaylistTemplates_ObjectInfoId",
                table: "CheapPlaylistTemplates",
                column: "ObjectInfoId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CheapPlaylistTemplates");
        }
    }
}
