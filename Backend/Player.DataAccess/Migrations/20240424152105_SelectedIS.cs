using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Player.DataAccess.Migrations
{
    public partial class SelectedIS : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSelected",
                table: "ObjectSelections",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSelected",
                table: "ObjectSelections");
        }
    }
}
