using Microsoft.EntityFrameworkCore.Migrations;

namespace Todo.Persistence.Migrations
{
    public partial class AddIndexForCreatedByColumnInsideTodoItemsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_CreatedBy",
                table: "TodoItems",
                column: "CreatedBy");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItems_CreatedBy",
                table: "TodoItems");
        }
    }
}
