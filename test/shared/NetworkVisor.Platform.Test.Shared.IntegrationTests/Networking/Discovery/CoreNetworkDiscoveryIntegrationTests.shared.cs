// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreNetworkDiscoveryIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Networking.Collection;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Ping;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Discovery
{
    /// <summary>
    /// Class CoreNetworkDiscoveryIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkDiscoveryIntegrationTests))]

    public class CoreNetworkDiscoveryIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkDiscoveryIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkDiscoveryIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task CoreNetworkDiscovery_GatewayDiscoveredDeviceInfo()
        {
            ICoreNetworkGatewayInfo? networkGatewayInfo = this.TestNetworkServices.PreferredNetworkGatewayInfo;

            networkGatewayInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkGatewayInfo>();

            this.TestOutputHelper.WriteLine($"Network Gateway: {networkGatewayInfo!.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine)}");

            networkGatewayInfo.GatewayNetworkAddressInfo.Should().NotBeNull();
            networkGatewayInfo.GatewayNetworkAddressInfo!.IPAddressSubnet.Should().NotBe(CoreIPAddressSubnet.None);
            networkGatewayInfo.GatewayNetworkAddressInfo!.IPAddress.Should().NotBe(IPAddress.None);

            networkGatewayInfo.PreferredGatewayNetworkInterface.Should().NotBeNull();

            // Cellular connections can have a broadcast subnet
            if (!networkGatewayInfo.PreferredGatewayNetworkInterface!.IsCellularConnection)
            {
                networkGatewayInfo.GatewayNetworkAddressInfo!.SubnetMask.Should().NotBe(IPAddress.None);
            }

            networkGatewayInfo.GatewayNetworkAddressInfo!.SubnetMask.Should().NotBe(IPAddress.Any);

            var networkGatewayEnum = new CoreGatewayNetworkCollection(networkGatewayInfo);
            networkGatewayEnum.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkCollection>();

            int iCount = 0;

            foreach (IPAddress ipAddress in networkGatewayEnum)
            {
                this.TestOutputHelper.WriteLine($"Pinging Network Gateway: {ipAddress}");
                CorePingResult pingResult = await this.TestNetworkServices.NetworkPing.PingAsync(ipAddress, 5000);

                pingResult.Should().NotBeNull();

                this.TestOutputHelper.WriteLine($"Ping Result ({ipAddress}): {pingResult.Status}");

                if (++iCount == 10)
                {
                    break;
                }
            }
        }
    }
}
