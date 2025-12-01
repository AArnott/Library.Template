// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TableChangedUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteTableChangedUnitTests.
    /// </summary>
    [PlatformTrait(typeof(TableChangedUnitTests))]

    public class TableChangedUnitTests : CoreTestCaseBase
    {
        private TestDb<TableChangedUnitTests> _db;
        private int changeCount = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="TableChangedUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public TableChangedUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._db = new TestDb<TableChangedUnitTests>(this.TestFileSystem);
            this._db.CreateTable<TestProduct>();
            this._db.CreateTable<TestOrder>();
            this._db.InsertAll(from i in Enumerable.Range(0, 22)
                               select new TestProduct { Name = "Thing" + i, Price = (decimal)Math.Pow(2, i) });

            this.changeCount = 0;

            this._db.TableChanged += (sender, e) =>
            {
                if (e.Table.TableName == "TestProduct")
                {
                    this.changeCount++;
                }
            };
        }

        [Fact]
        public void Insert()
        {
            IEnumerable<TestProduct> query =
                from p in this._db.Table<TestProduct>()
                select p;

            this.changeCount.Should().Be(0);
            query.Count().Should().Be(22);

            this._db.Insert(new TestProduct { Name = "Hello", Price = 1001 });

            this.changeCount.Should().Be(1);
            query.Count().Should().Be(23);
        }

        [Fact]
        public void InsertAll()
        {
            IEnumerable<TestProduct> query =
                from p in this._db.Table<TestProduct>()
                select p;

            this.changeCount.Should().Be(0);
            query.Count().Should().Be(22);

            this._db.InsertAll(from i in Enumerable.Range(0, 22)
                               select new TestProduct { Name = "Test" + i, Price = (decimal)Math.Pow(3, i) });

            this.changeCount.Should().Be(22);
            query.Count().Should().Be(44);
        }

        [Fact]
        public void Update()
        {
            IEnumerable<TestProduct> query =
                from p in this._db.Table<TestProduct>()
                select p;

            this.changeCount.Should().Be(0);
            query.Count().Should().Be(22);

            TestProduct? pr = query.First();
            pr.Price = 10000000;
            this._db.Update(pr);

            this.changeCount.Should().Be(1);
            query.Count().Should().Be(22);
        }

        [Fact]
        public void Delete()
        {
            IEnumerable<TestProduct> query =
                from p in this._db.Table<TestProduct>()
                select p;

            this.changeCount.Should().Be(0);
            query.Count().Should().Be(22);

            TestProduct? pr = query.First();
            pr.Price = 10000000;
            this._db.Delete(pr);

            this.changeCount.Should().Be(1);
            query.Count().Should().Be(21);

            this._db.DeleteAll<TestProduct>();

            this.changeCount.Should().Be(2);
            query.Count().Should().Be(0);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._db?.Close();
            }

            base.Dispose(disposing);
        }
    }
}
