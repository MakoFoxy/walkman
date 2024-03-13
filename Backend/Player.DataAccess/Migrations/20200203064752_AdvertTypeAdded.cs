using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class AdvertTypeAdded : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AdvertTypeId",
                table: "Adverts",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "AdvertTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Name = table.Column<string>(nullable: true),
                    Code = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdvertTypes", x => x.Id);
                });

            migrationBuilder.Sql("insert into \"AdvertTypes\"(\"Id\", \"Name\", \"Code\") VALUES ('8DD82515-3E2B-42E7-BB5F-7DBB550F09E4', 'Своя', 'Own')");
            migrationBuilder.Sql("insert into \"AdvertTypes\"(\"Id\", \"Name\", \"Code\") VALUES ('978B5E7D-0A54-496A-9052-48C63BF66550', 'Коммерческая', 'Commercial')");
            migrationBuilder.Sql("insert into \"AdvertTypes\"(\"Id\", \"Name\", \"Code\") VALUES ('78DBF737-60C4-4A9C-82C0-DD43CDADB2CF', 'Государственная', 'State')");

            migrationBuilder.Sql("update \"Adverts\" set \"AdvertTypeId\" = '978B5E7D-0A54-496A-9052-48C63BF66550'");
            
            migrationBuilder.CreateIndex(
                name: "IX_Adverts_AdvertTypeId",
                table: "Adverts",
                column: "AdvertTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Adverts_AdvertTypes_AdvertTypeId",
                table: "Adverts",
                column: "AdvertTypeId",
                principalTable: "AdvertTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Adverts_AdvertTypes_AdvertTypeId",
                table: "Adverts");

            migrationBuilder.DropTable(
                name: "AdvertTypes");

            migrationBuilder.DropIndex(
                name: "IX_Adverts_AdvertTypeId",
                table: "Adverts");

            migrationBuilder.DropColumn(
                name: "AdvertTypeId",
                table: "Adverts");
        }
    }
}
