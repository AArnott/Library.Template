// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_there_are_multiple_messages_and_some_are_received_and_Dispatched_bulk_Async.cs" company="Network Visor">
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
    /// Class SqliteOutboxBulkGetAsyncTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteOutboxBulkGetAsyncTests))]
    public class SqliteOutboxBulkGetAsyncTests : CoreSqliteTestCaseBase
    {
        private readonly RoutingKey _routingKeyOne = new("test_topic");
        private readonly RoutingKey _routingKeyTwo = new("test_topic3");
        private readonly Paramore.Brighter.Message _message1;
        private readonly Paramore.Brighter.Message _message2;
        private readonly Paramore.Brighter.Message _message3;
        private readonly Paramore.Brighter.Message _message;
        private readonly SqliteOutbox _sqlOutbox;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteOutboxBulkGetAsyncTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteOutboxBulkGetAsyncTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupMessageDb();
            this._sqlOutbox = new SqliteOutbox(this.OutboxConfiguration);

            this._message = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), this._routingKeyOne, MessageType.MT_COMMAND),
                new MessageBody("message body"));
            this._message1 = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), this._routingKeyTwo, MessageType.MT_EVENT),
                new MessageBody("message body2"));
            this._message2 = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), this._routingKeyOne, MessageType.MT_COMMAND),
                new MessageBody("message body3"));
            this._message3 = new Paramore.Brighter.Message(
                new MessageHeader(Guid.NewGuid().ToString(), this._routingKeyTwo, MessageType.MT_EVENT),
                new MessageBody("message body4"));
        }

        [Fact]
        public async Task When_there_are_multiple_messages_and_some_are_received_and_Dispatched_bulk_Async()
        {
            var context = new RequestContext();
            await this._sqlOutbox.AddAsync(this._message, context);
            await Task.Delay(100);
            await this._sqlOutbox.AddAsync(this._message1, context);
            await Task.Delay(100);
            await this._sqlOutbox.AddAsync(this._message2, context);
            await Task.Delay(100);
            await this._sqlOutbox.AddAsync(this._message3, context);
            await Task.Delay(100);

            await this._sqlOutbox.MarkDispatchedAsync(new[] { this._message1.Id, this._message2.Id }, context, DateTime.UtcNow);

            await Task.Delay(400);

            IEnumerable<Paramore.Brighter.Message> undispatchedMessages = await this._sqlOutbox.OutstandingMessagesAsync(TimeSpan.Zero, context);

            Assert.Equal(2, undispatchedMessages.Count());
        }
    }
}
