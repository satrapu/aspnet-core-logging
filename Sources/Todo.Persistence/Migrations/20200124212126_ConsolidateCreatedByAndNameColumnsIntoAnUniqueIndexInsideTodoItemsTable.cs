using Microsoft.EntityFrameworkCore.Migrations;

namespace Todo.Persistence.Migrations
{
    public partial class ConsolidateCreatedByAndNameColumnsIntoAnUniqueIndexInsideTodoItemsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CreatedBy",
                table: "TodoItems");

            migrationBuilder.DropIndex(
                name: "IX_TodoItems_Name",
                table: "TodoItems");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CreatedBy_Name",
                table: "TodoItems",
                columns: new[] { "CreatedBy", "Name" },
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CreatedBy_Name",
                table: "TodoItems");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CreatedBy",
                table: "TodoItems",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_Name",
                table: "TodoItems",
                column: "Name");
        }
    }
}
