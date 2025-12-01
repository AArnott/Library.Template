// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestReplyBody.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>The body of a reply message specific to the Multicast DNS platform within the NetworkVisor system.</summary>

using System.Net;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Messaging.Extensions;
using NetworkVisor.Core.Messaging.Messages.Base;
using NetworkVisor.Core.Messaging.Replies;
using Paramore.Brighter;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Replies
{
    /// <summary>
    /// Represents the body of a reply message specific to the Multicast DNS platform within the NetworkVisor system.
    /// </summary>
    /// <remarks>
    /// This class provides properties for managing the name and IP address associated with the reply message.
    /// It extends <see cref="CoreMessageBodyBase{TestReplyBody}"/>, inheriting serialization and logging capabilities.
    /// </remarks>
    /// <seealso cref="CoreMessageBodyBase{TestReplyBody}"/>
    /// <seealso cref="NetworkVisor.Platform.MulticastDns.Shared.Test.Replies.TestReply"/>
    public class TestReplyBody : CoreReplyBodyBase<TestReplyBody>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestReplyBody"/> class with the specified caller, name,
        /// IP address string, and an optional global logger.
        /// </summary>
        /// <param name="caller">The caller associated with this reply body.</param>
        /// <param name="name">The name to be assigned to this reply body.</param>
        /// <param name="ipAddressString">The IP address string to be assigned to this reply body.</param>
        /// <param name="globalLogger">An optional global logger for logging purposes.</param>
        public TestReplyBody(ICall caller, string name, string ipAddressString, ICoreGlobalLogger? globalLogger = null)
        : base(caller, globalLogger)
        {
            this.Name = name;
            this.IPAddressString = ipAddressString;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestReplyBody"/> class with the specified caller.
        /// </summary>
        /// <param name="caller">The caller associated with this reply body.</param>
        /// <remarks>
        /// This constructor provides a default initialization for the <see cref="TestReplyBody"/> class,
        /// setting the <see cref="Name"/> to an empty string and the <see cref="IPAddressString"/> to <see cref="IPAddress.None"/>.
        /// </remarks>
        public TestReplyBody(ICall caller)
        : this(caller, string.Empty, IPAddress.None.ToString(), null)
        {
        }

        public TestReplyBody()
        : this(CoreMessagingConstants.EmptyRequest)
        {
        }

        /// <summary>
        /// Gets or sets the string representation of the IP address associated with the reply message.
        /// </summary>
        /// <remarks>
        /// This property holds the IP address in its string format, which can be parsed into an
        /// <see cref="System.Net.IPAddress"/> object when needed. It is used to facilitate serialization
        /// and deserialization of the IP address in the context of the Multicast DNS platform.
        /// </remarks>
        /// <value>
        /// A <see cref="string"/> representing the IP address. Defaults to <see cref="System.Net.IPAddress.None"/>
        /// if not explicitly set.
        /// </value>
        public string IPAddressString { get; set; } = IPAddress.None.ToString();

        /// <summary>
        /// Gets or sets the name associated with the reply message body.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the sender or entity related to this reply message body.
        /// </value>
        /// <remarks>
        /// This property is used to store or retrieve the name information within the context of the
        /// Multicast DNS platform's reply message. It is primarily accessed through the <see cref="TestReply.Name"/> property.
        /// </remarks>
        public string? Name { get; set; }
    }
}
