using System;

namespace Todo.Persistence.Entities
{
    /// <summary>
    /// Represents an action to be performed at some point in the future.
    /// </summary>
    public class TodoItem
    {
        /// <summary>
        /// Gets or sets the identifier of this action.
        /// </summary>
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        public long Id { get; }

        /// <summary>
        /// Gets or sets the name of this action.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets whether this action has been completed.
        /// <br/>
        /// When an item is created, by default this property is set to <code>false</code>.
        /// </summary>
        public bool IsComplete { get; set; }

        /// <summary>
        /// Gets the identifier of the user who has created this item.
        /// </summary>
        public string CreatedBy { get; }

        /// <summary>
        /// Gets the date when this item was created.
        /// </summary>
        // ReSharper disable once MemberCanBePrivate.Global
        public DateTime CreatedOn { get; }

        /// <summary>
        /// Gets or sets the identifier of the user who last updated this item.
        /// <br/>
        /// When a user creates an item, this property is set to the same value as the <seealso cref="CreatedBy"/> one.
        /// </summary>
        public string LastUpdatedBy { get; set; }

        /// <summary>
        /// Gets or sets the date when this item was last updated.
        /// <br/>
        /// When a user creates an item, this property is set to the same value as the <seealso cref="CreatedOn"/> one.
        /// </summary>
        public DateTime? LastUpdatedOn { get; set; }
        
        /// <summary>
        /// The identity (transaction ID) of the inserting transaction for this row version.
        /// See more here: https://www.postgresql.org/docs/12/ddl-system-columns.html
        /// and here: https://www.npgsql.org/efcore/modeling/concurrency.html.
        /// </summary>
        // ReSharper disable once InconsistentNaming
        // ReSharper disable once UnusedMember.Global
        public uint xmin { get; set; }
        
        /// <summary>
        /// Creates a new instance of the <see cref="TodoItem"/> class.
        /// </summary>
        /// <param name="name">The name ot this item.</param>
        /// <param name="createdBy">The identifier of the user who's creating this item.</param>
        public TodoItem(string name, string createdBy)
        {
            Name = name;
            CreatedBy = createdBy;
            CreatedOn = DateTime.UtcNow;
            IsComplete = false;
        }

        /// <summary>
        /// Gets the string representation of this item.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return $"[{nameof(Id)}: {Id}, " +
                   $"{nameof(Name)}: {Name}, " +
                   $"{nameof(IsComplete)}: {IsComplete}, " +
                   $"{nameof(CreatedBy)}: {CreatedBy}, " +
                   $"{nameof(CreatedOn)}: {CreatedOn:u}, " +
                   $"{nameof(LastUpdatedBy)}: {LastUpdatedBy}, " +
                   $"{nameof(LastUpdatedOn)}: {LastUpdatedOn?.ToString("u")}, " + 
                   $"{nameof(xmin)}(concurrency token): {xmin}]";
        }
    }
}