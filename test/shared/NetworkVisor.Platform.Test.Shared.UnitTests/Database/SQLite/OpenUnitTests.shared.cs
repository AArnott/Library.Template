// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="OpenUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteOpenUnitTests.
    /// </summary>
    [PlatformTrait(typeof(OpenUnitTests))]

    public class OpenUnitTests : CoreTestCaseBase
    {
        private const string UnicodeText = "\u01F427 \u221E";

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public OpenUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void UnicodePaths()
        {
            string path = CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath(this.TestFileSystem, UnicodeText);

            using var db = new TestDb<OpenUnitTests>(this.TestFileSystem, path, true);
            db.CreateTable<TestOrderLine>();
            (new FileInfo(path).Length > 0).Should().BeTrue(path);
        }

        [Fact]
        public async Task UnicodePathsAsync()
        {
            string path = CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath(this.TestFileSystem, UnicodeText);

            using var db = new TestDbAsync<OpenUnitTests>(this.TestFileSystem, path, true);
            await db.CreateTableAsync<TestOrderLine>();

            (new FileInfo(path).Length > 0).Should().BeTrue(path);
        }

        [Fact]
        public void OpenTemporaryOnDisk()
        {
            Action act = () =>
            {
                using var db = new TestDb<OpenUnitTests>(this.TestFileSystem, string.Empty, true);
                db.CreateTable<TestOrderLine>();
            };

            act.Should().NotThrow();
        }

        [Fact]
        public async Task WithWalClosesAsync()
        {
            var db = new TestDbAsync<OpenUnitTests>(this.TestFileSystem);
            await db.CreateTableAsync<TestOrderLine>();
            await db.InsertAsync(new TestOrderLine { });
            List<TestOrderLine> lines = await db.Table<TestOrderLine>().ToListAsync();

            lines.Count.Should().Be(1);
            string databaseFileName = db.DatabasePath;
            db.Dispose();
        }

        [Fact]
        public void WithNoActionsCloses()
        {
            var db = new TestDbAsync<OpenUnitTests>(this.TestFileSystem);

            string databaseFileName = db.DatabasePath;
            db.Dispose();
        }
    }
}
