// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 05-12-2020
//
// Last Modified By : SteveBu
// Last Modified On : 05-12-2020
// ***********************************************************************
// <copyright file="CoreTestCaseLoggerFactoryIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Factory;
using NetworkVisor.Core.Logging.LogProvider;
using NetworkVisor.Core.Test.Logging.Factory;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Logging.Providers;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Logging.Logger
{
    /// <summary>
    /// Class CoreTestCaseLoggerFactoryIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreTestCaseLoggerFactoryIntegrationTests))]

    public class CoreTestCaseLoggerFactoryIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestCaseLoggerFactoryIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTestCaseLoggerFactoryIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.TestLoggerFactoryNull = new CoreTestCaseLoggerFactory(
                this.TestCaseServiceProvider,
                this.TestAssembly,
                this.TestCaseLoggerFactory.WrappedLoggerFactory,
                this.TestCaseLoggerFactory.GlobalLogLevel,
                new CoreTestCaseLoggerProvider(this.TestCaseServiceProvider));
        }

        private ICoreTestCaseLoggerFactory TestLoggerFactoryNull { get; }

        [Fact]
        public void CoreTestCaseLoggerFactoryIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }
    }
}
