// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_The_Message_Is_Already_In_The_Outbox_Async.cs" company="Network Visor">
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
    /// Class SqliteOutboxMessageAlreadyExistsAsyncTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteOutboxMessageAlreadyExistsAsyncTests))]
    public class SqliteOutboxMessageAlreadyExistsAsyncTests : CoreSqliteTestCaseBase
    {
        private readonly Paramore.Brighter.Message _messageEarliest;
        private readonly SqliteOutbox _sqlOutbox;
        private Exception? _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteOutboxMessageAlreadyExistsAsyncTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteOutboxMessageAlreadyExistsAsyncTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupMessageDb();
            this._sqlOutbox = new SqliteOutbox(this.OutboxConfiguration);
            this._messageEarliest = new Paramore.Brighter.Message(
                new MessageHeader(
                    Guid.NewGuid().ToString(),
                    new RoutingKey("test_topic"),
                    MessageType.MT_DOCUMENT),
                new MessageBody("message body"));
        }

        [Fact]
        public async Task When_The_Message_Is_Already_In_The_Outbox_Async()
        {
            await this._sqlOutbox.AddAsync(this._messageEarliest, new RequestContext());

            this._exception = await Catch.ExceptionAsync(() => this._sqlOutbox.AddAsync(this._messageEarliest, new RequestContext()));

            // should ignore the duplicate key and still succeed
            Assert.Null(this._exception);
        }
    }
}
