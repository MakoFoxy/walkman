using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class ClientConfigurationAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ClientConfigurationId",
                table: "Objects",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientConfiguration",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    VolumeTime = table.Column<string>(type: "jsonb", nullable: true),
                    AdvertVolume = table.Column<double>(nullable: false),
                    MusicVolume = table.Column<double>(nullable: false),
                    LastOnlineTime = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientConfiguration", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Objects_ClientConfigurationId",
                table: "Objects",
                column: "ClientConfigurationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Objects_ClientConfiguration_ClientConfigurationId",
                table: "Objects",
                column: "ClientConfigurationId",
                principalTable: "ClientConfiguration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Objects_ClientConfiguration_ClientConfigurationId",
                table: "Objects");

            migrationBuilder.DropTable(
                name: "ClientConfiguration");

            migrationBuilder.DropIndex(
                name: "IX_Objects_ClientConfigurationId",
                table: "Objects");

            migrationBuilder.DropColumn(
                name: "ClientConfigurationId",
                table: "Objects");
        }
    }
}
