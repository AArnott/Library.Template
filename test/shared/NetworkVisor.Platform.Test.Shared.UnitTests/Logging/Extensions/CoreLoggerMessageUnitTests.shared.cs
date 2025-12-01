// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreLoggerMessageUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Extensions
{
    /// <summary>
    /// Class LoggerExtensionsTest.
    /// </summary>
#pragma warning disable SA1402 // File may only contain a single type

    public static class LoggerExtensionsTest

#pragma warning restore SA1402 // File may only contain a single type
    {
        /// <summary>
        /// The quote added message.
        /// </summary>
        private static readonly Action<ILogger, string, Exception> QuoteAddedMessage = LoggerMessage.Define<string>(
                LogLevel.Information,
                new EventId(2, nameof(QuoteAdded)),
                "Quote added (Quote = '{Quote}')");

        /// <summary>
        /// Quotes the added.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="quote">The quote.</param>
        public static void QuoteAdded(this ICoreLogger logger, string quote)
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            QuoteAddedMessage(logger, quote, null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }

    /// <summary>
    /// Class CoreLoggerMessageUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreLoggerMessageUnitTests))]

    public class CoreLoggerMessageUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLoggerMessageUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLoggerMessageUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreLoggerMessageUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method LoggerExtensions_TestQuoteAdded.
        /// </summary>
        [Fact]
        public void LoggerExtensions_TestQuoteAdded()
        {
            this.TestCaseLogger.QuoteAdded("Test Quote Add");
        }
    }
}
