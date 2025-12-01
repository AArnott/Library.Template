// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="UnicodeUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreSQLiteTransactionUnitTests.
    /// </summary>
    [PlatformTrait(typeof(UnicodeUnitTests))]

    public class UnicodeUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnicodeUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public UnicodeUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void Insert()
        {
            using var db = new TestDb<UnicodeUnitTests>(this.TestFileSystem);

            db.CreateTable<TestProduct>();

            string testString = "\u2329\u221E\u232A";

            db.Insert(new TestProduct
            {
                Name = testString,
            });

            TestProduct? p = db.Get<TestProduct>(1);
            p.Should().NotBeNull();

            p!.Name.Should().Be(testString);
        }

        [Fact]
        public void Query()
        {
            using var db = new TestDb<UnicodeUnitTests>(this.TestFileSystem);

            db.CreateTable<TestProduct>();

            string testString = "\u2329\u221E\u232A";

            db.Insert(new TestProduct
            {
                Name = testString,
            });

            var ps = (from p in db.Table<TestProduct>() where p.Name == testString select p).ToList();
            ps.Should().NotBeNull();

            ps.Count.Should().Be(1);
            ps[0].Name.Should().Be(testString);
        }
    }
}
