// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="DropTableUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Exceptions;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Helpers;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteDropTableUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DropTableUnitTests))]

    public class DropTableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DropTableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DropTableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CreateInsertDrop()
        {
            using var db = new TestDbDrop(this.TestFileSystem);

            db.CreateTable<Product>();

            db.Insert(new Product
            {
                Name = "Hello",
                Price = 16,
            });

            var n = db.Table<Product>().Count();

            n.Should().Be(1);

            db.DropTable<Product>();

            ExceptionAssert.Throws<CoreSQLiteException>(() => db.Table<Product>().Count());
        }

        [Fact]
        public async Task CreateInsertDropAsync()
        {
            using var db = new TestDbDropAsync(this.TestFileSystem);

            await db.CreateTableAsync<Product>();

            await db.InsertAsync(new Product
            {
                Name = "Hello",
                Price = 16,
            });

            var n = await db.Table<Product>().CountAsync();

            n.Should().Be(1);

            await db.DropTableAsync<Product>();

            try
            {
                await db.Table<Product>().CountAsync();
                Assert.Fail("Should have thrown");
            }
            catch (CoreSQLiteException)
            {
                // Expected
            }
        }

        public class Product
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public string? Name { get; set; }

            public decimal Price { get; set; }
        }

        public class TestDbDrop : TestDbBase<DropTableUnitTests>
        {
            public TestDbDrop(ICoreFileSystem fileSystem)
                : base(fileSystem)
            {
            }
        }

        public class TestDbDropAsync : TestDbAsyncBase<DropTableUnitTests>
        {
            public TestDbDropAsync(ICoreFileSystem fileSystem)
                : base(fileSystem)
            {
            }
        }
    }
}
