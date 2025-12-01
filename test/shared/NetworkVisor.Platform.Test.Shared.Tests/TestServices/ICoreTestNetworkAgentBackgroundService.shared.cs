// Assembly         : NetworkVisor.Core
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// // ***********************************************************************
// <copyright file="ICoreTestNetworkAgentBackgroundService.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Test Network Agent hosted service.</summary>

using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Services.MulticastDns.Request;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Networking.Services.MulticastDns.Service;
using NetworkVisor.Core.Test.TestCase;

namespace NetworkVisor.Core.Networking.Services.Agent
{
    /// <summary>
    /// Represents a specialized background service for network agents, designed for test scenarios.
    /// Inherits from <see cref="ICoreNetworkAgentBackgroundService" />.
    /// </summary>
    /// <remarks>
    /// This interface extends the functionality of <see cref="ICoreNetworkAgentBackgroundService" />
    /// by incorporating test-specific capabilities.
    /// </remarks>
    public interface ICoreTestNetworkAgentBackgroundService : ICoreNetworkAgentBackgroundService
    {
        /// <summary>
        /// Gets the test case associated with the background service.
        /// </summary>
        /// <value>
        /// An instance of <see cref="ICoreTestCase" /> representing the test case.
        /// </value>
        /// <remarks>
        /// This property provides access to the test case, enabling integration with test-specific
        /// functionalities such as logging, networking, and operating system traits.
        /// </remarks>
        public ICoreTestCase TestCase { get; }

        /// <summary>
        /// Gets the multicast DNS background service associated with the network agent.
        /// </summary>
        /// <remarks>
        /// This property provides access to the <see cref="ICoreMulticastDnsBackgroundService" />,
        /// which facilitates multicast DNS operations, such as service discovery and network interface management.
        /// </remarks>
        public ICoreMulticastDnsBackgroundService MulticastDnsBackgroundService { get; }

        /// <summary>
        /// Sends a DNS response for a network agent in a test scenario.
        /// </summary>
        /// <param name="requestQuestion">
        /// The DNS request question containing the query details, such as QNAME, QTYPE, and QCLASS.
        /// </param>
        /// <param name="remoteIPEndPoint">
        /// The remote IP endpoint to which the DNS response will be sent.
        /// </param>
        /// <param name="ctx">
        /// An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete.
        /// </param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation,
        /// with the result being an integer indicating the status or outcome of the operation.
        /// </returns>
        /// <remarks>
        /// This method is specifically designed for testing purposes and extends the functionality
        /// of the network agent background service to handle DNS responses in test scenarios.
        /// </remarks>
        public Task<int> TestSendNetworkAgentDnsResponseAsync(DnsRequestQuestion requestQuestion, CoreIPEndPoint remoteIPEndPoint, CancellationToken ctx = default);

        /// <summary>
        /// Creates a DNS response for a network agent in a test scenario.
        /// </summary>
        /// <param name="requestQuestion">
        /// The DNS request question containing the query details.
        /// </param>
        /// <param name="remoteIPEndPoint">
        /// The remote IP endpoint associated with the DNS request.
        /// </param>
        /// <returns>
        /// A <see cref="DnsResponse"/> object representing the DNS response, or <see langword="null"/> if no response is generated.
        /// </returns>
        /// <remarks>
        /// This method is a test-specific implementation of creating a DNS response for a network agent.
        /// </remarks>
        public DnsResponse? TestCreateNetworkAgentDnsResponse(DnsRequestQuestion requestQuestion, CoreIPEndPoint remoteIPEndPoint);
    }
}
