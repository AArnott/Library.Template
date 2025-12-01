// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CorePreferredNetworkInfoIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Core Cached Task Base Integration Tests.</summary>

using FluentAssertions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Preferred
{
    /// <summary>
    /// Class CorePreferredNetworkInfoIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CorePreferredNetworkInfoIntegrationTests))]

    public class CorePreferredNetworkInfoIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CorePreferredNetworkInfoIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CorePreferredNetworkInfoIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CorePreferredNetworkInfoIntegrationTests_PreferredNetwork_Output()
        {
            this.TestNetworkServices.PreferredNetwork.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetwork>();

            this.TestOutputHelper.WriteLine(this.TestNetworkServices.PreferredNetwork?.ToStringWithPropNameMultiLine());
        }
    }
}
