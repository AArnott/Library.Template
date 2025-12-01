// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_the_message_is_already_in_the_inbox.cs" company="Network Visor">
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
using NetworkVisor.Core.Messaging.Inbox;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Base;
using NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.TestDoubles;
using NetworkVisor.Platform.Test.TestCase;
using Paramore.Brighter.Inbox.Sqlite;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Inbox
{
    /// <summary>
    /// Class SqliteInboxDuplicateMessageTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteInboxDuplicateMessageTests))]
    public class SqliteInboxDuplicateMessageTests : CoreSqliteTestCaseBase
    {
        private readonly ICoreSqliteInbox _sqlInbox;
        private readonly MyCommand _raisedCommand;
        private readonly string _contextKey;
        private Exception? _exception;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteInboxDuplicateMessageTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteInboxDuplicateMessageTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupCommandDb();
            this._sqlInbox = new CoreSqliteInbox(this.InboxConfiguration);
            this._raisedCommand = new MyCommand { Value = "Test" };
            this._contextKey = "context-key";
            this._sqlInbox.Add(this._raisedCommand, this._contextKey, null, -1);
        }

        [Fact]
        public void When_The_Message_Is_Already_In_The_Inbox()
        {
            this._exception = Catch.Exception(() => this._sqlInbox.Add(this._raisedCommand, this._contextKey, null, -1));

            // _should_succeed_even_if_the_message_is_a_duplicate
            Assert.Null(this._exception);
            Assert.True(this._sqlInbox.Exists<MyCommand>(this._raisedCommand.Id, this._contextKey, null, -1));
        }
    }
}
