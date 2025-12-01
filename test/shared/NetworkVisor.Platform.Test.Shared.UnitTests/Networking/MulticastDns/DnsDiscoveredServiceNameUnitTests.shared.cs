// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="DnsDiscoveredServiceNameUnitTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Networking.Services.MulticastDns.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.MulticastDns
{
    /// <summary>
    /// Class DnsDiscoveryHostUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DnsDiscoveredServiceNameUnitTests))]

    public class DnsDiscoveredServiceNameUnitTests : CoreTestCaseBase
    {
        private static readonly CoreIPEndPoint RemoteIPEndPoint = new(IPAddress.Loopback, CoreMulticastDnsConstants.MulticastDnsServerPort);
        private static readonly CoreIPEndPoint RemoteIPEndPoint1 = new(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreMulticastDnsConstants.MulticastDnsServerPort);

        private static readonly uint Ttl = 4500;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDiscoveredServiceNameUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DnsDiscoveredServiceNameUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData(null, CoreDomainNameType.Unknown)]
        [InlineData("", CoreDomainNameType.RootDomain)]
        [InlineData("_00000000-0116-7ff2-9c44-69a792684d42._sub._appletv-v2._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("_adb-tls-connect._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_adb-tls-pairing._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_adb._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_adisk._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_afpovertcp._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_airport._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_axis-video._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_elg._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_hap._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_hap._udp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Udp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_ipp._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_ipps._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_ippusb._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_meshcop._udp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Udp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_mieleathome._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_net-assistant._udp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Udp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_pdl-datastream._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_printer._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_ptp._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_rdlink._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_remotepairing-manual-pairing._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_remotepairing._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_rfb._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_scanner._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_smb._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_uscan._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("_uscans._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceName | CoreDomainNameType.LocalDomain)]
        [InlineData("103182565E53._hap._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("16C858EA-439A-4D23-834C-6B7EF8B35F7C._remotepairing._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("7e579e77-1d80-4dba-8c60-5dc9a792aa0a._remotepairing._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("99E52A5D-1C43-4755-A262-CB143D63360A._remotepairing._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("ally sonos move._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("Apple BorderRouter #1D07._meshcop._udp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Udp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("Apple BorderRouter #CFD4._meshcop._udp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Udp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("BCAirport._airport._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("BCDenPrinter._ipp._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("bumacbookm1 (2)._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("BuMacBookM1 (2)._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("BuMacBookM1 (2)._eppc._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("BuMacBookM1 (2)._net-assistant._udp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Udp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("BuMacBookM1 (2)._rfb._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("BuMacBookM1 (2)._sftp-ssh._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("BuMacBookM1 (2)._smb._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("BuMacBookM1 (2)._ssh._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("den._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("Doorstation - 1CCAE374BBC1._axis-video._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("downstairs._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("e88f52b0-a21e-47a9-b353-22c73c342718._remotepairing._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("F4D4886B2872@BuMacBookM1 (2)._raop._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("Gate Last Motion Photo 615A2A._hap._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("Gate Last Ring Photo 44D1F8._hap._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("Gate Live Video D1C39F._hap._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("great room   ._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("great room._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("Great Room._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("HASS Bridge 4EB7AA._hap._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("home theater._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("Home Theater._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("Steve Bush’s Library._home-sharing._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]
        [InlineData("theater._airplay._tcp.local.", CoreDomainNameType.DnsServiceDiscovery | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.LocalDomain)]

        [InlineData("b._dns-sd._udp.example.local.", CoreDomainNameType.DnsServiceBrowseLocalDomain)]
        [InlineData("db._dns-sd._udp.example.local.", CoreDomainNameType.DnsServiceDefaultBrowseLocalDomain)]
        [InlineData("lb._dns-sd._udp.example.local.", CoreDomainNameType.DnsServiceLocalBrowseLocalDomain)]
        [InlineData("lb._dns-sd._udp.local.", CoreDomainNameType.DnsServiceLocalBrowseLocalDomain)]
        [InlineData("r._dns-sd._udp.local.", CoreDomainNameType.DnsServiceRegistrationLocalDomain)]
        [InlineData("dr._dns-sd._udp.local.", CoreDomainNameType.DnsServiceDefaultRegistrationLocalDomain)]

        [InlineData("b._dns-sd._udp.example.com.", CoreDomainNameType.DnsServiceBrowseDomain)]
        [InlineData("db._dns-sd._udp.example.com.", CoreDomainNameType.DnsServiceDefaultBrowseDomain)]
        [InlineData("lb._dns-sd._udp.example.com.", CoreDomainNameType.DnsServiceLocalBrowseDomain)]
        [InlineData("r._dns-sd._udp.example.com.", CoreDomainNameType.DnsServiceRegistrationDomain)]
        [InlineData("dr._dns-sd._udp.example.com.", CoreDomainNameType.DnsServiceDefaultRegistrationDomain)]

        [InlineData(".", CoreDomainNameType.RootDomain)]

        [InlineData("foobar.arpa.", CoreDomainNameType.ArpaDomain)]
        [InlineData("home.arpa.", CoreDomainNameType.ArpaHomeDomain)]
        [InlineData("openthread.thread.home.arpa.", CoreDomainNameType.ArpaThreadHomeDomain)]

        [InlineData("_kerberos.BuMacBookM1.local.", CoreDomainNameType.Kerberos)]

        [InlineData("Home-Theater.local.", CoreDomainNameType.LocalDomainHost)]
        [InlineData("Great-Room.local.", CoreDomainNameType.LocalDomainHost)]
        [InlineData("BuMacBookM1.local.", CoreDomainNameType.LocalDomainHost)]

        [InlineData("microsoft.com.", CoreDomainNameType.PublicDomainHost)]
        [InlineData("BranchOffice.microsoft.com.", CoreDomainNameType.PublicDomainHost)]
        [InlineData("www.microsoft.com.", CoreDomainNameType.PublicDomainHost)]

        [InlineData("myserver", CoreDomainNameType.Host)]
        public void DnsDiscoveredServiceName_IsValidServiceName(string? serviceDomainNameString, CoreDomainNameType serviceDomainTypeTest)
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            ICoreDnsDiscoveredServiceName? dnsDiscoveredServiceName = CoreDnsDiscoveredServiceName.CreateServiceName(this.TestCaseServiceProvider, serviceDomainNameString!, modifiedTimeStamp, Ttl, this.TestCaseLogger);

            if (serviceDomainTypeTest.HasFlag(CoreDomainNameType.ServiceName) || serviceDomainTypeTest.HasFlag(CoreDomainNameType.ServiceInstance))
            {
                dnsDiscoveredServiceName.Should().NotBeNull();
                dnsDiscoveredServiceName!.Logger.Should().BeSameAs(this.TestCaseLogger);
                dnsDiscoveredServiceName.ModifiedTimestamp.Should().Be(modifiedTimeStamp);
                dnsDiscoveredServiceName.TimeToLive.Should().BeLessThanOrEqualTo(Ttl);
                dnsDiscoveredServiceName.CreatedTimestamp.Should().Be(modifiedTimeStamp);
                dnsDiscoveredServiceName.ObjectCacheVersion.Should().Be(1);
                dnsDiscoveredServiceName.ServiceName.IsValidServiceName().Should().BeTrue();
                this.TestOutputHelper.WriteLine(dnsDiscoveredServiceName.ToStringWithParentsPropNameMultiLine());
            }
            else
            {
                dnsDiscoveredServiceName.Should().BeNull();
            }
        }

        [Fact]
        public void DnsDiscoveredServiceName_UpdateServiceName_ModifiedTimestamp()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            ICoreDnsDiscoveredServiceName? dnsDiscoveredServiceName = CoreDnsDiscoveredServiceName.CreateServiceName(this.TestCaseServiceProvider, "_smb._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);

            dnsDiscoveredServiceName.Should().NotBeNull();
            dnsDiscoveredServiceName!.Logger.Should().BeSameAs(this.TestCaseLogger);
            dnsDiscoveredServiceName.ModifiedTimestamp.Should().Be(modifiedTimeStamp);
            dnsDiscoveredServiceName.TimeToLive.Should().BeLessThanOrEqualTo(Ttl);
            dnsDiscoveredServiceName.CreatedTimestamp.Should().Be(modifiedTimeStamp);
            dnsDiscoveredServiceName.ServiceName.IsValidServiceName().Should().BeTrue();
            dnsDiscoveredServiceName.ObjectCacheVersion.Should().Be(1);
            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceName.ToStringWithParentsPropNameMultiLine());

            dnsDiscoveredServiceName.UpdateFromServiceName(dnsDiscoveredServiceName).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            dnsDiscoveredServiceName.ObjectCacheVersion.Should().Be(1);

            ICoreDnsDiscoveredServiceName? dnsDiscoveredServiceNameUpdate = CoreDnsDiscoveredServiceName.CreateServiceName(this.TestCaseServiceProvider, "_smb._tcp.local.", modifiedTimeStamp.AddSeconds(1), Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceNameUpdate.Should().NotBeNull();

            uint ttlSave = dnsDiscoveredServiceName.TimeToLive;
            dnsDiscoveredServiceName.UpdateFromServiceName(dnsDiscoveredServiceNameUpdate!).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateExpire);
            dnsDiscoveredServiceName.ObjectCacheVersion.Should().Be(2);
            dnsDiscoveredServiceName.TimeToLive.Should().BeGreaterThanOrEqualTo(ttlSave);

            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine("Updated Service Name".CenterTitle());
            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceName.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void DnsDiscoveredServiceName_UpdateServiceName_TTL()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            ICoreDnsDiscoveredServiceName? dnsDiscoveredServiceName = CoreDnsDiscoveredServiceName.CreateServiceName(this.TestCaseServiceProvider, "_smb._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);

            dnsDiscoveredServiceName.Should().NotBeNull();
            dnsDiscoveredServiceName!.Logger.Should().BeSameAs(this.TestCaseLogger);
            dnsDiscoveredServiceName.ModifiedTimestamp.Should().Be(modifiedTimeStamp);
            dnsDiscoveredServiceName.TimeToLive.Should().BeLessThanOrEqualTo(Ttl);
            dnsDiscoveredServiceName.CreatedTimestamp.Should().Be(modifiedTimeStamp);
            dnsDiscoveredServiceName.ServiceName.IsValidServiceName().Should().BeTrue();
            dnsDiscoveredServiceName.ObjectCacheVersion.Should().Be(1);
            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceName.ToStringWithParentsPropNameMultiLine());

            dnsDiscoveredServiceName.UpdateFromServiceName(dnsDiscoveredServiceName).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            dnsDiscoveredServiceName.ObjectCacheVersion.Should().Be(1);

            ICoreDnsDiscoveredServiceName? dnsDiscoveredServiceNameUpdate = CoreDnsDiscoveredServiceName.CreateServiceName(this.TestCaseServiceProvider, "_smb._tcp.local.", modifiedTimeStamp, Ttl + 100, this.TestCaseLogger);
            dnsDiscoveredServiceNameUpdate.Should().NotBeNull();

            uint ttlSave = dnsDiscoveredServiceName.TimeToLive;
            dnsDiscoveredServiceName.UpdateFromServiceName(dnsDiscoveredServiceNameUpdate!).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateExpire);
            dnsDiscoveredServiceName.ObjectCacheVersion.Should().Be(2);
            dnsDiscoveredServiceName.TimeToLive.Should().BeGreaterThanOrEqualTo(ttlSave);

            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine("Updated Service Name".CenterTitle());
            this.TestOutputHelper.WriteLine(dnsDiscoveredServiceName.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void DnsDiscoveredServiceName_UpdateServiceName_Null()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            ICoreDnsDiscoveredServiceName? dnsDiscoveredServiceName = CoreDnsDiscoveredServiceName.CreateServiceName(this.TestCaseServiceProvider, "_smb._tcp.local.", modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveredServiceName.Should().NotBeNull();

            Func<CoreDnsServiceDiscoveryEventOperationType> fx = () => dnsDiscoveredServiceName!.UpdateFromServiceName(null!);
            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceName_CreateServiceName_RecordPtr_Null()
        {
            Func<ICoreDnsDiscoveredServiceName?> fx = () =>
            {
                return CoreDnsDiscoveredServiceName.CreateServiceName(this.TestCaseServiceProvider, null!, this.TestCaseLogger);
            };

            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceName_CreateServiceName_DnsResponseRecord_Null()
        {
            Func<ICoreDnsDiscoveredServiceName?> fx = () =>
            {
                return CoreDnsDiscoveredServiceName.CreateServiceName(this.TestCaseServiceProvider, "_smb._tcp.local.", null!, this.TestCaseLogger);
            };

            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceName_CreateServiceName_ServiceName_DnsResponseRecord_Null()
        {
            Func<ICoreDnsDiscoveredServiceName?> fx = () =>
            {
                return CoreDnsDiscoveredServiceName.CreateServiceName(null!, null!, this.TestCaseLogger);
            };

            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceName_CreateServiceName_ServiceName_Null()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;

            CoreDnsDiscoveredServiceName.CreateServiceName(this.TestCaseServiceProvider, null!, modifiedTimeStamp, Ttl, this.TestCaseLogger).Should().BeNull();
        }
    }
}
