namespace Todo.ArchitectureTests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Threading.Tasks;

    using FluentAssertions;

    using Microsoft.AspNetCore.Mvc;

    using NetArchTest.Rules;

    using NUnit.Framework;

    using Persistence;

    using Services.TodoItemManagement;

    using WebApi;

    /// <summary>
    /// Contains tests ensuring the architecture of this application has not deviate from the intended one.
    /// </summary>
    [SuppressMessage("ReSharper", "S1135", Justification = "The todo word represents an entity")]
    [TestFixture]
    public class TodoWebApiArchitectureTests
    {
        /// <summary>
        /// Ensures the controllers found in this application do not depend on any service interface.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GivenTodoWebApiControllers_WhenBeingAnalyzed_ThenEnsureTheyDoNotDependOnServiceInterfaces()
        {
            // Arrange
            PredicateList webApiControllers = GetWebApiControllers();

            string[] serviceInterfaces = Types.InAssembly(typeof(ITodoItemService).Assembly)
                .That()
                .ArePublic()
                .And().AreInterfaces()
                .GetTypes()
                .Where(localType => !string.IsNullOrWhiteSpace(localType.FullName))
                .Select(localType => localType.FullName)
                .ToArray();

            // Act
            TestResult testResult = webApiControllers.Should().NotHaveDependencyOnAny(serviceInterfaces).GetResult();

            await DisplayFailingTypesIfAnyAsync(testResult);

            // Assert
            testResult.IsSuccessful.Should().Be(true, "Web API controllers must *not* depend on any service interface");
        }

        /// <summary>
        /// Ensures the controllers of this application do not depend on any service class.
        /// </summary>
        /// <returns></returns>
        [Test]
        public async Task GivenTodoWebApiControllers_WhenBeingAnalyzed_ThenEnsureTheyDoNotDependOnServices()
        {
            // Arrange
            PredicateList webApiControllers = GetWebApiControllers();

            string[] services = Types.InAssembly(typeof(ITodoItemService).Assembly)
                .That()
                .ArePublic()
                .And().AreClasses()
                .And().HaveNameEndingWith("Service")
                .GetTypes()
                .Where(localType => !string.IsNullOrWhiteSpace(localType.FullName))
                .Select(localType => localType.FullName)
                .Distinct()
                .ToArray();

            // Act
            TestResult testResult = webApiControllers.Should().NotHaveDependencyOnAny(services).GetResult();

            await DisplayFailingTypesIfAnyAsync(testResult);

            // Assert
            testResult.IsSuccessful.Should().Be(true, "Web API controllers must *not* depend on any service");
        }

        [Test]
        public async Task GivenTodoWebApiControllers_WhenBeingAnalyzed_ThenEnsureTheyDoNotDependOnPersistence()
        {
            // Arrange
            PredicateList webApiControllers = GetWebApiControllers();

            string[] persistenceNamespaces = Types.InAssembly(typeof(TodoDbContext).Assembly)
                .GetTypes()
                .Where(localType => !string.IsNullOrWhiteSpace(localType.Namespace))
                .Select(localType => localType.Namespace)
                .Distinct()
                .ToArray();

            // Act
            TestResult testResult = webApiControllers.Should().NotHaveDependencyOnAny(persistenceNamespaces).GetResult();

            await DisplayFailingTypesIfAnyAsync(testResult);

            // Assert
            testResult.IsSuccessful.Should().Be(true, "Web API controllers must *not* depend on persistence related namespaces");
        }

        /// <summary>
        /// Fetches all controllers found in this application.
        /// </summary>
        private static PredicateList GetWebApiControllers()
        {
            PredicateList webApiControllers = Types.InAssembly(typeof(Startup).Assembly)
                .That()
                .AreClasses()
                .And().ArePublic()
                .And().Inherit(typeof(ControllerBase));

            return webApiControllers;
        }

        /// <summary>
        /// Display to console error stream any type found inside the <see cref="TestResult.FailingTypes"/> list.
        /// </summary>
        /// <param name="testResult">The <see cref="TestResult"/> instance which might contain failing types.</param>
        /// <returns>Nothing</returns>
        private static async Task DisplayFailingTypesIfAnyAsync(TestResult testResult)
        {
            if (testResult.FailingTypes == null)
            {
                return;
            }

            foreach (Type testResultFailingType in testResult.FailingTypes)
            {
                await Console.Error.WriteLineAsync($"Failing type: [{testResultFailingType.FullName}]");
            }
        }
    }
}
