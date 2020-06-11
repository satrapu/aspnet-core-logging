using Microsoft.EntityFrameworkCore.Migrations;

namespace Todo.Persistence.Migrations
{
    public partial class AddSupportForOptimisticLockingToTodoTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<uint>(
                name: "xmin",
                table: "TodoItems",
                type: "xid",
                nullable: false,
                defaultValue: 0u);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "xmin",
                table: "TodoItems");
        }
    }
}
