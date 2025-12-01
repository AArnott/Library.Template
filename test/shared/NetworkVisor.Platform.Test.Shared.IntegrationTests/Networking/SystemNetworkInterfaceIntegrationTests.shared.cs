// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// // ***********************************************************************
// <copyright file="SystemNetworkInterfaceIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Collections.Immutable;
using System.Net.NetworkInformation;
using System.Text;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Core.Utilities;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking
{
    /// <summary>
    /// Class SystemNetworkInterfaceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(SystemNetworkInterfaceIntegrationTests))]

    public class SystemNetworkInterfaceIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SystemNetworkInterfaceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public SystemNetworkInterfaceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void SystemNetworkInterface_OutputIndices()
        {
            int index = 0;

            ImmutableHashSet<NetworkInterface> systemNetworkInterfaces = this.TestNetworkingSystem.GetSystemNetworkInterfaces();
            systemNetworkInterfaces.Should().NotBeEmpty();

            foreach (NetworkInterface networkInterface in systemNetworkInterfaces)
            {
                // Microsoft ISTAP interface does not support IPv4.
                if (networkInterface.Supports(NetworkInterfaceComponent.IPv4) && networkInterface.Supports(NetworkInterfaceComponent.IPv6))
                {
                    this.TestOutputHelper.WriteLine($"{index}: Name={networkInterface.Name}, IndexIPv4={networkInterface.GetIPProperties().GetIPv4Properties().Index}, IndexIPv6={networkInterface.GetIPProperties().GetIPv6Properties().Index}");
                    networkInterface.GetIPProperties().GetIPv4Properties().Index.Should().Be(networkInterface.GetIPProperties().GetIPv6Properties().Index);
                }
                else if (networkInterface.Supports(NetworkInterfaceComponent.IPv4))
                {
                    this.TestOutputHelper.WriteLine($"{index}: Name={networkInterface.Name}, IndexIPv4={networkInterface.GetIPProperties().GetIPv4Properties().Index}");
                }
                else if (networkInterface.Supports(NetworkInterfaceComponent.IPv6))
                {
                    this.TestOutputHelper.WriteLine($"{index}: Name={networkInterface.Name}, IndexIPv6={networkInterface.GetIPProperties().GetIPv6Properties().Index}");
                }
                else
                {
                    this.TestOutputHelper.WriteLine($"{index}: Name={networkInterface.Name} doesn't support IPv4 or IPv6");
                }

                index++;
            }
        }

        [Fact]
        public void SystemNetworkInterface_Output()
        {
            ImmutableHashSet<NetworkInterface> systemNetworkInterfaces = this.TestNetworkingSystem.GetSystemNetworkInterfaces();
            systemNetworkInterfaces.Should().NotBeEmpty();

            this.TestOutputHelper.WriteLine($"Static Properties:\n\tIsNetworkAvailable: {NetworkInterface.GetIsNetworkAvailable()}\n\tLoopbackInterfaceIndex: {NetworkInterface.LoopbackInterfaceIndex}\n\tIPv6LoopbackInterfaceIndex: {NetworkInterface.IPv6LoopbackInterfaceIndex}\n\tNetworkInterfaces: {systemNetworkInterfaces.Count}\n");

            foreach (NetworkInterface networkInterface in systemNetworkInterfaces)
            {
                this.TestOutputHelper.WriteLine($"{this.OutputSystemNetworkInterface(networkInterface)}\n");
            }
        }

        private string OutputSystemNetworkInterface(NetworkInterface networkInterface)
        {
            return $"{this.OutputSystemNetworkInterfaceName(networkInterface)}{this.OutputSystemNetworkInterfaceInfo(networkInterface)}{this.OutputSystemNetworkInterface_GetIPProperties(networkInterface)}{this.OutputSystemNetworkInterface_GetIPStatistics(networkInterface)}";
        }

        private string OutputSystemNetworkInterfaceName(NetworkInterface networkInterface)
        {
            return $"{networkInterface.Name}\n";
        }

        private string OutputSystemNetworkInterfaceInfo(NetworkInterface networkInterface)
        {
            int systemIndex = -1;

            if (networkInterface.Supports(NetworkInterfaceComponent.IPv4))
            {
                systemIndex = networkInterface.GetIPProperties().GetIPv4Properties().Index;
            }
            else if (networkInterface.Supports(NetworkInterfaceComponent.IPv6))
            {
                systemIndex = networkInterface.GetIPProperties().GetIPv6Properties().Index;
            }

            return $"\tUniqueID: {networkInterface.Id}\n\tHashCode: {networkInterface.GetHashCode().ToHexString("0x")}\n\tIndex: {systemIndex}\n\tSpeed: {StringUtility.FormatLinkSpeed(networkInterface.Speed)}\n\tPhysicalAddress: {networkInterface.GetPhysicalAddress().ToColonString()}\n\tNetworkInterfaceType: {networkInterface.NetworkInterfaceType}\n\tOperationalStatus: {networkInterface.OperationalStatus}\n\tDescription: {networkInterface.Description}\n\tSupportsMulticast: {networkInterface.SupportsMulticast}\n\tIsReceiveOnly: {networkInterface.IsReceiveOnly}\n\tSupportsIPv4: {networkInterface.Supports(NetworkInterfaceComponent.IPv4)}\n\tSupportsIPv6: {networkInterface.Supports(NetworkInterfaceComponent.IPv6)}\n";
        }

        private string OutputSystemNetworkInterface_GetIPProperties(NetworkInterface networkInterface)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append($"\tUnicastAddresses: [{string.Join(",", CoreIPAddressScoreResult.ScoreIPAddresses(networkInterface.GetIPProperties().UnicastAddresses.Select(u => u.Address)).Select(score => score.IPAddress))}]\n");
            sb.Append($"\tMulticastAddresses: [{string.Join(",", CoreIPAddressScoreResult.ScoreIPAddresses(networkInterface.GetIPProperties().MulticastAddresses.Select(u => u.Address)).Select(score => score.IPAddress))}]\n");

            if (this.TestOperatingSystem.IsAndroid)
            {
                sb.Append($"\tGateways: <Unsupported>\n");
                sb.Append($"\tDnsServers: <Unsupported>\n");
                sb.Append($"\tDnsSuffix: <Unsupported>\n");
            }
            else
            {
#pragma warning disable CA1416 // Validate platform compatibility
                sb.Append($"\tGateways: [{string.Join(",", CoreIPAddressScoreResult.ScoreIPAddresses(networkInterface.GetIPProperties().GatewayAddresses.Select(gw => gw.Address)).Select(score => score.IPAddress))}]\n");

                if (this.TestOperatingSystem.IsIOS)
                {
                    sb.Append($"\tDnsServers: <Unsupported>\n");
                    sb.Append($"\tDnsSuffix: <Unsupported>\n");
                }
                else
                {
                    sb.Append($"\tDnsServers: [{string.Join(",", CoreIPAddressScoreResult.ScoreIPAddresses(networkInterface.GetIPProperties().DnsAddresses).Select(score => score.IPAddress))}]\n");
                    sb.Append($"\tDnsSuffix: {networkInterface.GetIPProperties().DnsSuffix}\n");
                }
            }

            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                sb.Append($"\tIsDnsEnabled: {networkInterface.GetIPProperties().IsDnsEnabled}\n");
                sb.Append($"\tIsDynamicDnsEnabled: {networkInterface.GetIPProperties().IsDynamicDnsEnabled}\n");
                sb.Append($"\tDhcpServers: [{string.Join(",", CoreIPAddressScoreResult.ScoreIPAddresses(networkInterface.GetIPProperties().DhcpServerAddresses).Select(score => score.IPAddress))}]\n");
                sb.Append($"\tWinsServers: [{string.Join(",", CoreIPAddressScoreResult.ScoreIPAddresses(networkInterface.GetIPProperties().WinsServersAddresses).Select(score => score.IPAddress))}]\n");
            }
            else
            {
                sb.Append($"\tIsDnsEnabled: <Unsupported>\n");
                sb.Append($"\tIsDynamicDnsEnabled: <Unsupported>\n");
                sb.Append($"\tDhcpServers: <Unsupported>\n");
                sb.Append($"\tWinsServers: <Unsupported>\n");
            }

            return sb.ToString();
        }

