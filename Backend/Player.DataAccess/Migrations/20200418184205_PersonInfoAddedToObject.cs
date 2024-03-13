using Microsoft.EntityFrameworkCore.Migrations;

namespace Player.DataAccess.Migrations
{
    public partial class PersonInfoAddedToObject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonOne_ComplexName",
                table: "Objects",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonOne_Email",
                table: "Objects",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonOne_Phone",
                table: "Objects",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonTwo_ComplexName",
                table: "Objects",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonTwo_Email",
                table: "Objects",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsiblePersonTwo_Phone",
                table: "Objects",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ResponsiblePersonOne_ComplexName",
                table: "Objects");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonOne_Email",
                table: "Objects");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonOne_Phone",
                table: "Objects");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonTwo_ComplexName",
                table: "Objects");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonTwo_Email",
                table: "Objects");

            migrationBuilder.DropColumn(
                name: "ResponsiblePersonTwo_Phone",
                table: "Objects");
        }
    }
}
