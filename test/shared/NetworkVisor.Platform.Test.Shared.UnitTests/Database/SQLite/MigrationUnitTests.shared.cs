// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="MigrationUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteMigrationUnitTests.
    /// </summary>
    [PlatformTrait(typeof(MigrationUnitTests))]

    public class MigrationUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MigrationUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public MigrationUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void UpperAndLowerColumnNames()
        {
            using var db = new TestDb<MigrationUnitTests>(this.TestFileSystem, true);
            db.CreateTable<LowerId>();
            db.CreateTable<UpperId>();

            var cols = db.GetTableInfo("Test").ToList();
            cols.Count.Should().Be(1);
            cols[0].Name.Should().Be("Id");
        }

        [Fact]
        public void AddColumns()
        {
            // Init the DB
            string path = string.Empty;
            using (var db = new TestDb<MigrationUnitTests>(this.TestFileSystem, true) { CleanupDatabaseOnClose = false })
            {
                path = db.DatabasePath;

                db.CreateTable<TestAddBefore>();

                List<TableColumnInfo> cols = db.GetTableInfo("TestAdd");
                cols.Count.Should().Be(2);

                var o = new TestAddBefore
                {
                    Name = "Foo",
                };

                db.Insert(o);

                TestAddBefore oo = db.Table<TestAddBefore>().First();

                oo.Name.Should().Be("Foo");
            }

            // Migrate and use it
            using (var db = new TestDb<MigrationUnitTests>(this.TestFileSystem, path, true))
            {
                db.CreateTable<TestAddAfter>();

                List<TableColumnInfo> cols = db.GetTableInfo("TestAdd");
                cols.Count.Should().Be(4);

                TestAddAfter oo = db.Table<TestAddAfter>().First();

                oo.Name.Should().Be("Foo");
                oo.IntValue.Should().Be(0);
                oo.StringValue.Should().Be(null);

                var o = new TestAddAfter
                {
                    Name = "Bar",
                    IntValue = 42,
                    StringValue = "Hello",
                };
                db.Insert(o);

                TestAddAfter? ooo = db.Get<TestAddAfter>(o.Id);
                ooo.Should().NotBeNull();
                ooo!.Name.Should().Be("Bar");
                ooo.IntValue.Should().Be(42);
                ooo.StringValue.Should().Be("Hello");
            }
        }

        [Table("TestAdd")]
        private class TestAddBefore
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string? Name { get; set; }
        }

        [Table("TestAdd")]
        private class TestAddAfter
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string? Name { get; set; }

            public int IntValue { get; set; }

            public string? StringValue { get; set; }
        }

        [Table("Test")]
        private class LowerId
        {
            public int Id { get; set; }
        }

        [Table("Test")]
        private class UpperId
        {
            public int ID { get; set; }
        }
    }
}
