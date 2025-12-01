// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_Writing_A_Message_To_The_Outbox_Async.cs" company="Network Visor">
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
    /// Class SqliteOutboxWritingMessageAsyncTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteOutboxWritingMessageAsyncTests))]
    public class SqliteOutboxWritingMessageAsyncTests : CoreSqliteTestCaseBase
    {
        private readonly SqliteOutbox _sqlOutbox;
        private readonly string _key1 = "name1";
        private readonly string _key2 = "name2";
        private readonly string _key3 = "name3";
        private readonly string _key4 = "name4";
        private readonly string _key5 = "name5";
        private readonly string _value1 = "_value1";
        private readonly string _value2 = "_value2";
        private readonly int _value3 = 123;
        private readonly Guid _value4 = Guid.NewGuid();
        private readonly DateTime _value5 = DateTime.UtcNow;
        private readonly Paramore.Brighter.Message _messageEarliest;
        private Paramore.Brighter.Message? _storedMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteOutboxWritingMessageAsyncTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteOutboxWritingMessageAsyncTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupMessageDb();
            this._sqlOutbox = new SqliteOutbox(this.OutboxConfiguration);

            var messageHeader = new MessageHeader(Guid.NewGuid().ToString(), new RoutingKey("test_topic"), MessageType.MT_DOCUMENT, timeStamp: DateTime.UtcNow.AddDays(-1), handledCount: 5, delayed: TimeSpan.FromMilliseconds(5));
            messageHeader.Bag.Add(this._key1, this._value1);
            messageHeader.Bag.Add(this._key2, this._value2);
            messageHeader.Bag.Add(this._key3, this._value3);
            messageHeader.Bag.Add(this._key4, this._value4);
            messageHeader.Bag.Add(this._key5, this._value5);

            this._messageEarliest = new Paramore.Brighter.Message(messageHeader, new MessageBody("message body"));
        }

        [Fact]
        public async Task When_Writing_A_Message_To_The_Outbox_Async()
        {
            await this._sqlOutbox.AddAsync(this._messageEarliest, new RequestContext());

            this._storedMessage = await this._sqlOutbox.GetAsync(this._messageEarliest.Id, new RequestContext());

            // should read the message from the sql outbox
            Assert.Equal(this._messageEarliest.Body.Value, this._storedMessage.Body.Value);

            // should read the message header first bag item from the sql outbox
            // should read the message header timestamp from the sql outbox
            Assert.Equal(this._messageEarliest.Header.TimeStamp.ToString("yyyy-MM-ddTHH:mm:ss"), this._storedMessage.Header.TimeStamp.ToString("yyyy-MM-ddTHH:mm:ss"));

            // should read the message header topic from the sql outbox =
            Assert.Equal(this._messageEarliest.Header.Topic, this._storedMessage.Header.Topic);

            // should read the message header type from the sql outbox
            Assert.Equal(this._messageEarliest.Header.MessageType, this._storedMessage.Header.MessageType);

            // Bag serialization
            Assert.True(this._storedMessage.Header.Bag.ContainsKey(this._key1));
            Assert.Equal(this._value1, this._storedMessage.Header.Bag[this._key1]);
            Assert.True(this._storedMessage.Header.Bag.ContainsKey(this._key2));
            Assert.Equal(this._value2, this._storedMessage.Header.Bag[this._key2]);
            Assert.True(this._storedMessage.Header.Bag.ContainsKey(this._key3));
            Assert.Equal(this._value3, this._storedMessage.Header.Bag[this._key3]);
            Assert.True(this._storedMessage.Header.Bag.ContainsKey(this._key4));
            Assert.Equal(this._value4, this._storedMessage.Header.Bag[this._key4]);
            Assert.True(this._storedMessage.Header.Bag.ContainsKey(this._key5));
            Assert.Equal(this._value5, this._storedMessage.Header.Bag[this._key5]);
        }
    }
}
