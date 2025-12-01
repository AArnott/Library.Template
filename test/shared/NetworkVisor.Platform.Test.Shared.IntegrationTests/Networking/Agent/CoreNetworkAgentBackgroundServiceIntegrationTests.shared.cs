// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="CoreNetworkAgentBackgroundServiceIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net.Sockets;
using FluentAssertions;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Services.Agent.Service;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Networking.Services.MulticastDns.Discovery;
using NetworkVisor.Core.Networking.Services.MulticastDns.Extensions;
using NetworkVisor.Core.Networking.Services.MulticastDns.Records;
using NetworkVisor.Core.Networking.Services.MulticastDns.Request;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Networking.Services.MulticastDns.Types;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestServices;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Agent
{
    /// <summary>
    /// Class CoreNetworkAgentBackgroundServiceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkAgentBackgroundServiceIntegrationTests))]

    public class CoreNetworkAgentBackgroundServiceIntegrationTests : CoreNetworkAgentTestCaseBase
    {
        private static readonly DnsRequestQuestionType[] KnownDnsQuestionRecordTypes = [DnsRequestQuestionType.A, DnsRequestQuestionType.AAAA, DnsRequestQuestionType.PTR, DnsRequestQuestionType.SRV, DnsRequestQuestionType.TXT, DnsRequestQuestionType.NS, DnsRequestQuestionType.ANY];
        private static readonly DnsRecordType[] KnownDnsAnswerRecordTypes = [DnsRecordType.A, DnsRecordType.AAAA, DnsRecordType.PTR, DnsRecordType.SRV, DnsRecordType.TXT, DnsRecordType.NS];
        private static readonly DnsRecordType[] KnownDnsResponseRecordTypes = [DnsRecordType.A, DnsRecordType.AAAA, DnsRecordType.PTR, DnsRecordType.SRV, DnsRecordType.TXT, DnsRecordType.NS, DnsRecordType.OPT, DnsRecordType.NSEC];

        // Default timeout before returning due lack of activity.
        private readonly int _defaultTimeoutNoActivityMilliseconds = 3000;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkAgentBackgroundServiceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkAgentBackgroundServiceIntegrationTests(CoreNetworkAgentTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NetworkAgentBackgroundServiceIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Theory]
        [InlineData(DnsRequestQuestionType.PTR)]
        [InlineData(DnsRequestQuestionType.ANY)]
        public async Task NetworkAgentBackgroundServiceIntegration_ResolveNetworkAgentServiceName(DnsRequestQuestionType requestQuestionType)
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            using ICoreTestNetworkAgentBackgroundService networkAgentBackgroundService = CoreTestNetworkAgentBackgroundService.Create(this, CoreTaskCacheStateFlags.NotInitialized);
            _ = networkAgentBackgroundService.Should().NotBeNull().And.BeAssignableTo<ICoreTestNetworkAgentBackgroundService>();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(30));
            await networkAgentBackgroundService.StartAsync(cts.Token);
            _ = networkAgentBackgroundService.MulticastDnsBackgroundService?.MulticastNetworkInterface.Should().NotBeNull();

            if (networkAgentBackgroundService.MulticastDnsBackgroundService!.MulticastNetworkInterface?.IsLocalNetworkAccessRestricted ?? true)
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} requires a multicast interface and unrestricted local network access.");

                return;
            }

            if (!networkAgentBackgroundService.MulticastDnsBackgroundService!.MulticastNetworkInterface?.PreferredIPAddress.IsNullNoneAnyOrLoopback() ?? false)
            {
                ISet<ICoreDnsDiscoveredServiceName> discoveredServiceNames =
                    await networkAgentBackgroundService.MulticastDnsBackgroundService!.WaitToDiscoverServiceNamesAsync(
                        [CoreNetworkAgentDnsProvider.NetworkAgentServiceName],
                        this._defaultTimeoutNoActivityMilliseconds,
                        requestQuestionType,
                        CoreTaskCacheLookupFlags.RefreshCacheBeforeLookup,
                        TimeSpan.FromSeconds(20),
                        cts.Token);

                var discoveredServiceInstances = networkAgentBackgroundService.MulticastDnsBackgroundService.FindDiscoveredServiceInstances().ToList();

                if (discoveredServiceInstances.Any())
                {
                    this.TestOutputHelper.WriteLine("Discovered Service Instances".CenterTitle());
                    foreach (ICoreDnsDiscoveredServiceInstance discoveredServiceInstance in discoveredServiceInstances)
                    {
                        this.TestOutputHelper.WriteLine($"{discoveredServiceInstance.ToStringWithPropNameMultiLine()}\n");
                    }
                }

                // Disable Discovered Names Validations in CI
                if (!CoreAppConstants.IsRunningInCI)
                {
                    _ = discoveredServiceNames.Should().NotBeEmpty();

                    this.TestOutputHelper.WriteLine("Returned Discovered Services".CenterTitle());

                    foreach (ICoreDnsDiscoveredServiceName discoveredServiceName in discoveredServiceNames)
                    {
                        this.TestOutputHelper.WriteLine($"{discoveredServiceName.ToStringWithPropNameMultiLine()}\n");
                    }

                    ICoreDnsDiscoveredServiceInstance? networkAgentDiscoveredServiceInstance = discoveredServiceInstances.FirstOrDefault(sn =>
                        sn.ServiceName.Equals(CoreNetworkAgentDnsProvider.NetworkAgentServiceName));

                    _ = networkAgentDiscoveredServiceInstance.Should().NotBeNull();
                    _ = networkAgentDiscoveredServiceInstance!.ServiceName.Should().Be(CoreNetworkAgentDnsProvider.NetworkAgentServiceName);
                    _ = networkAgentDiscoveredServiceInstance.IsExpired.Should().BeFalse();

                    _ = networkAgentDiscoveredServiceInstance.IsNetworkAgentService.Should().BeTrue();

                    string? serviceHostDomainName = networkAgentDiscoveredServiceInstance.ServiceHostDomainNames.FirstOrDefault();

                    if (!string.IsNullOrEmpty(serviceHostDomainName))
                    {
                        ICoreDnsDiscoveredServiceHost? networkAgentDiscoveredServiceHost = networkAgentBackgroundService.MulticastDnsBackgroundService.FindDiscoveredServiceHosts(serviceHostDomainName!).FirstOrDefault();
                        networkAgentDiscoveredServiceHost.Should().NotBeNull();

                        ICoreRemoteNetworkAgentDevice remoteNetworkAgentDevice = new CoreRemoteNetworkAgentDevice(networkAgentDiscoveredServiceInstance, networkAgentDiscoveredServiceHost!, DateTimeOffset.UtcNow);

                        remoteNetworkAgentDevice.Should().NotBeNull();

                        this.TestOutputHelper.WriteLine(remoteNetworkAgentDevice.ToStringWithParentsPropNameMultiLine());
                    }
                }
            }

            await networkAgentBackgroundService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public async Task NetworkAgentBackgroundServiceIntegration_ResolveNetworkAgentServiceInstances()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            using ICoreTestNetworkAgentBackgroundService networkAgentBackgroundService = CoreTestNetworkAgentBackgroundService.Create(this, CoreTaskCacheStateFlags.NotInitialized);
            _ = networkAgentBackgroundService.Should().NotBeNull().And.BeAssignableTo<ICoreTestNetworkAgentBackgroundService>();

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(30));
            await networkAgentBackgroundService.StartAsync(cts.Token);
            _ = networkAgentBackgroundService.MulticastDnsBackgroundService?.MulticastNetworkInterface.Should().NotBeNull();

            if (networkAgentBackgroundService.MulticastDnsBackgroundService!.MulticastNetworkInterface?.IsLocalNetworkAccessRestricted ?? true)
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} requires a multicast interface and unrestricted local network access.");

                return;
            }

            if (!networkAgentBackgroundService.MulticastDnsBackgroundService!.MulticastNetworkInterface
                    ?.PreferredIPAddress.IsNullNoneAnyOrLoopback() ?? false)
            {
                ISet<ICoreDnsDiscoveredServiceName> discoveredServiceNames =
                    await networkAgentBackgroundService!.MulticastDnsBackgroundService!.WaitToDiscoverServiceNamesAsync(
                        [CoreNetworkAgentDnsProvider.NetworkAgentServiceName],
                        this._defaultTimeoutNoActivityMilliseconds,
                        DnsRequestQuestionType.ANY,
                        CoreTaskCacheLookupFlags.RefreshCacheBeforeLookup,
                        TimeSpan.FromSeconds(10),
                        cts.Token);

                ICoreDnsDiscoveredServiceName? networkAgentDiscoveredServiceName = discoveredServiceNames
                    .FirstOrDefault(sn => sn.ServiceName.Equals(CoreNetworkAgentDnsProvider.NetworkAgentServiceName));

                _ = networkAgentDiscoveredServiceName.Should().NotBeNull();
                _ = networkAgentDiscoveredServiceName!.ServiceName.Should().Be(CoreNetworkAgentDnsProvider.NetworkAgentServiceName);
                _ = networkAgentDiscoveredServiceName.IsExpired.Should().BeFalse();
            }

            await networkAgentBackgroundService.StopAsync(CancellationToken.None);
        }

        [Fact]
        public void NetworkAgentBackgroundServiceIntegration_TestCreateNetworkAgentDnsResponse_Discovery_PTR()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            if (!this.TestNetworkingSystem.PreferredLocalNetworkAddress?.IPAddress.IsNullNoneAnyOrLoopback() ?? false)
            {
                var dnsRequestQuestion = new DnsRequestQuestion(CoreNetworkAgentDnsProvider.DnsNetworkAgentServiceDiscoveryQuery, DnsRequestQuestionType.PTR, DnsRequestQueryClass.IN);
                var remoteIPEndPoint = new CoreIPEndPoint(this.TestNetworkingSystem.PreferredLocalNetworkAddress!.IPAddress!, CoreMulticastDnsConstants.MulticastDnsServerPort);

                using ICoreTestNetworkAgentBackgroundService networkAgentBackgroundService = CoreTestNetworkAgentBackgroundService.Create(this, CoreTaskCacheStateFlags.NotInitialized);
                _ = networkAgentBackgroundService.Should().NotBeNull().And.BeAssignableTo<ICoreTestNetworkAgentBackgroundService>();
                _ = networkAgentBackgroundService?.MulticastDnsBackgroundService?.MulticastNetworkInterface.Should().NotBeNull();

                DnsResponse? dnsResponse = networkAgentBackgroundService!.TestCreateNetworkAgentDnsResponse(dnsRequestQuestion, remoteIPEndPoint);
                _ = dnsResponse.Should().NotBeNull();

                var dnsAnswerRecordPtr = new DnsRecordPtr(CoreNetworkAgentDnsProvider.NetworkAgentServiceName);

                // Header properties
                _ = dnsResponse!.Header.AdditionalRecordCount.Should().Be(0);
                _ = dnsResponse.Header.AnswerCount.Should().Be(1);
                _ = dnsResponse.Header.DnsOperationCode.Should().Be(DnsOperationCode.Query);
                _ = dnsResponse.Header.ID.Should().Be(0);
                _ = dnsResponse.Header.IsAuthoritativeAnswer.Should().BeTrue();
                _ = dnsResponse.Header.IsQueryResponse.Should().BeTrue();
                _ = dnsResponse.Header.IsRecursionAvailable.Should().BeFalse();
                _ = dnsResponse.Header.IsRecursionDesired.Should().BeFalse();
                _ = dnsResponse.Header.IsTruncation.Should().BeFalse();
                _ = dnsResponse.Header.NameServerCount.Should().Be(0);
                _ = dnsResponse.Header.QueryCount.Should().Be(0);
                _ = dnsResponse.Header.ReservedZ.Should().Be(0);

                // Response Properties
                _ = dnsResponse.IsQueryResponse.Should().BeTrue();
                _ = dnsResponse.MessageSize.Should().Be(78);
                _ = dnsResponse.RemoteIPEndPoint.Should().Be(remoteIPEndPoint);

                // Response Records
                _ = dnsResponse.Questions.Should().BeEmpty();
                _ = dnsResponse.RecordsA.Should().BeEmpty();
                _ = dnsResponse.RecordsAAAA.Should().BeEmpty();
                _ = dnsResponse.RecordsNS.Should().BeEmpty();
                _ = dnsResponse.RecordsNSEC.Should().BeEmpty();
                _ = dnsResponse.RecordsOpt.Should().BeEmpty();
                _ = dnsResponse.RecordsPTR.Count().Should().Be(1);
                _ = dnsResponse.RecordsPTR.FirstOrDefault().Should().NotBeNull();
                _ = dnsResponse.RecordsPTR.First().Equals(dnsAnswerRecordPtr).Should().BeTrue();
                _ = dnsResponse.RecordsSRV.Should().BeEmpty();
                _ = dnsResponse.RecordsTXT.Should().BeEmpty();

                _ = dnsResponse.ResponseRecords.Count().Should().Be(1);
                _ = dnsResponse.ResponseRecords.FirstOrDefault().Should().NotBeNull();
                _ = dnsResponse.ResponseRecords.First().Equals(new AnswerDnsResponseRecord(dnsAnswerRecordPtr, CoreNetworkAgentDnsProvider.DnsNetworkAgentServiceDiscoveryQuery, DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH, CoreNetworkAgentBackgroundService.DefaultTimeToLive, DateTimeOffset.UtcNow)).Should().BeTrue();
            }
        }

        [Fact]
        public void NetworkAgentBackgroundServiceIntegration_TestCreateNetworkAgentDnsResponse_Discovery_ANY()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            if (!this.TestNetworkingSystem.PreferredLocalNetworkAddress?.IPAddress.IsNullNoneAnyOrLoopback() ?? false)
            {
                using ICoreTestNetworkAgentBackgroundService networkAgentBackgroundService = CoreTestNetworkAgentBackgroundService.Create(this, CoreTaskCacheStateFlags.NotInitialized);
                _ = networkAgentBackgroundService.Should().NotBeNull();

                var dnsRequestQuestion = new DnsRequestQuestion(CoreNetworkAgentDnsProvider.DnsNetworkAgentServiceDiscoveryQuery, DnsRequestQuestionType.ANY, DnsRequestQueryClass.IN);
                var remoteIPEndPoint = new CoreIPEndPoint(this.TestNetworkingSystem.PreferredLocalNetworkAddress!.IPAddress!, CoreMulticastDnsConstants.MulticastDnsServerPort);

                DnsResponse? dnsResponse = networkAgentBackgroundService!.TestCreateNetworkAgentDnsResponse(dnsRequestQuestion, remoteIPEndPoint);
                _ = dnsResponse.Should().NotBeNull();

                var dnsAnswerRecordPtr = new DnsRecordPtr(CoreNetworkAgentDnsProvider.NetworkAgentServiceName);

                // Header properties
                _ = dnsResponse!.Header.AdditionalRecordCount.Should().Be(0);
                _ = dnsResponse.Header.AnswerCount.Should().Be(1);
                _ = dnsResponse.Header.DnsOperationCode.Should().Be(DnsOperationCode.Query);
                _ = dnsResponse.Header.ID.Should().Be(0);
                _ = dnsResponse.Header.IsAuthoritativeAnswer.Should().BeTrue();
                _ = dnsResponse.Header.IsQueryResponse.Should().BeTrue();
                _ = dnsResponse.Header.IsRecursionAvailable.Should().BeFalse();
                _ = dnsResponse.Header.IsRecursionDesired.Should().BeFalse();
                _ = dnsResponse.Header.IsTruncation.Should().BeFalse();
                _ = dnsResponse.Header.NameServerCount.Should().Be(0);
                _ = dnsResponse.Header.QueryCount.Should().Be(0);
                _ = dnsResponse.Header.ReservedZ.Should().Be(0);

                // Response Properties
                _ = dnsResponse.IsQueryResponse.Should().BeTrue();
                _ = dnsResponse.MessageSize.Should().Be(78);
                _ = dnsResponse.RemoteIPEndPoint.Should().Be(remoteIPEndPoint);

                // Response Records
                _ = dnsResponse.Questions.Should().BeEmpty();
                _ = dnsResponse.RecordsA.Should().BeEmpty();
                _ = dnsResponse.RecordsAAAA.Should().BeEmpty();
                _ = dnsResponse.RecordsNS.Should().BeEmpty();
                _ = dnsResponse.RecordsNSEC.Should().BeEmpty();
                _ = dnsResponse.RecordsOpt.Should().BeEmpty();
                _ = dnsResponse.RecordsPTR.Count().Should().Be(1);
                _ = dnsResponse.RecordsPTR.FirstOrDefault().Should().NotBeNull();
                _ = dnsResponse.RecordsPTR.First().Equals(dnsAnswerRecordPtr).Should().BeTrue();
                _ = dnsResponse.RecordsSRV.Should().BeEmpty();
                _ = dnsResponse.RecordsTXT.Should().BeEmpty();

                _ = dnsResponse.ResponseRecords.Count().Should().Be(1);
                _ = dnsResponse.ResponseRecords.FirstOrDefault().Should().NotBeNull();
                _ = dnsResponse.ResponseRecords.First().Equals(new AnswerDnsResponseRecord(dnsAnswerRecordPtr, CoreNetworkAgentDnsProvider.DnsNetworkAgentServiceDiscoveryQuery, DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH, CoreNetworkAgentBackgroundService.DefaultTimeToLive, DateTimeOffset.UtcNow)).Should().BeTrue();
            }
        }

        [Fact]
        public void NetworkAgentBackgroundServiceIntegration_TestCreateNetworkAgentDnsResponse_ServiceName_PTR()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            if (!this.TestNetworkingSystem.PreferredLocalNetworkAddress?.IPAddress.IsNullNoneAnyOrLoopback() ?? false)
            {
                using ICoreTestNetworkAgentBackgroundService networkAgentBackgroundService = CoreTestNetworkAgentBackgroundService.Create(this, CoreTaskCacheStateFlags.NotInitialized);
                _ = networkAgentBackgroundService.Should().NotBeNull();

                var dnsRequestQuestion = new DnsRequestQuestion(CoreNetworkAgentDnsProvider.NetworkAgentServiceName, DnsRequestQuestionType.PTR, DnsRequestQueryClass.IN);
                var remoteIPEndPoint = new CoreIPEndPoint(this.TestNetworkingSystem.PreferredLocalNetworkAddress!.IPAddress!, CoreMulticastDnsConstants.MulticastDnsServerPort);

                DnsResponse? dnsResponse = networkAgentBackgroundService!.TestCreateNetworkAgentDnsResponse(dnsRequestQuestion, remoteIPEndPoint);
                _ = dnsResponse.Should().NotBeNull();

                _ = networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentServiceInstanceDomainName.Should()
                    .NotBeNullOrEmpty();

                var dnsAnswerRecordPtr = new DnsRecordPtr(networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentServiceInstanceDomainName!);

                // Header properties
                _ = dnsResponse!.Header.AdditionalRecordCount.Should().Be(0);
                _ = dnsResponse.Header.AnswerCount.Should().Be(1);
                _ = dnsResponse.Header.DnsOperationCode.Should().Be(DnsOperationCode.Query);
                _ = dnsResponse.Header.ID.Should().Be(0);
                _ = dnsResponse.Header.IsAuthoritativeAnswer.Should().BeTrue();
                _ = dnsResponse.Header.IsQueryResponse.Should().BeTrue();
                _ = dnsResponse.Header.IsRecursionAvailable.Should().BeFalse();
                _ = dnsResponse.Header.IsRecursionDesired.Should().BeFalse();
                _ = dnsResponse.Header.IsTruncation.Should().BeFalse();
                _ = dnsResponse.Header.NameServerCount.Should().Be(0);
                _ = dnsResponse.Header.QueryCount.Should().Be(0);
                _ = dnsResponse.Header.ReservedZ.Should().Be(0);

                // Response Properties
                _ = dnsResponse.IsQueryResponse.Should().BeTrue();
                _ = dnsResponse.MessageSize.Should().Be(103);
                _ = dnsResponse.RemoteIPEndPoint.Should().Be(remoteIPEndPoint);

                // Response Records
                _ = dnsResponse.Questions.Should().BeEmpty();
                _ = dnsResponse.RecordsA.Should().BeEmpty();
                _ = dnsResponse.RecordsAAAA.Should().BeEmpty();
                _ = dnsResponse.RecordsNS.Should().BeEmpty();
                _ = dnsResponse.RecordsNSEC.Should().BeEmpty();
                _ = dnsResponse.RecordsOpt.Should().BeEmpty();
                _ = dnsResponse.RecordsPTR.Count().Should().Be(1);
                _ = dnsResponse.RecordsPTR.FirstOrDefault().Should().NotBeNull();
                _ = dnsResponse.RecordsPTR.First().Equals(dnsAnswerRecordPtr).Should().BeTrue();
                _ = dnsResponse.RecordsSRV.Should().BeEmpty();
                _ = dnsResponse.RecordsTXT.Should().BeEmpty();

                _ = dnsResponse.ResponseRecords.Count().Should().Be(1);
                _ = dnsResponse.ResponseRecords.FirstOrDefault().Should().NotBeNull();
                _ = dnsResponse.ResponseRecords.First().Equals(new AnswerDnsResponseRecord(dnsAnswerRecordPtr, CoreNetworkAgentDnsProvider.NetworkAgentServiceName, DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH, CoreNetworkAgentBackgroundService.DefaultTimeToLive, DateTimeOffset.UtcNow)).Should().BeTrue();
            }
        }

        [Fact]
        public void NetworkAgentBackgroundServiceIntegration_TestCreateNetworkAgentDnsResponse_ServiceName_ANY()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.MulticastDns))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.MulticastDns} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            if (!this.TestNetworkingSystem.PreferredLocalNetworkAddress?.IPAddress.IsNullNoneAnyOrLoopback() ?? false)
            {
                using ICoreTestNetworkAgentBackgroundService networkAgentBackgroundService = CoreTestNetworkAgentBackgroundService.Create(this, CoreTaskCacheStateFlags.NotInitialized);
                _ = networkAgentBackgroundService.Should().NotBeNull();

                var remoteIPEndPoint = new CoreIPEndPoint(this.TestNetworkingSystem.PreferredLocalNetworkAddress!.IPAddress!, CoreMulticastDnsConstants.MulticastDnsServerPort);
                var dnsRequestQuestion = new DnsRequestQuestion(CoreNetworkAgentDnsProvider.NetworkAgentServiceName, DnsRequestQuestionType.ANY, DnsRequestQueryClass.IN);

                DnsResponse? dnsResponse = networkAgentBackgroundService!.TestCreateNetworkAgentDnsResponse(dnsRequestQuestion, remoteIPEndPoint);
                _ = dnsResponse.Should().NotBeNull();

                // Header properties
                _ = dnsResponse!.Header.AdditionalRecordCount.Should().BeGreaterThan(4);
                _ = dnsResponse.Header.AnswerCount.Should().Be(1);
                _ = dnsResponse.Header.DnsOperationCode.Should().Be(DnsOperationCode.Query);
                _ = dnsResponse.Header.ID.Should().Be(0);
                _ = dnsResponse.Header.IsAuthoritativeAnswer.Should().BeTrue();
                _ = dnsResponse.Header.IsQueryResponse.Should().BeTrue();
                _ = dnsResponse.Header.IsRecursionAvailable.Should().BeFalse();
                _ = dnsResponse.Header.IsRecursionDesired.Should().BeFalse();
                _ = dnsResponse.Header.IsTruncation.Should().BeFalse();
                _ = dnsResponse.Header.NameServerCount.Should().Be(0);
                _ = dnsResponse.Header.QueryCount.Should().Be(0);
                _ = dnsResponse.Header.ReservedZ.Should().Be(0);

                // Response Properties
                _ = dnsResponse.IsQueryResponse.Should().BeTrue();
                _ = dnsResponse.RemoteIPEndPoint.Should().Be(remoteIPEndPoint);

                // Response Records
                _ = dnsResponse.Questions.Should().BeEmpty();
                _ = (dnsResponse.RecordsA.Count() + dnsResponse.RecordsAAAA.Count()).Should().BeGreaterThan(0);
                _ = dnsResponse.RecordsNS.Should().BeEmpty();
                _ = dnsResponse.RecordsNSEC.Should().BeEmpty();
                _ = dnsResponse.RecordsOpt.Should().BeEmpty();
                _ = dnsResponse.RecordsPTR.Count().Should().Be(2);
                _ = dnsResponse.RecordsSRV.Count().Should().Be(1);
                _ = dnsResponse.RecordsTXT.Count().Should().Be(1);

                _ = dnsResponse.ResponseRecords.Count().Should().BeGreaterThan(4);

                // Validate OwnerNames
                foreach (DnsResponseRecord responseRecord in dnsResponse.ResponseRecords)
                {
                    switch (responseRecord.OwnerDomainNameType)
                    {
                        // Mapping between service name and instance.
                        case CoreDomainNameType.NetworkAgentServiceName:
                            {
                                _ = responseRecord.DnsRecordClass.Should().Be(DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH);
                                _ = responseRecord.DnsRecordType.Should().Be(DnsRecordType.PTR);
                                _ = responseRecord.IsCacheFlush.Should().BeTrue();
                                _ = responseRecord.IsExpired.Should().BeFalse();
                                _ = responseRecord.IsServiceDiscoveryQueryResponse.Should().BeFalse();
                                _ = responseRecord.IsValidResponse.Should().BeTrue();

                                _ = responseRecord.OwnerDomainName.Equals(CoreNetworkAgentDnsProvider.NetworkAgentServiceName, StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();

                                DnsRecordPtr? dnsRecordPtr = responseRecord.GetDnsRecord<DnsRecordPtr>();
                                _ = dnsRecordPtr.Should().NotBeNull();

                                _ = dnsRecordPtr!.PtrDomainName
                                    .Equals(
                                        networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentServiceInstanceDomainName,
                                        StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();

                                _ = dnsRecordPtr.PtrDomainNameType.Should()
                                    .Be(CoreDomainNameType.NetworkAgentServiceInstance);
                                break;
                            }

                        // Service Hosts
                        case CoreDomainNameType.NetworkVisorDomainHost:
                            {
                                _ = responseRecord.DnsRecordClass.Should().Be(DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH);
                                _ = responseRecord.IsCacheFlush.Should().BeTrue();
                                _ = responseRecord.IsExpired.Should().BeFalse();
                                _ = responseRecord.IsServiceDiscoveryQueryResponse.Should().BeFalse();
                                _ = responseRecord.IsValidResponse.Should().BeTrue();
                                _ = responseRecord.OwnerDomainName
                                    .Equals(
                                        networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentHostName,
                                        StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();

                                switch (responseRecord.DnsRecordType)
                                {
                                    case DnsRecordType.A:
                                        {
                                            // Maps from Host to IPv4 Address
                                            DnsRecordA? dnsRecordA = responseRecord.GetDnsRecord<DnsRecordA>();
                                            _ = dnsRecordA.Should().NotBeNull();
                                            _ = dnsRecordA!.Address.AddressFamily.Should().Be(AddressFamily.InterNetwork);
                                            _ = dnsRecordA.HostName?.Equals(networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentHostName, StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();

                                            break;
                                        }

                                    case DnsRecordType.AAAA:
                                        {
                                            // Maps from Host to IPv6 Address
                                            DnsRecordAAAA? dnsRecordAAAA = responseRecord.GetDnsRecord<DnsRecordAAAA>();
                                            _ = dnsRecordAAAA.Should().NotBeNull();
                                            _ = dnsRecordAAAA!.Address.AddressFamily.Should().Be(AddressFamily.InterNetworkV6);
                                            _ = dnsRecordAAAA.HostName?.Equals(networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentHostName, StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();

                                            break;
                                        }

                                    case DnsRecordType.PTR:
                                        {
                                            // Maps from Service Instance to Host
                                            DnsRecordPtr? dnsRecordPtr = responseRecord.GetDnsRecord<DnsRecordPtr>();
                                            _ = dnsRecordPtr.Should().NotBeNull();
                                            _ = dnsRecordPtr!.PtrDomainName?.Equals(networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentHostName, StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();
                                            _ = dnsRecordPtr!.PtrDomainNameType.Should().Be(CoreDomainNameType.LocalDomainHost);
                                            break;
                                        }

                                    default:
                                        {
                                            throw new ArgumentOutOfRangeException(nameof(responseRecord.DnsRecordType), responseRecord.DnsRecordType, "Unhandled DnsRecordType");
                                        }
                                }

                                break;
                            }

                        // Service Instance
                        case CoreDomainNameType.NetworkAgentServiceInstance:
                            {
                                _ = responseRecord.DnsRecordClass.Should().Be(DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH);
                                _ = responseRecord.IsCacheFlush.Should().BeTrue();
                                _ = responseRecord.IsExpired.Should().BeFalse();
                                _ = responseRecord.IsServiceDiscoveryQueryResponse.Should().BeFalse();
                                _ = responseRecord.IsValidResponse.Should().BeTrue();
                                _ = responseRecord.OwnerDomainName
                                    .Equals(
                                        networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentServiceInstanceDomainName,
                                        StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();

                                switch (responseRecord.DnsRecordType)
                                {
                                    case DnsRecordType.PTR:
                                        {
                                            // Maps from Service Instance to Host
                                            DnsRecordPtr? dnsRecordPtr = responseRecord.GetDnsRecord<DnsRecordPtr>();
                                            _ = dnsRecordPtr.Should().NotBeNull();
                                            _ = dnsRecordPtr!.PtrDomainName?.Equals(networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentHostName, StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();
                                            _ = dnsRecordPtr!.PtrDomainNameType.Should().Be(CoreDomainNameType.NetworkVisorDomainHost);
                                            break;
                                        }

                                    case DnsRecordType.TXT:
                                        {
                                            // Maps from service instance to Txt records
                                            DnsRecordTxt? dnsRecordTxt = responseRecord.GetDnsRecord<DnsRecordTxt>();
                                            _ = dnsRecordTxt.Should().NotBeNull();
                                            _ = dnsRecordTxt!.TxtRecords.Should().BeEquivalentTo(networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentServiceInstanceTxtRecords);
                                            break;
                                        }

                                    case DnsRecordType.SRV:
                                        {
                                            // Maps from service instance to Host
                                            DnsRecordSrv? dnsRecordSrv = responseRecord.GetDnsRecord<DnsRecordSrv>();
                                            _ = dnsRecordSrv.Should().NotBeNull();

                                            _ = dnsRecordSrv!.TargetDomain.Equals(
                                                networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentHostName,
                                                StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();
                                            _ = dnsRecordSrv.Port.Should().Be(networkAgentBackgroundService.LocalNetworkAgent.NetworkAgentServiceInstancePort);
                                            _ = dnsRecordSrv.Priority.Should().Be(0);
                                            _ = dnsRecordSrv.Weight.Should().Be(0);
                                            break;
                                        }

                                    default:
                                        {
                                            throw new ArgumentOutOfRangeException(nameof(responseRecord.DnsRecordType), responseRecord.DnsRecordType, "Unhandled DnsRecordType");
                                        }
                                }

                                break;
                            }

                        default:
                            {
                                throw new ArgumentOutOfRangeException(nameof(responseRecord.OwnerDomainNameType), responseRecord.OwnerDomainNameType, "Unhandled OwnerDomainNameType");
                            }
                    }
                }
            }
        }
    }
}
