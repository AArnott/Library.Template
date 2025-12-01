// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreTestNetworkAgentBackgroundService.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.MulticastDns;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Services.Agent.Service;
using NetworkVisor.Core.Networking.Services.MulticastDns;
using NetworkVisor.Core.Networking.Services.MulticastDns.Events;
using NetworkVisor.Core.Networking.Services.MulticastDns.Request;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Networking.Services.MulticastDns.Service;
using NetworkVisor.Core.Test.TestCase;

namespace NetworkVisor.Platform.Test.TestServices
{
    /// <summary>
    /// Represents a specialized background service for testing network agents within the NetworkVisor platform.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="CoreNetworkAgentBackgroundService"/> and implements <see cref="ICoreTestNetworkAgentBackgroundService"/>.
    /// It provides additional functionality specific to test scenarios, such as handling test cases and sending or creating DNS responses.
    /// </remarks>
    public class CoreTestNetworkAgentBackgroundService : CoreNetworkAgentBackgroundService, ICoreTestNetworkAgentBackgroundService
    {
        private bool isDisposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestNetworkAgentBackgroundService"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> used to resolve dependencies required by the service.
        /// </param>
        /// <param name="testCase">
        /// The <see cref="ICoreTestCase"/> representing the test case associated with this service.
        /// </param>
        /// <param name="multicastDnsBackgroundService">
        /// The <see cref="ICoreMulticastDnsBackgroundService"/> responsible for handling multicast DNS operations.
        /// </param>
        /// <param name="taskCacheStateFlags">
        /// The initial state of the task cache, represented by <see cref="CoreTaskCacheStateFlags"/>.
        /// Defaults to <see cref="CoreTaskCacheStateFlags.NotInitialized"/>.
        /// </param>
        /// <param name="logger">
        /// An optional <see cref="ICoreTestCaseLogger"/> instance for logging test-specific information.
        /// </param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="testCase"/> is <see langword="null"/>.
        /// </exception>
        public CoreTestNetworkAgentBackgroundService(IServiceProvider serviceProvider, ICoreTestCase testCase, ICoreMulticastDnsBackgroundService multicastDnsBackgroundService, CoreTaskCacheStateFlags taskCacheStateFlags = CoreTaskCacheStateFlags.NotInitialized, ICoreTestCaseLogger? logger = null)
            : base(serviceProvider, multicastDnsBackgroundService, taskCacheStateFlags, logger)
        {
            this.TestCase = testCase ?? throw new ArgumentNullException(nameof(testCase));
        }

        /// <inheritdoc/>
        public ICoreTestCase TestCase { get; }

        public static ICoreTestNetworkAgentBackgroundService Create(ICoreTestCase testCase, CoreTaskCacheStateFlags taskCacheStateFlags)
        {
            CoreMulticastDnsBackgroundService? multicastDnsBackgroundService = new CoreMulticastDnsBackgroundService(testCase.TestCaseServiceProvider, testCase.TestNetworkServices, taskCacheStateFlags, testCase.TestCaseLogger);
            multicastDnsBackgroundService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMulticastDnsBackgroundService>();

            var networkAgentBackgroundService = new CoreTestNetworkAgentBackgroundService(testCase.TestCaseServiceProvider, testCase, multicastDnsBackgroundService!, taskCacheStateFlags, testCase.TestCaseLogger);
            networkAgentBackgroundService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestNetworkAgentBackgroundService>();

            return networkAgentBackgroundService;
        }

        /// <inheritdoc/>
        public override Task StartAsync(CancellationToken cancellationToken)
        {
            this.MulticastDnsBackgroundService!.OnDiscoveredServiceName += this.MulticastDnsBackgroundService_OnDiscoveredServiceName;
            this.MulticastDnsBackgroundService!.OnDiscoveredServiceInstance += this.CoreNetworkAgentBackgroundServiceIntegrationTests_OnDiscoveredServiceInstance;
            this.MulticastDnsBackgroundService.OnNetworkAgentDiscoveryQuery += this.NetworkAgentBackgroundServiceIntegrationTests_OnNetworkAgentDiscoveryQuery;
            this.MulticastDnsBackgroundService.OnDiscoveredNetworkAgent += this.MulticastDnsBackgroundService_OnDiscoveredNetworkAgent;

            return this.MulticastDnsBackgroundService.StartAsync(cancellationToken).ContinueWith(
                t =>
                {
                    this.MulticastDnsBackgroundService.IsRunning.Should().BeTrue();

                    return base.StartAsync(cancellationToken);
                },
                cancellationToken);
        }

