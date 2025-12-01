// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreMulticastDnsCacheIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Core MulticastDns Device Cache Integration Tests.</summary>

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Networking.Services.MulticastDns.Discovery;
using NetworkVisor.Core.Networking.Services.MulticastDns.Service;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.MulticastDns.Cache
{
    /// <summary>
    /// Class CoreMulticastDnsCacheIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreMulticastDnsCacheIntegrationTests))]

    public class CoreMulticastDnsCacheIntegrationTests : CoreTestCaseBase
    {
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreMulticastDnsCacheIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreMulticastDnsCacheIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void MulticastDnsCacheIntegration_NetworkServices_IsRunning()
        {
            this.TestNetworkServicesHost.IsRunning.Should().BeFalse();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestNetworkServices.IsRunningMulticastDnsService.Should().BeFalse();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.UPnP))
            {
                this.TestNetworkServices.IsRunningUPnPService.Should().BeFalse();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer))
            {
                this.TestNetworkServices.IsRunningDhcpServerService.Should().BeFalse();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient))
            {
                this.TestNetworkServices.IsRunningDhcpClientService.Should().BeFalse();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.UPnP))
            {
                this.TestNetworkServices.IsRunningUPnPService.Should().BeFalse();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.HangfireScheduler))
            {
                this.TestNetworkServices.IsRunningNetworkDeviceMonitoringService.Should().BeTrue();
            }

            this.TestNetworkServices.IsRunningNetworkAgentService.Should().BeFalse();

            // Force lazy initialization of CommandDispatchService
            this.TestNetworkServicesHost.CommandDispatchService.IsRunning.Should().BeTrue();

            this.TestNetworkServicesHost.Start();
            this.TestNetworkServicesHost.IsRunning.Should().BeTrue();
            this.TestNetworkServices.IsRunningNetworkAgentService.Should().BeTrue();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestNetworkServices.IsRunningMulticastDnsService.Should().BeTrue();
                ICoreMulticastDnsBackgroundService multicastDnsBackgroundService = this.TestNetworkServicesHost.MulticastDnsBackgroundService;

                multicastDnsBackgroundService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMulticastDnsBackgroundService>();
                multicastDnsBackgroundService.NetworkServices.Should().BeSameAs(this.TestNetworkServices);
                multicastDnsBackgroundService.NetworkServices.NetworkingSystem.Should().BeSameAs(this.TestNetworkServices.NetworkingSystem);

                multicastDnsBackgroundService.ServiceHostStatus.Should().Be(CoreServiceHostStatus.Started);
                multicastDnsBackgroundService.MulticastNetworkInterface.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkInterface>();
                multicastDnsBackgroundService.MulticastNetworkInterface.Should().Be(this.TestNetworkServices.PreferredNetwork.PreferredNetworkInterface!);

                multicastDnsBackgroundService.ActiveIPAddress.Should().Be(IPAddress.Any);
                multicastDnsBackgroundService.ActiveMulticastDnsServerIPEndPoint.Should().NotBeNull();
                multicastDnsBackgroundService.ActiveMulticastDnsServerIPEndPoint.Should().Be(new CoreIPEndPoint(IPAddress.Any, CoreMulticastDnsConstants.MulticastDnsServerPort));
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer))
            {
                this.TestNetworkServices.IsRunningDhcpServerService.Should().BeTrue();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient))
            {
                this.TestNetworkServices.IsRunningDhcpClientService.Should().BeTrue();
            }

            this.TestNetworkServices.MulticastDnsDeviceCache.Should().NotBeNull();
        }

        [Fact]
        public void MulticastDnsCacheIntegration_NetworkServices_IsRunning_Delay()
        {
            this.TestNetworkServicesHost.IsRunning.Should().BeFalse();
            this.TestNetworkServices.IsRunningNetworkAgentService.Should().BeFalse();

            // Force lazy initialization of CommandDispatchService
            this.TestNetworkServicesHost.CommandDispatchService.IsRunning.Should().BeTrue();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.UPnP))
            {
                this.TestNetworkServices.IsRunningUPnPService.Should().BeFalse();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestNetworkServices.IsRunningMulticastDnsService.Should().BeFalse();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer))
            {
                this.TestNetworkServices.IsRunningDhcpServerService.Should().BeFalse();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient))
            {
                this.TestNetworkServices.IsRunningDhcpClientService.Should().BeFalse();
            }

            this.TestNetworkServicesHost.Start();

            this.TestNetworkServicesHost.IsRunning.Should().BeTrue();
            this.TestNetworkServices.IsRunningNetworkAgentService.Should().BeTrue();

            this.TestNetworkServices.MulticastDnsDeviceCache.Should().NotBeNull();

            // Wait for 2 seconds
            this.TestDelay(2000, this.TestCaseLogger).Should().BeTrue();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.UPnP))
            {
                this.TestNetworkServices.IsRunningUPnPService.Should().BeTrue();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestNetworkServices.IsRunningMulticastDnsService.Should().BeTrue();
                ICoreMulticastDnsBackgroundService multicastDnsBackgroundService = this.TestNetworkServicesHost.MulticastDnsBackgroundService;

                multicastDnsBackgroundService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMulticastDnsBackgroundService>();
                this.TestNetworkingSystem.PreferredLocalNetworkAddress?.IPAddress.IsNullNoneAnyOrLoopback().Should().BeFalse();

                IList<ICoreDnsDiscoveryHost> discoverHosts = multicastDnsBackgroundService.FindDiscoveryHosts().ToList();

                discoverHosts.Should().NotBeEmpty();

                this.TestOutputHelper.WriteLine($"PreferredLocalNetworkAddress:\n{this.TestNetworkingSystem.PreferredLocalNetworkAddress.ToStringWithParentsPropNameMultiLine()}\n");

                // Output Discovery Hosts
                foreach (ICoreDnsDiscoveryHost dh in discoverHosts)
                {
                    this.TestOutputHelper.WriteLine(dh.ToStringWithParentsPropNameMultiLine());
                }

                discoverHosts.FirstOrDefault(dh => dh.RemoteIPEndPoint.Address.Equals(this.TestNetworkingSystem.PreferredLocalNetworkAddress!.IPAddress)).Should().NotBeNull();
                multicastDnsBackgroundService.FindDiscoveredServiceNames().Count().Should().BeGreaterThanOrEqualTo(0);
                multicastDnsBackgroundService.FindDiscoveredServiceInstances().Count().Should().BeGreaterThanOrEqualTo(0);
                multicastDnsBackgroundService.FindDiscoveredServiceHosts().Count().Should().BeGreaterThanOrEqualTo(0);
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpServer))
            {
                this.TestNetworkServices.IsRunningDhcpServerService.Should().BeTrue();
            }

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.DhcpClient))
            {
                this.TestNetworkServices.IsRunningDhcpClientService.Should().BeTrue();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (disposing)
                    {
                        this.TestNetworkServicesHost.Stop();
                    }
                }
                finally
                {
                    this._disposed = true;
                }
            }

            base.Dispose(disposing);
        }
    }
}
