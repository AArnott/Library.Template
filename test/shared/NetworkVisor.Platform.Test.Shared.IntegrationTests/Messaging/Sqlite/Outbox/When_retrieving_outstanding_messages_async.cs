// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_retrieving_outstanding_messages_async.cs" company="Network Visor">
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
using System;
using System.Linq;
using System.Threading.Tasks;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Base;
using Paramore.Brighter;
using Paramore.Brighter.Outbox.Sqlite;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Outbox
{
    /// <summary>
    /// Class SqliteFetchOutStandingMessageAsyncTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteFetchOutStandingMessageAsyncTests))]
    public class SqliteFetchOutStandingMessageAsyncTests : CoreSqliteTestCaseBase
    {
        private readonly Paramore.Brighter.Message _messageEarliest;
        private readonly Paramore.Brighter.Message _messageDispatched;
        private readonly Paramore.Brighter.Message _messageUnDispatched;
        private readonly SqliteOutbox _sqlOutbox;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteFetchOutStandingMessageAsyncTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteFetchOutStandingMessageAsyncTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupMessageDb();
            this._sqlOutbox = new SqliteOutbox(this.OutboxConfiguration);
            var routingKey = new RoutingKey("test_topic");

            this._messageEarliest = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_DOCUMENT)
                {
                    TimeStamp = DateTimeOffset.UtcNow.AddHours(-3),
                },
                new MessageBody("message body"));
            this._messageDispatched = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_DOCUMENT),
                new MessageBody("message body"));
            this._messageUnDispatched = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_DOCUMENT),
                new MessageBody("message body"));
        }

        [Fact(Skip = "DateTimeIncompatible")]
        public async Task When_Retrieving_Not_Dispatched_Messages_Async()
        {
            var context = new RequestContext();
            await this._sqlOutbox.AddAsync([this._messageEarliest, this._messageDispatched, this._messageUnDispatched], context);
            await this._sqlOutbox.MarkDispatchedAsync(this._messageDispatched.Id, context);

            int total = await this._sqlOutbox.GetNumberOfOutstandingMessagesAsync(null, CancellationToken.None);

            IEnumerable<Paramore.Brighter.Message> allUnDispatched = await this._sqlOutbox.OutstandingMessagesAsync(TimeSpan.Zero, context);
            IEnumerable<Paramore.Brighter.Message> messagesOverAnHour = await this._sqlOutbox.OutstandingMessagesAsync(TimeSpan.FromHours(1), context);
            IEnumerable<Paramore.Brighter.Message> messagesOver4Hours = await this._sqlOutbox.OutstandingMessagesAsync(TimeSpan.FromHours(4), context);

            // Assert
            Assert.Equal(2, total);
            Assert.Equal(2, allUnDispatched.Count());
            _ = Assert.Single(messagesOverAnHour);
            Assert.Empty(messagesOver4Hours ?? []);
        }
    }
}
