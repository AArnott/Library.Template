// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_Removing_Messages_From_The_Outbox.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>
//  Ported from the Brighter project: https://github.com/BrighterCommand/Brighter
//
//  The MIT License (MIT)
//  Copyright © 2014 Ian Cooper (ian_hammond_cooper@yahoo.co.uk)
// </summary>
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Base;
using Paramore.Brighter;
using Paramore.Brighter.Outbox.Sqlite;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Outbox
{
    /// <summary>
    /// Class SqlOutboxDeletingMessagesTests.
    /// </summary>
    [PlatformTrait(typeof(SqlOutboxDeletingMessagesTests))]
    public class SqlOutboxDeletingMessagesTests : CoreSqliteTestCaseBase
    {
        private readonly SqliteOutbox _outbox;
        private readonly Paramore.Brighter.Message _firstMessage;
        private readonly Paramore.Brighter.Message _secondMessage;
        private readonly Paramore.Brighter.Message _thirdMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqlOutboxDeletingMessagesTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqlOutboxDeletingMessagesTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupMessageDb();
            this._outbox = new SqliteOutbox(this.OutboxConfiguration);

            this._firstMessage =
                new Paramore.Brighter.Message(
                    new MessageHeader(Guid.NewGuid().ToString(), new RoutingKey("Test"), MessageType.MT_COMMAND, timeStamp: DateTime.UtcNow.AddHours(-3)), new MessageBody("Body"));
            this._secondMessage =
                new Paramore.Brighter.Message(
                    new MessageHeader(Guid.NewGuid().ToString(), new RoutingKey("Test2"), MessageType.MT_COMMAND, timeStamp: DateTime.UtcNow.AddHours(-2)), new MessageBody("Body2"));
            this._thirdMessage =
                new Paramore.Brighter.Message(
                    new MessageHeader(Guid.NewGuid().ToString(), new RoutingKey("Test3"), MessageType.MT_COMMAND, timeStamp: DateTime.UtcNow.AddHours(-1)), new MessageBody("Body3"));
        }

        [Fact]
        public void When_Removing_Messages_From_The_Outbox()
        {
            var context = new RequestContext();
            this._outbox.Add(this._firstMessage, context);
            this._outbox.Add(this._secondMessage, context);
            this._outbox.Add(this._thirdMessage, context);

            this._outbox.Delete([this._firstMessage.Id, this._thirdMessage.Id], context);

            Assert.Equal(MessageType.MT_COMMAND, this._outbox.Get(this._secondMessage.Id, context).Header.MessageType);
            Assert.Equal(MessageType.MT_NONE, this._outbox.Get(this._firstMessage.Id, context).Header.MessageType);
            Assert.Equal(MessageType.MT_NONE, this._outbox.Get(this._thirdMessage.Id, context).Header.MessageType);

            this._outbox.Delete([this._secondMessage.Id], context);

            Assert.NotNull(this._outbox.Get(this._secondMessage.Id, context));
        }
    }
}
