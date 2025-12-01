// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreEnvironmentSettingsIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using NetworkVisor.Core.Configuration;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Settings
{
    /// <summary>
    /// Class CoreCloudClientIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreEnvironmentSettingsIntegrationTests))]

    public class CoreEnvironmentSettingsIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEnvironmentSettingsIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreEnvironmentSettingsIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreEnvironmentSettings_CurrentHostEnvironment()
        {
            this.TestOutputHelper.WriteLine($"CurrentHostEnvironment: {CoreEnvironmentSettings.CurrentHostEnvironment}");
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreEnvironmentSettings_RuntimeHostEnvironment()
        {
            this.TestOutputHelper.WriteLine($"RuntimeHostEnvironment: {CoreEnvironmentSettings.RuntimeHostEnvironment}");
        }
    }
}
