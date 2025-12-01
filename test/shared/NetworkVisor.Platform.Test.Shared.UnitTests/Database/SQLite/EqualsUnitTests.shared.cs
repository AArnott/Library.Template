// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="EqualsUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteEqualsUnitTests.
    /// </summary>
    [PlatformTrait(typeof(EqualsUnitTests))]

    public class EqualsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EqualsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public EqualsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CanCompareAnyField()
        {
            int n = 20;
            IEnumerable<TestObjString> cq =
                from i in Enumerable.Range(1, n)
                select new TestObjString { Data = Convert.ToString(i), Date = new DateTime(2013, 1, i), };

            using var db = new TestDb<EqualsUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObjString>();
            db.InsertAll(cq);

            TableQuery<TestObjString> results = db.Table<TestObjString>().Where(o => o.Data!.Equals("10"));
            results.Count().Should().Be(1);
            results.FirstOrDefault()!.Data.Should().Be("10");

            results = db.Table<TestObjString>().Where(o => o.Id.Equals(10));
            results.Count().Should().Be(1);
            results.FirstOrDefault()!.Data.Should().Be("10");

            var date = new DateTime(2013, 1, 10);
            results = db.Table<TestObjString>().Where(o => o.Date.Equals(date));
            results.Count().Should().Be(1);
            results.FirstOrDefault()!.Data.Should().Be("10");
        }

        public abstract class TestObjBase<T>
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public T? Data { get; set; }

            public DateTime Date { get; set; }
        }

        public class TestObjString : TestObjBase<string>
        {
        }
    }
}
