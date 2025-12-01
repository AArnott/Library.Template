// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 08-11-2021
//
// Last Modified By : SteveBu
// Last Modified On : 08-11-2021
// ***********************************************************************
// <copyright file="CoreGlobalLoggerFactoryIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Logging.Loggers
{
    /// <summary>
    /// Class CoreGlobalLoggerFactoryIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreGlobalLoggerFactoryIntegrationTests))]

    public class CoreGlobalLoggerFactoryIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreGlobalLoggerFactoryIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreGlobalLoggerFactoryIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void GlobalLoggerFactoryIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void GlobalLoggerFactoryIntegration_GlobalLogger()
        {
            this.TestApplication.GlobalLogger.Should().Be(this.GetTestAppRequiredService<ICoreGlobalLogger>());
        }

        [Fact]
        public void GlobalLoggerFactoryIntegration_CreateGlobalLogger_WrappedLogger()
        {
            ICoreGlobalLogger globalLoggerCreated = this.TestCaseLoggerFactory.CreateGlobalLogger();

            globalLoggerCreated.WrappedLogger.Should().BeSameAs(this.GetTestAppRequiredService<ICoreGlobalLogger>().WrappedLogger);
        }
    }
}
