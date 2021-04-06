﻿using System.ComponentModel.DataAnnotations;
using System.Security.Principal;
using System.Text;
using Todo.Services.Security;

namespace Todo.Services.TodoItemLifecycleManagement
{
    public class TodoItemQuery
    {
        public const int DefaultPageIndex = 0;
        public const int DefaultPageSize = 25;

        /// <summary>
        /// Gets or sets the id of the todo item to be fetched using this query.
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// Gets or sets the pattern the name of the todo items must match to be fetched using this query.
        /// <br/>
        /// This pattern may contain wildcards - see more here:
        /// https://docs.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbfunctionsextensions.like?view=efcore-3.1.
        /// </summary>
        public string NamePattern { get; set; }

        /// <summary>
        /// Gets or sets whether this query will fetch todo items which have been completed.
        /// </summary>
        public bool? IsComplete { get; set; }

        /// <summary>
        /// Gets or sets the user who has created the todo items to be fetched using this query.
        /// </summary>
        [Required]
        public IPrincipal Owner { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of todo items to be fetched using this query.
        /// </summary>
        [Required]
        [Range(1, 1000)]
        public int? PageSize { get; set; } = DefaultPageSize;

        /// <summary>
        /// Gets or sets the 0-based index of the current batch of todo items to be fetched using this query. 
        /// </summary>
        [Required]
        [Range(0, int.MaxValue)]
        public int? PageIndex { get; set; } = DefaultPageIndex;

        /// <summary>
        /// Gets or sets the property name used for sorting the todo items to be fetched using this query. 
        /// </summary>
        public string SortBy { get; set; }

        /// <summary>
        /// Gets or sets whether the todo items to be fetched using this query will be sorted using
        /// the <see cref="SortBy"/> property in an ascending order.
        /// </summary>
        public bool? IsSortAscending { get; set; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.AppendJoin(";",
                $"[{nameof(Id)}={Id}",
                $"{nameof(NamePattern)}={NamePattern}",
                $"{nameof(IsComplete)}={IsComplete}",
                $"{nameof(Owner)}={Owner.GetNameOrDefault()}",
                $"{nameof(PageIndex)}={PageIndex}",
                $"{nameof(PageSize)}={PageSize}",
                $"{nameof(SortBy)}={SortBy}",
                $"{nameof(IsSortAscending)}={IsSortAscending}]");
            return stringBuilder.ToString();
        }
    }
}