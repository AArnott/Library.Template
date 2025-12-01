// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="StringQueryUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreSQLiteReadmeUnitTests.
    /// </summary>
    [PlatformTrait(typeof(StringQueryUnitTests))]

    public class StringQueryUnitTests : CoreTestCaseBase
    {
        private TestDb<StringQueryUnitTests> _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringQueryUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public StringQueryUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._db = new TestDb<StringQueryUnitTests>(this.TestFileSystem);
            this._db.CreateTable<TestProduct>();

            TestProduct[] prods = new[]
            {
                new TestProduct { Name = "Foo" },
                new TestProduct { Name = "Bar" },
                new TestProduct { Name = "Foobar" },
                new TestProduct { Name = null, Price = 100 },
                new TestProduct { Name = string.Empty, Price = 1000 },
            };

            this._db.InsertAll(prods);
        }

        [Fact]
        public void StringEquals()
        {
            // C#: x => x.Name == "Foo"
            var fs = this._db.Table<TestProduct>().Where(x => x.Name == "Foo").ToList();
            fs.Count.Should().Be(1);
        }

        [Fact]
        public void StartsWith()
        {
            var fs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.StartsWith("F")).ToList();
            fs.Count.Should().Be(2);

            var lfs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.StartsWith("f")).ToList();
            lfs.Count.Should().Be(0);

            var lfs2 = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.StartsWith("f", StringComparison.OrdinalIgnoreCase)).ToList();
            lfs2.Count.Should().Be(2);

            var bs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.StartsWith("B")).ToList();
            bs.Count.Should().Be(1);
        }

        [Fact]
        public void EndsWith()
        {
            var fs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.EndsWith("ar")).ToList();
            fs.Count.Should().Be(2);

            var lfs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.EndsWith("Ar")).ToList();
            lfs.Count.Should().Be(0);

            var bs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.EndsWith("o")).ToList();
            bs.Count.Should().Be(1);
        }

        [Fact]
        public void Contains()
        {
            var fs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.Contains("o")).ToList();
            fs.Count.Should().Be(2);

            var lfs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.Contains("O")).ToList();
            lfs.Count.Should().Be(0);

            var lfsu = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.ToUpper().Contains("O")).ToList();
            lfsu.Count.Should().Be(2);

            var bs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.Contains("a")).ToList();
            bs.Count.Should().Be(2);

            var zs = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name) && x.Name!.Contains("z")).ToList();
            zs.Count.Should().Be(0);
        }

        [Fact]
        public void IsNullOrEmpty()
        {
            var isNullorEmpty = this._db.Table<TestProduct>().Where(x => string.IsNullOrEmpty(x.Name)).ToList();
            isNullorEmpty.Count.Should().Be(2);

            var isNotnNllorEmpty = this._db.Table<TestProduct>().Where(x => !string.IsNullOrEmpty(x.Name)).ToList();
            isNotnNllorEmpty.Count.Should().Be(3);
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
