using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class ClientSettingsAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
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

            migrationBuilder.AddColumn<string>(
                name: "ClientSettings",
                table: "Objects",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ClientSettings",
                table: "Objects");

            migrationBuilder.AddColumn<Guid>(
                name: "ClientConfigurationId",
                table: "Objects",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClientConfiguration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdvertVolume = table.Column<double>(type: "double precision", nullable: false),
                    LastOnlineTime = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    MusicVolume = table.Column<double>(type: "double precision", nullable: false),
                    VolumeTime = table.Column<string>(type: "jsonb", nullable: true)
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
    }
}
