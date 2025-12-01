// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_retrieving_messages_to_archive.cs" company="Network Visor">
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
    /// Class SqliteArchiveFetchTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteArchiveFetchTests))]
    public class SqliteArchiveFetchTests : CoreSqliteTestCaseBase
    {
        private readonly Paramore.Brighter.Message _messageEarliest;
        private readonly Paramore.Brighter.Message _messageDispatched;
        private readonly Paramore.Brighter.Message _messageUnDispatched;
        private readonly SqliteOutbox _sqlOutbox;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteArchiveFetchTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteArchiveFetchTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupMessageDb();

            this._sqlOutbox = new SqliteOutbox(this.OutboxConfiguration);
            var routingKey = new RoutingKey("test_topic");

            this._messageEarliest = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_DOCUMENT),
                new MessageBody("message body"));
            this._messageDispatched = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_DOCUMENT),
                new MessageBody("message body"));
            this._messageUnDispatched = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_DOCUMENT),
                new MessageBody("message body"));
        }

        [Fact(Skip = "DateTimeIncompatible")]
        public void When_Retrieving_Messages_To_Archive()
        {
            var context = new RequestContext();
            this._sqlOutbox.Add([this._messageEarliest, this._messageDispatched, this._messageUnDispatched], context);
            this._sqlOutbox.MarkDispatched(this._messageEarliest.Id, context, DateTime.UtcNow.AddHours(-3));
            this._sqlOutbox.MarkDispatched(this._messageDispatched.Id, context);

            IEnumerable<Paramore.Brighter.Message> allDispatched = this._sqlOutbox.DispatchedMessages(TimeSpan.FromHours(0), context);
            IEnumerable<Paramore.Brighter.Message> messagesOverAnHour = this._sqlOutbox.DispatchedMessages(TimeSpan.FromHours(1), context);
            IEnumerable<Paramore.Brighter.Message> messagesOver4Hours = this._sqlOutbox.DispatchedMessages(TimeSpan.FromHours(4), context);

            // Assert
            Assert.Equal(2, allDispatched.Count());
            _ = Assert.Single(messagesOverAnHour);
            Assert.Empty(messagesOver4Hours);
        }

        [Fact(Skip = "DateTimeIncompatible")]
        public void When_Retrieving_Messages_To_Archive_UsingTimeSpan()
        {
            var context = new RequestContext();
            this._sqlOutbox.Add([this._messageEarliest, this._messageDispatched, this._messageUnDispatched], context);
            this._sqlOutbox.MarkDispatched(this._messageEarliest.Id, context, DateTime.UtcNow.AddHours(-3));
            this._sqlOutbox.MarkDispatched(this._messageDispatched.Id, context);

            IEnumerable<Paramore.Brighter.Message> allDispatched = this._sqlOutbox.DispatchedMessages(TimeSpan.Zero, context);
            IEnumerable<Paramore.Brighter.Message> messagesOverAnHour = this._sqlOutbox.DispatchedMessages(TimeSpan.FromHours(2), context);
            IEnumerable<Paramore.Brighter.Message> messagesOver4Hours = this._sqlOutbox.DispatchedMessages(TimeSpan.FromHours(4), context);

            // Assert
            Assert.Equal(2, allDispatched.Count());
            _ = Assert.Single(messagesOverAnHour);
            Assert.Empty(messagesOver4Hours ?? []);
        }
    }
}
