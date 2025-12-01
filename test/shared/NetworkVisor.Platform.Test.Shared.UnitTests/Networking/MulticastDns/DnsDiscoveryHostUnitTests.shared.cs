// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="DnsDiscoveryHostUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.MulticastDns.Discovery;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.MulticastDns
{
    /// <summary>
    /// Class DnsDiscoveryHostUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DnsDiscoveryHostUnitTests))]

    public class DnsDiscoveryHostUnitTests : CoreTestCaseBase
    {
        private static readonly CoreIPEndPoint RemoteIPEndPoint = new(IPAddress.Loopback, CoreMulticastDnsConstants.MulticastDnsServerPort);
        private static readonly CoreIPEndPoint RemoteIPEndPoint1 = new(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreMulticastDnsConstants.MulticastDnsServerPort);

        private static readonly uint Ttl = 4500;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsDiscoveryHostUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DnsDiscoveryHostUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void DnsDiscoveryHost_Ctor()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsDiscoveryHost = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);

            dnsDiscoveryHost.Should().NotBeNull();
            dnsDiscoveryHost.Logger.Should().BeSameAs(this.TestCaseLogger);
            dnsDiscoveryHost.RemoteIPEndPoint.Should().BeSameAs(RemoteIPEndPoint);
            dnsDiscoveryHost.ModifiedTimestamp.Should().Be(modifiedTimeStamp);

            // Wait for 1.1 seconds before checking TimeToLive
            this.TestDelay(1100, this.TestCaseLogger).Should().BeTrue();
            dnsDiscoveryHost.TimeToLive.Should().BeLessThan(Ttl);
            dnsDiscoveryHost.CreatedTimestamp.Should().Be(modifiedTimeStamp);
            dnsDiscoveryHost.DiscoveryQueryCount.Should().Be(1);
            dnsDiscoveryHost.ObjectCacheVersion.Should().Be(dnsDiscoveryHost.DiscoveryQueryCount);
            this.TestOutputHelper.WriteLine(dnsDiscoveryHost.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void DnsDiscoveryHost_CompareTo_Equals()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsDiscoveryHost1 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);
            var dnsDiscoveryHost2 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);

            dnsDiscoveryHost1.CompareTo(dnsDiscoveryHost2).Should().Be(0);
            dnsDiscoveryHost2.CompareTo(dnsDiscoveryHost1).Should().Be(0);
        }

        [Fact]
        public void DnsDiscoveryHost_CompareTo_NotEquals_RemoteIPEndPoint()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsDiscoveryHost1 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);
            var dnsDiscoveryHost2 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint1, modifiedTimeStamp, Ttl, this.TestCaseLogger);

            dnsDiscoveryHost1.CompareTo(dnsDiscoveryHost2).Should().BeGreaterThan(0);
            dnsDiscoveryHost2.CompareTo(dnsDiscoveryHost1).Should().BeLessThan(0);
        }

        [Fact]
        public void DnsDiscoveryHost_CompareTo_NotEquals_ModifiedTimeStamp()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsDiscoveryHost1 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);
            var dnsDiscoveryHost2 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp.AddSeconds(-1), Ttl, this.TestCaseLogger);

            // Ignore ModifiedTimeStamp when comparing.  We update ModifiedTimestamp, ObjectCacheVersion and TTL on update.
            dnsDiscoveryHost1.CompareTo(dnsDiscoveryHost2).Should().Be(0);
            dnsDiscoveryHost2.CompareTo(dnsDiscoveryHost1).Should().Be(0);
        }

        [Fact]
        public void DnsDiscoveryHost_CompareTo_NotEquals_TTL()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsDiscoveryHost1 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);
            var dnsDiscoveryHost2 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl + 1, this.TestCaseLogger);

            // Ignore TTL when comparing.  We update ModifiedTimestamp, ObjectCacheVersion and TTL on update.
            dnsDiscoveryHost1.CompareTo(dnsDiscoveryHost2).Should().Be(0);
            dnsDiscoveryHost2.CompareTo(dnsDiscoveryHost1).Should().Be(0);
        }

        [Fact]
        public void DnsDiscoveryHost_CompareTo_NotEquals_DiscoveryQueryCount()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsDiscoveryHost1 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);
            var dnsDiscoveryHost2 = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);
            dnsDiscoveryHost2.ObjectCacheVersion += 1;
            dnsDiscoveryHost2.DiscoveryQueryCount += 1;
            dnsDiscoveryHost2.IncrementDiscoveryQueryCount();

            dnsDiscoveryHost2.DiscoveryQueryCount.Should().Be(4);

            // Ignore ObjectCacheVersion when comparing.  We update ModifiedTimestamp, ObjectCacheVersion and TTL on an update.
            dnsDiscoveryHost1.CompareTo(dnsDiscoveryHost2).Should().Be(0);
            dnsDiscoveryHost2.CompareTo(dnsDiscoveryHost1).Should().Be(0);
        }

        [Fact]
        public void DnsDiscoveryHost_UpdateDiscoveryHost()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsDiscoveryHost = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);
            var ttlSave = dnsDiscoveryHost.TimeToLive;
            dnsDiscoveryHost.UpdateDiscoveryHost(modifiedTimeStamp.AddSeconds(1), Ttl - 10);
            dnsDiscoveryHost.ModifiedTimestamp.Should().Be(modifiedTimeStamp.AddSeconds(1));
            dnsDiscoveryHost.TimeToLive.Should().BeLessThan(ttlSave);
            dnsDiscoveryHost.DiscoveryQueryCount.Should().Be(2);
        }

        [Fact]
        public void DnsDiscoveryHost_CompareTo_Equals_SameAs()
        {
            DateTimeOffset modifiedTimeStamp = DateTimeOffset.UtcNow;
            var dnsDiscoveryHost = new CoreDnsDiscoveryHost(this.TestCaseServiceProvider, RemoteIPEndPoint, modifiedTimeStamp, Ttl, this.TestCaseLogger);

            dnsDiscoveryHost.CompareTo(dnsDiscoveryHost).Should().Be(0);
            dnsDiscoveryHost.Should().BeSameAs(dnsDiscoveryHost);
        }
    }
}
