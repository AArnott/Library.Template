// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="ScalarUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteInsertUnitTests.
    /// </summary>
    [PlatformTrait(typeof(ScalarUnitTests))]

    public class ScalarUnitTests : CoreTestCaseBase
    {
        private const int Count = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public ScalarUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void Sum_Int32()
        {
            using TestDb<ScalarUnitTests> db = this.CreateDb();

            int r = db.ExecuteScalar<int>("SELECT SUM(Two) FROM TestTable");

            r.Should().Be(Count * 2);

            db.DeleteAll<TestTable>();

            int r1 = db.ExecuteScalar<int>("SELECT SUM(Two) FROM TestTable");

            r1.Should().Be(0);
        }

        [Fact]
        public void SelectSingleRowValue()
        {
            using TestDb<ScalarUnitTests> db = this.CreateDb();

            int r = db.ExecuteScalar<int>("SELECT Two FROM TestTable WHERE Id = 1 LIMIT 1");

            r.Should().Be(2);
        }

        [Fact]
        public void SelectNullableSingleRowValue()
        {
            using TestDb<ScalarUnitTests> db = this.CreateDb();

            int? r = db.ExecuteScalar<int?>("SELECT Two FROM TestTable WHERE Id = 1 LIMIT 1");

            r.HasValue.Should().BeTrue();
            r.Should().Be(2);
        }

        [Fact]
        public void SelectNoRowValue()
        {
            using TestDb<ScalarUnitTests> db = this.CreateDb();

            int? r = db.ExecuteScalar<int?>("SELECT Two FROM TestTable WHERE Id = 999");

            r.HasValue.Should().Be(false);
        }

        [Fact]
        public void SelectNullRowValue()
        {
            using TestDb<ScalarUnitTests> db = this.CreateDb();

            int? r = db.ExecuteScalar<int?>("SELECT null AS Unknown FROM TestTable WHERE Id = 1 LIMIT 1");

            r.HasValue.Should().Be(false);
        }

        private TestDb<ScalarUnitTests> CreateDb()
        {
            var db = new TestDb<ScalarUnitTests>(this.TestFileSystem);
            db.CreateTable<TestTable>();
            IEnumerable<TestTable> items = from i in Enumerable.Range(0, Count)
                                           select new TestTable { Two = 2 };
            db.InsertAll(items);
            db.Table<TestTable>().Count().Should().Be(Count);
            return db;
        }

        private class TestTable
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public int Two { get; set; }
        }
    }
}
