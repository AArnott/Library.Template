// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="When_writing_a_message_to_the_inbox.cs" company="Network Visor">
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
using Paramore.Brighter.Inbox.Sqlite;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Inbox
{
    /// <summary>
    /// Class SqliteInboxAddMessageTests.
    /// </summary>
    [PlatformTrait(typeof(SqliteInboxAddMessageTests))]
    public class SqliteInboxAddMessageTests : CoreSqliteTestCaseBase
    {
        private readonly ICoreSqliteInbox _sqlInbox;
        private readonly MyCommand _raisedCommand;
        private readonly string _contextKey;
        private MyCommand? _storedCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="SqliteInboxAddMessageTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public SqliteInboxAddMessageTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SetupCommandDb();

            this._sqlInbox = new CoreSqliteInbox(this.InboxConfiguration);
            this._raisedCommand = new MyCommand { Value = "Test" };
            this._contextKey = "context-key";
            this._sqlInbox.Add(this._raisedCommand, this._contextKey, null, -1);
        }

        [Fact]
        public void When_Writing_A_Message_To_The_Inbox()
        {
            this._storedCommand = this._sqlInbox.Get<MyCommand>(this._raisedCommand.Id, this._contextKey, null, -1);

            // _should_read_the_command_from_the__sql_inbox
            Assert.NotNull(this._storedCommand);

            // _should_read_the_command_value
            Assert.Equal(this._raisedCommand.Value, this._storedCommand.Value);

            // _should_read_the_command_id
            Assert.Equal(this._raisedCommand.Id, this._storedCommand.Id);
        }
    }
}
