using Microsoft.EntityFrameworkCore.Migrations;

namespace Plugin.Database.Migrations
{
    public partial class settings : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HideCD",
                schema: "characters",
                table: "characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HideHPMP",
                schema: "characters",
                table: "characters",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HideCD",
                schema: "characters",
                table: "characters");

            migrationBuilder.DropColumn(
                name: "HideHPMP",
                schema: "characters",
                table: "characters");
        }
    }
}
