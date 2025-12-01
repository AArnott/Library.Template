// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="CoreMulticastDnsBackgroundServiceIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Collections.Immutable;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Entities.Services.Properties;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Metadata.Extensions;
using NetworkVisor.Core.Networking.MulticastDns.Discovery;
using NetworkVisor.Core.Networking.Services.MulticastDns.Discovery;
using NetworkVisor.Core.Networking.Services.MulticastDns.Events;
using NetworkVisor.Core.Networking.Services.MulticastDns.Records;
using NetworkVisor.Core.Networking.Services.MulticastDns.Request;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Networking.Services.MulticastDns.Service;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.MulticastDns.Service
{
    /// <summary>
    /// Class CoreMulticastDnsBackgroundServiceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreMulticastDnsBackgroundServiceIntegrationTests))]

    public class CoreMulticastDnsBackgroundServiceIntegrationTests : CoreEntityTestCaseBase
    {
        private static readonly DnsRequestQuestionType[] KnownDnsQuestionRecordTypes = [DnsRequestQuestionType.A, DnsRequestQuestionType.AAAA, DnsRequestQuestionType.PTR, DnsRequestQuestionType.SRV, DnsRequestQuestionType.TXT, DnsRequestQuestionType.NS, DnsRequestQuestionType.ANY];
        private static readonly DnsRecordType[] KnownDnsAnswerRecordTypes = [DnsRecordType.A, DnsRecordType.AAAA, DnsRecordType.PTR, DnsRecordType.SRV, DnsRecordType.TXT, DnsRecordType.NS];
        private static readonly DnsRecordType[] KnownDnsResponseRecordTypes = [DnsRecordType.A, DnsRecordType.AAAA, DnsRecordType.PTR, DnsRecordType.SRV, DnsRecordType.TXT, DnsRecordType.NS, DnsRecordType.OPT, DnsRecordType.NSEC];

        private readonly int _defaultTimeoutNoActivityMilliseconds = 2000;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreMulticastDnsBackgroundServiceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreMulticastDnsBackgroundServiceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void MulticastDnsBackgroundServiceIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public async Task MulticastDnsBackgroundServiceIntegration_StartStop()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            // using IDisposable globalLogLevel = this.BeginGlobalLogScope(LogLevel.Trace);
            var cts = new CancellationTokenSource();

            using CoreMulticastDnsBackgroundService? multicastDnsBackgroundService = await this.StartMulticastDnsBackgroundServiceAsync(cts, TimeSpan.FromSeconds(30), true, CoreTaskCacheStateFlags.NotInitialized);
            multicastDnsBackgroundService.Should().NotBeNull();

            // Wait for 15 seconds, 5 seconds in CI.
            this.TestDelay(TimeSpan.FromSeconds(CoreAppConstants.IsRunningInCI ? 5 : 15), this.TestCaseLogger, cts.Token);

            this.StopMulticastDnsBackgroundService(multicastDnsBackgroundService!, true);
        }

        [Fact]
        public async Task MulticastDnsBackgroundServiceIntegration_WaitToDiscoverAllServiceNamesAsync()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var cts = new CancellationTokenSource();

            // using IDisposable globalLogLevel = this.BeginGlobalLogScope(LogLevel.Trace);
            using CoreMulticastDnsBackgroundService? multicastDnsBackgroundService = await this.StartMulticastDnsBackgroundServiceAsync(cts, TimeSpan.FromSeconds(30), true, CoreTaskCacheStateFlags.Current);

            multicastDnsBackgroundService.Should().NotBeNull();

            ISet<ICoreDnsDiscoveredServiceName> discoveredServiceNames =
                await multicastDnsBackgroundService!.WaitToDiscoverAllServiceNamesAsync(this._defaultTimeoutNoActivityMilliseconds, CoreTaskCacheLookupFlags.RefreshCacheBeforeLookup, TimeSpan.FromSeconds(10), cts.Token);

            this.StopMulticastDnsBackgroundService(multicastDnsBackgroundService, true);

            discoveredServiceNames.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Services Count: {discoveredServiceNames.Count}");
            foreach (ICoreDnsDiscoveredServiceName? discoveredServiceName in discoveredServiceNames)
            {
                this.TestOutputHelper.WriteLine(discoveredServiceName.ServiceName);
            }
        }

        [Fact]
        public async Task MulticastDnsBackgroundServiceIntegration_WaitToDiscoverServiceNamesAsync()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var cts = new CancellationTokenSource();

            // using IDisposable globalLogLevel = this.BeginGlobalLogScope(LogLevel.Trace);
            using CoreMulticastDnsBackgroundService? multicastDnsBackgroundService = await this.StartMulticastDnsBackgroundServiceAsync(cts, TimeSpan.FromSeconds(15), true, CoreTaskCacheStateFlags.Current);

            multicastDnsBackgroundService.Should().NotBeNull();

            var services = new HashSet<string>(["_smb._tcp.local.", "_http._tcp.local.", "_unknown-service._udp.local."]).ToImmutableHashSet();

            ISet<ICoreDnsDiscoveredServiceName> discoveredServiceNames =
                await multicastDnsBackgroundService!.WaitToDiscoverServiceNamesAsync(services, this._defaultTimeoutNoActivityMilliseconds, DnsRequestQuestionType.PTR, CoreTaskCacheLookupFlags.RefreshCacheBeforeLookup, TimeSpan.FromSeconds(5), cts.Token);

            this.StopMulticastDnsBackgroundService(multicastDnsBackgroundService, true);

            var serviceNamesResult = discoveredServiceNames.Select(dsn => dsn.ServiceName).ToArray();

            serviceNamesResult.Should().BeSubsetOf(services);
            serviceNamesResult.Should().NotContain("_unknown-service._udp.local.");

            this.TestOutputHelper.WriteLine($"Services Count: {discoveredServiceNames.Count}");
            foreach (ICoreDnsDiscoveredServiceName? discoveredServiceName in discoveredServiceNames)
            {
                this.TestOutputHelper.WriteLine(discoveredServiceName.ServiceName);
            }
        }

        [Fact]
        public async Task MulticastDnsBackgroundServiceIntegration_WaitToResolveAllServiceInstancesAsync()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var cts = new CancellationTokenSource();

            // using IDisposable globalLogLevel = this.BeginGlobalLogScope(LogLevel.Trace);
            using CoreMulticastDnsBackgroundService? multicastDnsBackgroundService = await this.StartMulticastDnsBackgroundServiceAsync(cts, TimeSpan.FromSeconds(30), true, CoreTaskCacheStateFlags.Current);

            multicastDnsBackgroundService.Should().NotBeNull();

            ILookup<string, ICoreDnsDiscoveredServiceInstance> discoveredServiceInstances =
                await multicastDnsBackgroundService!.WaitToResolveAllServiceInstancesAsync(this._defaultTimeoutNoActivityMilliseconds, CoreTaskCacheLookupFlags.RefreshCacheBeforeLookup, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(15), cts.Token);

            this.StopMulticastDnsBackgroundService(multicastDnsBackgroundService, true);

            this.TestOutputHelper.WriteLine($"Services Count: {discoveredServiceInstances.Count}");
            this.TestOutputHelper.WriteLine("********************************");
            foreach (IGrouping<string, ICoreDnsDiscoveredServiceInstance> serviceInstanceGroup in discoveredServiceInstances)
            {
                this.TestOutputHelper.WriteLine(serviceInstanceGroup.Key);
                this.TestOutputHelper.WriteLine("********************************");

                foreach (ICoreDnsDiscoveredServiceInstance serviceInstance in serviceInstanceGroup)
                {
                    this.TestOutputHelper.WriteLine(serviceInstance.ToStringWithPropNameMultiLine());
                    serviceInstance.ServiceName.Should().Be(serviceInstanceGroup.Key);

                    foreach (ICoreDnsDiscoveredServiceHost? discoveredServiceHost in serviceInstance.ServiceHostDomainNames.SelectMany(multicastDnsBackgroundService.FindDiscoveredServiceHosts))
                    {
                        this.TestOutputHelper.WriteLine(discoveredServiceHost?.ToStringWithPropNameMultiLine());
                    }

                    this.TestOutputHelper.WriteLine();
                }

                this.TestOutputHelper.WriteLine("********************************");
            }
        }

        [Fact]
        public async Task MulticastDnsBackgroundServiceIntegration_WaitToResolveAllServiceInstancesWithoutHostsAsync()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var cts = new CancellationTokenSource();

            // using IDisposable globalLogLevel = this.BeginGlobalLogScope(LogLevel.Trace);
            using CoreMulticastDnsBackgroundService? multicastDnsBackgroundService = await this.StartMulticastDnsBackgroundServiceAsync(cts, TimeSpan.FromSeconds(60), true, CoreTaskCacheStateFlags.Current);

            multicastDnsBackgroundService.Should().NotBeNull();

            ILookup<string, ICoreDnsDiscoveredServiceInstance> discoveredServiceInstances =
                await multicastDnsBackgroundService!.WaitToResolveAllServiceInstancesWithoutHostsAsync(this._defaultTimeoutNoActivityMilliseconds, CoreTaskCacheLookupFlags.RefreshCacheBeforeLookup, TimeSpan.FromSeconds(15), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(2), cts.Token);

            this.StopMulticastDnsBackgroundService(multicastDnsBackgroundService, true);

            this.TestOutputHelper.WriteLine($"Services Count: {discoveredServiceInstances.Count}");
            this.TestOutputHelper.WriteLine("********************************");
            foreach (IGrouping<string, ICoreDnsDiscoveredServiceInstance> serviceInstanceGroup in discoveredServiceInstances)
            {
                this.TestOutputHelper.WriteLine(serviceInstanceGroup.Key);
                this.TestOutputHelper.WriteLine("********************************");

                foreach (ICoreDnsDiscoveredServiceInstance serviceInstance in serviceInstanceGroup)
                {
                    this.TestOutputHelper.WriteLine(serviceInstance.ToStringWithPropNameMultiLine());
                    serviceInstance.ServiceName.Should().Be(serviceInstanceGroup.Key);

                    foreach (ICoreDnsDiscoveredServiceHost? discoveredServiceHost in serviceInstance.ServiceHostDomainNames.SelectMany(multicastDnsBackgroundService.FindDiscoveredServiceHosts))
                    {
                        this.TestOutputHelper.WriteLine(discoveredServiceHost?.ToStringWithPropNameMultiLine());
                    }

                    this.TestOutputHelper.WriteLine();
                }

                this.TestOutputHelper.WriteLine("********************************");
            }
        }

        [Fact]
        public async Task MulticastDnsBackgroundServiceIntegration_WaitToResolveServiceNameAsync_Smb()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            var cts = new CancellationTokenSource();

            // using IDisposable globalLogLevel = this.BeginGlobalLogScope(LogLevel.Trace);
            using CoreMulticastDnsBackgroundService? multicastDnsBackgroundService = await this.StartMulticastDnsBackgroundServiceAsync(cts, TimeSpan.FromSeconds(20), true, CoreTaskCacheStateFlags.Current);

            multicastDnsBackgroundService.Should().NotBeNull();

            ISet<ICoreDnsDiscoveredServiceInstance> discoveredServiceInstances =
                await multicastDnsBackgroundService!.WaitToResolveServiceNameAsync("_smb._tcp.local.", this._defaultTimeoutNoActivityMilliseconds, CoreTaskCacheLookupFlags.RefreshCacheBeforeLookup, TimeSpan.FromSeconds(10), cts.Token);

            this.StopMulticastDnsBackgroundService(multicastDnsBackgroundService, true);

            this.TestOutputHelper.WriteLine($"Service Instance Count: {discoveredServiceInstances.Count}");
            foreach (ICoreDnsDiscoveredServiceInstance? serviceInstance in discoveredServiceInstances)
            {
                this.TestOutputHelper.WriteLine();
                this.TestOutputHelper.WriteLine(serviceInstance.ToStringWithPropNameMultiLine());
                serviceInstance.ServiceName.Should().Be("_smb._tcp.local.");
                serviceInstance.ServiceHostDomainNames.Count.Should().BeGreaterThan(0);

                foreach (ICoreDnsDiscoveredServiceHost? discoveredServiceHost in serviceInstance.ServiceHostDomainNames.SelectMany(multicastDnsBackgroundService.FindDiscoveredServiceHosts))
                {
                    this.TestOutputHelper.WriteLine(discoveredServiceHost?.ToStringWithPropNameMultiLine());
                }
            }
        }

        private async Task<CoreMulticastDnsBackgroundService?> StartMulticastDnsBackgroundServiceAsync(CancellationTokenSource cts, TimeSpan timeout, bool hookEvents, CoreTaskCacheStateFlags taskCacheStateFlags)
        {
            var multicastDnsBackgroundService = new CoreMulticastDnsBackgroundService(this.TestCaseServiceProvider, this.TestNetworkServices, taskCacheStateFlags, this.TestCaseLogger);

            if (hookEvents)
            {
                multicastDnsBackgroundService.OnMulticastDnsAnswer += this.MulticastDnsBackgroundService_OnMulticastDnsAnswer;
                multicastDnsBackgroundService.OnMulticastDnsQuestion += this.MulticastDnsBackgroundService_OnMulticastDnsQuestion;
                multicastDnsBackgroundService.OnServiceDiscoveryQuery += this.MulticastDnsBackgroundService_OnServiceDiscoveryQuery;
                multicastDnsBackgroundService.OnDiscoveredServiceName += this.MulticastDnsBackgroundService_OnDiscoveredServiceName;
                multicastDnsBackgroundService.OnDiscoveredServiceInstance += this.MulticastDnsBackgroundService_OnDiscoveredServiceInstance;
                multicastDnsBackgroundService.OnDiscoveredServiceHost += this.MulticastDnsBackgroundService_OnDiscoveredServiceHost;
                multicastDnsBackgroundService.OnDiscoveredNameServer += this.MulticastDnsBackgroundService_OnDiscoveredNameServer;
            }

            await multicastDnsBackgroundService.StartAsync(cts.Token);
            cts.CancelAfter(timeout);
            multicastDnsBackgroundService.IsRunning.Should().BeTrue();

            return multicastDnsBackgroundService;
        }

        private bool StopMulticastDnsBackgroundService(CoreMulticastDnsBackgroundService multicastDnsBackgroundService, bool unhookEvents)
        {
            multicastDnsBackgroundService.Stop();
            multicastDnsBackgroundService.IsRunning.Should().BeFalse();

            if (unhookEvents)
            {
                multicastDnsBackgroundService.OnMulticastDnsAnswer -= this.MulticastDnsBackgroundService_OnMulticastDnsAnswer;
                multicastDnsBackgroundService.OnMulticastDnsQuestion -= this.MulticastDnsBackgroundService_OnMulticastDnsQuestion;
                multicastDnsBackgroundService.OnServiceDiscoveryQuery -= this.MulticastDnsBackgroundService_OnServiceDiscoveryQuery;
                multicastDnsBackgroundService.OnDiscoveredServiceName -= this.MulticastDnsBackgroundService_OnDiscoveredServiceName;
                multicastDnsBackgroundService.OnDiscoveredServiceInstance -= this.MulticastDnsBackgroundService_OnDiscoveredServiceInstance;
                multicastDnsBackgroundService.OnDiscoveredServiceHost -= this.MulticastDnsBackgroundService_OnDiscoveredServiceHost;
                multicastDnsBackgroundService.OnDiscoveredNameServer -= this.MulticastDnsBackgroundService_OnDiscoveredNameServer;
            }

            return multicastDnsBackgroundService.IsRunning;
        }

        private void MulticastDnsBackgroundService_OnDiscoveredNameServer(object? sender, CoreDnsDiscoveredNameServerEvent nameServerDiscoveryEvent)
        {
            nameServerDiscoveryEvent.Should().NotBeNull();

            if (nameServerDiscoveryEvent.IsProviderSockets)
            {
                DnsResponse? dnsResponse = nameServerDiscoveryEvent.DnsResponse;
                dnsResponse.Should().NotBeNull();
            }
            else
            {
                nameServerDiscoveryEvent.IsProviderBonjour.Should().BeTrue();
                nameServerDiscoveryEvent.DnsResponse.Should().BeNull();
            }

            nameServerDiscoveryEvent.RemoteIPEndPoint.Should().NotBeNull();

            if (this.TestCaseLoggerFactory.TestCaseLoggerProvider.ProviderLogLevel.Current <= CoreAppConstants.GetMinimumLogLevel())
            {
                this.TestOutputHelper.WriteLine($"Name Server Discovered ({nameServerDiscoveryEvent.DiscoveryEventOperationType}):{Environment.NewLine}{nameServerDiscoveryEvent.ToStringWithParentsPropName()}{Environment.NewLine}");
            }
        }

        private void MulticastDnsBackgroundService_OnServiceDiscoveryQuery(object? sender, CoreDnsServiceDiscoveryQueryEvent serviceDiscoveryQueryEvent)
        {
            serviceDiscoveryQueryEvent.Should().NotBeNull();

            if (serviceDiscoveryQueryEvent.IsProviderSockets)
            {
                DnsResponse? dnsResponse = serviceDiscoveryQueryEvent.DnsResponse;
                dnsResponse.Should().NotBeNull();
                dnsResponse!.IsQueryResponse.Should().BeFalse();
                dnsResponse.Questions.Should().NotBeEmpty();
            }
            else
            {
                serviceDiscoveryQueryEvent.IsProviderBonjour.Should().BeTrue();
                serviceDiscoveryQueryEvent.DnsResponse.Should().BeNull();
            }

            serviceDiscoveryQueryEvent.RemoteIPEndPoint.Should().NotBeNull();

            if (this.TestCaseLoggerFactory.TestCaseLoggerProvider.ProviderLogLevel.Current <= CoreAppConstants.GetMinimumLogLevel())
            {
                this.TestOutputHelper.WriteLine($"Service Discovery Query ({serviceDiscoveryQueryEvent.DiscoveryEventOperationType}):{Environment.NewLine}{serviceDiscoveryQueryEvent.ToStringWithParentsPropName()}{Environment.NewLine}");
            }
        }

        private void MulticastDnsBackgroundService_OnDiscoveredServiceHost(object? sender, CoreDnsDiscoveredServiceHostEvent discoveredServiceHostEvent)
        {
            discoveredServiceHostEvent.Should().NotBeNull();

            if (discoveredServiceHostEvent.IsProviderSockets)
            {
                DnsResponse? dnsResponse = discoveredServiceHostEvent.DnsResponse;
                dnsResponse.Should().NotBeNull();
            }
            else
            {
                discoveredServiceHostEvent.IsProviderBonjour.Should().BeTrue();
                discoveredServiceHostEvent.DnsResponse.Should().BeNull();
            }

            discoveredServiceHostEvent.RemoteIPEndPoint.Should().NotBeNull();

            if (this.TestCaseLoggerFactory.TestCaseLoggerProvider.ProviderLogLevel.Current <= CoreAppConstants.GetMinimumLogLevel())
            {
                this.TestOutputHelper.WriteLine($"Discovered Service Host ({discoveredServiceHostEvent.DiscoveryEventOperationType}):{Environment.NewLine}{discoveredServiceHostEvent.ToStringWithParentsPropName()}{Environment.NewLine}");
            }
        }

        private void MulticastDnsBackgroundService_OnDiscoveredServiceInstance(object? sender, CoreDnsDiscoveredServiceInstanceEvent discoveredServiceInstanceEvent)
        {
            discoveredServiceInstanceEvent.Should().NotBeNull();

            if (discoveredServiceInstanceEvent.IsProviderSockets)
            {
                DnsResponse? dnsResponse = discoveredServiceInstanceEvent.DnsResponse;
                dnsResponse.Should().NotBeNull();
            }
            else
            {
                discoveredServiceInstanceEvent.IsProviderBonjour.Should().BeTrue();
                discoveredServiceInstanceEvent.DnsResponse.Should().BeNull();
            }

            discoveredServiceInstanceEvent.RemoteIPEndPoint.Should().NotBeNull();

            if (this.TestCaseLoggerFactory.TestCaseLoggerProvider.ProviderLogLevel.Current <= CoreAppConstants.GetMinimumLogLevel())
            {
                this.TestOutputHelper.WriteLine($"Discovered Service Instance ({discoveredServiceInstanceEvent.DiscoveryEventOperationType}):{Environment.NewLine}{discoveredServiceInstanceEvent.ToStringWithParentsPropName()}{Environment.NewLine}");
            }

            foreach (KeyValuePair<string, string?> txtRecordItem in discoveredServiceInstanceEvent.DiscoveredServiceInstance.FindTxtRecordItems())
            {
                if (!this.TestMetadataEntityDatabase.SQLiteAsyncConnection.IsOpen)
                {
                    break;
                }

                try
                {
                    this.TestMetadataEntityDatabase.InsertMetadataEntityAsync(new CoreServicePropertyValueEntity(txtRecordItem.Key, txtRecordItem.Value, CoreServicePropertyType.Unknown, discoveredServiceInstanceEvent.DiscoveredServiceInstance.ServiceInstanceName)).GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    this.TestCaseLogger.LogError(ex, "Failed to InsertMetadataEntityAsync {Key}={Value}", txtRecordItem.Key, txtRecordItem.Value);
                }
            }
        }

        private void MulticastDnsBackgroundService_OnDiscoveredServiceName(object? sender, CoreDnsDiscoveredServiceNameEvent discoveredServiceNameEvent)
        {
            discoveredServiceNameEvent.Should().NotBeNull();

            if (discoveredServiceNameEvent.IsProviderSockets)
            {
                DnsResponse? dnsResponse = discoveredServiceNameEvent.DnsResponse;
                dnsResponse.Should().NotBeNull();
            }
            else
            {
                discoveredServiceNameEvent.IsProviderBonjour.Should().BeTrue();
                discoveredServiceNameEvent.DnsResponse.Should().BeNull();
            }

            discoveredServiceNameEvent.RemoteIPEndPoint.Should().NotBeNull();

            if (this.TestCaseLoggerFactory.TestCaseLoggerProvider.ProviderLogLevel.Current <= CoreAppConstants.GetMinimumLogLevel())
            {
                this.TestOutputHelper.WriteLine($"Discovered Service ({discoveredServiceNameEvent.DiscoveryEventOperationType}):{Environment.NewLine}{discoveredServiceNameEvent.ToStringWithParentsPropName()}{Environment.NewLine}");
            }
        }

        private void MulticastDnsBackgroundService_OnMulticastDnsMessage(object? sender, CoreDnsServiceDiscoveryResponseEvent dnsServiceDiscoveryResponseEvent)
        {
            dnsServiceDiscoveryResponseEvent.Should().NotBeNull();

            if (dnsServiceDiscoveryResponseEvent.IsProviderSockets)
            {
                DnsResponse? dnsResponse = dnsServiceDiscoveryResponseEvent.DnsResponse;
                dnsResponse.Should().NotBeNull();

                // Validate DNSRecordTypes of Questions
                foreach (DnsRequestQuestion responseQuestion in dnsResponse!.Questions)
                {
                    responseQuestion.DnsRequestQuestionType.Should().BeOneOf(KnownDnsQuestionRecordTypes);
                }

                // Validate DNSRecordTypes of Answers
                foreach (AnswerDnsResponseRecord responseAnswer in dnsResponse.Answers)
                {
                    responseAnswer.DnsRecordType.Should().BeOneOf(KnownDnsAnswerRecordTypes);
                }

                // Validate DNSRecordTypes of ResponseRecords
                foreach (DnsResponseRecord responseRecord in dnsResponse.ResponseRecords)
                {
                    responseRecord.DnsRecordType.Should().BeOneOf(KnownDnsResponseRecordTypes);
                }
            }
            else
            {
                dnsServiceDiscoveryResponseEvent.IsProviderBonjour.Should().BeTrue();
                dnsServiceDiscoveryResponseEvent.DnsResponse.Should().BeNull();
            }

            dnsServiceDiscoveryResponseEvent.LocalEndPoint.Should().NotBeNull();

            // dnsServiceDiscoveryResponseEvent.LocalEndPoint!.Equals(new IPEndPoint(IPAddress.Any, CoreMulticastDnsConstants.MulticastDnsServerPort)).Should().BeTrue();
            dnsServiceDiscoveryResponseEvent.RemoteIPEndPoint.Should().NotBeNull();

            // dnsServiceDiscoveryResponseEvent.RemoteEndPoint.As<IPEndPoint>().Port.Should().Be(CoreMulticastDnsConstants.MulticastDnsServerPort);
            dnsServiceDiscoveryResponseEvent.RemoteIPEndPoint!.Address.Should().Be(dnsServiceDiscoveryResponseEvent.RemoteIpAddress);
        }

        private void MulticastDnsBackgroundService_OnMulticastDnsAnswer(object? sender, CoreDnsServiceDiscoveryResponseEvent dnsServiceDiscoveryResponseEvent)
        {
            if (this.TestCaseLoggerFactory.TestCaseLoggerProvider.ProviderLogLevel.Current <= Microsoft.Extensions.Logging.LogLevel.Trace)
            {
                this.TestOutputHelper.WriteLine($"Answer:{Environment.NewLine}{dnsServiceDiscoveryResponseEvent.ToStringWithPropNameMultiLine()}{Environment.NewLine}");
            }

            this.MulticastDnsBackgroundService_OnMulticastDnsMessage(sender, dnsServiceDiscoveryResponseEvent);

            if (dnsServiceDiscoveryResponseEvent.IsProviderSockets)
            {
                DnsResponse? dnsResponse = dnsServiceDiscoveryResponseEvent.DnsResponse;
                dnsResponse.Should().NotBeNull();

                dnsResponse!.Header.IsAuthoritativeAnswer.Should().BeTrue();
                dnsResponse.Header.IsQueryResponse.Should().BeTrue();

                if (dnsResponse.Questions.Count > 0)
                {
                    this.TestOutputHelper.WriteLine($"Found Question in Answer: {Environment.NewLine}{dnsServiceDiscoveryResponseEvent.ToStringWithPropNameMultiLine()}{Environment.NewLine}");
                }

                // dnsServiceDiscoveryResponseEvent.DnsResponse.Questions.Count.Should().Be(0);
                (dnsResponse.Answers.Count > 0 || dnsResponse.Additionals.Count > 0).Should().BeTrue();

                dnsResponse.ResponseRecords.Count.Should().BeGreaterThan(0);
                dnsResponse.Header.DnsResponseCode.Should().Be(DnsResponseCode.NoError);
            }
            else
            {
                dnsServiceDiscoveryResponseEvent.IsProviderBonjour.Should().BeTrue();
                dnsServiceDiscoveryResponseEvent.DnsResponse.Should().BeNull();
            }
        }

        private void MulticastDnsBackgroundService_OnMulticastDnsQuestion(object? sender, CoreDnsServiceDiscoveryResponseEvent dnsServiceDiscoveryResponseEvent)
        {
            if (this.TestCaseLoggerFactory.TestCaseLoggerProvider.ProviderLogLevel.Current <= Microsoft.Extensions.Logging.LogLevel.Trace)
            {
                this.TestOutputHelper.WriteLine($"Question:{Environment.NewLine}{dnsServiceDiscoveryResponseEvent.ToStringWithPropNameMultiLine()}{Environment.NewLine}");
            }

            this.MulticastDnsBackgroundService_OnMulticastDnsMessage(sender, dnsServiceDiscoveryResponseEvent);

            if (dnsServiceDiscoveryResponseEvent.IsProviderSockets)
            {
                DnsResponse? dnsResponse = dnsServiceDiscoveryResponseEvent.DnsResponse;
                dnsResponse.Should().NotBeNull();

                dnsResponse!.IsQueryResponse.Should().BeFalse();
                dnsResponse.Questions.Count.Should().BeGreaterThan(0);

                // Probe to other hosts claiming authority for question records.
                if (dnsResponse.Header.IsAuthoritativeAnswer)
                {
                    dnsResponse.Questions.Count.Should().BeGreaterThan(0);
                    dnsResponse.ResponseRecords.Count.Should().BeGreaterThan(0);

                    foreach (DnsRequestQuestion question in dnsResponse.Questions)
                    {
                        dnsResponse.FindMatchingResponse(question).Should().NotBeNull();
                    }
                }

                // Known-Answer Suppression. The sender already the included answers.
                if (dnsResponse.Answers.Count > 0)
                {
                    // Sender is asking questions and supplying the answers it knows already.
                }

                dnsResponse.Header.DnsResponseCode.Should().Be(DnsResponseCode.NoError);
            }
            else
            {
                dnsServiceDiscoveryResponseEvent.IsProviderBonjour.Should().BeTrue();
                dnsServiceDiscoveryResponseEvent.DnsResponse.Should().BeNull();
            }
        }
    }
}
