// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="DeleteUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteDeleteUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DeleteUnitTests))]

    public class DeleteUnitTests : CoreTestCaseBase
    {
        private const int Count = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DeleteUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void DeleteEntityOne()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();

            int r = db.Delete(db.Get<TestTable>(1));

            r.Should().Be(1);
            db.Table<TestTable>().Count().Should().Be(Count - 1);
        }

        [Fact]
        public async Task DeleteEntityOneAsync()
        {
            using TestDbAsync<DeleteUnitTests> db = await this.CreateDbAsync();

            int r = await db.DeleteAsync(await db.GetAsync<TestTable>(1));

            r.Should().Be(1);
            (await db.Table<TestTable>().CountAsync()).Should().Be(Count - 1);
        }

        [Fact]
        public void DeletePKOne()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();

            int r = db.Delete<TestTable>(1);

            r.Should().Be(1);
            db.Table<TestTable>().Count().Should().Be(Count - 1);
        }

        [Fact]
        public void DeletePKNone()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();

            int r = db.Delete<TestTable>(348597);

            r.Should().Be(0);
            db.Table<TestTable>().Count().Should().Be(Count);
        }

        [Fact]
        public void DeleteAll()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();

            int r = db.DeleteAll<TestTable>();

            r.Should().Be(Count);
            db.Table<TestTable>().Count().Should().Be(0);
        }

        [Fact]
        public void DeleteWithPredicate()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();

            int r = db.Table<TestTable>().Delete(p => p.Test == "Hello World");

            r.Should().Be(Count);
            db.Table<TestTable>().Count().Should().Be(0);
        }

        [Fact]
        public void DeleteWithPredicateHalf()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();
            db.Insert(new TestTable() { Datum = 1, Test = "Hello World 2" });

            int r = db.Table<TestTable>().Delete(p => p.Test == "Hello World");

            r.Should().Be(Count);
            db.Table<TestTable>().Count().Should().Be(1);
        }

        [Fact]
        public void DeleteWithWherePredicate()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();

            int r = db.Table<TestTable>().Where(p => p.Test == "Hello World").Delete();

            r.Should().Be(Count);
            db.Table<TestTable>().Count().Should().Be(0);
        }

        [Fact]
        public void DeleteWithoutPredicate()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();

            try
            {
                int r = db.Table<TestTable>().Delete();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Fact]
        public void DeleteWithTake()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();

            try
            {
                int r = db.Table<TestTable>().Where(p => p.Test == "Hello World").Take(2).Delete();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }
        }

        [Fact]
        public void DeleteWithSkip()
        {
            using TestDb<DeleteUnitTests> db = this.CreateDb();

            try
            {
                int r = db.Table<TestTable>().Where(p => p.Test == "Hello World").Skip(2).Delete();
                Assert.Fail();
            }
            catch (InvalidOperationException)
            {
            }
        }

        private TestDb<DeleteUnitTests> CreateDb()
        {
            var db = new TestDb<DeleteUnitTests>(this.TestFileSystem);
            db.CreateTable<TestTable>();
            IEnumerable<TestTable> items = from i in Enumerable.Range(0, Count)
                                           select new TestTable { Datum = 1000 + i, Test = "Hello World" };
            db.InsertAll(items);
            db.Table<TestTable>().Count().Should().Be(Count);
            return db;
        }

        private async Task<TestDbAsync<DeleteUnitTests>> CreateDbAsync()
        {
            var db = new TestDbAsync<DeleteUnitTests>(this.TestFileSystem);
            await db.CreateTableAsync<TestTable>();
            IEnumerable<TestTable> items = from i in Enumerable.Range(0, Count)
                                           select new TestTable { Datum = 1000 + i, Test = "Hello World" };
            await db.InsertAllAsync(items);
            (await db.Table<TestTable>().CountAsync()).Should().Be(Count);
            return db;
        }

        private class TestTable
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public int Datum { get; set; }

            public string? Test { get; set; }
        }
    }
}
