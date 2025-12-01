// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreNetworkServiceTypeUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking
{
    /// <summary>
    /// Class CoreNetworkingUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreNetworkServiceTypeUnitTests))]

    public class CoreNetworkServiceTypeUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkServiceTypeUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkServiceTypeUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        ///  Test method CoreNetworkServiceType_Test.
        /// </summary>
        [Fact]
        public void CoreNetworkServiceType_Test()
        {
            CoreNetworkServiceTypes.Arp.IsArpSupported().Should().BeTrue();
            CoreNetworkServiceTypes.SocketsPrivileged.IsPrivilegedSocketsSupported().Should().BeTrue();
            CoreNetworkServiceTypes.SocketsNonPrivileged.IsNonPrivilegedSocketsSupported().Should().BeTrue();
            CoreNetworkServiceTypes.Ping.IsPingSupported().Should().BeTrue();
            CoreNetworkServiceTypes.DhcpServer.IsDhcpServerSupported().Should().BeTrue();
            CoreNetworkServiceTypes.DhcpClient.IsDhcpClientSupported().Should().BeTrue();
            CoreNetworkServiceTypes.Wmi.IsWmiSupported().Should().BeTrue();
            CoreNetworkServiceTypes.GatewayPhysicalAddress.IsGatewayPhysicalAddressSupported().Should().BeTrue();
            CoreNetworkServiceTypes.UPnP.IsUPnPSupported().Should().BeTrue();
            CoreNetworkServiceTypes.Multicast.IsMulticastSupported().Should().BeTrue();
            CoreNetworkServiceTypes.MulticastDns.IsMulticastDnsSupported().Should().BeTrue();
            CoreNetworkServiceTypes.LocalPhysicalAddress.IsLocalPhysicalAddressSupported().Should().BeTrue();
            CoreNetworkServiceTypes.RunProcess.IsRunProcessSupported().Should().BeTrue();
            CoreNetworkServiceTypes.LocalFirewallReadOnly.IsLocalFirewallReadOnlySupported().Should().BeTrue();
            CoreNetworkServiceTypes.LocalFirewallReadWrite.IsLocalFirewallReadWriteSupported().Should().BeTrue();
            CoreNetworkServiceTypes.SendToLoopback.IsSendToLoopbackSupported().Should().BeTrue();
            CoreNetworkServiceTypes.HangfireScheduler.IsHangfireSchedulerSupported().Should().BeTrue();
        }
    }
}
