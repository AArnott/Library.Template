// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="DbCommandUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteDbCommandUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DbCommandUnitTests))]

    public class DbCommandUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DbCommandUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DbCommandUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void QueryCommand()
        {
            using var db = new TestDb<DbCommandUnitTests>(this.TestFileSystem, true);
            db.CreateTable<TestProduct>();
            var b = new TestProduct();
            db.Insert(b);

            var test = db.CreateCommand("select * from TestProduct")
                .ExecuteDeferredQuery<TestProduct>(new TableMapping(typeof(TestProduct))).ToList();

            test.Count.Should().Be(1);
        }

        [Fact]
        public void QueryCommand_Id()
        {
            using var db = new TestDb<DbCommandUnitTests>(this.TestFileSystem, true);
            db.CreateTable<TestProduct>();
            var b = new TestProduct()
            {
                Id = 1,
                Name = "Test",
            };
            db.Insert(b);
            db.Insert(b);

            var test = db.CreateCommand("select Id from TestProduct")
                .ExecuteDeferredQuery<TestProduct>(new TableMapping(typeof(TestProduct))).ToList();

            test.Count.Should().Be(2);
            this.TestOutputHelper.WriteLine(string.Join(", ", test.Select(r => r.Id)));
        }

        [Fact]
        public void QueryCommandCastToObject()
        {
            using var db = new TestDb<DbCommandUnitTests>(this.TestFileSystem, true);
            db.CreateTable<TestProduct>();
            var b = new TestProduct();
            db.Insert(b);

            var test = db.CreateCommand("select * from TestProduct")
                .ExecuteDeferredQuery<object>(new TableMapping(typeof(TestProduct))).ToList();

            test.Count.Should().Be(1);
        }
    }
}
