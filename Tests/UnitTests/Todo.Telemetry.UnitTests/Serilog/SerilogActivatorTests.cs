namespace Todo.Telemetry.Serilog
{
    using System.Collections.Generic;

    using FluentAssertions;

    using Microsoft.Extensions.Configuration;

    using NUnit.Framework;

    /// <summary>
    /// Contains unit tests targeting <see cref="SerilogActivator"/> class.
    /// </summary>
    [TestFixture]
    public class SerilogActivatorTests
    {
        private const string FileSinkJsonFragment =
            """
            {
                "Serilog": {
                    "LevelSwitches": {
                        "$controlSwitch": "Information"
                    },
                    "MinimumLevel": {
                        "ControlledBy": "$controlSwitch"
                    },
                    "Using": [
                        "Serilog.Sinks.File"
                    ],
                    "WriteTo": [
                        {
                            "Name": "File",
                            "Args": {
                                "path": "%LOGS_HOME%/todo-web-api.log",
                                "outputTemplate": "{SourceContext}{NewLine}{Message:lj}{NewLine}{Properties}{NewLine}{Exception}",
                                "rollingInterval": "Day",
                                "rollOnFileSizeLimit": "true",
                                "fileSizeLimitBytes": "1073741824",
                                "retainedFileCountLimit": "31"
                            }
                        }
                    ]
                }
            }
            """;

        private const string ConsoleSinkJsonFragment =
            """
            {
                "Serilog": {
                    "LevelSwitches": {
                        "$controlSwitch": "Information"
                    },
                    "MinimumLevel": {
                        "ControlledBy": "$controlSwitch"
                    },
                    "Using": [
                        "Serilog.Sinks.Console"
                    ],
                    "WriteTo": [
                        {
                            "Name": "Console",
                            "Args": {
                                "theme": "Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme::Code, Serilog.Sinks.Console",
                                "outputTemplate": "{SourceContext}{NewLine}{Message:lj}{NewLine}{Properties}{NewLine{Exception}"
                            }
                        }
                    ]
                }
            }
            """;

        [Test]
        [TestCaseSource(nameof(GetFileSinkConfigurations))]
        public void IsFileSinkConfigured_WhenConfigurationIsValid_MustReturnExpectedValue(IConfiguration configuration, bool expectedReturnValue)
        {
            // Arrange & Act
            bool actualReturnValue = SerilogActivator.IsFileSinkConfigured(configuration);

            // Assert
            actualReturnValue.Should().Be(expectedReturnValue);
        }

        private static IEnumerable<object[]> GetFileSinkConfigurations()
        {
            yield return [FileSinkJsonFragment.ToConfiguration(), true];
            yield return [ConsoleSinkJsonFragment.ToConfiguration(), false];
        }
    }
}
