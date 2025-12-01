// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 08-11-2021
//
// Last Modified By : SteveBu
// Last Modified On : 08-11-2021
// ***********************************************************************
// <copyright file="CoreLoggerExtensionsIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Logging.Extensions
{
    /// <summary>
    /// Class CoreLoggerExtensionsIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLoggerExtensionsIntegrationTests))]

    public class CoreLoggerExtensionsIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLoggerExtensionsIntegrationTests"/> class.
        /// Initializes a new instance of the <see cref="CoreLoggerExtensionsIntegrationTests"/> test class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLoggerExtensionsIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreLoggerExtensionsIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }
    }
}
