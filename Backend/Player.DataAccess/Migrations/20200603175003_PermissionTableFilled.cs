using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class PermissionTableFilled : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Permission",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { new Guid("79ed7efc-d0c0-4e37-92cf-05358142af21"), "CreateAdvert" },
                    { new Guid("dd71d278-fefe-4c76-8d1d-a4a157108d87"), "ReadClientById" },
                    { new Guid("ba1ff31b-4ae3-4906-b51f-82238d237100"), "ReadAllTrackTypes" },
                    { new Guid("3e6231e2-7643-42c4-9296-347ba9653c86"), "ReadAllServiceCompanies" },
                    { new Guid("92a30ca9-8850-498f-9b15-9faff4a9299b"), "ReadAllSelections" },
                    { new Guid("2cd4c765-4567-4a52-b3a2-99d7c1194f85"), "ReadAllObjectsForDropDown" },
                    { new Guid("8cd4daeb-dc96-4f64-b175-f42cef040188"), "ReadAllObjects" },
                    { new Guid("16918507-55ae-4657-a72c-cf5d9759a808"), "ReadAllObjectById" },
                    { new Guid("84ba4a7b-1b3c-43ad-81ca-1a73168da536"), "ReadAllMusic" },
                    { new Guid("13a6811f-f9b9-4354-be6f-48df248aca04"), "ReadAllGenres" },
                    { new Guid("31941bbe-4308-49f3-acb6-58db963d836a"), "ReadAllClients" },
                    { new Guid("76249744-9e95-4dec-9665-09719993f0a2"), "ReadMediaPlanReport" },
                    { new Guid("2cde726b-c5c4-4441-bf50-9f92cf674ae9"), "ReadAllCities" },
                    { new Guid("f249b48d-7c83-4c79-aa83-930c705e0e3e"), "ReadAllAdvertTypes" },
                    { new Guid("6a004ddf-5920-450b-b16d-86e7c95e403a"), "ReadAllActivityTypes" },
                    { new Guid("e8d11089-e690-4d9f-b91e-ac2f1a3cdc21"), "ReadAdvertById" },
                    { new Guid("3db34eba-7903-4e80-83ec-c93fa5603eb1"), "EditSelection" },
                    { new Guid("45efa881-e08a-4119-88ed-6eb535c441ec"), "EditObject" },
                    { new Guid("107bc7a5-51ab-40a5-8c2c-e31d0f3ea6f9"), "EditClient" },
                    { new Guid("fdce8ba2-c23c-4c22-a341-8c359ac0039e"), "CreateSelection" },
                    { new Guid("66d28efd-c157-4f18-946f-8be64d839048"), "CreateObject" },
                    { new Guid("2e9fda7f-5d33-4142-8035-e14f00a8d48d"), "CreateMusic" },
                    { new Guid("b617006f-e30d-4f30-97e5-4422574b35cd"), "CreateClient" },
                    { new Guid("f2e268dd-2313-400d-82e5-7b7c0455dcc3"), "ReadAllAdverts" },
                    { new Guid("05abbc51-15d6-4c28-9197-a6fedec33257"), "ReadSelectionById" }
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("05abbc51-15d6-4c28-9197-a6fedec33257"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("107bc7a5-51ab-40a5-8c2c-e31d0f3ea6f9"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("13a6811f-f9b9-4354-be6f-48df248aca04"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("16918507-55ae-4657-a72c-cf5d9759a808"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("2cd4c765-4567-4a52-b3a2-99d7c1194f85"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("2cde726b-c5c4-4441-bf50-9f92cf674ae9"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("2e9fda7f-5d33-4142-8035-e14f00a8d48d"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("31941bbe-4308-49f3-acb6-58db963d836a"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("3db34eba-7903-4e80-83ec-c93fa5603eb1"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("3e6231e2-7643-42c4-9296-347ba9653c86"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("45efa881-e08a-4119-88ed-6eb535c441ec"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("66d28efd-c157-4f18-946f-8be64d839048"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("6a004ddf-5920-450b-b16d-86e7c95e403a"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("76249744-9e95-4dec-9665-09719993f0a2"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("79ed7efc-d0c0-4e37-92cf-05358142af21"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("84ba4a7b-1b3c-43ad-81ca-1a73168da536"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("8cd4daeb-dc96-4f64-b175-f42cef040188"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("92a30ca9-8850-498f-9b15-9faff4a9299b"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("b617006f-e30d-4f30-97e5-4422574b35cd"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("ba1ff31b-4ae3-4906-b51f-82238d237100"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("dd71d278-fefe-4c76-8d1d-a4a157108d87"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("e8d11089-e690-4d9f-b91e-ac2f1a3cdc21"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("f249b48d-7c83-4c79-aa83-930c705e0e3e"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("f2e268dd-2313-400d-82e5-7b7c0455dcc3"));

            migrationBuilder.DeleteData(
                table: "Permission",
                keyColumn: "Id",
                keyValue: new Guid("fdce8ba2-c23c-4c22-a341-8c359ac0039e"));
        }
    }
}
