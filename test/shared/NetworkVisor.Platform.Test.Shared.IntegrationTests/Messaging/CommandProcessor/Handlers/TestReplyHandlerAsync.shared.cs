// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestReplyHandlerAsync.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Represents an asynchronous handler for processing <see cref="TestReply"/> requests.</summary>

using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Messaging.Handlers.Base;
using NetworkVisor.Core.Messaging.Services.CommandProcessor;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Replies;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.Attributes;
using Paramore.Brighter.Logging.Attributes;
using Xunit;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Handlers
{
    /// <summary>
    /// Represents an asynchronous handler for processing <see cref="TestReply"/> requests.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="CoreRequestHandlerAsyncBase{TestReply}"/> and provides additional functionality
    /// for handling <see cref="TestReply"/> objects, including logging and command processing capabilities.
    /// </remarks>
    /// <seealso cref="CoreRequestHandlerAsyncBase{TestReply}"/>
    /// <seealso cref="TestReply"/>
    public class TestReplyHandlerAsync : CoreRequestHandlerAsyncBase<TestReply>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestReplyHandlerAsync"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> instance used to resolve dependencies.
        /// </param>
        /// <param name="networkServices">
        /// The <see cref="ICoreNetworkServices"/> instance providing network-related services.
        /// </param>
        /// <param name="commandProcessor">
        /// The <see cref="ICoreCommandProcessor"/> instance for processing commands.
        /// </param>
        /// <remarks>
        /// This constructor sets up the base class with the provided services, enabling the handler
        /// to process <see cref="TestReply"/> objects asynchronously with network and command processing capabilities.
        /// </remarks>
        public TestReplyHandlerAsync(IServiceProvider serviceProvider, ICoreNetworkServices networkServices, ICoreCommandProcessor commandProcessor)
        : base(serviceProvider, networkServices, commandProcessor)
        {
        }

        /// <summary>
        /// Handles the asynchronous processing of a <see cref="TestReply"/> request.
        /// </summary>
        /// <param name="reply">The <see cref="TestReply"/> object containing the request data.</param>
        /// <param name="cancellationToken">
        /// A <see cref="CancellationToken"/> that can be used to cancel the operation.
        /// </param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the processed <see cref="TestReply"/>.
        /// </returns>
        /// <remarks>
        /// This method logs the request details using <see cref="ITestOutputHelper"/> and delegates further processing
        /// to the base implementation of <see cref="CoreRequestHandlerAsyncBase{TestReply}"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">
        /// Thrown if the <paramref name="reply"/> parameter is <see langword="null"/>.
        /// </exception>
        /// <seealso cref="CoreRequestHandlerAsyncBase{TestReply}"/>
        [UseInboxAsync(0, typeof(TestReplyHandlerAsync), true)]
        [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
        public override async Task<TestReply> HandleAsync(TestReply reply, CancellationToken cancellationToken = default)
        {
            if (reply is null)
            {
                throw new ArgumentNullException(nameof(reply));
            }

            this.Logger.LogInformation("Reply: Hello {0}, your IP address is: {1}", reply.Name!, reply.IPAddress?.ToString() ?? "Unknown");

            return await base.HandleAsync(reply, cancellationToken).ConfigureAwait(this.ContinueOnCapturedContext);
        }
    }
}
