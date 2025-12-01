// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CollateUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
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
    /// Class CollateUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CollateUnitTests))]

    public class CollateUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CollateUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CollateUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void Collate()
        {
            var obj = new TestObj()
            {
                CollateDefault = "Alpha ",
                CollateBinary = "Alpha ",
                CollateRTrim = "Alpha ",
                CollateNoCase = "Alpha ",
            };

            using var db = new TestDb<CollateUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>();

            db.Insert(obj);

            (from o in db.Table<TestObj>() where o.CollateDefault == "Alpha " select o).Count().Should().Be(1);
            (from o in db.Table<TestObj>() where o.CollateDefault == "ALPHA " select o).Count().Should().Be(0);
            (from o in db.Table<TestObj>() where o.CollateDefault == "Alpha" select o).Count().Should().Be(0);
            (from o in db.Table<TestObj>() where o.CollateDefault == "ALPHA" select o).Count().Should().Be(0);

            (from o in db.Table<TestObj>() where o.CollateBinary == "Alpha " select o).Count().Should().Be(1);
            (from o in db.Table<TestObj>() where o.CollateBinary == "ALPHA " select o).Count().Should().Be(0);
            (from o in db.Table<TestObj>() where o.CollateBinary == "Alpha" select o).Count().Should().Be(0);
            (from o in db.Table<TestObj>() where o.CollateBinary == "ALPHA" select o).Count().Should().Be(0);

            (from o in db.Table<TestObj>() where o.CollateRTrim == "Alpha " select o).Count().Should().Be(1);
            (from o in db.Table<TestObj>() where o.CollateRTrim == "ALPHA " select o).Count().Should().Be(0);
            (from o in db.Table<TestObj>() where o.CollateRTrim == "Alpha" select o).Count().Should().Be(1);
            (from o in db.Table<TestObj>() where o.CollateRTrim == "ALPHA" select o).Count().Should().Be(0);

            (from o in db.Table<TestObj>() where o.CollateNoCase == "Alpha " select o).Count().Should().Be(1);
            (from o in db.Table<TestObj>() where o.CollateNoCase == "ALPHA " select o).Count().Should().Be(1);
            (from o in db.Table<TestObj>() where o.CollateNoCase == "Alpha" select o).Count().Should().Be(0);
            (from o in db.Table<TestObj>() where o.CollateNoCase == "ALPHA" select o).Count().Should().Be(0);
        }

        public class TestObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public string? CollateDefault { get; set; }

            [Collation("BINARY")]
            public string? CollateBinary { get; set; }

            [Collation("RTRIM")]
            public string? CollateRTrim { get; set; }

            [Collation("NOCASE")]
            public string? CollateNoCase { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}]";
            }
        }
    }
}
