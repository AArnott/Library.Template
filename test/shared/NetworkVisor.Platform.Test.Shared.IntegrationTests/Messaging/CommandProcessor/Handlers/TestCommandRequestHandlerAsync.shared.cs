// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestCommandRequestHandlerAsync.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Handles asynchronous processing of <see cref="TestCommand"/> requests within the NetworkVisor platform.</summary>

using System.Net;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Messaging.Handlers.Base;
using NetworkVisor.Core.Messaging.Services.CommandProcessor;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Commands;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.Attributes;
using Paramore.Brighter.Logging.Attributes;
using Xunit;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Handlers
{
    /// <summary>
    /// Handles asynchronous processing of <see cref="TestCommand"/> requests within the NetworkVisor platform.
    /// </summary>
    /// <remarks>
    /// This handler is responsible for executing operations related to the <see cref="TestCommand"/>.
    /// It utilizes network services and test output helpers to perform its tasks.
    /// </remarks>
    /// <seealso cref="TestCommand"/>
    /// <seealso cref="ICoreNetworkServices"/>
    public class TestCommandRequestHandlerAsync : CoreRequestHandlerAsyncBase<TestCommand>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCommandRequestHandlerAsync"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve dependencies.
        /// </param>
        /// <param name="networkServices">
        /// The network services interface providing access to network-related functionalities.
        /// </param>
        /// <param name="commandProcessor">
        /// The command processor responsible for handling command execution.
        /// </param>
        public TestCommandRequestHandlerAsync(IServiceProvider serviceProvider, ICoreNetworkServices networkServices, ICoreCommandProcessor commandProcessor)
        : base(serviceProvider, networkServices, commandProcessor)
        {
        }

        /// <summary>
        /// Asynchronously handles the processing of a <see cref="TestCommand"/> request.
        /// </summary>
        /// <param name="command">The <see cref="TestCommand"/> instance containing the request details.</param>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the processed <see cref="TestCommand"/> instance.
        /// </returns>
        /// <remarks>
        /// This method retrieves the public IP address using <see cref="ICoreNetworkingSystem.GetPublicIPAddressAsync"/>
        /// and logs the result using the provided <see cref="ITestOutputHelper"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if the <paramref name="command"/> is null.</exception>
        /// <seealso cref="TestCommand"/>
        /// <seealso cref="ICoreNetworkingSystem.GetPublicIPAddressAsync"/>
        [UseInboxAsync(0, typeof(TestCommandRequestHandlerAsync), true)]
        [RequestLoggingAsync(step: 1, timing: HandlerTiming.Before)]
        public override async Task<TestCommand> HandleAsync(TestCommand command, CancellationToken cancellationToken = default)
        {
            if (command is null)
            {
                throw new ArgumentNullException(nameof(command));
            }

            IPAddress? publicIPAddress = await this.NetworkServices.NetworkingSystem.GetPublicIPAddressAsync(cancellationToken: cancellationToken);
            this.Logger.LogInformation("Command: Hello {0}, your IP address is: {1}", command.Name, publicIPAddress?.ToString() ?? "Unknown");

            return await base.HandleAsync(command, cancellationToken).ConfigureAwait(this.ContinueOnCapturedContext);
        }
    }
}
