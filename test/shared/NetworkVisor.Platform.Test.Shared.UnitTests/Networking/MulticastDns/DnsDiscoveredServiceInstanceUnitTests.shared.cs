// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="DnsDiscoveredServiceInstanceUnitTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Networking.Services.MulticastDns.Records;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.MulticastDns
{
    /// <summary>
    /// Class DnsDiscoveryHostUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DnsDiscoveredServiceInstanceUnitTests))]

    public class DnsDiscoveredServiceInstanceUnitTests : CoreTestCaseBase
    {
        private static readonly byte[] BufferRecordPtrServiceName = [0, 0, 132, 0, 0, 0, 0, 2, 0, 0, 0, 0, 9, 95, 115, 101, 114, 118, 105, 99, 101, 115, 7, 95, 100, 110, 115, 45, 115, 100, 4, 95, 117, 100, 112, 5, 108, 111, 99, 97, 108, 0, 0, 12, 0, 1, 0, 0, 17, 148, 0, 22, 14, 95, 114, 101, 109, 111, 116, 101, 112, 97, 105, 114, 105, 110, 103, 4, 95, 116, 99, 112, 192, 35, 192, 12, 0, 12, 0, 1, 0, 0, 17, 148, 0, 17, 14, 95, 97, 112, 112, 108, 101, 45, 109, 111, 98, 100, 101, 118, 50, 192, 67,];

        private static readonly byte[] BufferRecordPtrServiceInstance = [0, 0, 132, 0, 0, 0, 0, 1, 0, 0, 0, 5, 4, 95, 104, 97, 112, 4, 95, 116, 99, 112, 5, 108, 111, 99, 97, 108, 0, 0, 12, 0, 1, 0, 0, 17, 148, 0, 19, 16, 76, 71, 32, 119, 101, 98, 79, 83, 32, 84, 86, 32, 48, 52, 68, 69, 192, 12, 192, 39, 0, 16, 128, 1, 0, 0, 17, 148, 0, 75, 4, 99, 35, 61, 49, 4, 102, 102, 61, 56, 20, 105, 100, 61, 49, 67, 58, 53, 65, 58, 53, 54, 58, 54, 48, 58, 65, 51, 58, 67, 70, 8, 109, 100, 61, 87, 69, 66, 79, 83, 6, 112, 118, 61, 49, 46, 49, 4, 115, 35, 61, 49, 4, 115, 102, 61, 48, 5, 99, 105, 61, 51, 49, 11, 115, 104, 61, 99, 105, 89, 72, 82, 65, 61, 61, 192, 39, 0, 33, 128, 1, 0, 0, 0, 120, 0, 18, 0, 0, 0, 0, 135, 41, 9, 76, 71, 119, 101, 98, 79, 83, 84, 86, 192, 22, 192, 163, 0, 1, 128, 1, 0, 0, 0, 120, 0, 4, 10, 1, 10, 202, 192, 39, 0, 47, 128, 1, 0, 0, 17, 148, 0, 9, 192, 39, 0, 5, 0, 0, 128, 0, 64, 192, 163, 0, 47, 128, 1, 0, 0, 0, 120, 0, 5, 192, 163, 0, 1, 64,];

        private static readonly CoreIPEndPoint RemoteIPEndPoint = new(IPAddress.Loopback, CoreMulticastDnsConstants.MulticastDnsServerPort);

        private static readonly uint Ttl = 4500;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDiscoveredServiceInstanceUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DnsDiscoveredServiceInstanceUnitTests(CoreTestClassFixture testClassFixture)
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
        public void DnsDiscoveredServiceInstance_IsValidServiceInstance(string? serviceDomainNameString, CoreDomainNameType serviceDomainTypeTest)
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            ICoreDnsDiscoveredServiceInstance? dnsDiscoveredServiceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, serviceDomainNameString!, modifiedTimeStamp, Ttl, this.TestCaseLogger);

            if (serviceDomainTypeTest.HasFlag(CoreDomainNameType.ServiceInstance))
            {
                dnsDiscoveredServiceInstance.Should().NotBeNull();
                this.TestOutputHelper.WriteLine(dnsDiscoveredServiceInstance!.ToStringWithParentsPropNameMultiLine());

                dnsDiscoveredServiceInstance!.Logger.Should().BeSameAs(this.TestCaseLogger);
                dnsDiscoveredServiceInstance.ModifiedTimestamp.Should().Be(modifiedTimeStamp);
                dnsDiscoveredServiceInstance.TimeToLive.Should().BeLessThanOrEqualTo(Ttl);
                dnsDiscoveredServiceInstance.CreatedTimestamp.Should().Be(modifiedTimeStamp);
                dnsDiscoveredServiceInstance.ObjectCacheVersion.Should().Be(1);
                dnsDiscoveredServiceInstance.ServiceInstanceName.IsValidServiceInstance().Should().BeTrue();
                dnsDiscoveredServiceInstance.ServiceName.IsValidServiceName().Should().BeTrue();

                dnsDiscoveredServiceInstance.ServiceHostDomainNames.Count.Should().Be(0);
            }
            else
            {
                dnsDiscoveredServiceInstance.Should().BeNull();
            }
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_CreateServiceInstance_ResponseRecord_Null()
        {
            Func<ICoreDnsDiscoveredServiceInstance?> fx = () => CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, "Home Theater._airplay._tcp.local.", null!, this.TestCaseLogger);
            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_CreateServiceInstance_DnsRecordPtr_ServiceName()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceName, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger).Should().BeNull();
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_CreateServiceInstance_DnsRecordPtr_ServiceInstance()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            this.TestOutputHelper.WriteLine(serviceInstance.ToStringWithParentsPropName());

            serviceInstance!.Logger.Should().BeSameAs(this.TestCaseLogger);
            serviceInstance.ServiceHostDomainNames.Count.Should().Be(0);
            serviceInstance.ServiceName.Should().Be("_hap._tcp.local.");
            serviceInstance.ServiceInstanceName.Should().Be("LG webOS TV 04DE._hap._tcp.local.");
            serviceInstance.TimeToLive.Should().BeLessThanOrEqualTo(4500);
            serviceInstance.CreatedTimestamp.Should().Be(serviceInstance.ModifiedTimestamp);
            serviceInstance.FindTxtRecordItems().Should().HaveCount(0);
            serviceInstance.FindTxtRecordItem("foo").Should().BeNull();
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_CreateServiceInstance_TxtRecord()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            this.TestOutputHelper.WriteLine(serviceInstance.ToStringWithParentsPropName());

            serviceInstance!.Logger.Should().BeSameAs(this.TestCaseLogger);
            serviceInstance.ServiceHostDomainNames.Count.Should().Be(0);
            serviceInstance.ServiceName.Should().Be("_hap._tcp.local.");
            serviceInstance.ServiceInstanceName.Should().Be("LG webOS TV 04DE._hap._tcp.local.");
            serviceInstance.TimeToLive.Should().BeLessThanOrEqualTo(4500);
            serviceInstance.CreatedTimestamp.Should().Be(serviceInstance.ModifiedTimestamp);
            serviceInstance.FindTxtRecordItems().Should().HaveCount(0);
            serviceInstance.FindTxtRecordItem("foo").Should().BeNull();

            var recordTxt = dnsResponse.Additionals[0].DnsRecordBase as DnsRecordTxt;
            recordTxt.Should().NotBeNull();
            serviceInstance.AddPropertySet(recordTxt!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateTxtRecords);

            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine("Updated with TxtRecord".CenterTitle());
            this.TestOutputHelper.WriteLine(serviceInstance.ToStringWithParentsPropName());

            serviceInstance.AddPropertySet(recordTxt!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            serviceInstance.RemoveTxtRecordItem(serviceInstance.FindTxtRecordItems().ToList().FirstOrDefault().Key);

            serviceInstance.AddPropertySet(recordTxt!, true).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateTxtRecords | CoreDnsServiceDiscoveryEventOperationType.CacheFlush);
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_CreateServiceInstance_TxtRecord_AddItem()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            var recordTxt = dnsResponse.Additionals[0].DnsRecordBase as DnsRecordTxt;
            recordTxt.Should().NotBeNull();
            serviceInstance!.AddPropertySet(recordTxt!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateTxtRecords);

            serviceInstance.AddPropertySet(recordTxt!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            serviceInstance.RemoveTxtRecordItem(serviceInstance.FindTxtRecordItems().ToList().FirstOrDefault().Key);
            recordTxt!.Add("foo");
            recordTxt!.Add("bar=foo");

            serviceInstance.AddPropertySet(recordTxt!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateTxtRecords);
            serviceInstance.RemoveTxtRecordItem(serviceInstance.FindTxtRecordItems().ToList().FirstOrDefault().Key);

            serviceInstance.AddPropertySet(recordTxt!, true).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateTxtRecords | CoreDnsServiceDiscoveryEventOperationType.CacheFlush);

            this.TestOutputHelper.WriteLine("Updated TxtRecord".CenterTitle());
            this.TestOutputHelper.WriteLine(serviceInstance.ToStringWithParentsPropName());
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_CreateServiceInstance_TxtRecord_Null()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            Func<CoreDnsServiceDiscoveryEventOperationType> fx = () => serviceInstance!.AddPropertySet(null!, false);
            fx.Should().Throw<ArgumentNullException>();

            var recordTxt = dnsResponse.Additionals[0].DnsRecordBase as DnsRecordTxt;
            recordTxt.Should().NotBeNull();
            serviceInstance!.AddPropertySet(recordTxt!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateTxtRecords);

            recordTxt!.DnsResponseRecord = null;
            fx = () => serviceInstance!.AddPropertySet(recordTxt!, false);
            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_AddServiceHost()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            serviceInstance!.FindServiceHostDomainNames().Should().BeEmpty();
            serviceInstance.AddDiscoveredHost(CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger)!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.HostDomainNames);
            serviceInstance.FindServiceHostDomainNames().FirstOrDefault().Should().Be("MyComputer.Local");
            serviceInstance.AddDiscoveredHost(CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger)!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            serviceInstance.AddDiscoveredHost(CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger)!, true).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            serviceInstance.AddDiscoveredHost(CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyNewComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger)!, true).Should().Be(CoreDnsServiceDiscoveryEventOperationType.HostDomainNames | CoreDnsServiceDiscoveryEventOperationType.CacheFlush);

            serviceInstance.FindServiceHostDomainNames().Count().Should().Be(1);
            serviceInstance.FindServiceHostDomainNames().FirstOrDefault().Should().Be("MyNewComputer.Local");
            this.TestOutputHelper.WriteLine(serviceInstance.ToStringWithParentsPropName());
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_RemoveServiceHost()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            serviceInstance!.FindServiceHostDomainNames().Should().BeEmpty();
            ICoreDnsDiscoveredServiceHost? serviceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger);
            serviceInstance.AddDiscoveredHost(serviceHost!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.HostDomainNames);
            serviceInstance!.FindServiceHostDomainNames().FirstOrDefault().Should().BeSameAs("MyComputer.Local");
            serviceInstance.RemoveServiceHost(serviceHost!).Should().BeTrue();
            serviceInstance!.FindServiceHostDomainNames().Should().BeEmpty();
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_RemoveServiceHostDomainName()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            serviceInstance!.FindServiceHostDomainNames().Should().BeEmpty();
            ICoreDnsDiscoveredServiceHost? serviceHost = CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger);
            serviceInstance.AddDiscoveredHost(serviceHost!, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.HostDomainNames);
            serviceInstance!.FindServiceHostDomainNames().FirstOrDefault().Should().BeSameAs("MyComputer.Local");
            serviceInstance.RemoveServiceHostDomainName("mycomputer.local").Should().BeTrue();
            serviceInstance!.FindServiceHostDomainNames().Should().BeEmpty();
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_AddServiceHost_Null()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            serviceInstance!.FindServiceHostDomainNames().Should().BeEmpty();
            Func<CoreDnsServiceDiscoveryEventOperationType> fx = () => serviceInstance.AddDiscoveredHost(null!, false);
            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_CreateServiceInstance_DnsRecordPtr_Null()
        {
            Func<ICoreDnsDiscoveredServiceInstance?> fx = () =>
            {
                return CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, null!, this.TestCaseLogger);
            };

            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_UpdateFromServiceInstance_SameAs()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            serviceInstance!.UpdateFromServiceInstance(serviceInstance, null, false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            serviceInstance!.UpdateFromServiceInstance(serviceInstance, CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger), false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.HostDomainNames);
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_UpdateFromServiceInstance_UpdateTimeToLive()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance2 = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance2.Should().NotBeNull();

            serviceInstance!.UpdateFromServiceInstance(serviceInstance, CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger), false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.HostDomainNames);

            serviceInstance.UpdateFromServiceInstance(serviceInstance2!, CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger), false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.None);
            serviceInstance2!.TimeToLive += 1000;

            serviceInstance.UpdateFromServiceInstance(serviceInstance2!, CoreDnsDiscoveredServiceHost.CreateServiceHost(this.TestNetworkServicesHost.MulticastDnsBackgroundService, "MyComputer.Local", serviceInstance.ModifiedTimestamp, serviceInstance.TimeToLive, this.TestCaseLogger), false).Should().Be(CoreDnsServiceDiscoveryEventOperationType.UpdateExpire);
        }

        [Fact]
        public void DnsDiscoveredServiceInstance_UpdateFromServiceInstance_Null()
        {
            var dnsResponse = new DnsResponse(this.TestCaseServiceProvider, BufferRecordPtrServiceInstance, RemoteIPEndPoint, this.TestCaseLogger);
            dnsResponse.Should().NotBeNull();

            var recordPtr = dnsResponse.Answers[0].DnsRecordBase as DnsRecordPtr;
            recordPtr.Should().NotBeNull();

            ICoreDnsDiscoveredServiceInstance? serviceInstance = CoreDnsDiscoveredServiceInstance.CreateServiceInstance(this.TestCaseServiceProvider, recordPtr!, this.TestCaseLogger);
            serviceInstance.Should().NotBeNull();

            Func<CoreDnsServiceDiscoveryEventOperationType> fx = () => serviceInstance!.UpdateFromServiceInstance(null!, null, false);
            fx.Should().Throw<ArgumentNullException>();
        }
    }
}
