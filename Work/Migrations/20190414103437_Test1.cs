using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Work.Migrations
{
    public partial class Test1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Value1_ValueRelationId",
                table: "Events",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Value2_ValueRelationId",
                table: "Events",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ValueRelation",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Description = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValueRelation", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Events_Value1_ValueRelationId",
                table: "Events",
                column: "Value1_ValueRelationId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_Value2_ValueRelationId",
                table: "Events",
                column: "Value2_ValueRelationId");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_ValueRelation_Value1_ValueRelationId",
                table: "Events",
                column: "Value1_ValueRelationId",
                principalTable: "ValueRelation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Events_ValueRelation_Value2_ValueRelationId",
                table: "Events",
                column: "Value2_ValueRelationId",
                principalTable: "ValueRelation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_ValueRelation_Value1_ValueRelationId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_ValueRelation_Value2_ValueRelationId",
                table: "Events");

            migrationBuilder.DropTable(
                name: "ValueRelation");

            migrationBuilder.DropIndex(
                name: "IX_Events_Value1_ValueRelationId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_Value2_ValueRelationId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Value1_ValueRelationId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "Value2_ValueRelationId",
                table: "Events");
        }
    }
}
