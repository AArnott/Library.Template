// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreNetworkAgentUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Services.Agent.Service;
using NetworkVisor.Core.Networking.Services.MulticastDns.Extensions;
using NetworkVisor.Core.Networking.Services.MulticastDns.Records;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking.Agent
{
    /// <summary>
    /// Class CoreNetworkAgentUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkAgentUnitTests))]

    public class CoreNetworkAgentUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkAgentUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkAgentUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method NetworkAgent_ReadWriteNetworkAgentResponse.
        /// </summary>
        /// <param name="questionName">Question name.</param>
        [Theory]
        [InlineData("_services._dns-sd._udp.networkvisor.local.")]
        [InlineData("_sack._tcp.networkvisor.local.")]
        public void NetworkAgent_ReadWriteNetworkAgentResponse(string questionName)
        {
            var dnsResponseRead = new DnsResponse(this.TestCaseServiceProvider, 0, DnsRecordHeader.DefaultFlags, null, this.TestCaseLogger);
            DateTimeOffset timeStamp = DateTimeOffset.UtcNow;

            CoreDomainNameType questionDomainNameType = questionName.ToDomainNameType();

            if (questionDomainNameType.IsNetworkAgentServiceDiscoveryQuery())
            {
                dnsResponseRead.AddAnswer(new AnswerDnsResponseRecord(new DnsRecordPtr(CoreNetworkAgentDnsProvider.NetworkAgentServiceName), questionName, DnsRecordClass.IN, CoreNetworkAgentBackgroundService.DefaultTimeToLive, timeStamp));
                questionName.Should().Be(CoreNetworkAgentDnsProvider.DnsNetworkAgentServiceDiscoveryQuery);
            }
            else if (questionDomainNameType.IsNetworkAgentQuery())
            {
                dnsResponseRead.AddAnswer(new AnswerDnsResponseRecord(new DnsRecordPtr(CoreNetworkAgentDnsProvider.NetworkAgentServiceName), questionName, DnsRecordClass.IN, CoreNetworkAgentBackgroundService.DefaultTimeToLive, timeStamp));
                questionName.Should().Be(CoreNetworkAgentDnsProvider.NetworkAgentServiceName);
            }

            byte[] responseBytes = dnsResponseRead.GetBytes();
            responseBytes.Should().NotBeNullOrEmpty();

            var dnsResponseWrite = new DnsResponse(this.TestCaseServiceProvider, responseBytes, null, this.TestCaseLogger);
            dnsResponseWrite.Equals(dnsResponseRead).Should().BeTrue();

            // Validate message sizes are equal
            dnsResponseRead.MessageSize.Should().Be(dnsResponseWrite.MessageSize);
        }
    }
}
