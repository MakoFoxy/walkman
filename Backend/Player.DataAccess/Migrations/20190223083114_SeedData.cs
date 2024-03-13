using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class SeedData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "TrackTypes",
                columns: new[] { "Id", "Code", "Name" },
                values: new object[,]
                {
                    { new Guid("225ea1d7-ede3-4733-ba4a-8ed9459f94c6"), "Advert", "Реклама" },
                    { new Guid("b8b2d657-394e-45a3-9a4d-25ffba47dcaf"), "Music", "Музыка" },
                    { new Guid("cdd0f07d-e4af-43d3-9a28-3b8674649fe4"), "Silent", "Тишина" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "Email", "FirstName", "LastName", "Password", "PhoneNumber", "RoleId", "SecondName" },
                values: new object[] { new Guid("07abf324-2fc7-44fd-ba41-ada305a33c9d"), "system@walkman.org", null, null, null, null, null, null });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TrackTypes",
                keyColumn: "Id",
                keyValue: new Guid("225ea1d7-ede3-4733-ba4a-8ed9459f94c6"));

            migrationBuilder.DeleteData(
                table: "TrackTypes",
                keyColumn: "Id",
                keyValue: new Guid("b8b2d657-394e-45a3-9a4d-25ffba47dcaf"));

            migrationBuilder.DeleteData(
                table: "TrackTypes",
                keyColumn: "Id",
                keyValue: new Guid("cdd0f07d-e4af-43d3-9a28-3b8674649fe4"));

            migrationBuilder.DeleteData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("07abf324-2fc7-44fd-ba41-ada305a33c9d"));
        }
    }
}
