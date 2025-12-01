// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// // ***********************************************************************
// <copyright file="CoreSqliteTestCaseBase.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Database.Providers.SQLite.Logging;
using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Messaging.Extensions;
using NetworkVisor.Core.Test.Fixtures;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Paramore.Brighter;
using Paramore.Brighter.Inbox.Sqlite;
using Paramore.Brighter.Outbox.Sqlite;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.Base
{
    /// <summary>
    /// Represents the base class for core test cases in the NetworkVisor platform.
    /// </summary>
    /// <remarks>
    /// This abstract class provides a foundational implementation for test cases, integrating
    /// with xUnit and offering various utilities and services for testing within the NetworkVisor platform.
    /// </remarks>
    public abstract class CoreSqliteTestCaseBase : CoreTestClassBase, IClassFixture<CoreTestClassFixture>
    {
        public const string SqliteDatabaseName = "Sqlite_V1.db";
        private readonly Lazy<ICoreMessagingDatabase> _messagingDatabaseLazy;
        private readonly bool _binaryMessagePayload;
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSqliteTestCaseBase"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture, which provides shared setup and teardown logic for test cases.
        /// </param>
        /// <param name="binaryMessagePayload">
        /// A boolean value indicating whether the message payload should be treated as binary.
        /// Defaults to <see langword="false"/>.
        /// </param>
        /// <remarks>
        /// This constructor sets up the base class with the provided test class fixture and
        /// optionally configures the handling of binary message payloads.
        /// </remarks>
        protected CoreSqliteTestCaseBase(ICoreTestClassFixture testClassFixture, bool binaryMessagePayload = false)
            : base(testClassFixture)
        {
            this._binaryMessagePayload = binaryMessagePayload;
            this._messagingDatabaseLazy = new Lazy<ICoreMessagingDatabase>(() =>
            {
                var messagingDatabase = new CoreMessagingDatabase(this.TestFileSystem, this.TestFileSystem.GetLocalUserAppDatabaseTempFilePath(SqliteDatabaseName), true, null, this.TestCaseLogger);

                messagingDatabase.IsDatabaseInitialized(true).Should().BeTrue();

                return messagingDatabase;
            });
        }

        public RelationalDatabaseConfiguration InboxConfiguration => new(this.TestMessagingDatabase.ConnectionString, inboxTableName: CoreAppConstants.InboxTableName);

        public RelationalDatabaseConfiguration OutboxConfiguration => new(this.TestMessagingDatabase.ConnectionString, outBoxTableName: CoreAppConstants.OutboxTableName, binaryMessagePayload: this._binaryMessagePayload);

        public ICoreMessagingDatabase TestMessagingDatabase => this._messagingDatabaseLazy.Value;

        public void SetupCommandDb()
        {
        }

        public void SetupMessageDb()
        {
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                try
                {
                    if (disposing)
                    {
                        // Close database first
                        this.TestMessagingDatabase.Dispose();
                    }
                }
                catch (Exception e)
                {
                    this.TestCaseLogger.LogError(e, "Failed to delete {TestDatabase}", this.TestMessagingDatabase.DatabasePath);
                }
                finally
                {
                    this.disposedValue = true;
                    base.Dispose(disposing);
                }
            }
        }
    }
}
