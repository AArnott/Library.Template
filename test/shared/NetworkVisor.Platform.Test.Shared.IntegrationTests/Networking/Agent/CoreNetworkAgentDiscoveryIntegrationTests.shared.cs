// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="CoreNetworkAgentDiscoveryIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Net.Sockets;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Services.Agent.Service;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Networking.Services.MulticastDns.Extensions;
using NetworkVisor.Core.Networking.Services.MulticastDns.Records;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Networking.Sockets.Listeners;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Agent
{
    /// <summary>
    /// Class CoreNetworkAgentDiscoveryIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkAgentDiscoveryIntegrationTests))]

    public class CoreNetworkAgentDiscoveryIntegrationTests : CoreTestCaseBase
    {
        private static int agentPortBase = 10000;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkAgentDiscoveryIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkAgentDiscoveryIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NetworkAgentDiscoveryIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void NetworkAgentDiscoveryIntegration_SendServiceQueryResponseToLoopback()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.SendToLoopback))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.SendToLoopback} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            if (this.TestNetworkingSystem.PreferredMulticastNetworkInterface?.IsLocalNetworkAccessRestricted ?? true)
            {
                this.TestOutputHelper.WriteLine($"Network Agent discovery requires a multicast interface and unrestricted local network access.");

                return;
            }

            DateTimeOffset timeStamp = DateTimeOffset.UtcNow;

            CoreIPEndPoint listenerIPEndPoint = new CoreIPEndPoint(IPAddress.Loopback, this.GetRandomPort());
            var dnsResponseSend = new DnsResponse(this.TestCaseServiceProvider, 0, DnsRecordHeader.DefaultFlags, null, this.TestCaseLogger);

            dnsResponseSend.AddAnswer(new AnswerDnsResponseRecord(new DnsRecordPtr(CoreNetworkAgentDnsProvider.NetworkAgentServiceName), CoreDnsConstants.DnsBrowseQueryLocalDomain, DnsRecordClass.IN, CoreNetworkAgentBackgroundService.DefaultTimeToLive, timeStamp));

            // Validate network agent answer properties
            dnsResponseSend.IsQueryResponse.Should().BeTrue();
            AnswerDnsResponseRecord? dnsAnswer = dnsResponseSend.Answers.FirstOrDefault();
            dnsAnswer.Should().NotBeNull();
            dnsAnswer!.IsServiceDiscoveryQueryResponse.Should().BeTrue();
            dnsAnswer!.IsNetworkAgentResponse().Should().BeFalse();
            dnsAnswer!.IsNetworkAgentDiscoveryResponse().Should().BeTrue();

            // Validate network agent PTR properties
            DnsRecordPtr? ptrRecord = dnsAnswer.GetDnsRecord<DnsRecordPtr>();
            ptrRecord.Should().NotBeNull();
            ptrRecord!.IsServiceDiscoveryQueryResponse.Should().BeTrue();
            ptrRecord.PtrDomainNameType.IsNetworkAgentQuery();

            // Create Multicast Dns Server UDP Listener
            var coreSocketListenerServerOptions = new CoreSocketListenerOptions(listenerIPEndPoint)
            {
                Broadcast = true,
                ProtocolType = ProtocolType.Udp,
                SocketType = SocketType.Dgram,
                ReuseAddress = true,
                SendTimeout = CoreMulticastDnsConstants.MulticastDnsSendTimeout,
                ReceiveTimeout = CoreMulticastDnsConstants.MulticastDnsReceiveTimeout,
            };

            this.TestOutputHelper.WriteLine($"Send:\n{dnsResponseSend.ToStringWithPropNameMultiLine()}\n");
            using var networkAgentListener = new CoreUdpListener(coreSocketListenerServerOptions, this.TestCaseLogger);
            DnsResponse? dnsResponseReceived = null;

            networkAgentListener.Connected += (sender, channel) =>
            {
                if (channel?.InputStream is null || channel.InputStream.Length < CoreMulticastDnsConstants.MulticastDnsMinMessageSize ||
                    channel.InputStream.Length > CoreMulticastDnsConstants.MulticastDnsMaxMessageSize)
                {
                    channel?.Logger.LogError("Unknown Multicast packet received with {BytesTransferred} bytes transferred", channel?.InputStream?.Length ?? 0);
                    return;
                }

                // Do not set the remoteIPEndPoint so the comparison below succeeds.
                dnsResponseReceived = new DnsResponse(this.TestCaseServiceProvider, channel.InputStream.ToByteArray(), null, channel.Logger);
                this.TestOutputHelper.WriteLine($"Received:\n{dnsResponseReceived.ToStringWithPropNameMultiLine()}");
                this.TestOutputHelper.WriteLine();
            };

            this.TestOutputHelper.WriteLine($"Listening on endpoint {listenerIPEndPoint}");
            networkAgentListener.Start();

            dnsResponseSend.SendUdpResponse(listenerIPEndPoint, true).Should().BeGreaterThan(0);

            // Delay for 2 seconds
            this.TestDelay(2000, this.TestCaseLogger).Should().BeTrue();

            networkAgentListener.Stop();

            // Verify received and sent responses are identical except for the remote end point.
            dnsResponseReceived.Should().NotBeNull();
            dnsResponseReceived!.Equals(dnsResponseSend).Should().BeTrue();
        }

        private ushort GetRandomPort()
        {
            return (ushort)(Interlocked.Increment(ref agentPortBase) + (DateTime.UtcNow.Ticks % 10000));
        }
    }
}
