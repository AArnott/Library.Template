// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_retrieving_messages_async.cs" company="Network Visor">
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
    /// Class SqliteFetchMessageAsyncTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteFetchMessageAsyncTests))]
    public class SqliteFetchMessageAsyncTests : CoreSqliteTestCaseBase
    {
        private readonly Paramore.Brighter.Message _messageEarliest;
        private readonly Paramore.Brighter.Message _messageDispatched;
        private readonly Paramore.Brighter.Message _messageUnDispatched;
        private readonly SqliteOutbox _sqlOutbox;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteFetchMessageAsyncTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteFetchMessageAsyncTests(CoreTestClassFixture testClassFixture)
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

        [Fact]
        public async Task When_Retrieving_Messages_Async()
        {
            var context = new RequestContext();
            await this._sqlOutbox.AddAsync([this._messageEarliest, this._messageDispatched, this._messageUnDispatched], context);
            await this._sqlOutbox.MarkDispatchedAsync(this._messageEarliest.Id, context, DateTime.UtcNow.AddHours(-3));
            await this._sqlOutbox.MarkDispatchedAsync(this._messageDispatched.Id, context);

            IList<Paramore.Brighter.Message> messages = await this._sqlOutbox.GetAsync(null);

            // Assert
            Assert.Equal(3, messages.Count());
        }

        [Fact]
        public async Task When_Retrieving_Messages_By_Id_Async()
        {
            var context = new RequestContext();
            await this._sqlOutbox.AddAsync([this._messageEarliest, this._messageDispatched, this._messageUnDispatched], context);
            await this._sqlOutbox.MarkDispatchedAsync(this._messageEarliest.Id, context, DateTime.UtcNow.AddHours(-3));
            await this._sqlOutbox.MarkDispatchedAsync(this._messageDispatched.Id, context);

            IEnumerable<Paramore.Brighter.Message> messages = await this._sqlOutbox.GetAsync(
                [this._messageEarliest.Id, this._messageUnDispatched.Id],
                context);

            // Assert
            var messageList = messages.ToList();
            Assert.Equal(2, messageList.Count);
            Assert.Contains(messageList, x => x.Id == this._messageEarliest.Id);
            Assert.Contains(messageList, x => x.Id == this._messageUnDispatched.Id);
            Assert.DoesNotContain(messageList, x => x.Id == this._messageDispatched.Id);
        }

        [Fact]
        public async Task When_Retrieving_Message_By_Id_Async()
        {
            var context = new RequestContext();
            await this._sqlOutbox.AddAsync([this._messageEarliest, this._messageDispatched, this._messageUnDispatched], context);
            await this._sqlOutbox.MarkDispatchedAsync(this._messageEarliest.Id, context, DateTime.UtcNow.AddHours(-3));
            await this._sqlOutbox.MarkDispatchedAsync(this._messageDispatched.Id, context);

            Paramore.Brighter.Message message = await this._sqlOutbox.GetAsync(this._messageDispatched.Id, context);

            // Assert
            Assert.Equal(this._messageDispatched.Id, message.Id);
        }
    }
}
