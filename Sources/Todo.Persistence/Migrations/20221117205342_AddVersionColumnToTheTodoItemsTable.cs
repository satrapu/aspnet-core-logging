namespace Todo.Persistence.Migrations
{
    using System.Diagnostics.CodeAnalysis;

    using Microsoft.EntityFrameworkCore.Migrations;

    /// <inheritdoc />
    public partial class AddVersionColumnToTheTodoItemsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {

        }

        /// <inheritdoc />
        [SuppressMessage("Critical Code Smell", "S1186:Methods should not be empty",
            Justification = "The newly added Version property to the TodoItem entity will not result in database changes")]
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
