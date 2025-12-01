// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CreateTableUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
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
    /// Class CoreSQLiteCreateTableUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CreateTableUnitTests))]

    public class CreateTableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CreateTableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CreateTypeWithNoProps()
        {
            using var db = new TestDb<CreateTableUnitTests>(this.TestFileSystem);
            Assert.Throws<ArgumentException>(() => db.CreateTable<NoPropObject>());
        }

        [Fact]
        public void CreateThem()
        {
            using var db = new TestDb<CreateTableUnitTests>(this.TestFileSystem);
            db.CreateTable<TestProduct>();
            db.CreateTable<TestOrder>();
            db.CreateTable<TestOrderLine>();
            db.CreateTable<TestOrderHistory>();

            VerifyCreations(db);
        }

        [Fact]
        public void CreateAsPassedInTypes()
        {
            using var db = new TestDb<CreateTableUnitTests>(this.TestFileSystem);

            db.CreateTable(typeof(TestProduct));
            db.CreateTable(typeof(TestOrder));
            db.CreateTable(typeof(TestOrderLine));
            db.CreateTable(typeof(TestOrderHistory));

            VerifyCreations(db);
        }

        [Fact]
        public void CreateTwice()
        {
            using var db = new TestDb<CreateTableUnitTests>(this.TestFileSystem);

            db.CreateTable<TestProduct>();
            db.CreateTable<TestOrderLine>();
            db.CreateTable<TestOrder>();
            db.CreateTable<TestOrderLine>();
            db.CreateTable<TestOrderHistory>();

            VerifyCreations(db);
        }

        [Fact]
        public void Issue115_MissingPrimaryKey()
        {
            using var db = new TestDb<CreateTableUnitTests>(this.TestFileSystem);

            db.CreateTable<Issue115_MyObject>();
            db.InsertAll(from i in Enumerable.Range(0, 10)
                         select new Issue115_MyObject
                         {
                             UniqueId = i.ToString(),
                             OtherValue = (byte)(i * 10),
                         });

            TableQuery<Issue115_MyObject> query = db.Table<Issue115_MyObject>();
            foreach (Issue115_MyObject itm in query)
            {
                itm.OtherValue++;
                db.Update(itm, typeof(Issue115_MyObject)).Should().Be(1);
            }
        }

        [Fact]
        public void WithoutRowId()
        {
            using var db = new TestDb<CreateTableUnitTests>(this.TestFileSystem);
            db.CreateTable<TestOrderLine>();
            SqliteMaster info = db.Table<SqliteMaster>().First(m => m.TableName == "TestOrderLine");
            info.Sql.Should().NotContain("without rowid");

            db.CreateTable<WantsNoRowId>();
            info = db.Table<SqliteMaster>().First(m => m.TableName == "WantsNoRowId");
            info.Sql.Should().Contain("without rowid");
        }

        private static void VerifyCreations(TestDb<CreateTableUnitTests> db)
        {
            TableMapping? orderLine = db.GetMapping(typeof(TestOrderLine));
            orderLine.Should().NotBeNull();
            orderLine!.Columns.Length.Should().Be(6);

            var l = new TestOrderLine()
            {
                Status = TestOrderLineStatus.Shipped,
            };
            db.Insert(l);
            TestOrderLine? lo = db.Table<TestOrderLine>().First(x => x.Status == TestOrderLineStatus.Shipped);
            l.Id.Should().Be(lo.Id);
        }

        private class NoPropObject
        {
        }

        [Table("sqlite_master")]
        private class SqliteMaster
        {
            [Column("type")]
            public string? Type { get; set; }

            [Column("name")]
            public string? Name { get; set; }

            [Column("tbl_name")]
            public string? TableName { get; set; }

            [Column("rootpage")]
            public int RootPage { get; set; }

            [Column("sql")]
            public string? Sql { get; set; }
        }

        [Table("WantsNoRowId", WithoutRowId = true)]
        private class WantsNoRowId
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string? Name { get; set; }
        }

        private class Issue115_MyObject
        {
            [PrimaryKey]
            public string? UniqueId { get; set; }

            public byte OtherValue { get; set; }
        }
    }
}
