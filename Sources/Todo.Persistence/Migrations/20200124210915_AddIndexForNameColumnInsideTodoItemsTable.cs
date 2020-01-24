using Microsoft.EntityFrameworkCore.Migrations;

namespace Todo.Persistence.Migrations
{
    public partial class AddIndexForNameColumnInsideTodoItemsTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_TodoItems_Name",
                table: "TodoItems",
                column: "Name");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TodoItems_Name",
                table: "TodoItems");
        }
    }
}
