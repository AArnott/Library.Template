// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="CoreNetworkAgentCacheIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using Microsoft.Extensions.Options;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Services.Agent.Service;
using NetworkVisor.Core.Networking.Services.MulticastDns;
using NetworkVisor.Core.Networking.Services.MulticastDns.Events;
using NetworkVisor.Core.Networking.Services.MulticastDns.Request;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Networking.Services.MulticastDns.Service;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.Services.Agent.Cache;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Agent
{
    /// <summary>
    /// Class CoreNetworkAgentCacheIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkAgentCacheIntegrationTests))]

    public class CoreNetworkAgentCacheIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkAgentCacheIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkAgentCacheIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreNetworkAgentCacheIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public async Task CoreNetworkAgentCacheIntegration_StartStop()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var cts = new CancellationTokenSource();

            using ICoreNetworkAgentCache? networkAgentCacheAsync = await this.StartNetworkAgentCacheAsync(this.TestCaseLogger, cts, TimeSpan.FromSeconds(10), true, CoreTaskCacheStateFlags.Current);

            networkAgentCacheAsync.Should().NotBeNull();

            while (!cts.IsCancellationRequested)
            {
                this.TestDelay(1000, this.TestCaseLogger, cts.Token);
            }

            this.StopNetworkAgentCacheAsync(networkAgentCacheAsync!, true);

            this.TestOutputHelper.WriteLine("Discovered Network Agents".CenterTitle());
            foreach (ICoreRemoteNetworkAgentDevice? remoteNetworkAgentDevice in networkAgentCacheAsync!.FindAllItems())
            {
                this.TestOutputHelper.WriteLine($"{remoteNetworkAgentDevice?.ToStringWithParentsPropNameMultiLine()}\n");
            }
        }

        private async Task<CoreMulticastDnsBackgroundService?> StartMulticastDnsBackgroundServiceAsync(ICoreTestCaseLogger testCaseLogger, CancellationTokenSource cts, TimeSpan timeout, bool hookEvents, CoreTaskCacheStateFlags taskCacheStateFlags)
        {
            var multicastDnsBackgroundService = new CoreMulticastDnsBackgroundService(this.TestCaseServiceProvider, this.TestNetworkServices, taskCacheStateFlags, this.TestCaseLogger);

            if (hookEvents)
            {
            }

            await multicastDnsBackgroundService.StartAsync(cts.Token);
            cts.CancelAfter(timeout);
            multicastDnsBackgroundService.IsRunning.Should().BeTrue();

            return multicastDnsBackgroundService;
        }

        private bool StopMulticastDnsBackgroundService(CoreMulticastDnsBackgroundService multicastDnsBackgroundService, CancellationTokenSource cts, bool unhookEvents)
        {
            multicastDnsBackgroundService.Stop();
            multicastDnsBackgroundService.IsRunning.Should().BeFalse();

            if (unhookEvents)
            {
            }

            return multicastDnsBackgroundService.IsRunning;
        }

        private TestCoreNetworkAgentBackgroundService CreateTestNetworkAgentBackgroundService(CoreTaskCacheStateFlags taskCacheStateFlags, ICoreTestCaseLogger testCaseLogger)
        {
            var multicastDnsBackgroundService = new CoreMulticastDnsBackgroundService(this.TestCaseServiceProvider, this.TestNetworkServices, taskCacheStateFlags, this.TestCaseLogger);

            multicastDnsBackgroundService.Should().NotBeNull();

            var networkAgentBackgroundService = new TestCoreNetworkAgentBackgroundService(this.TestCaseServiceProvider, multicastDnsBackgroundService!, taskCacheStateFlags, this.TestCaseLogger);
            networkAgentBackgroundService.Should().NotBeNull();

            return networkAgentBackgroundService;
        }

        private async Task<ICoreNetworkAgentCache?> StartNetworkAgentCacheAsync(ICoreTestCaseLogger testCaseLogger, CancellationTokenSource cts, TimeSpan timeout, bool hookEvents, CoreTaskCacheStateFlags taskCacheStateFlags)
        {
            CoreMulticastDnsBackgroundService? multicastDnsBackgroundService = await this.StartMulticastDnsBackgroundServiceAsync(testCaseLogger, cts, timeout, hookEvents, CoreTaskCacheStateFlags.NotInitialized);
            multicastDnsBackgroundService.Should().NotBeNull();

            var networkAgentBackgroundService = new TestCoreNetworkAgentBackgroundService(this.TestCaseServiceProvider, multicastDnsBackgroundService!, taskCacheStateFlags, this.TestCaseLogger);
            networkAgentBackgroundService.Should().NotBeNull();

            if (hookEvents)
            {
                multicastDnsBackgroundService!.OnDiscoveredServiceName += this.MulticastDnsBackgroundService_OnDiscoveredServiceName;
                multicastDnsBackgroundService!.OnDiscoveredServiceInstance += this.CoreNetworkAgentBackgroundServiceIntegrationTests_OnDiscoveredServiceInstance;
                multicastDnsBackgroundService.OnNetworkAgentDiscoveryQuery += this.NetworkAgentBackgroundServiceIntegrationTests_OnNetworkAgentDiscoveryQuery;
                multicastDnsBackgroundService.OnDiscoveredNetworkAgent += this.MulticastDnsBackgroundService_OnDiscoveredNetworkAgent;
            }

            await networkAgentBackgroundService.StartAsync(cts.Token);
            cts.CancelAfter(timeout);
            networkAgentBackgroundService.IsRunning.Should().BeTrue();

            return new TestCoreNetworkAgentCache(networkAgentBackgroundService, taskCacheStateFlags);
        }

        private void CoreNetworkAgentBackgroundServiceIntegrationTests_OnDiscoveredServiceInstance(object? sender, CoreDnsDiscoveredServiceInstanceEvent? dnsDiscoveredServiceInstanceEvent)
        {
            if (dnsDiscoveredServiceInstanceEvent is not null && dnsDiscoveredServiceInstanceEvent.IsNetworkAgentEvent)
            {
                this.TestOutputHelper.WriteLine($"Network Agent Instance:\n{dnsDiscoveredServiceInstanceEvent.ToStringWithParentsPropNameMultiLine()}\n");
            }
        }

        private void MulticastDnsBackgroundService_OnDiscoveredServiceName(object? sender, CoreDnsDiscoveredServiceNameEvent? dnsDiscoveredServiceNameEvent)
        {
            if (dnsDiscoveredServiceNameEvent is not null && dnsDiscoveredServiceNameEvent.DiscoveredServiceName.ServiceName.Equals(CoreNetworkAgentDnsProvider.NetworkAgentServiceName, StringComparison.InvariantCultureIgnoreCase))
            {
                this.TestOutputHelper.WriteLine($"Discovered Network Agent:\n{dnsDiscoveredServiceNameEvent.ToStringWithParentsPropNameMultiLine()}\n");
            }
        }

        private bool StopNetworkAgentCacheAsync(ICoreNetworkAgentCache networkAgentCache, bool unhookEvents)
        {
            var testNetworkAgentCache = networkAgentCache as TestCoreNetworkAgentCache;

            if ((networkAgentCache.NetworkServices as ICoreNetworkServicesHost)?.NetworkAgentBackgroundService is TestCoreNetworkAgentBackgroundService networkAgentBackgroundService)
            {
                networkAgentBackgroundService.MulticastDnsBackgroundService.Stop();
                networkAgentBackgroundService.MulticastDnsBackgroundService.IsRunning.Should().BeFalse();
                networkAgentBackgroundService.Stop();
                networkAgentBackgroundService.IsRunning.Should().BeFalse();

                if (unhookEvents)
                {
                    networkAgentBackgroundService.MulticastDnsBackgroundService.OnNetworkAgentDiscoveryQuery -= this.NetworkAgentBackgroundServiceIntegrationTests_OnNetworkAgentDiscoveryQuery;
                    networkAgentBackgroundService.MulticastDnsBackgroundService.OnDiscoveredNetworkAgent -= this.MulticastDnsBackgroundService_OnDiscoveredNetworkAgent;
                }

                var isRunning = networkAgentBackgroundService.IsRunning;
                networkAgentBackgroundService?.Dispose();
                return isRunning;
            }

            return false;
        }

        private void MulticastDnsBackgroundService_OnDiscoveredNetworkAgent(object? sender, CoreDnsDiscoveredNetworkAgentEvent discoveredNetworkAgentEvent)
        {
            this.TestOutputHelper.WriteLine($"Network Agent Discovered ({discoveredNetworkAgentEvent.DiscoveryEventOperationType}):{Environment.NewLine}{discoveredNetworkAgentEvent.ToStringWithParentsPropName()}{Environment.NewLine}");
        }

        private void NetworkAgentBackgroundServiceIntegrationTests_OnNetworkAgentDiscoveryQuery(object? sender, CoreDnsNetworkAgentDiscoveryQueryEvent networkAgentDiscoveryQueryEvent)
        {
            this.TestOutputHelper.WriteLine($"Network Agent Discovery Query ({networkAgentDiscoveryQueryEvent.DiscoveryEventOperationType}):{Environment.NewLine}{networkAgentDiscoveryQueryEvent.ToStringWithParentsPropName()}{Environment.NewLine}");
        }

        private class TestCoreNetworkAgentCache : CoreNetworkAgentCache
        {
            public TestCoreNetworkAgentCache(ICoreNetworkAgentBackgroundService networkAgentBackgroundService, TimeSpan? cacheTimeout, CoreTaskCacheStateFlags taskCacheStateFlags = CoreTaskCacheStateFlags.NotInitialized)
                : base(networkAgentBackgroundService, cacheTimeout, taskCacheStateFlags)
            {
            }

            public TestCoreNetworkAgentCache(ICoreNetworkAgentBackgroundService networkAgentBackgroundService, CoreTaskCacheStateFlags taskCacheStateFlags = CoreTaskCacheStateFlags.NotInitialized)
                : base(networkAgentBackgroundService, taskCacheStateFlags)
            {
            }
        }

        private class TestCoreNetworkAgentBackgroundService : CoreNetworkAgentBackgroundService
        {
            public TestCoreNetworkAgentBackgroundService(IServiceProvider serviceProvider, ICoreMulticastDnsBackgroundService multicastDnsBackgroundService, CoreTaskCacheStateFlags taskCacheStateFlags = CoreTaskCacheStateFlags.NotInitialized, ICoreLogger? logger = null)
                : base(serviceProvider, multicastDnsBackgroundService, taskCacheStateFlags, logger)
            {
            }

            /// <summary>
            /// Test version of SendNetworkAgentDnsResponse.
            /// </summary>
            /// <param name="requestQuestion"></param>
            /// <param name="remoteIPEndPoint">Remote IPEndPoint.</param>
            /// <param name="ctx">Optional cancellation token.</param>
            public Task<int> TestSendNetworkAgentDnsResponseAsync(DnsRequestQuestion requestQuestion, CoreIPEndPoint remoteIPEndPoint, CancellationToken ctx = default)
            {
                return this.SendNetworkAgentDnsResponseAsync(requestQuestion, remoteIPEndPoint, ctx);
            }

            /// <summary>
            /// Test version of CreateNetworkAgentDnsResponse.
            /// </summary>
            /// <param name="requestQuestion"></param>
            /// <param name="remoteIPEndPoint">Remote IPEndPoint.</param>
            public DnsResponse? TestCreateNetworkAgentDnsResponse(DnsRequestQuestion requestQuestion, CoreIPEndPoint remoteIPEndPoint)
            {
                return this.CreateNetworkAgentDnsResponse(requestQuestion, remoteIPEndPoint);
            }
        }
    }
}