#pragma warning disable CA1416 // Validate platform compatibility
        private string OutputSystemNetworkInterface_GetIPStatistics(NetworkInterface networkInterface)
        {
            StringBuilder sb = new StringBuilder();

            if (!this.TestOperatingSystem.IsAndroid)
            {
                sb.Append($"\tBytesReceived: {networkInterface.GetIPStatistics().BytesReceived.ToString("N0")}\n");
                sb.Append($"\tBytesSent: {networkInterface.GetIPStatistics().BytesSent.ToString("N0")}\n");
                sb.Append($"\tIncomingPacketsDiscarded: {networkInterface.GetIPStatistics().IncomingPacketsDiscarded.ToString("N0")}\n");
                sb.Append($"\tIncomingPacketsWithErrors: {networkInterface.GetIPStatistics().IncomingPacketsWithErrors.ToString("N0")}\n");

                if (!this.TestOperatingSystem.IsLinux)
                {
                    sb.Append($"\tIncomingUnknownProtocolPackets: {networkInterface.GetIPStatistics().IncomingUnknownProtocolPackets.ToString("N0")}\n");
                }
                else
                {
                    sb.Append($"\tIncomingUnknownProtocolPackets <Unsupported>\n");
                }

                sb.Append($"\tNonUnicastPacketsReceived: {networkInterface.GetIPStatistics().NonUnicastPacketsReceived.ToString("N0")}\n");

                if (!this.TestOperatingSystem.IsLinux)
                {
                    sb.Append($"\tNonUnicastPacketsSent: {networkInterface.GetIPStatistics().NonUnicastPacketsSent.ToString("N0")}\n");
                }
                else
                {
                    sb.Append($"\tNonUnicastPacketsSent <Unsupported>\n");
                }

                if (this.TestOperatingSystem.IsWindowsPlatform)
                {
                    sb.Append($"\tOutgoingPacketsDiscarded: {networkInterface.GetIPStatistics().OutgoingPacketsDiscarded.ToString("N0")}\n");
                }
                else
                {
                    sb.Append($"\tOutgoingPacketsDiscarded: <Unsupported>\n");
                }

                sb.Append($"\tOutgoingPacketsWithErrors: {networkInterface.GetIPStatistics().OutgoingPacketsWithErrors.ToString("N0")}\n");
                sb.Append($"\tOutputQueueLength: {networkInterface.GetIPStatistics().OutputQueueLength.ToString("N0")}\n");
                sb.Append($"\tUnicastPacketsReceived: {networkInterface.GetIPStatistics().UnicastPacketsReceived.ToString("N0")}\n");
                sb.Append($"\tUnicastPacketsSent: {networkInterface.GetIPStatistics().UnicastPacketsSent.ToString("N0")}\n");
            }
            else
            {
                sb.Append($"\tBytesSent: <Unsupported>\n");
            }

            return sb.ToString();
        }
#pragma warning restore CA1416 // Validate platform compatibility
    }
}
