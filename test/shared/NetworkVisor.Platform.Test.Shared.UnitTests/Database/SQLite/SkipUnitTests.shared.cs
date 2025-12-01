// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="SkipUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
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
    /// Class CoreSQLiteSkipUnitTests.
    /// </summary>
    [PlatformTrait(typeof(SkipUnitTests))]

    public class SkipUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SkipUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public SkipUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void Skip()
        {
            int n = 100;

            IEnumerable<TestObj> cq = from i in Enumerable.Range(1, n)
                                      select new TestObj()
                                      {
                                          Order = i,
                                      };
            TestObj[] objs = cq.ToArray();
            using var db = new TestDb<SkipUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>();

            int numIn = db.InsertAll(objs);
            n.Should().Be(numIn, "Num inserted must = num objects");

            TableQuery<TestObj> q = from o in db.Table<TestObj>()
                                    orderby o.Order
                                    select o;

            TableQuery<TestObj> qs1 = q.Skip(1);
            var s1 = qs1.ToList();
            s1.Count.Should().Be(n - 1);
            s1[0].Order.Should().Be(2);

            TableQuery<TestObj> qs5 = q.Skip(5);
            var s5 = qs5.ToList();
            s5.Count.Should().Be(n - 5);
            s5[0].Order.Should().Be(6);
        }

        public class TestObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public int Order { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}, Order={this.Order}]";
            }
        }
    }
}
