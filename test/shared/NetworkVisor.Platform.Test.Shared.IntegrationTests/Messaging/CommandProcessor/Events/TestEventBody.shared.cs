// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestEventBody.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>The body of a test event in the Multicast DNS platform.</summary>

using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Messaging.Messages.Base;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Commands
{
    /// <summary>
    /// Represents the body of a test event in the Multicast DNS platform.
    /// </summary>
    /// <remarks>
    /// This class is used to encapsulate the data associated with a test event.
    /// It inherits from <see cref="CoreMessageBodyBase{TMessageBody}"/> to provide serialization and deserialization capabilities.
    /// </remarks>
    public class TestEventBody : CoreMessageBodyBase<TestEventBody>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestEventBody"/> class with the specified name and optional global logger.
        /// </summary>
        /// <param name="name">The name associated with the test event body. Cannot be <see langword="null"/>.</param>
        /// <param name="globalLogger">
        /// An optional instance of <see cref="ICoreGlobalLogger"/> to provide logging capabilities.
        /// If not provided, logging will be disabled.
        /// </param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/> parameter is <see langword="null"/>.</exception>
        public TestEventBody(string name, ICoreGlobalLogger? globalLogger = null)
        : base(globalLogger)
        {
            this.Name = name ?? throw new ArgumentNullException(nameof(name));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestEventBody"/> class with default values.
        /// </summary>
        /// <remarks>
        /// This parameterless constructor sets the <see cref="Name"/> property to an empty string.
        /// It is primarily used for scenarios where a default instance of <see cref="TestEventBody"/> is required.
        /// </remarks>
        public TestEventBody()
        : this(string.Empty)
        {
        }

        /// <summary>
        /// Gets or sets the name associated with the test event body.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the test event.
        /// </value>
        /// <remarks>
        /// This property is used to uniquely identify or describe the test event.
        /// It is initialized during the construction of the <see cref="TestCommandBody"/> instance
        /// and can be modified as needed.
        /// </remarks>
        public string Name { get; set; }
    }
}
