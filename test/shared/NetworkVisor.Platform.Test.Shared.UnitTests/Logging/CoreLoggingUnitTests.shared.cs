// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 08-14-2021
//
// Last Modified By : SteveBu
// Last Modified On : 08-14-2021
// ***********************************************************************
// <copyright file="CoreLoggingUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Logging
{
    /// <summary>
    /// Class CoreUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLoggingUnitTests))]

    public class CoreLoggingUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLoggingUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLoggingUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void LoggingUnitTests_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method LoggingUnitTests_BeginScope_String.
        /// </summary>
        [Fact]
        public void LoggingUnitTests_BeginScope_String()
        {
            using (this.TestCaseLogger.BeginScope("TestScope"))
            {
                this.TestCaseLogger.Log(LogLevel.Debug, "TestMessage");
            }
        }

        /// <summary>
        /// Defines the test method LoggingUnitTests_BeginScope_AnonymousType.
        /// </summary>
        [Fact]
        public void LoggingUnitTests_BeginScope_AnonymousType()
        {
            using (this.TestCaseLogger.BeginScope(new { TestProperty1 = "TestValue1", TestProperty2 = "TestValue2" }))
            {
                this.TestCaseLogger.Log(LogLevel.Debug, "TestMessage");
            }
        }

        /// <summary>
        /// Defines the test method LoggingUnitTests_BeginScope_AnonymousType.
        /// </summary>
        [Fact]
        public void LoggingUnitTests_BeginScope_Dictionary()
        {
            using (this.TestCaseLogger.BeginScope(new Dictionary<string, object> { { "TestProperty1", "TestValue1" }, { "TestProperty2", "TestValue2" } }))
            {
                this.TestCaseLogger.Log(LogLevel.Debug, "TestMessage");
            }
        }
    }
}
