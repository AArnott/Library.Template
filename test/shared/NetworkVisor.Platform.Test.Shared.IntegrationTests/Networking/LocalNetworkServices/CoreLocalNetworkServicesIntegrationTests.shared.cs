// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreLocalNetworkServicesIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Services.LocalNetwork;
using NetworkVisor.Core.Networking.Services.MulticastDns.Client;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Networking.Sockets.Listeners;
using NetworkVisor.Core.Networking.Sockets.Sockets;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.NetworkingSystem;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Networking.Sockets;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.LocalNetworkServices
{
    /// <summary>
    /// Class CoreLocalNetworkServicesIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLocalNetworkServicesIntegrationTests))]

    public class CoreLocalNetworkServicesIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLocalNetworkServicesIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLocalNetworkServicesIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public IPAddress? PreferredLocalIPAddress => this.TestNetworkingSystem?.PreferredLocalNetworkAddress?.IPAddress;

        [Theory]
        [InlineData(null, 0, null, 0, false, true, false, CoreLocalNetworkCapabilities.SendReceiveUDPUnicast)]
        [InlineData(null, 0, CoreIPAddressExtensions.StringAny, 0, false, true, false, CoreLocalNetworkCapabilities.SendReceiveUDPUnicast)]
        [InlineData(null, 0, CoreIPAddressExtensions.StringAny, 0, true, true, false, CoreLocalNetworkCapabilities.SendReceiveUDPBroadcast)]
        [InlineData(CoreIPAddressExtensions.StringMulticastDnsBroadcast, CoreMulticastDnsConstants.MulticastDnsServerPort, CoreIPAddressExtensions.StringAny, CoreMulticastDnsConstants.MulticastDnsServerPort, false, true, true, CoreLocalNetworkCapabilities.SendReceiveUDPMulticastDns)]

        public async Task LocalNetworkServicesIntegrationTests_SendUdpPacket(string? senderIPAddress, int senderPort, string? listenerIPAddress, int listenerPort, bool enableBroadcast, bool reuseAddress, bool multicastDns, CoreLocalNetworkCapabilities expectedLocalNetworkCapabilities)
        {
            this.PreferredLocalIPAddress.IsNullNoneOrAny().Should().BeFalse();
            CoreIPEndPoint senderIPEndPoint = new CoreIPEndPoint(senderIPAddress is null ? this.PreferredLocalIPAddress! : IPAddress.Parse(senderIPAddress), senderPort);
            CoreIPEndPoint listenerIPEndPoint = new CoreIPEndPoint(listenerIPAddress is null ? this.PreferredLocalIPAddress! : IPAddress.Parse(listenerIPAddress), listenerPort);

            if (multicastDns)
            {
                // Create a Multicast Dns service discovery request: [Guid]._nvtest._udp.local.
                var dnsUdpTestServiceQueryServiceInstance = $"{Guid.NewGuid().ToStringNoDashes()}.{CoreDnsConstants.DnsUdpTestServiceQueryLocalDomain}";
                this.TestOutputHelper.WriteLine($"DnsUdpTestServiceQueryServiceInstance: {dnsUdpTestServiceQueryServiceInstance}");
                var multicastDnsServiceDiscoveryRequest = CoreMulticastDnsSendClientBase.GetDnsRequestBytes(dnsUdpTestServiceQueryServiceInstance);
                var testUdpBuffer = new CoreTestUdpBuffer(multicastDnsServiceDiscoveryRequest, senderIPEndPoint, listenerIPEndPoint);

                this.TestOutputHelper.WriteLine($"Sending multicast UDP packet from {senderIPEndPoint} to {listenerIPEndPoint} using preferred network interface ({this.TestNetworkingSystem.PreferredNetworkInterface?.PreferredIPAddress}), Broadcast={enableBroadcast}, ReuseAddress={reuseAddress}.");
                CoreSocketListenerOptions coreSocketListenerOptions = CoreLocalNetworkServicesBase.CreateSocketListenerOptions(testUdpBuffer, enableBroadcast, reuseAddress, new CoreMulticastOption(senderIPEndPoint.Address, this.TestNetworkingSystem.PreferredNetworkInterface!), true);

                var cts = new CancellationTokenSource();

                CoreLocalNetworkCapabilities localNetworkCapabilities = await CoreLocalNetworkServicesBase.TestSendUdpPacketAsync(this.TestNetworkingSystem, coreSocketListenerOptions, cts.Token, this.TestCaseLogger);
                CoreTestSocketHelper.OutputUdpTestBuffer(this.TestOutputHelper, testUdpBuffer, localNetworkCapabilities);

                // Output the received multicast [Guid]._nvtest._udp.local service discovery request.
                if (testUdpBuffer.ReceiveBuffer is not null)
                {
                    var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, testUdpBuffer.ReceiveBuffer!, senderIPEndPoint, this.TestCaseLogger);
                    this.TestOutputHelper.WriteLine($"\nReceived DnsResponse:\n{dnsResponse.ToStringWithPropNameMultiLine()}");
                    dnsResponse.Questions.FirstOrDefault()?.QuestionName.Should().Be(dnsUdpTestServiceQueryServiceInstance);
                    localNetworkCapabilities.Should().Be(expectedLocalNetworkCapabilities);
                }
                else if (this.TestNetworkingSystem.PreferredNetworkInterface!.IsLocalNetworkAccessRestricted)
                {
                    this.TestOutputHelper.WriteLine("Failure: No response received due to local network policy requirement");
                    localNetworkCapabilities.Should().Be(CoreLocalNetworkCapabilities.None);
                }
                else
                {
                    this.TestOutputHelper.WriteLine("Failure: No multicast DNS response received.");
                    localNetworkCapabilities.Should().Be(expectedLocalNetworkCapabilities);
                }
            }
            else
            {
                var testUdpBuffer = new CoreTestUdpBuffer(senderIPEndPoint, listenerIPEndPoint);

                this.TestOutputHelper.WriteLine($"Sending UDP packet from {senderIPEndPoint} to {listenerIPEndPoint}, Broadcast={enableBroadcast}, ReuseAddress={reuseAddress}.");
                CoreSocketListenerOptions coreSocketListenerOptions = CoreLocalNetworkServicesBase.CreateSocketListenerOptions(testUdpBuffer, enableBroadcast, reuseAddress);

                var cts = new CancellationTokenSource();

                CoreLocalNetworkCapabilities localNetworkCapabilities = await CoreLocalNetworkServicesBase.TestSendUdpPacketAsync(this.TestNetworkingSystem, coreSocketListenerOptions, cts.Token, this.TestCaseLogger);
                CoreTestSocketHelper.OutputUdpTestBuffer(this.TestOutputHelper, testUdpBuffer, localNetworkCapabilities);
                localNetworkCapabilities.Should().Be(expectedLocalNetworkCapabilities);
            }
        }
    }
}
