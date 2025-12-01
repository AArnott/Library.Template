// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="ContainsUnitTests.shared.cs" company="Network Visor">
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
    /// Class ContainsUnitTests.
    /// </summary>
    [PlatformTrait(typeof(ContainsUnitTests))]

    public class ContainsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ContainsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public ContainsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void ContainsConstantData()
        {
            int n = 20;
            IEnumerable<TestObj> cq = from i in Enumerable.Range(1, n)
                                      select new TestObj()
                                      {
                                          Name = i.ToString(),
                                      };

            using var db = new TestDb<ContainsUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>();

            db.InsertAll(cq);

            string[] tensq = new string[] { "0", "10", "20" };
            var tens = (from o in db.Table<TestObj>() where tensq.Contains(o.Name) select o).ToList();
            tens.Count.Should().Be(2);

            string[] moreq = new string[] { "0", "x", "99", "10", "20", "234324" };
            var more = (from o in db.Table<TestObj>() where moreq.Contains(o.Name) select o).ToList();
            more.Count.Should().Be(2);
        }

        [Fact]
        public void ContainsQueriedData()
        {
            int n = 20;
            IEnumerable<TestObj> cq = from i in Enumerable.Range(1, n)
                                      select new TestObj()
                                      {
                                          Name = i.ToString(),
                                      };

            using var db = new TestDb<ContainsUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>();

            db.InsertAll(cq);

            string[] tensq = new string[] { "0", "10", "20" };
            var tens = (from o in db.Table<TestObj>() where tensq.Contains(o.Name) select o).ToList();
            tens.Count.Should().Be(2);

            string[] moreq = new string[] { "0", "x", "99", "10", "20", "234324" };
            var more = (from o in db.Table<TestObj>() where moreq.Contains(o.Name) select o).ToList();
            more.Count.Should().Be(2);

            // https://github.com/praeclarum/sqlite-net/issues/28
            List<string> moreq2 = moreq.ToList();
            var more2 = (from o in db.Table<TestObj>() where moreq2.Contains(o.Name!) select o).ToList();
            more2.Count.Should().Be(2);
        }

        public class TestObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public string? Name { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}, Name={this.Name}]";
            }
        }
    }
}
