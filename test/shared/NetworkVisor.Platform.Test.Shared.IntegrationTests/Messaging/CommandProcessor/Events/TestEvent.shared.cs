// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestEvent.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>a test event used within the NetworkVisor platform.</summary>

using NetworkVisor.Core.Messaging.Events.Base;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Commands;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Requests
{
    /// <summary>
    /// Represents a test event in the Multicast DNS platform.
    /// </summary>
    /// <remarks>
    /// This class is a specialized event that inherits from <see cref="CoreEventBase{TMessageBody}"/>
    /// and is used to encapsulate information related to a test event.
    /// </remarks>
    public class TestEvent : CoreEventBase<TestEventBody>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestEvent"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public TestEvent(string name)
            : base(new TestEventBody(name))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestEvent"/> class.
        /// </summary>
        /// <param name="testEventBody">Name.</param>
        public TestEvent(TestEventBody testEventBody)
            : base(testEventBody)
        {
        }

        /// <summary>
        /// Gets the name associated with the test event.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the test event.
        /// </value>
        /// <remarks>
        /// This property is set during the initialization of the <see cref="TestEvent"/> instance
        /// and is used to identify the specific command.
        /// </remarks>
        public string Name => this.MessageBody.Name;
    }
}
