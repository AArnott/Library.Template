// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 08-14-2021
//
// Last Modified By : SteveBu
// Last Modified On : 08-14-2021
// ***********************************************************************
// <copyright file="CoreLoggingIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Logging
{
    /// <summary>
    /// Class CoreLoggingIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLoggingIntegrationTests))]

    public class CoreLoggingIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLoggingIntegrationTests"/> class.
        /// Initializes a new instance of the <see cref="CoreLoggingIntegrationTests"/> test class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLoggingIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreLoggingIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }
    }
}
