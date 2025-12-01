// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CipherUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CipherUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CipherUnitTests))]

    public class CipherUnitTests : CoreTestCaseBase
    {
        private bool disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CipherUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CipherUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            // open an in memory connection and reset SQLCipher default pragma settings
            using var c = new CoreSQLiteConnection(this.TestFileSystem, ":memory:");
            c.Execute("PRAGMA cipher_default_use_hmac = ON;");
        }

        [Fact]
        public void SetStringKey()
        {
            string path;

            string key = "SecretPassword";

            using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, key: key) { CleanupDatabaseOnClose = false, })
            {
                path = db.DatabasePath;

                db.CreateTable<TestTable>();
                db.Insert(new TestTable { Value = "Hello" });
            }

            using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, path, key: key))
            {
                TestTable r = db.Table<TestTable>().First();

                r.Value.Should().Be("Hello");
            }
        }

        [Fact]
        public void SetBytesKey()
        {
            string path;

            var rand = new Random();
            byte[] key = new byte[32];
            rand.NextBytes(key);

            using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, key: key) { CleanupDatabaseOnClose = false, })
            {
                path = db.DatabasePath;

                db.CreateTable<TestTable>();
                db.Insert(new TestTable { Value = "Hello" });
            }

            using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, path, key: key))
            {
                path = db.DatabasePath;

                TestTable r = db.Table<TestTable>().First();

                r.Value.Should().Be("Hello");
            }
        }

        [Fact]
        public void SetEmptyStringKey()
        {
            using var db = new TestDb<CipherUnitTests>(this.TestFileSystem, key: string.Empty);
        }

        [Fact]
        public void SetBadTypeKey()
        {
            try
            {
                using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, key: 42))
                {
                }

                Assert.Fail("Should have thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [Fact]
        public void SetBadBytesKey()
        {
            try
            {
                using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, key: new byte[] { 1, 2, 3, 4 }))
                {
                }

                Assert.Fail("Should have thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [Fact]
        public void SetPreKeyAction()
        {
            string? path = this.TestFileSystem.GetLocalUserAppDatabaseTempFilePath("SetPreKeyAction", ".db");
            path.Should().NotBeNull();
            string key = "SecretKey";

            using var db = new TestDb<CipherUnitTests>(this.TestFileSystem, new CoreSQLiteConnectionString(path!, true, key, preKeyAction: conn => conn.Execute("PRAGMA page_size = 4096;")));
            db.CreateTable<TestTable>();
            db.Insert(new TestTable { Value = "Secret Value" });
            db.ExecuteScalar<string>("PRAGMA page_size;").Should().Be("4096");
        }

        [Fact]
        public void SetPostKeyAction()
        {
            string? path = this.TestFileSystem.GetLocalUserAppDatabaseTempFilePath("SetPostKeyAction", ".db");
            path.Should().NotBeNull();
            string key = "SecretKey";

            using var db = new TestDb<CipherUnitTests>(this.TestFileSystem, new CoreSQLiteConnectionString(path!, true, key, postKeyAction: conn => conn.Execute("PRAGMA page_size = 1024;")));
            db.CreateTable<TestTable>();
            db.Insert(new TestTable { Value = "Secret Value" });
            db.ExecuteScalar<string>("PRAGMA page_size;").Should().Be("1024");
        }

        [Fact]
        public void CheckJournalModeForNonKeyed()
        {
            using var db = new TestDb<CipherUnitTests>(this.TestFileSystem);
            db.CreateTable<TestTable>();
            db.ExecuteScalar<string>("PRAGMA journal_mode;").Should().Be("wal");
        }

        [Fact]
        public void ResetStringKey()
        {
            string path;

            string key = "SecretPassword";
            string reKey = "SecretKey";

            using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, key: key) { CleanupDatabaseOnClose = false })
            {
                db.ReKey(reKey);
                path = db.DatabasePath;

                db.CreateTable<TestTable>();
                db.Insert(new TestTable { Value = "Hello" });
            }

            using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, path, key: reKey))
            {
                TestTable r = db.Table<TestTable>().First();

                r.Value.Should().Be("Hello");
            }
        }

        [Fact]
        public void ResetByteKey()
        {
            string path;

            var rand = new Random();
            byte[] key = new byte[32];
            rand.NextBytes(key);
            byte[] reKey = new byte[32];
            rand.NextBytes(reKey);

            using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, key: key) { CleanupDatabaseOnClose = false })
            {
                db.ReKey(reKey);
                path = db.DatabasePath;

                db.CreateTable<TestTable>();
                db.Insert(new TestTable { Value = "Hello" });
            }

            using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem, path, key: reKey))
            {
                TestTable r = db.Table<TestTable>().First();

                r.Value.Should().Be("Hello");
            }
        }

        [Fact]
        public void ResetBadKey()
        {
            byte[] key = new byte[] { 42 };

            try
            {
                using (var db = new TestDb<CipherUnitTests>(this.TestFileSystem))
                {
                    db.ReKey(key);
                }

                Assert.Fail("Should have thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                try
                {
                    if (disposing)
                    {
                    }
                }
                catch (Exception ex)
                {
                    this.TestCaseLogger.LogError(ex, "Failed to delete temporary database folder {DatabaseTempFolderPath}", this.TestFileSystem.GetLocalUserAppDatabaseTempFolderPath());
                }
                finally
                {
                    this.disposedValue = true;
                }
            }

            base.Dispose(disposing);
        }

        private class TestTable
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string? Value { get; set; }
        }
    }
}
