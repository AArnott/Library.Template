// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreNetworkConnectionProfileIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>CoreNetworkConnectionProfile IntegrationTests.</summary>
// ***********************************************************************

using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Networking.Connectivity;
using NetworkVisor.Core.Networking.Interfaces;
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
    [PlatformTrait(typeof(CoreNetworkConnectionProfileIntegrationTests))]

    public class CoreNetworkConnectionProfileIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkConnectionProfileIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkConnectionProfileIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method ConnectionProfileIntegration_NetworkingSystem.
        /// </summary>
        [Fact]
        public void ConnectionProfileIntegration_NetworkingSystem()
        {
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
        }

        /// <summary>
        /// Defines the test method ConnectionProfileIntegration_ValidateConnectionProfile.
        /// </summary>
        [Fact]
        public void ConnectionProfileIntegration_ValidateConnectionProfile()
        {
            foreach (ICoreNetworkInterface networkInterface in this.TestNetworkingSystem.GetAllNetworkInterfaces())
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());

                networkInterface.ConnectionProfile.Should().NotBe(CoreConnectionProfile.Unknown);

                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method ConnectionProfileIntegration_ValidateNetworkInterfaceType.
        /// </summary>
        [Fact]
        public void ConnectionProfileIntegration_ValidateNetworkInterfaceType()
        {
            foreach (ICoreNetworkInterface networkInterface in this.TestNetworkingSystem.GetAllNetworkInterfaces())
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());

                networkInterface.NetworkInterfaceType.Should().NotBe(NetworkInterfaceType.Unknown);

                this.TestOutputHelper.WriteLine();
            }
        }
    }
}
