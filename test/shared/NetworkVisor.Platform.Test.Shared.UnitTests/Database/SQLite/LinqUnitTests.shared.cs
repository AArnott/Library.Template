// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="LinqUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteLinqUnitTests.
    /// </summary>
    [PlatformTrait(typeof(LinqUnitTests))]

    public class LinqUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinqUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public LinqUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public interface IEntityNoSetter
        {
            int Id { get; }

            string? Value { get; }
        }

        public interface IEntitySetter
        {
            int Id { get; set; }

            string? Value { get; set; }
        }

        [Fact]
        public void FunctionParameter()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();

            db.Insert(new TestProduct
            {
                Name = "A",
                Price = 20,
            });

            db.Insert(new TestProduct
            {
                Name = "B",
                Price = 10,
            });

            List<TestProduct> GetProductsWithPriceAtLeast(decimal val)
            {
                return (from p in db.Table<TestProduct>() where p.Price > val select p).ToList();
            }

            List<TestProduct> r = GetProductsWithPriceAtLeast(15);
            r.Count.Should().Be(1);
            r[0].Name.Should().Be("A");
        }

        [Fact]
        public void WhereGreaterThan()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();

            db.Insert(new TestProduct
            {
                Name = "A",
                Price = 20,
            });

            db.Insert(new TestProduct
            {
                Name = "B",
                Price = 10,
            });

            db.Table<TestProduct>().Count().Should().Be(2);

            var r = (from p in db.Table<TestProduct>() where p.Price > 15 select p).ToList();
            r.Count.Should().Be(1);
            r[0].Name.Should().Be("A");
        }

        [Fact]
        public void GetWithExpression()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();

            db.Insert(new TestProduct
            {
                Name = "A",
                Price = 20,
            });

            db.Insert(new TestProduct
            {
                Name = "B",
                Price = 10,
            });

            db.Insert(new TestProduct
            {
                Name = "C",
                Price = 5,
            });

            db.Table<TestProduct>().Count().Should().Be(3);

            TestProduct? r = db.Get<TestProduct>(x => x.Price == 10);
            r.Should().NotBeNull();
            r!.Name.Should().Be("B");
        }

        [Fact]
        public void FindWithExpression()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();

            TestProduct? r = db.Find<TestProduct>(x => x.Price == 10);
            r.Should().BeNull();
        }

        [Fact]
        public void OrderByCast()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();

            db.Insert(new TestProduct
            {
                Name = "A",
                TotalSales = 1,
            });
            db.Insert(new TestProduct
            {
                Name = "B",
                TotalSales = 100,
            });

            var noCast = (from p in db.Table<TestProduct>() orderby p.TotalSales descending select p).ToList();
            noCast.Count.Should().Be(2);
            noCast[0].Name.Should().Be("B");

            var cast = (from p in db.Table<TestProduct>() orderby (int)p.TotalSales descending select p).ToList();
            cast.Count.Should().Be(2);
            cast[0].Name.Should().Be("B");
        }

        [Fact]
        public void Issue96_NullabelIntsInQueries()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();
            db.CreateTable<Issue96_A>();

            int id = 42;

            db.Insert(new Issue96_A
            {
                ClassB = id,
            });
            db.Insert(new Issue96_A
            {
                ClassB = null,
            });
            db.Insert(new Issue96_A
            {
                ClassB = null,
            });
            db.Insert(new Issue96_A
            {
                ClassB = null,
            });

            db.Table<Issue96_A>().Where(p => p.ClassB == id).Count().Should().Be(1);
            db.Table<Issue96_A>().Where(p => p.ClassB == null).Count().Should().Be(3);
        }

        [Fact]
        public void Issue303_WhereNot_A()
        {
            using var db = new TestDb<LinqUnitTests>(this.TestFileSystem);
            db.CreateTable<Issue303_A>();
            db.Insert(new Issue303_A { Id = 1, Name = "aa" });
            db.Insert(new Issue303_A { Id = 2, Name = null });
            db.Insert(new Issue303_A { Id = 3, Name = "test" });
            db.Insert(new Issue303_A { Id = 4, Name = null });

            var r = (from p in db.Table<Issue303_A>() where !(p.Name == null) select p).ToList();
            r.Count.Should().Be(2);
            r[0].Id.Should().Be(1);
            r[1].Id.Should().Be(3);
        }

        [Fact]
        public void Issue303_WhereNot_B()
        {
            using var db = new TestDb<LinqUnitTests>(this.TestFileSystem);
            db.CreateTable<Issue303_B>();
            db.Insert(new Issue303_B { Id = 1, Flag = true });
            db.Insert(new Issue303_B { Id = 2, Flag = false });
            db.Insert(new Issue303_B { Id = 3, Flag = true });
            db.Insert(new Issue303_B { Id = 4, Flag = false });

            var r = (from p in db.Table<Issue303_B>() where !p.Flag select p).ToList();
            r.Count.Should().Be(2);
            r[0].Id.Should().Be(2);
            r[1].Id.Should().Be(4);
        }

        [Fact]
        public void QuerySelectAverage()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();

            db.Insert(new TestProduct
            {
                Name = "A",
                Price = 20,
                TotalSales = 100,
            });

            db.Insert(new TestProduct
            {
                Name = "B",
                Price = 10,
                TotalSales = 100,
            });

            db.Insert(new TestProduct
            {
                Name = "C",
                Price = 1000,
                TotalSales = 1,
            });

            decimal r = db.Table<TestProduct>().Where(x => x.TotalSales > 50).Select(s => s.Price).Average();

            r.Should().Be(15m);
        }

        [Fact]
        public void CastedParameters_NoSetter()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();
            db.CreateTable<EntityNoSetter>();

            db.Insert(new EntityNoSetter
            {
                Value = "Foo",
            });

            EntityNoSetter? r = GetEntityNoSetter<EntityNoSetter>(db, 1);

            r.Should().NotBeNull();
            r!.Value.Should().Be("Foo");
        }

        [Fact]
        public void CastedParameters_Setter()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();
            db.CreateTable<EntitySetter>();

            db.Insert(new EntitySetter
            {
                Value = "Foo",
            });

            var fx = new Func<EntitySetter?>(() => GetEntitySetter<EntitySetter>(db, 1));

            // Throws on IOS and MacCatalyst running in AOT only model
            // Setter on IEntitySetter interface cannot be JITed
            if (this.TestOperatingSystem.IsAppleMobileOS && false)
            {
#pragma warning disable CS0618 // Type or member is obsolete
                _ = fx.Should().Throw<ExecutionEngineException>();
#pragma warning restore CS0618 // Type or member is obsolete
            }
            else
            {
                EntitySetter? r = fx();

                r.Should().NotBeNull();
                r!.Value.Should().Be("Foo");
            }
        }

        [Fact]
        public void Issue460_ReplaceWith2Args()
        {
            using TestDb<LinqUnitTests> db = this.CreateDb();

            db.Insert(new TestProduct
            {
                Name = "I am not B X B",
            });
            db.Insert(new TestProduct
            {
                Name = "I am B O B",
            });

            TestProduct? cl = (from c in db.Table<TestProduct>()
                               where !string.IsNullOrEmpty(c.Name) && c.Name!.Replace(" ", string.Empty).Contains("BOB")
                               select c).FirstOrDefault();

            cl.Should().NotBeNull();
            cl!.Id.Should().Be(2);
            cl.Name.Should().Be("I am B O B");
        }

        private static T? GetEntityNoSetter<T>(TestDb<LinqUnitTests> db, int id)
            where T : IEntityNoSetter, new()
        {
            return db.Table<T>().FirstOrDefault(x => x.Id == id);
        }

        private static T? GetEntitySetter<T>(TestDb<LinqUnitTests> db, int id)
            where T : IEntitySetter, new()
        {
            return db.Table<T>().FirstOrDefault(x => x.Id == id);
        }

        private TestDb<LinqUnitTests> CreateDb()
        {
            var db = new TestDb<LinqUnitTests>(this.TestFileSystem);
            db.CreateTable<TestProduct>();
            db.CreateTable<TestOrder>();
            db.CreateTable<TestOrderLine>();
            db.CreateTable<TestOrderHistory>();
            return db;
        }

        public class Issue96_A
        {
            [AutoIncrement, PrimaryKey]
            public int ID { get; set; }

            public string? AddressLine { get; set; }

            [Indexed]
            public int? ClassB { get; set; }

            [Indexed]
            public int? ClassC { get; set; }
        }

        public class Issue96_B
        {
            [AutoIncrement, PrimaryKey]
            public int ID { get; set; }

            public string? CustomerName { get; set; }
        }

        public class Issue96_C
        {
            [AutoIncrement, PrimaryKey]
            public int ID { get; set; }

            public string? SupplierName { get; set; }
        }

        public class Issue303_A
        {
            [PrimaryKey, NotNull]
            public int Id { get; set; }

            public string? Name { get; set; }
        }

        public class Issue303_B
        {
            [PrimaryKey, NotNull]
            public int Id { get; set; }

            public bool Flag { get; set; }
        }

        public class EntityNoSetter : IEntityNoSetter
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public string? Value { get; set; }
        }

        public class EntitySetter : IEntitySetter
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public string? Value { get; set; }
        }
    }
}
