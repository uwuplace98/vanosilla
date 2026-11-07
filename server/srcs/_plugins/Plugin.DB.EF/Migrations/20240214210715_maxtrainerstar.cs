using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using WingsAPI.Data.Character;

namespace Plugin.Database.Migrations
{
    public partial class maxtrainerstar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<List<MaxStarTrainerDto>>(
                name: "MaxStarTrainerDto",
                schema: "characters",
                table: "characters",
                type: "jsonb",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MaxStarTrainerDto",
                schema: "characters",
                table: "characters");
        }
    }
}
