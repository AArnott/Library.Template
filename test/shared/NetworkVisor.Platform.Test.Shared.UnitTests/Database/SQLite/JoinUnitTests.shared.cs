// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="JoinUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreSQLiteJoinUnitTests.
    /// </summary>
    [PlatformTrait(typeof(JoinUnitTests))]

    public class JoinUnitTests : CoreTestCaseBase
    {
        private TestDb<JoinUnitTests> _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="JoinUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public JoinUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._db = new TestDb<JoinUnitTests>(this.TestFileSystem);
            this._db.CreateTable<TestProduct>();
            this._db.CreateTable<TestOrder>();
            this._db.CreateTable<TestOrderLine>();

            var p1 = new TestProduct { Name = "One", };
            var p2 = new TestProduct { Name = "Two", };
            var p3 = new TestProduct { Name = "Three", };
            this._db.InsertAll(new[] { p1, p2, p3 });

            var o1 = new TestOrder { PlacedTime = DateTime.Now, };
            var o2 = new TestOrder { PlacedTime = DateTime.Now, };
            this._db.InsertAll(new[] { o1, o2 });

            this._db.InsertAll(new[]
            {
                new TestOrderLine
                {
                    OrderId = o1.Id,
                    ProductId = p1.Id,
                    Quantity = 1,
                },
                new TestOrderLine
                {
                    OrderId = o1.Id,
                    ProductId = p2.Id,
                    Quantity = 2,
                },
                new TestOrderLine
                {
                    OrderId = o2.Id,
                    ProductId = p3.Id,
                    Quantity = 3,
                },
            });
        }

        [Fact]
        public void JoinThenWhere()
        {
            var q = from ol in this._db.Table<TestOrderLine>()
                    join o in this._db.Table<TestOrder>() on ol.OrderId equals o.Id
                    where o.Id == 1
                    select new { o.Id, ol.ProductId, ol.Quantity };

            var r = q.ToList();

            r.Count.Should().Be(2);
        }

        [Fact]
        public void WhereThenJoin()
        {
            var q = from ol in this._db.Table<TestOrderLine>()
                    where ol.OrderId == 1
                    join o in this._db.Table<TestOrder>() on ol.OrderId equals o.Id
                    select new { o.Id, ol.ProductId, ol.Quantity };

            var r = q.ToList();

            r.Count.Should().Be(2);
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
