// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_retrieving_messages_to_archive_async.cs" company="Network Visor">
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
    /// Class SqliteArchiveFetchAsyncTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteArchiveFetchAsyncTests))]
    public class SqliteArchiveFetchAsyncTests : CoreSqliteTestCaseBase
    {
        private readonly Paramore.Brighter.Message _messageEarliest;
        private readonly Paramore.Brighter.Message _messageDispatched;
        private readonly Paramore.Brighter.Message _messageUnDispatched;
        private readonly SqliteOutbox _sqlOutbox;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteArchiveFetchAsyncTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteArchiveFetchAsyncTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupMessageDb();

            this._sqlOutbox = new SqliteOutbox(this.OutboxConfiguration);
            var routingKey = new RoutingKey("test_topic");

            this._messageEarliest = new Paramore.Brighter.Message(new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_DOCUMENT), new MessageBody("message body"));
            this._messageDispatched = new Paramore.Brighter.Message(new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_DOCUMENT), new MessageBody("message body"));
            this._messageUnDispatched = new Paramore.Brighter.Message(new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_DOCUMENT), new MessageBody("message body"));
        }

        [Fact(Skip = "DateTimeIncompatible")]
        public async Task When_Retrieving_Messages_To_Archive_Async()
        {
            var context = new RequestContext();
            await this._sqlOutbox.AddAsync([this._messageEarliest, this._messageDispatched, this._messageUnDispatched], context);
            await this._sqlOutbox.MarkDispatchedAsync(this._messageEarliest.Id, context, DateTime.UtcNow.AddHours(-3));
            await this._sqlOutbox.MarkDispatchedAsync(this._messageDispatched.Id, context);

            IEnumerable<Paramore.Brighter.Message> allDispatched =
                await this._sqlOutbox.DispatchedMessagesAsync(0, context, cancellationToken: CancellationToken.None);
            IEnumerable<Paramore.Brighter.Message> messagesOverAnHour =
                await this._sqlOutbox.DispatchedMessagesAsync(1, context, cancellationToken: CancellationToken.None);
            IEnumerable<Paramore.Brighter.Message> messagesOver4Hours =
                await this._sqlOutbox.DispatchedMessagesAsync(4, context, cancellationToken: CancellationToken.None);

            // Assert
            Assert.Equal(2, allDispatched.Count());
            _ = Assert.Single(messagesOverAnHour);
            Assert.Empty(messagesOver4Hours);
        }

        [Fact(Skip = "DateTimeIncompatible")]
        public async Task When_Retrieving_Messages_To_Archive_UsingTimeSpan_Async()
        {
            var context = new RequestContext();
            await this._sqlOutbox.AddAsync([this._messageEarliest, this._messageDispatched, this._messageUnDispatched], context);
            await this._sqlOutbox.MarkDispatchedAsync(this._messageEarliest.Id, context, DateTime.UtcNow.AddHours(-3));
            await this._sqlOutbox.MarkDispatchedAsync(this._messageDispatched.Id, context);

            IEnumerable<Paramore.Brighter.Message> allDispatched =
                await this._sqlOutbox.DispatchedMessagesAsync(TimeSpan.Zero, context, 100, cancellationToken: CancellationToken.None);
            IEnumerable<Paramore.Brighter.Message> messagesOverAnHour =
                await this._sqlOutbox.DispatchedMessagesAsync(TimeSpan.FromHours(2), context, 100, cancellationToken: CancellationToken.None);
            IEnumerable<Paramore.Brighter.Message> messagesOver4Hours =
                await this._sqlOutbox.DispatchedMessagesAsync(TimeSpan.FromHours(4), context, 100, cancellationToken: CancellationToken.None);

            // Assert
            Assert.Equal(2, allDispatched?.Count());
            _ = Assert.Single(messagesOverAnHour);
            Assert.Empty(messagesOver4Hours);
        }
    }
}
