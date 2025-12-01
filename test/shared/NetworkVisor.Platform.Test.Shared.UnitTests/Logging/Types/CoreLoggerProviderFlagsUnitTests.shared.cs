// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreLoggerProviderFlagsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************
using FluentAssertions;
using NetworkVisor.Core.Logging.LogProvider;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Types
{
    /// <summary>
    /// Class LoggingOutputFlagsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreLoggerProviderFlagsUnitTests))]

    public class CoreLoggerProviderFlagsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLoggerProviderFlagsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLoggerProviderFlagsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreLoggerProviderFlagsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method LoggingOutputFlags_ToKey.
        /// </summary>
        /// <param name="loggerProviderFlags">The logging provider flags.</param>
        /// <param name="testValue">The test value.</param>
        [Theory]
        [InlineData(LoggerProviderFlags.TestingAppConsoleFile, "TestCaseLogger")]
        [InlineData(LoggerProviderFlags.Testing, "TestCaseLogger")]
        [InlineData(LoggerProviderFlags.MauiTest, "TestCaseLogger")]
        [InlineData(LoggerProviderFlags.MauiApp, "Maui")]
        [InlineData(LoggerProviderFlags.AppFull, "Console")]
        [InlineData(LoggerProviderFlags.AppConsoleFile, "Console")]
        [InlineData(LoggerProviderFlags.Silent, "Silent")]
        [InlineData(LoggerProviderFlags.Console, "Console")]
        [InlineData(LoggerProviderFlags.RollingFile, "RollingFile")]
        [InlineData(LoggerProviderFlags.OpenTelemetry, "OpenTelemetry")]
        [InlineData(LoggerProviderFlags.TestCaseLogger, "TestCaseLogger")]
        [InlineData(LoggerProviderFlags.TestCorrelator, "TestCorrelator")]
        [InlineData(LoggerProviderFlags.AppleLog, "AppleLog")]
        [InlineData(LoggerProviderFlags.AndroidLog, "AndroidLog")]
        [InlineData(LoggerProviderFlags.Maui, "Maui")]
        [InlineData(LoggerProviderFlags.DesktopApp, "DesktopApp")]
        [InlineData(LoggerProviderFlags.DesktopAppConsole, "DesktopAppConsole")]
        [InlineData(LoggerProviderFlags.DesktopAppConsoleFile, "DesktopAppConsoleFile")]

        public void LoggingOutputFlags_ToKey(LoggerProviderFlags loggerProviderFlags, string testValue)
        {
            loggerProviderFlags.ToKey().Should().Be(testValue);
        }
    }
}
