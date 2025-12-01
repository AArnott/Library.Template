// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreNetworkInterfaceUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Firewall;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Networking.Services.Ping;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking
{
    /// <summary>
    /// Class CoreNetworkInterfaceUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkInterfaceUnitTests))]

    public class CoreNetworkInterfaceUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkInterfaceUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkInterfaceUnitTests(CoreTestClassFixture testClassFixture)
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
            this.TestNetworkingSystem.Logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();

            this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            this.TestNetworkServices.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);
            this.TestNetworkServices.NetworkPing.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkPing>();
            this.TestNetworkServices.NetworkFirewall.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkFirewall>();
            this.TestNetworkServices.NetworkFirewall.Should().BeSameAs(this.TestNetworkFirewall);
        }
    }
}
