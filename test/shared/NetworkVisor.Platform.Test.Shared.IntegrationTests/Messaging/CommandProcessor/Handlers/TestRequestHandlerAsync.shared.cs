// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestRequestHandlerAsync.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>An asynchronous request handler for processing <see cref="TestCall"/> instances.</summary>

using System.Net;
using NetworkVisor.Core.Messaging.Handlers.Base;
using NetworkVisor.Core.Messaging.Services.CommandProcessor;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Replies;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Requests;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.Attributes;
using Paramore.Brighter.Logging.Attributes;
using Xunit;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Handlers
{
    /// <summary>
    /// Represents an asynchronous request handler for processing <see cref="TestCall"/> instances.
    /// </summary>
    /// <remarks>
    /// This class is responsible for handling <see cref="TestCall"/> objects asynchronously,
    /// utilizing the provided network services, command processor, and test output helper.
    /// </remarks>
    /// <seealso cref="CoreRequestHandlerAsyncBase{TRequest}"/>
    /// <seealso cref="ICoreRequestHandlerAsync{TRequest}"/>
    /// <seealso cref="TestCall"/>
    public class TestRequestHandlerAsync : CoreRequestHandlerAsyncBase<TestCall>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestRequestHandlerAsync"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> instance used to resolve dependencies.
        /// </param>
        /// <param name="networkServices">
        /// The <see cref="ICoreNetworkServices"/> instance providing network-related services.
        /// </param>
        /// <param name="commandProcessor">
        /// The <see cref="ICoreCommandProcessor"/> instance used for processing commands.
        /// </param>
        public TestRequestHandlerAsync(IServiceProvider serviceProvider, ICoreNetworkServices networkServices, ICoreCommandProcessor commandProcessor)
        : base(serviceProvider, networkServices, commandProcessor, networkServices?.Logger)
        {
        }

        /// <summary>
        /// Handles the asynchronous processing of a <see cref="TestCall"/>.
        /// </summary>
        /// <param name="call">The <see cref="TestCall"/> to be processed. Must not be <see langword="null"/>.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> to observe while waiting for the task to complete. Defaults to <see cref="CancellationToken.None"/>.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the processed <see cref="TestCall"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="call"/> is <see langword="null"/>.</exception>
        /// <remarks>
        /// This method retrieves the public IP address using the <see cref="ICoreNetworkingSystem"/> and sends a <see cref="TestReply"/>
        /// using the <see cref="ICoreCommandProcessor"/>. It then invokes the base implementation of <see cref="HandleAsync"/>.
        /// </remarks>
        [UseInboxAsync(0, typeof(TestRequestHandlerAsync), true)]
        [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
        public override async Task<TestCall> HandleAsync(TestCall call, CancellationToken cancellationToken = default)
        {
            if (call is null)
            {
                throw new ArgumentNullException(nameof(call));
            }

            IPAddress? publicIPAddress = await this.NetworkServices.NetworkingSystem.GetPublicIPAddressAsync(cancellationToken: cancellationToken);
            await this.CommandProcessor.PublishAsync(new TestReply(call, call.Name, publicIPAddress ?? IPAddress.None), cancellationToken: cancellationToken);
            return await base.HandleAsync(call, cancellationToken).ConfigureAwait(this.ContinueOnCapturedContext);
        }
    }
}
