// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_there_is_no_message_in_the_sql_inbox.cs" company="Network Visor">
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
using Paramore.Brighter.Inbox.Exceptions;
using Paramore.Brighter.Inbox.Sqlite;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Inbox
{
    /// <summary>
    /// Class SqliteInboxEmptyWhenSearchedTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteInboxEmptyWhenSearchedTests))]
    public class SqliteInboxEmptyWhenSearchedTests : CoreSqliteTestCaseBase
    {
        private readonly ICoreSqliteInbox _sqlInbox;
        private readonly string _contextKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteInboxEmptyWhenSearchedTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteInboxEmptyWhenSearchedTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupCommandDb();
            this._sqlInbox = new CoreSqliteInbox(this.InboxConfiguration);
            this._contextKey = "context-key";
        }

        [Fact]
        public void When_There_Is_No_Message_In_The_Sql_Inbox_Get()
        {
            string commandId = Guid.NewGuid().ToString();
            Exception? exception = Catch.Exception(() => this._sqlInbox.Get<MyCommand>(commandId, this._contextKey, null, -1));
            _ = Assert.IsType<RequestNotFoundException<MyCommand>>(exception);
        }

        [Fact]
        public void When_There_Is_No_Message_In_The_Sql_Inbox_Exists()
        {
            string commandId = Guid.NewGuid().ToString();
            Assert.False(this._sqlInbox.Exists<MyCommand>(commandId, this._contextKey, null, -1));
        }
    }
}
