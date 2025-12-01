// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="SQlOutboxMigrationTests.cs" company="Network Visor">
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
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Messaging.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Base;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.Outbox.Sqlite;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Outbox
{
    /// <summary>
    /// Class SQlOutboxMigrationTests.
    /// </summary>
    [PlatformTrait(typeof(SQlOutboxMigrationTests))]
    public class SQlOutboxMigrationTests : CoreSqliteTestCaseBase
    {
        private readonly SqliteOutbox _sqlOutbox;
        private readonly Paramore.Brighter.Message _message;
        private Paramore.Brighter.Message? _storedMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SQlOutboxMigrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SQlOutboxMigrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupMessageDb();
            this._sqlOutbox = new SqliteOutbox(new RelationalDatabaseConfiguration(this.TestMessagingDatabase.ConnectionString, outBoxTableName: CoreAppConstants.OutboxTableName));

            this._message = new Paramore.Brighter.Message(
                new MessageHeader(
                    Guid.NewGuid().ToString(),
                    new RoutingKey("test_topic"),
                    MessageType.MT_DOCUMENT),
                new MessageBody("message body"));
            this.AddHistoricMessage(this._message);
        }

        [Fact]
        public void When_writing_a_message_with_minimal_header_information_to_the_outbox()
        {
            this._storedMessage = this._sqlOutbox.Get(this._message.Id, new RequestContext());

            // Should read the message from the sql outbox
            Assert.Equal(this._message.Body.Value, this._storedMessage.Body.Value);

            // Should read the message header type from the sql outbox
            Assert.Equal(this._message.Header.MessageType, this._storedMessage.Header.MessageType);

            // Should read the message header topic from the sql outbox
            Assert.Equal(this._message.Header.Topic, this._storedMessage.Header.Topic);

            // Should default the timestamp from the sql outbox
            Assert.Equal(
                this._message.Header.TimeStamp.ToString("yyyy-MM-ddTHH:mm:ss"),
                this._storedMessage.Header.TimeStamp.ToString("yyyy-MM-ddTHH:mm:ss"));

            // Should read empty header bag from the sql outbox
            Assert.Empty(this._storedMessage.Header.Bag.Keys);
        }

        private void AddHistoricMessage(Paramore.Brighter.Message message)
        {
            string sql = string.Format("INSERT INTO {0} (MessageId, MessageType, Topic, Timestamp, HeaderBag, Body) VALUES (@MessageId, @MessageType, @Topic, @Timestamp, @HeaderBag, @Body)", CoreAppConstants.OutboxTableName);
            SqliteParameter[] parameters = new[]
            {
                new SqliteParameter("MessageId", message.Id.ToString()),
                new SqliteParameter("MessageType", message.Header.MessageType.ToString()),
                new SqliteParameter("Topic", message.Header.Topic.Value),
                new SqliteParameter("Timestamp", SqliteType.Text) { Value = message.Header.TimeStamp.ToString("s") },
                new SqliteParameter("HeaderBag", SqliteType.Text) { Value = JsonSerializer.Serialize(message.Header.Bag, JsonSerialisationOptions.Options) },
                new SqliteParameter("Body", message.Body.Value),
            };

            using var connection = new SqliteConnection(this.TestMessagingDatabase.ConnectionString);
            using SqliteCommand command = connection.CreateCommand();
            connection.Open();

            command.CommandText = sql;

            // command.Parameters.AddRange(parameters); used to work... but can't with current Sqlite lib. Iterator issue
            for (int index = 0; index < parameters.Length; index++)
            {
                _ = command.Parameters.Add(parameters[index]);
            }

            _ = command.ExecuteNonQuery();
        }
    }
}
