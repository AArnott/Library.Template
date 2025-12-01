// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_the_message_is_already_in_The_inbox_async.cs" company="Network Visor">
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

using NetworkVisor.Core.Messaging.Inbox;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Base;
using NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.TestDoubles;
using Paramore.Brighter.Inbox.Sqlite;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Inbox
{
    /// <summary>
    /// Class SqliteInboxDuplicateMessageAsyncTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteInboxDuplicateMessageAsyncTests))]
    public class SqliteInboxDuplicateMessageAsyncTests : CoreSqliteTestCaseBase
    {
        private readonly ICoreSqliteInbox _sqlInbox;
        private readonly MyCommand _raisedCommand;
        private readonly string _contextKey;
        private Exception? _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteInboxDuplicateMessageAsyncTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteInboxDuplicateMessageAsyncTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupCommandDb();

            this._sqlInbox = new CoreSqliteInbox(this.InboxConfiguration);
            this._raisedCommand = new MyCommand { Value = "Test" };
            this._contextKey = "context-key";
        }

        [Fact]
        public async Task When_The_Message_Is_Already_In_The_Inbox_Async()
        {
            await this._sqlInbox.AddAsync(this._raisedCommand, this._contextKey, null, -1, default);

            this._exception = await Catch.ExceptionAsync(() => this._sqlInbox.AddAsync(this._raisedCommand, this._contextKey, null, -1, default));

            // _should_succeed_even_if_the_message_is_a_duplicate
            Assert.Null(this._exception);
            bool exists = await this._sqlInbox.ExistsAsync<MyCommand>(this._raisedCommand.Id, this._contextKey, null, -1, default);
            Assert.True(exists);
        }
    }
}
