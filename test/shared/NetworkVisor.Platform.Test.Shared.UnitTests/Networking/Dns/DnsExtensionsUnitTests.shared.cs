// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="DnsExtensionsUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Networking.Services.MulticastDns.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;
using CoreDnsExtensions = NetworkVisor.Core.Networking.Services.MulticastDns.Extensions.CoreDnsExtensions;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Dns
{
    /// <summary>
    /// Class DnsExtensionsUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DnsExtensionsUnitTests))]

    public class DnsExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnsExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DnsExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData(null, CoreDomainNameType.Unknown)]
        [InlineData("", CoreDomainNameType.RootDomain)]
        [InlineData("_00000000-0116-7ff2-9c44-69a792684d42._sub._appletv-v2._tcp.local.", CoreDomainNameType.LocalDomain | CoreDomainNameType.Tcp | CoreDomainNameType.ServiceInstance | CoreDomainNameType.DnsServiceDiscovery)]
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

        public void DnsExtensions_ToServiceName(string? serviceDomainNameString, CoreDomainNameType serviceDomainTypeTest)
        {
            CoreDomainNameType serviceDomainType = CoreDnsExtensions.ToDomainNameType(serviceDomainNameString!);
            this.TestOutputHelper.WriteLine($"{serviceDomainNameString}: {serviceDomainNameString.ToServiceName()} [{serviceDomainType}]");
            serviceDomainType.Should().Be(serviceDomainTypeTest);

            // serviceDomainNameString.ToServiceName().Should().Be(toServiceName);
        }

        [InlineData(null, null, null)]
        [InlineData("", null, null)]
        [InlineData(".", null, null)]
        [InlineData("_00000000-0116-7ff2-9c44-69a792684d42._sub._appletv-v2._tcp.local.", "_appletv-v2._tcp.", "local.")]
        [InlineData("_adb-tls-connect._tcp.local.", "_adb-tls-connect._tcp.", "local.")]
        [InlineData("_adb-tls-pairing._tcp.local.", "_adb-tls-pairing._tcp.", "local.")]
        [InlineData("_adb._tcp.local.", "_adb._tcp.", "local.")]
        [InlineData("_adisk._tcp.local.", "_adisk._tcp.", "local.")]
        [InlineData("_afpovertcp._tcp.local.", "_afpovertcp._tcp.", "local.")]
        [InlineData("_airplay._tcp.local.", "_airplay._tcp.", "local.")]
        [InlineData("_airport._tcp.local.", "_airport._tcp.", "local.")]
        [InlineData("_axis-video._tcp.local.", "_axis-video._tcp.", "local.")]
        [InlineData("_elg._tcp.local.", "_elg._tcp.", "local.")]
        [InlineData("_hap._tcp.local.", "_hap._tcp.", "local.")]
        [InlineData("_hap._udp.local.", "_hap._udp.", "local.")]
        [InlineData("_ipp._tcp.local.", "_ipp._tcp.", "local.")]
        [InlineData("_ipps._tcp.local.", "_ipps._tcp.", "local.")]
        [InlineData("_ippusb._tcp.local.", "_ippusb._tcp.", "local.")]
        [InlineData("_meshcop._udp.local.", "_meshcop._udp.", "local.")]
        [InlineData("_mieleathome._tcp.local.", "_mieleathome._tcp.", "local.")]
        [InlineData("_net-assistant._udp.local.", "_net-assistant._udp.", "local.")]
        [InlineData("_pdl-datastream._tcp.local.", "_pdl-datastream._tcp.", "local.")]
        [InlineData("_printer._tcp.local.", "_printer._tcp.", "local.")]
        [InlineData("_ptp._tcp.local.", "_ptp._tcp.", "local.")]
        [InlineData("_rdlink._tcp.local.", "_rdlink._tcp.", "local.")]
        [InlineData("_remotepairing-manual-pairing._tcp.local.", "_remotepairing-manual-pairing._tcp.", "local.")]
        [InlineData("_remotepairing._tcp.local.", "_remotepairing._tcp.", "local.")]
        [InlineData("_rfb._tcp.local.", "_rfb._tcp.", "local.")]
        [InlineData("_scanner._tcp.local.", "_scanner._tcp.", "local.")]
        [InlineData("_smb._tcp.local.", "_smb._tcp.", "local.")]
        [InlineData("_uscan._tcp.local.", "_uscan._tcp.", "local.")]
        [InlineData("_uscans._tcp.local.", "_uscans._tcp.", "local.")]
        [InlineData("103182565E53._hap._tcp.local.", "_hap._tcp.", "local.")]
        [InlineData("16C858EA-439A-4D23-834C-6B7EF8B35F7C._remotepairing._tcp.local.", "_remotepairing._tcp.", "local.")]
        [InlineData("7e579e77-1d80-4dba-8c60-5dc9a792aa0a._remotepairing._tcp.local.", "_remotepairing._tcp.", "local.")]
        [InlineData("99E52A5D-1C43-4755-A262-CB143D63360A._remotepairing._tcp.local.", "_remotepairing._tcp.", "local.")]
        [InlineData("ally sonos move._airplay._tcp.local.", "_airplay._tcp.", "local.")]
        [InlineData("Apple BorderRouter #1D07._meshcop._udp.local.", "_meshcop._udp.", "local.")]
        [InlineData("Apple BorderRouter #CFD4._meshcop._udp.local.", "_meshcop._udp.", "local.")]
        [InlineData("BCAirport._airport._tcp.local.", "_airport._tcp.", "local.")]
        [InlineData("BCDenPrinter._ipp._tcp.local.", "_ipp._tcp.", "local.")]
        [InlineData("bumacbookm1 (2)._airplay._tcp.local.", "_airplay._tcp.", "local.")]
        [InlineData("BuMacBookM1 (2)._airplay._tcp.local.", "_airplay._tcp.", "local.")]
        [InlineData("BuMacBookM1 (2)._eppc._tcp.local.", "_eppc._tcp.", "local.")]
        [InlineData("BuMacBookM1 (2)._net-assistant._udp.local.", "_net-assistant._udp.", "local.")]
        [InlineData("BuMacBookM1 (2)._rfb._tcp.local.", "_rfb._tcp.", "local.")]
        [InlineData("BuMacBookM1 (2)._sftp-ssh._tcp.local.", "_sftp-ssh._tcp.", "local.")]
        [InlineData("BuMacBookM1 (2)._smb._tcp.local.", "_smb._tcp.", "local.")]
        [InlineData("BuMacBookM1 (2)._ssh._tcp.local.", "_ssh._tcp.", "local.")]
        [InlineData("den._airplay._tcp.local.", "_airplay._tcp.", "local.")]

        [InlineData("b._dns-sd._udp.example.local.", "_dns-sd._udp.", "example.local.")]
        [InlineData("db._dns-sd._udp.example.local.", "_dns-sd._udp.", "example.local.")]
        [InlineData("lb._dns-sd._udp.example.local.", "_dns-sd._udp.", "example.local.")]
        [InlineData("lb._dns-sd._udp.local.", "_dns-sd._udp.", "local.")]
        [InlineData("r._dns-sd._udp.local.", "_dns-sd._udp.", "local.")]
        [InlineData("dr._dns-sd._udp.local.", "_dns-sd._udp.", "local.")]

        [InlineData("b._dns-sd._udp.example.com.", "_dns-sd._udp.", "example.com.")]
        [InlineData("db._dns-sd._udp.example.com.", "_dns-sd._udp.", "example.com.")]
        [InlineData("lb._dns-sd._udp.example.com.", "_dns-sd._udp.", "example.com.")]
        [InlineData("r._dns-sd._udp.example.com.", "_dns-sd._udp.", "example.com.")]
        [InlineData("dr._dns-sd._udp.example.com.", "_dns-sd._udp.", "example.com.")]

        [InlineData("foobar.arpa.", null, null)]
        [InlineData("home.arpa.", null, null)]
        [InlineData("openthread.thread.home.arpa.", null, null)]

        [InlineData("_kerberos.BuMacBookM1.local.", null, null)]

        [InlineData("Home-Theater.local.", null, null)]
        [InlineData("Great-Room.local.", null, null)]
        [InlineData("BuMacBookM1.local.", null, null)]

        [InlineData("microsoft.com.", null, null)]
        [InlineData("BranchOffice.microsoft.com.", null, null)]
        [InlineData("www.microsoft.com.", null, null)]

        [InlineData("myserver", null, null)]
        [Theory]
        public void DnsExtensions_ToServiceNameDomain(string? serviceDomainNameString, string? serviceNameTest, string? serviceDomainTest)
        {
            (string? ServiceName, string? ServiceDomain)? serviceNameDomain = serviceDomainNameString.ToServiceNameDomain();

            if (serviceNameTest is null && serviceDomainTest is null)
            {
                serviceNameDomain.Should().BeNull();
            }
            else
            {
                serviceNameDomain.Should().NotBeNull();

                serviceNameDomain!.Value.ServiceName.Should().Be(serviceNameTest);
                serviceNameDomain.Value.ServiceName.IsValidServiceName().Should().BeTrue();
                serviceNameDomain.Value.ServiceDomain.Should().Be(serviceDomainTest);
            }
        }
    }
}
