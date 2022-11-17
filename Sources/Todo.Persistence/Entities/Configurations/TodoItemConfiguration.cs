namespace Todo.Persistence.Entities.Configurations
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata.Builders;

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

            builder.Property(todoItem => todoItem.Version)
                .IsRowVersion();

            builder.HasIndex(nameof(TodoItem.CreatedBy), nameof(TodoItem.Name))
                .IsUnique();
        }
    }
}
