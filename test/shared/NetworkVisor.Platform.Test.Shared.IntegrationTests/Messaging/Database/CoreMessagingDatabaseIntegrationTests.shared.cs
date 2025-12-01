// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreMessagingDatabaseIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
//  Ported from the Brighter project: https://github.com/BrighterCommand/Brighter
//
//  The MIT License (MIT)
//  Copyright © 2014 Ian Cooper (ian_hammond_cooper@yahoo.co.uk)
// </summary>

using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Logging;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Messaging.Tables;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Database
{
    /// <summary>
    /// Class CoreMessagingDatabaseIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreMessagingDatabaseIntegrationTests))]
    public class CoreMessagingDatabaseIntegrationTests : CoreTestCaseBase
    {
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreMessagingDatabaseIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreMessagingDatabaseIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.TemporaryDatabasePath = CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath(this.TestFileSystem, CoreAppConstants.MessagingDatabaseName);
            this.TestMessagingDatabase = new CoreMessagingDatabase(this.TestFileSystem, this.TemporaryDatabasePath, true, null, this.TestCaseLogger);
        }

        public string TemporaryDatabasePath { get; }

        public ICoreMessagingDatabase TestMessagingDatabase { get; }

        [Fact]
        public void MessagingDatabaseIntegration_MessagingDatabase()
        {
            _ = this.TestMessagingDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMessagingDatabase>();
            this.TestMessagingDatabase.IsDatabaseInitialized().Should().BeFalse();
            _ = this.TestMessagingDatabase.InboxTable.Should().NotBeNull().And.Subject.Should().BeAssignableTo<AsyncTableQuery<CoreMessagingInbox>>();
            this.TestMessagingDatabase.IsDatabaseInitialized().Should().BeTrue();
            _ = this.TestMessagingDatabase.OutboxTable.Should().NotBeNull().And.Subject.Should().BeAssignableTo<AsyncTableQuery<CoreMessagingOutbox>>();
            _ = this.TestFileSystem.FileExists(this.TemporaryDatabasePath).Should().BeTrue();
            _ = this.TestMessagingDatabase.QueueTable.Should().NotBeNull().And.Subject.Should().BeAssignableTo<AsyncTableQuery<CoreMessagingQueue>>();
        }

        protected override void Dispose(bool disposing)
        {
            if (this._disposedValue)
            {
                return;
            }

            try
            {
                this.TestMessagingDatabase?.Dispose();
                this.TestFileSystem.WaitToDeleteLockedFile(this.TemporaryDatabasePath);
            }
            finally
            {
                this._disposedValue = true;
                base.Dispose(disposing);
            }
        }
    }
}
