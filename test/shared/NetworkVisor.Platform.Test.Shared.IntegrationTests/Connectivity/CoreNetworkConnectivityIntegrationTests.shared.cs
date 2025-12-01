// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreNetworkConnectivityIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Connectivity;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Connectivity
{
    /// <summary>
    /// Class CoreNetworkConnectivityIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkConnectivityIntegrationTests))]

    public class CoreNetworkConnectivityIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkConnectivityIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkConnectivityIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
        }

        /// <summary>
        /// Defines the test method NetworkConnectivity_ConnectionProfiles.
        /// </summary>
        [Fact]
        public void NetworkConnectivity_ConnectionProfiles()
        {
            foreach (CoreConnectionProfile connectionProfile in this.TestNetworkingSystem.ConnectionProfiles)
            {
                this.TestOutputHelper.WriteLine($"Profile: {connectionProfile}");
            }
        }

        /// <summary>
        /// Defines the test method NetworkConnectivity_NetworkAccess.
        /// </summary>
        [Fact]
        public void NetworkConnectivity_NetworkAccess()
        {
            this.TestOutputHelper.WriteLine($"NetworkAccess: {this.TestNetworkingSystem.NetworkAccess}");
            this.TestNetworkingSystem.NetworkAccess.Should().NotBe(CoreNetworkAccess.Unknown);
        }
    }
}
