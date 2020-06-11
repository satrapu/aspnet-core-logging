using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Todo.Persistence.Entities.Configurations
{
    internal class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
    {
        public void Configure(EntityTypeBuilder<TodoItem> builder)
        {
            builder.ToTable("TodoItems");

            builder.HasKey(todoItem => todoItem.Id);

            builder.Property(todoItem => todoItem.Id)
                .ValueGeneratedOnAdd();

            builder.Property(todoItem => todoItem.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(todoItem => todoItem.IsComplete)
                .IsRequired();

            builder.Property(todoItem => todoItem.CreatedBy)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(todoItem => todoItem.CreatedOn)
                .IsRequired();

            builder.Property(todoItem => todoItem.LastUpdatedBy)
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property(todoItem => todoItem.LastUpdatedOn)
                .IsRequired(false);

            // Enable optimistic locking for this table using PostgreSQL xmin feature (holds the ID of the latest
            // updating transaction).
            // See more about xmin here: https://www.npgsql.org/efcore/modeling/concurrency.html.
            // See more about PostgreSQL system columns here: https://www.postgresql.org/docs/12/ddl-system-columns.html.
            // See more about handling concurrency with EF Cor here: https://docs.microsoft.com/en-us/ef/core/saving/concurrency.
            builder.UseXminAsConcurrencyToken();

            builder.HasIndex(nameof(TodoItem.CreatedBy), nameof(TodoItem.Name))
                .IsUnique();
        }
    }
}