using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MarketRadio.Player.DataAccess.Migrations
{
    public partial class BanListAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BanLists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MusicTrackId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ObjectId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BanLists", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BanLists");
        }
    }
}
