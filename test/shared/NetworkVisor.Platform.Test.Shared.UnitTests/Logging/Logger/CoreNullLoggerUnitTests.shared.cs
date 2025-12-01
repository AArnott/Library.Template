// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreNullLoggerUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Logger
{
    /// <summary>
    /// Class CoreNullLoggerUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreNullLoggerUnitTests))]

    public class CoreNullLoggerUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNullLoggerUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNullLoggerUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.NullLogger = NullCoreLogger<CoreNullLoggerUnitTests>.Instance;
        }

        private ICoreLogger NullLogger { get; }

        [Fact]
        public void NullLoggerUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        [Fact]
        public void NullCoreLogger_Constructor()
        {
            this.NullLogger.Should().NotBeNull();
        }

        [Fact]
        public void NullCoreLogger_ILogger_Methods()
        {
            using (this.NullLogger.BeginScope("Test"))
            {
#pragma warning disable CA2254 // Template should be a static expression
                this.NullLogger.Log(LogLevel.Debug, new EventId(1), (Exception?)null, null);
#pragma warning restore CA2254 // Template should be a static expression
            }
        }

        [Fact]
        public void NullCoreLogger_Properties()
        {
            this.NullLogger.Should().NotBeNull();
#if DEBUG
            this.NullLogger.IsEnabled(LogLevel.Trace).Should().BeTrue();
#else
            this.NullLogger.IsEnabled(LogLevel.Trace).Should().BeFalse();
#endif
        }

        [Fact]
        public void NullCoreLogger_IsNullLogger()
        {
            this.NullLogger.Should().NotBeNull();
            this.NullLogger.IsNullLogger.Should().BeTrue();
        }

        [Fact]
        public void NullCoreLogger_IsSilentLogger()
        {
            this.NullLogger.Should().NotBeNull();
            this.NullLogger.IsSilentLogger.Should().BeTrue();
        }

        [Fact]
        public void NullCoreLogger_LoggerLogLevel_Current()
        {
            this.NullLogger.Should().NotBeNull();
            this.NullLogger.LoggerLogLevel.Current.Should().Be(LogLevel.None);
        }

        [Fact]
        public void NullCoreLogger_LoggerLogLevel_Default()
        {
            this.NullLogger.Should().NotBeNull();
            this.NullLogger.LoggerLogLevel.Default.Should().Be(LogLevel.None);
        }

        [Fact]
        public void NullCoreLogger_CategoryName()
        {
            this.NullLogger.Should().NotBeNull();
            this.NullLogger.CategoryName.Should().Be(nameof(NullCoreLogger));
        }
    }
}
