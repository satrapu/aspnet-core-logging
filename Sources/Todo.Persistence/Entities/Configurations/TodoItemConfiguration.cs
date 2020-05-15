using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Todo.Persistence.Entities.Configurations
{
    internal class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
    {
        public void Configure(EntityTypeBuilder<TodoItem> builder)
        {
            builder.ToTable("TodoItems");

            builder.HasKey("Id");

            builder.Property<long>("Id")
                .ValueGeneratedOnAdd();

            builder.Property<string>("Name")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property<bool>("IsComplete")
                .IsRequired();

            builder.Property<string>("CreatedBy")
                .IsRequired()
                .HasMaxLength(100);

            builder.Property<DateTime>("CreatedOn")
                .IsRequired();

            builder.Property<string>("LastUpdatedBy")
                .IsRequired(false)
                .HasMaxLength(100);

            builder.Property<DateTime?>("LastUpdatedOn")
                .IsRequired(false);

            // Enable optimistic locking for this table using PostgreSQL xmin feature (holds the ID of the latest
            // updating transaction).
            // See more about xmin here: https://www.npgsql.org/efcore/modeling/concurrency.html.
            // See more about handling concurrency with EF Cor here: https://docs.microsoft.com/en-us/ef/core/saving/concurrency.
            builder.UseXminAsConcurrencyToken();

            builder.HasIndex(nameof(TodoItem.CreatedBy), nameof(TodoItem.Name))
                .IsUnique();
        }
    }
}