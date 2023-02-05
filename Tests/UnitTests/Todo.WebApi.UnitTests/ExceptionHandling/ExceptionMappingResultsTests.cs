namespace Todo.WebApi.ExceptionHandling
{
    using System;
    using System.Collections.Generic;
    using System.Transactions;

    using FluentAssertions;

    using Npgsql;

    using NUnit.Framework;

    using Persistence.Entities;

    using Services.TodoItemManagement;

    /// <summary>
    /// Contains unit tests targeting <see cref="ExceptionMappingResults"/> class.
    /// </summary>
    [TestFixture]
    public class ExceptionMappingResultsTests
    {
        [Test]
        [TestCaseSource(nameof(GetExceptions))]
        public void GetMappingResult_WhenUsingKnownExceptionType_ReturnsExpectedResult(Exception exception, ExceptionMappingResult expectedResult)
        {
            // Act
            ExceptionMappingResult actualResult = ExceptionMappingResults.GetMappingResult(exception);

            // Assert
            actualResult.Should().Be(expectedResult);
        }

        private static IEnumerable<object[]> GetExceptions()
        {
            return new List<object[]>
            {
                new object[]{ null, ExceptionMappingResults.GenericError },
                new object[]{ new Exception("Hard-coded exception"), ExceptionMappingResults.GenericError },
                new object[]{ new EntityNotFoundException(typeof(TodoItem), "test-entity-key"), ExceptionMappingResults.EntityNotFound },
                new object[]{ new NpgsqlException(), ExceptionMappingResults.DatabaseError },
                new object[]{ new Exception("Hard-coded exception with a cause", new NpgsqlException()), ExceptionMappingResults.DatabaseError },
                new object[]{ new TransactionException("Hard-coded exception"), ExceptionMappingResults.DatabaseError }
            };
        }
    }
}