        /// <inheritdoc/>
        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this.MulticastDnsBackgroundService!.OnDiscoveredServiceName -= this.MulticastDnsBackgroundService_OnDiscoveredServiceName;
            this.MulticastDnsBackgroundService!.OnDiscoveredServiceInstance -= this.CoreNetworkAgentBackgroundServiceIntegrationTests_OnDiscoveredServiceInstance;
            this.MulticastDnsBackgroundService.OnNetworkAgentDiscoveryQuery -= this.NetworkAgentBackgroundServiceIntegrationTests_OnNetworkAgentDiscoveryQuery;
            this.MulticastDnsBackgroundService.OnDiscoveredNetworkAgent -= this.MulticastDnsBackgroundService_OnDiscoveredNetworkAgent;

            return this.MulticastDnsBackgroundService.StopAsync(cancellationToken).ContinueWith(
                t =>
                {
                    this.MulticastDnsBackgroundService.IsRunning.Should().BeFalse();

                    return base.StopAsync(cancellationToken);
                },
                cancellationToken);
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

        protected override void MulticastDnsBackgroundService_OnDiscoveredNetworkAgent(object? sender, CoreDnsDiscoveredNetworkAgentEvent discoveredNetworkAgentEvent)
        {
            base.MulticastDnsBackgroundService_OnDiscoveredNetworkAgent(sender, discoveredNetworkAgentEvent);
            this.TestCase.TestOutputHelper.WriteLine($"Network Agent Discovered ({discoveredNetworkAgentEvent.DiscoveryEventOperationType}):{Environment.NewLine}{discoveredNetworkAgentEvent.ToStringWithParentsPropName()}{Environment.NewLine}");
        }

        protected virtual void CoreNetworkAgentBackgroundServiceIntegrationTests_OnDiscoveredServiceInstance(object? sender, CoreDnsDiscoveredServiceInstanceEvent? dnsDiscoveredServiceInstanceEvent)
        {
            if (dnsDiscoveredServiceInstanceEvent is not null && dnsDiscoveredServiceInstanceEvent.IsNetworkAgentEvent)
            {
                this.TestCase.TestOutputHelper.WriteLine($"Network Agent Instance:\n{dnsDiscoveredServiceInstanceEvent.ToStringWithParentsPropNameMultiLine()}\n");
            }
        }

        protected virtual void MulticastDnsBackgroundService_OnDiscoveredServiceName(object? sender, CoreDnsDiscoveredServiceNameEvent? dnsDiscoveredServiceNameEvent)
        {
            if (dnsDiscoveredServiceNameEvent is not null && dnsDiscoveredServiceNameEvent.DiscoveredServiceName.ServiceName.Equals(CoreNetworkAgentDnsProvider.NetworkAgentServiceName, StringComparison.InvariantCultureIgnoreCase))
            {
                this.TestCase.TestOutputHelper.WriteLine($"Discovered Network Agent:\n{dnsDiscoveredServiceNameEvent.ToStringWithParentsPropNameMultiLine()}\n");
            }
        }

        protected virtual void NetworkAgentBackgroundServiceIntegrationTests_OnNetworkAgentDiscoveryQuery(object? sender, CoreDnsNetworkAgentDiscoveryQueryEvent networkAgentDiscoveryQueryEvent)
        {
            this.TestCase.TestOutputHelper.WriteLine($"Network Agent Discovery Query ({networkAgentDiscoveryQueryEvent.DiscoveryEventOperationType}):{Environment.NewLine}{networkAgentDiscoveryQueryEvent.ToStringWithParentsPropName()}{Environment.NewLine}");
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                // Dispose the base class first.
                base.Dispose(disposing);
            }
            catch (Exception ex)
            {
                this.TestCase.TestCaseLogger.LogError(ex, "An error occurred while disposing the base class.");
            }
            finally
            {
                if (!this.isDisposed)
                {
                    // Dispose the MulticastDnsBackgroundService we created
                    this.MulticastDnsBackgroundService?.Dispose();
                    this.isDisposed = true;
                }
            }
        }
    }
}
