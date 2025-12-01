// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="CoreDnsResolverIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Dns DnsResolver Integration Tests.</summary>

using System.Collections.Immutable;
using System.Net;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.MulticastDns.Resolver;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Dns
{
    /// <summary>
    /// Class CoreDnsResolverIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreDnsResolverIntegrationTests))]

    public class CoreDnsResolverIntegrationTests : CoreTestCaseBase
    {
        private readonly Lazy<DnsResolver?> dnsResolverLazy;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreDnsResolverIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreDnsResolverIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.dnsResolverLazy = new Lazy<DnsResolver?>(() => new DnsResolver(this.TestCaseServiceProvider, this.TestNetworkingSystem));
        }

        /// <summary>
        /// Gets the DnsResolver.
        /// </summary>
        public DnsResolver? DnsResolver => this.dnsResolverLazy.Value;

        [Fact]
        public void DnsResolverIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void DnsResolverIntegration_PreferredDnsServers_Output()
        {
            this.TestNetworkServices.PreferredNetwork.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetwork>();
            this.TestNetworkServices.PreferredNetwork.PreferredDnsServerAddresses.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Preferred Dns Servers: {string.Join(", ", this.TestNetworkServices.PreferredNetwork.PreferredDnsServerAddresses)}");
        }

        [Fact]
        public void DnsResolverIntegration_PreferredScoredDnsServers_Output()
        {
            this.TestNetworkServices.PreferredNetwork.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetwork>();
            this.TestNetworkServices.PreferredNetwork.PreferredScoredDnsServerAddresses.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Preferred Dns Servers: {string.Join(", ", this.TestNetworkServices.PreferredNetwork.PreferredScoredDnsServerAddresses)}");
        }

        [Fact]
        public void DnsResolverIntegration_DnsServers_Count()
        {
            this.DnsResolver.Should().NotBeNull();
            IList<CoreIPEndPoint> dnsServers = this.DnsResolver!.DnsServers.ToList();
            dnsServers.Count.Should().BeGreaterThan(0);
            this.TestOutputHelper.WriteLine($"Active Dns Servers: {dnsServers.Count}");
        }

        [Fact]
        public void DnsResolverIntegration_DnsServers_Output()
        {
            this.OutputDnsServers();
        }

        [Fact]
        public void DnsResolverIntegration_DnsServer_NotEmptyOrNull()
        {
            this.DnsResolver.Should().NotBeNull();
            this.DnsResolver!.DnsServer.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void DnsResolverIntegration_GetHostByName_GoogleDns()
        {
            this.DnsResolver.Should().NotBeNull();

            IPHostEntry ipHostEntry = this.DnsResolver!.GetHostByName(CoreIPAddressExtensions.StringGooglePublicDnsServer);
            ipHostEntry.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"{ipHostEntry.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine, LogLevel.Trace)}");

            ipHostEntry.AddressList.Should().Contain(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
        }

        [Fact]
        public void DnsResolverIntegration_GetHostByAddress_GoogleDnsIP()
        {
            this.DnsResolver.Should().NotBeNull();

            IPHostEntry ipHostEntry = this.DnsResolver!.GetHostByAddress(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address2);
            ipHostEntry.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"{ipHostEntry.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine, LogLevel.Trace)}");

            ipHostEntry.AddressList.Should().Contain(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address2);
        }

        [Fact]
        public void DnsResolverIntegration_GetHostByName_CName()
        {
            this.DnsResolver.Should().NotBeNull();

            IPHostEntry ipHostEntry = this.DnsResolver!.GetHostByName("www.networkvisor.com");
            ipHostEntry.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"{ipHostEntry.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine, LogLevel.Trace)}");
            ipHostEntry.HostName.Should().Be("networkvisor.com.");
            ipHostEntry.Aliases.Should().Contain("www.networkvisor.com.");
            ipHostEntry.AddressList.Should().Contain(CoreIPAddressExtensions.NetworkVisorComAddress);
        }

        [Fact]
        public async Task DnsResolverIntegration_OS_GetHostByName_CNameAsync()
        {
            this.DnsResolver.Should().NotBeNull();

            IPHostEntry? ipHostEntry = await this.TestNetworkingSystem.GetDnsHostEntryAsync("www.networkvisor.com", CancellationToken.None);
            ipHostEntry.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"{ipHostEntry.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine, LogLevel.Trace)}");
            ipHostEntry!.HostName.Should().Be("networkvisor.com");

            // ipHostEntry.Aliases.Should().Contain("www.networkvisor.com");
            ipHostEntry.AddressList.Should().Contain(CoreIPAddressExtensions.NetworkVisorComAddress);
        }

        private void OutputDnsServers()
        {
            this.DnsResolver.Should().NotBeNull();
            this.DnsResolver!.DnsServers.Any().Should().BeTrue();

            this.TestOutputHelper.WriteLine("Active Dns Servers".CenterTitle());

            foreach (CoreIPEndPoint dnsServer in this.DnsResolver.DnsServers)
            {
                this.TestOutputHelper.WriteLine(dnsServer.ToString());
            }
        }
    }
}
