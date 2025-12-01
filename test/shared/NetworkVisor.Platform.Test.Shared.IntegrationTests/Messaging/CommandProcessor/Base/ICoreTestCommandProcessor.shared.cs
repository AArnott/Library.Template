// Assembly         : NetworkVisor.Core.Networking.Services
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// // ***********************************************************************
// <copyright file="ICoreTestCommandProcessor.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>A specialized Command processor for handling test-related queries within the NetworkVisor platform.</summary>

using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Messaging.Services.CommandProcessor;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base
{
    /// <summary>
    /// Represents a specialized Command processor for handling test-related queries within the NetworkVisor platform.
    /// </summary>
    /// <remarks>
    /// This interface extends the <see cref="ICoreCommandProcessor"/> interface,
    /// providing additional functionality specific to test scenarios.
    /// </remarks>
    public interface ICoreTestCommandProcessor : ICoreCommandProcessor
    {
        /// <summary>
        /// Gets the messaging database associated with the test command processor.
        /// </summary>
        /// <value>
        /// An instance of <see cref="NetworkVisor.Core.Messaging.Database.ICoreMessagingDatabase"/>
        /// that provides asynchronous SQLite connection capabilities and resource management.
        /// </value>
        /// <remarks>
        /// This property is used to interact with the core messaging database, enabling
        /// operations such as command storage, retrieval, and other database-related functionalities
        /// specific to the test scenarios.
        /// </remarks>
        ICoreMessagingDatabase MessagingDatabase { get; }
    }
}
