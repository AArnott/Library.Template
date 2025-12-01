// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestCall.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>A test request within the NetworkVisor platform, specifically designed for multicast DNS operations.</summary>

using NetworkVisor.Core.Messaging.Calls.Base;
using NetworkVisor.Core.Messaging.ReplyAddresses.Base;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Requests
{
    /// <summary>
    /// Represents a test request within the NetworkVisor platform, specifically designed for multicast DNS operations.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="CoreCallBase{TMessageBody}"/> base class, utilizing <see cref="TestCallBody"/>
    /// as its message body. It provides multiple constructors to initialize the request with varying levels of detail,
    /// including reply addresses and request names.
    /// </remarks>
    /// <seealso cref="CoreCallBase{TMessageBody}"/>
    /// <seealso cref="TestCallBody"/>
    /// <seealso cref="ICoreCall{TMessageBody}"/>
    public class TestCall : CoreCallBase<TestCallBody>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCall"/> class with the specified message body and optional reply address.
        /// </summary>
        /// <param name="replyBody">
        /// The <see cref="TestCallBody"/> instance representing the message body of the request.
        /// </param>
        /// <param name="replyAddress">
        /// An optional <see cref="ICoreReplyAddress"/> instance specifying the reply address for the request.
        /// If not provided, the default reply address will be used.
        /// </param>
        /// <remarks>
        /// This constructor allows for the creation of a <see cref="TestCall"/> with a predefined message body and an optional reply address.
        /// </remarks>
        public TestCall(TestCallBody replyBody, ICoreReplyAddress? replyAddress = null)
            : base(replyBody, replyAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCall"/> class
        /// with the specified name and an optional reply address.
        /// </summary>
        /// <param name="name">The name associated with the request.</param>
        /// <param name="replyAddress">
        /// An optional <see cref="NetworkVisor.Core.Messaging.ReplyAddresses.Base.ICoreReplyAddress"/>
        /// specifying the reply address for the request. Defaults to <see langword="null"/>.
        /// </param>
        public TestCall(string name, ICoreReplyAddress? replyAddress = null)
            : this(new TestCallBody(name), replyAddress)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCall"/> class with a default message body.
        /// </summary>
        /// <remarks>
        /// This constructor creates a test request within the NetworkVisor platform for multicast DNS operations,
        /// using a default <see cref="TestCallBody"/>. It is intended for scenarios where no specific reply address
        /// or request name is required during initialization.
        /// </remarks>
        /// <seealso cref="TestCallBody"/>
        /// <seealso cref="CoreCallBase{TMessageBody}"/>
        /// <seealso cref="ICoreCall{TMessageBody}"/>
        public TestCall()
            : this(string.Empty)
        {
        }

        /// <summary>
        /// Gets or sets the name associated with the test request.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the test request.
        /// This value is stored in the <see cref="TestCallBody"/> associated with the request.
        /// </value>
        public string Name
        {
            get => this.MessageBody.Name;
            set => this.MessageBody.Name = value;
        }
    }
}
