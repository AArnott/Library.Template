// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="AsyncJoinUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class AsyncJoinUnitTests.
    /// </summary>
    [PlatformTrait(typeof(AsyncJoinUnitTests))]

    public class AsyncJoinUnitTests : CoreTestCaseBase
    {
        private TestDbAsync<JoinUnitTests> _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncJoinUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public AsyncJoinUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._db = new TestDbAsync<JoinUnitTests>(this.TestFileSystem);
            this.InitializeDatabaseAsync().GetAwaiter().GetResult();
        }

        [Fact]
        public async Task AsyncJoin_DatabaseExistsAsync()
        {
            (await this._db.Table<TestProduct>().CountAsync()).Should().Be(3);
        }

        [Fact]
        public async Task AsyncJoin_ThenWhere()
        {
            List<TestOrderLineJoin> r = await this._db.QueryAsync<TestOrderLineJoin>("select ol.Id, ol.ProductId, ol.Quantity from TestOrderLine ol join TestOrder o on ol.OrderId == o.Id where o.Id == 1");

            r.Count.Should().Be(2);

            foreach (TestOrderLineJoin row in r)
            {
                this.TestOutputHelper.WriteLine($"Id={row.Id}, ProductId={row.ProductId}, Quantity={row.Quantity}");
                row.Id.Should().BeGreaterThan(0);
                row.ProductId.Should().BeGreaterThan(0);
                row.Quantity.Should().BeGreaterThan(0);
            }
        }

        [Fact]
        public async Task AsyncJoin_WhereThenJoin()
        {
            List<TestOrderLineJoin> r = await this._db.QueryAsync<TestOrderLineJoin>("select ol.Id, ol.ProductId, ol.Quantity from TestOrderLine ol join TestOrder o on ol.OrderId == o.Id where ol.OrderId == 1");

            r.Count.Should().Be(2);

            foreach (TestOrderLineJoin row in r)
            {
                this.TestOutputHelper.WriteLine($"Id={row.Id}, ProductId={row.ProductId}, Quantity={row.Quantity}");
                row.Id.Should().BeGreaterThan(0);
                row.ProductId.Should().BeGreaterThan(0);
                row.Quantity.Should().BeGreaterThan(0);
            }
        }

        protected async Task InitializeDatabaseAsync()
        {
            await this._db.CreateTableAsync<TestProduct>();
            await this._db.CreateTableAsync<TestOrder>();
            await this._db.CreateTableAsync<TestOrderLine>();

            var p1 = new TestProduct { Name = "One", };
            var p2 = new TestProduct { Name = "Two", };
            var p3 = new TestProduct { Name = "Three", };
            await this._db.InsertAllAsync(new[] { p1, p2, p3 });

            var o1 = new TestOrder { PlacedTime = DateTime.Now, };
            var o2 = new TestOrder { PlacedTime = DateTime.Now, };
            await this._db.InsertAllAsync(new[] { o1, o2 });

            await this._db.InsertAllAsync(new[]
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._db?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected record TestOrderLineJoin
        {
            public int Id { get; set; }

            public int ProductId { get; set; }

            public int Quantity { get; set; }
        }
    }
}
