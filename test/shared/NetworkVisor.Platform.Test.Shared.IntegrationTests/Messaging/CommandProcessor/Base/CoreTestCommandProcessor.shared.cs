// Assembly         : NetworkVisor.Core.Networking.Services
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// // ***********************************************************************
// <copyright file="CoreTestCommandProcessor.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>
//  A test implementation of a command processor, providing functionality for sending, publishing,
//  and managing commands and events within the system.
// </summary>

using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Messaging.Services.CommandProcessor;
using Paramore.Brighter;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base
{
    /// <summary>
    /// Represents the test implementation of a command processor, providing functionality for sending, publishing,
    /// and managing commands and events within the system.
    /// </summary>
    /// <remarks>
    /// This class serves as a wrapper around an instance of <see cref="IAmACommandProcessor"/>, extending its capabilities
    /// with additional methods for handling requests, posts, and transactions. It supports both synchronous and asynchronous
    /// operations, as well as advanced features like outbox management and transaction-based processing.
    /// </remarks>
    public class CoreTestCommandProcessor : CoreCommandProcessor, ICoreTestCommandProcessor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestCommandProcessor"/> class.
        /// </summary>
        /// <param name="serviceProvider">
        /// The service provider used to resolve dependencies required by the command processor.
        /// </param>
        /// <param name="commandProcessor">
        /// The underlying command processor instance implementing <see cref="IAmACommandProcessor"/>
        /// for handling commands and events.
        /// </param>
        /// <param name="messagingDatabase">
        /// The messaging database implementing <see cref="ICoreMessagingDatabase"/> for managing
        /// messaging-related data storage and retrieval.
        /// </param>
        /// <param name="producersConfiguration">
        /// The producers configuration implementing <see cref="IAmProducersConfiguration"/>
        /// for configuring external message buses.
        /// </param>
        /// <param name="serviceActivatorConsumerOptions">
        /// The service activator consumer options implementing <see cref="IAmConsumerOptions"/>
        /// for configuring service activation behavior.
        /// </param>
        /// <remarks>
        /// This constructor provides a comprehensive initialization of the <see cref="CoreTestCommandProcessor"/>
        /// by configuring its dependencies, including the command processor, messaging database,
        /// external bus configuration, and service activator options.
        /// </remarks>
        public CoreTestCommandProcessor(IServiceProvider serviceProvider, IAmACommandProcessor commandProcessor, ICoreMessagingDatabase messagingDatabase, IAmProducersConfiguration producersConfiguration, IAmConsumerOptions serviceActivatorConsumerOptions)
        : base(serviceProvider, commandProcessor, messagingDatabase, producersConfiguration, serviceActivatorConsumerOptions)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestCommandProcessor"/> class with the specified service provider and command processor.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> used to resolve dependencies required by the command processor.
        /// </param>
        /// <param name="commandProcessor">
        /// The <see cref="IAmACommandProcessor"/> instance that provides core command processing functionality.
        /// </param>
        /// <remarks>
        /// This constructor is intended for scenarios where a custom command processor is provided, enabling integration with
        /// the system's messaging infrastructure while leveraging the base functionality of <see cref="CoreCommandProcessor"/>.
        /// </remarks>
        public CoreTestCommandProcessor(IServiceProvider serviceProvider, IAmACommandProcessor commandProcessor)
        : base(serviceProvider, commandProcessor)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base.CoreTestCommandProcessor"/> class
        /// using the specified <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="serviceProvider">
        /// The <see cref="IServiceProvider"/> used to resolve dependencies required by the command processor.
        /// </param>
        /// <remarks>
        /// This constructor simplifies the initialization process by resolving the required dependencies
        /// from the provided <paramref name="serviceProvider"/>.
        /// </remarks>
        public CoreTestCommandProcessor(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
        }
    }
}
