// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreNetworkingIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking
{
    /// <summary>
    /// Class CoreNetworkingIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreNetworkingIntegrationTests))]

    public class CoreNetworkingIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkingIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkingIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method CoreNetworking_NetworkingSystem.
        /// </summary>
        [Fact]
        public void CoreNetworking_NetworkingSystem()
        {
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);

            // These network services are support on all platforms.
            this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.SocketsNonPrivileged).Should().BeTrue();
            this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Ping).Should().BeTrue();
        }
    }
}
