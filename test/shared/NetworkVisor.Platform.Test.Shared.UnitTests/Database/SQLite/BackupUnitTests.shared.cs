// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="BackupUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Logging;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class BackupUnitTests.
    /// </summary>
    [PlatformTrait(typeof(BackupUnitTests))]

    public class BackupUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BackupUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public BackupUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task BackupOneTableAsync()
        {
            string? pathSrc = this.TestFileSystem.GetLocalUserAppDatabaseTempFilePath("BackupOneTableAsync", ".db");
            pathSrc.Should().NotBeNull();

            string? pathDest = this.TestFileSystem.GetLocalUserAppDatabaseTempFilePath("BackupOneTableAsync", ".db");
            pathDest.Should().NotBeNull();

            var db = new CoreSQLiteAsyncConnection(this.TestFileSystem, pathSrc!, true, new CoreQueryLogger(this.TestCaseLogger));
            await db.CreateTableAsync<TestOrderLine>();
            await db.InsertAsync(new TestOrderLine { });

            this.TestFileSystem.FileExists(pathSrc).Should().BeTrue();

            List<TestOrderLine> lines = await db.Table<TestOrderLine>().ToListAsync();
            lines.Count.Should().Be(1);

            await db.BackupAsync(pathDest!);
            this.TestFileSystem.FileExists(pathDest).Should().BeTrue();

            long destLen = new FileInfo(pathDest!).Length;
            Assert.True(destLen >= 4096);

            await db.CloseAsync();
        }
    }
}
