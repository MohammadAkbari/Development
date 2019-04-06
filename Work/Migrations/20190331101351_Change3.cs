using Microsoft.EntityFrameworkCore.Migrations;

namespace Work.Migrations
{
    public partial class Change3 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Value1_Name",
                table: "Events",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Value2_Name",
                table: "Events",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Value1_Name",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Value2_Name",
                table: "Events");
        }
    }
}
