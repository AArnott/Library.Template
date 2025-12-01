// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestReply.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>A reply message specific to the Multicast DNS platform within the NetworkVisor system.</summary>

using System.Net;
using System.Text.Json.Serialization;
using NetworkVisor.Core.Messaging.Replies.Base;
using NetworkVisor.Core.Serialization;
using Paramore.Brighter;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Replies
{
    /// <summary>
    /// Represents a reply message specific to the Multicast DNS platform within the NetworkVisor system.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="CoreReplyBase{TestReplyBody}"/> and provides functionality for managing
    /// reply messages with a specific structure, including properties for the sender's address, name, and IP address.
    /// </remarks>
    /// <seealso cref="CoreReplyBase{TestReplyBody}"/>
    /// <seealso cref="TestReplyBody"/>
    public class TestReply : CoreReplyBase<TestReplyBody>
    {
        public TestReply(ICall request, string name, IPAddress ipAddress)
            : base(request, new TestReplyBody(request, name, (ipAddress ?? System.Net.IPAddress.None).ToString()))
        {
        }

        public TestReply(ReplyAddress replyAddress, TestReplyBody testReplyBody)
            : base(replyAddress, testReplyBody)
        {
        }

        public TestReply(TestReplyBody testReplyBody)
            : base(testReplyBody)
        {
        }

        public TestReply(ICall request)
            : base(new TestReplyBody(request))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestReply"/> class with default values.
        /// </summary>
        /// <remarks>
        /// This constructor creates a new <see cref="TestReply"/> instance using a default message body
        /// and a default sender's address. It is primarily used for scenarios where no specific
        /// initialization parameters are required.
        /// </remarks>
        public TestReply()
            : base(new TestReplyBody())
        {
        }

        /// <summary>
        /// Gets or sets the name associated with the reply message.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the sender or entity related to this reply.
        /// </value>
        /// <remarks>
        /// This property interacts with the <see cref="TestReplyBody.Name"/> property to manage the name information
        /// within the reply's message body.
        /// </remarks>
        public string? Name
        {
            get => this.MessageBody.Name;
            set => this.MessageBody.Name = value;
        }

        /// <summary>
        /// Gets or sets the IP address associated with the reply.
        /// </summary>
        /// <remarks>
        /// This property uses a custom JSON converter, <see cref="NetworkVisor.Core.Serialization.CoreIPAddressJsonConverter"/>,
        /// to handle serialization and deserialization of the <see cref="System.Net.IPAddress"/> type.
        /// If no IP address is provided, it defaults to <see cref="System.Net.IPAddress.None"/>.
        /// </remarks>
        [JsonConverter(typeof(CoreIPAddressJsonConverter))]
        public IPAddress? IPAddress
        {
            get => IPAddress.Parse(this.MessageBody.IPAddressString);
            set => this.MessageBody.IPAddressString = (value ?? IPAddress.None).ToString();
        }
    }
}
