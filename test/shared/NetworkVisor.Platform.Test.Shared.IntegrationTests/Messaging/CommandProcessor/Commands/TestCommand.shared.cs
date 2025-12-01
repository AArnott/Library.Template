// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestCommand.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using NetworkVisor.Core.Messaging.Commands.Base;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Commands
{
    /// <summary>
    /// Represents a test command within the NetworkVisor platform, inheriting from <see cref="CoreCommandBase{TMessageBody}"/>.
    /// </summary>
    /// <remarks>
    /// This class is used to encapsulate the details of a test command, including its associated message body.
    /// It is designed to work within the messaging infrastructure of the NetworkVisor platform.
    /// </remarks>
    /// <seealso cref="CoreCommandBase{TMessageBody}"/>
    /// <seealso cref="TestCommandBody"/>
    public class TestCommand : CoreCommandBase<TestCommandBody>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TestCommand"/> class.
        /// </summary>
        /// <param name="name">Name.</param>
        public TestCommand(string name)
            : base(new TestCommandBody(name))
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestCommand"/> class.
        /// </summary>
        /// <param name="testCommandBody">Name.</param>
        public TestCommand(TestCommandBody testCommandBody)
            : base(testCommandBody)
        {
        }

        /// <summary>
        /// Gets the name associated with the test command.
        /// </summary>
        /// <value>
        /// A <see cref="string"/> representing the name of the test command.
        /// </value>
        /// <remarks>
        /// This property is set during the initialization of the <see cref="TestCommand"/> instance
        /// and is used to identify the specific command.
        /// </remarks>
        public string Name => this.MessageBody.Name;
    }
}
