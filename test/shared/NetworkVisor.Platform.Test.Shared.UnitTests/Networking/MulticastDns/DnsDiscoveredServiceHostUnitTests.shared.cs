// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="DnsDiscoveredServiceHostUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.MulticastDns.Discovery;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Networking.Services.MulticastDns.Discovery;
using NetworkVisor.Core.Networking.Services.MulticastDns.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.MulticastDns
{
    /// <summary>
    /// Class DnsDiscoveryHostUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DnsDiscoveredServiceHostUnitTests))]

    public class DnsDiscoveredServiceHostUnitTests : CoreTestCaseBase
    {
        private static readonly uint Ttl = 4500;
        private static readonly byte[] OptionBytesPassword = [0x00, 0x00, 0x8B, 0xDC, 0xAB, 0x5D, 0xAD, 0xEA, 0xF3, 0xD2, 0x88, 0x6B, 0x28, 0x71, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36];
        private static readonly CoreIPEndPoint RemoteIPEndPoint = new(IPAddress.Loopback, CoreMulticastDnsConstants.MulticastDnsServerPort);
        private static readonly ushort SenderUdpPayloadSize = 1440;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDiscoveredServiceHostUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DnsDiscoveredServiceHostUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void DnsDiscoveredServiceHost_CreateServiceHost()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();
            dnsDiscoveredServiceHost!.IPAddresses.Any().Should().BeFalse();
            dnsDiscoveredServiceHost.ServiceHostDomainName.Should().Be("theater._airplay._tcp.local.");
            dnsDiscoveredServiceHost.CreatedTimestamp.Should().Be(modifiedTimeStamp);
            dnsDiscoveredServiceHost.ModifiedTimestamp.Should().Be(modifiedTimeStamp);
            dnsDiscoveredServiceHost.ObjectCacheVersion.Should().Be(1);
            dnsDiscoveredServiceHost.DnsWakeUpOnLan.Should().BeNull();
            dnsDiscoveredServiceHost.Logger.Should().BeSameAs(this.TestCaseLogger);

            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceHost.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void DnsDiscoveredServiceHost_CreateServiceHost_ServiceHostDomainName_Null()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            Func<ICoreDnsDiscoveredServiceHost?> fx = () => CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, null!, modifiedTimeStamp, Ttl, this.TestCaseLogger);
            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceHost_CreateServiceHost_RecordSrv_Null()
        {
            Func<ICoreDnsDiscoveredServiceHost?> fx = () =>
            {
                return CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, null!, this.TestCaseLogger);
            };

            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateHostIPAddresses()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();

            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceHost.ToStringWithParentsPropNameMultiLine());
            dnsDiscoveredServiceHost!.IPAddresses.Any().Should().BeFalse();
            dnsDiscoveredServiceHost.UpdateHostIPAddresses([IPAddress.Loopback]).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateIPAddresses);

            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine("Updated IP".CenterTitle());
            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceHost.ToStringWithParentsPropNameMultiLine());

            var ipAddresses = dnsDiscoveredServiceHost.IPAddresses.ToList();

            ipAddresses.Count.Should().Be(1);
            dnsDiscoveredServiceHost.IPAddresses.FirstOrDefault().Should().Be(IPAddress.Loopback);

            dnsDiscoveredServiceHost.UpdateHostIPAddresses([IPAddress.Loopback]).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            ipAddresses = dnsDiscoveredServiceHost.IPAddresses.ToList();
            ipAddresses.Count.Should().Be(1);
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateHostIPAddresses_Null()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();

            Func<CoreDnsServiceDiscoveryEventOperationType?> fx = () => dnsDiscoveredServiceHost!.UpdateHostIPAddresses(null!);
            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateHostIPAddresses_Empty()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();
            dnsDiscoveredServiceHost!.IPAddresses.Any().Should().BeFalse();
            dnsDiscoveredServiceHost.UpdateHostIPAddresses([]).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            dnsDiscoveredServiceHost!.IPAddresses.Any().Should().BeFalse();
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateWakeOnLan()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsWakeUpOnLan = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();
            dnsDiscoveredServiceHost!.UpdateWakeOnLan(dnsWakeUpOnLan).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateWakeUpOnLan);
            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceHost.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateWakeOnLan_Same()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsWakeUpOnLan = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();
            dnsDiscoveredServiceHost!.UpdateWakeOnLan(dnsWakeUpOnLan).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateWakeUpOnLan);

            dnsDiscoveredServiceHost.UpdateWakeOnLan(dnsWakeUpOnLan).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);

            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceHost.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateWakeOnLan_Null()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();
            Func<CoreDnsServiceDiscoveryEventOperationType> fx = () => dnsDiscoveredServiceHost!.UpdateWakeOnLan(null!);
            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateFromServiceHost_SameAs()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();
            dnsDiscoveredServiceHost!.UpdateFromServiceHost(dnsDiscoveredServiceHost).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateFromServiceHost_Null()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();

            Func<CoreDnsServiceDiscoveryEventOperationType> fx = () => dnsDiscoveredServiceHost!.UpdateFromServiceHost(null!);
            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateFromServiceHost_ModifiedTimeStamp()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost1 = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp.AddSeconds(1), Ttl, this.TestCaseLogger);

            dnsDiscoveredServiceHost.Should().NotBeNull();
            dnsDiscoveredServiceHost1.Should().NotBeNull();

            uint ttlSave = dnsDiscoveredServiceHost!.TimeToLive;
            dnsDiscoveredServiceHost.UpdateFromServiceHost(dnsDiscoveredServiceHost1!).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateExpire);
            dnsDiscoveredServiceHost.TimeToLive.Should().BeGreaterThanOrEqualTo(ttlSave);
            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceHost.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void DnsDiscoveredServiceHost_UpdateFromServiceHost_TimeToLive()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            ICoreDnsDiscoveredServiceHost? dnsDiscoveredServiceHost1 = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "theater._airplay._tcp.local.", modifiedTimeStamp, Ttl + 20, this.TestCaseLogger);
            dnsDiscoveredServiceHost.Should().NotBeNull();
            dnsDiscoveredServiceHost1.Should().NotBeNull();

            uint ttlSave = dnsDiscoveredServiceHost!.TimeToLive;
            dnsDiscoveredServiceHost.UpdateFromServiceHost(dnsDiscoveredServiceHost1!).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateExpire);
            dnsDiscoveredServiceHost.TimeToLive.Should().BeGreaterThanOrEqualTo(ttlSave);
        }
    }
}
