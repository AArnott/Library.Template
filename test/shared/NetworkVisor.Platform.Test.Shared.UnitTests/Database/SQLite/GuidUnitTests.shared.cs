// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="GuidUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteGuidUnitTests.
    /// </summary>
    [PlatformTrait(typeof(GuidUnitTests))]

    public class GuidUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GuidUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public GuidUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void ShouldPersistAndReadGuid()
        {
            using var db = new TestDb<GuidUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>();

            var obj1 = new TestObj() { Id = new Guid("36473164-c9e4-4cdf-b266-a0b287c85623"), Text = "First Guid Object" };
            var obj2 = new TestObj() { Id = new Guid("bc5c4c4a-ca57-4b61-8b53-9fd4673528b6"), Text = "Second Guid Object" };

            var numIn1 = db.Insert(obj1);
            var numIn2 = db.Insert(obj2);
            numIn1.Should().Be(1);
            numIn2.Should().Be(1);

            var result = db.Query<TestObj>("select * from TestObj").ToList();
            result.Count.Should().Be(2);
            result[0].Text.Should().Be(obj1.Text);
            result[1].Text.Should().Be(obj2.Text);

            result[0].Id.Should().Be(obj1.Id);
            result[1].Id.Should().Be(obj2.Id);

            db.Close();
        }

        [Fact]
        public void AutoGuid_HasGuid()
        {
            using var db = new TestDb<GuidUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>(CreateFlags.AutoIncPK);

            var guid1 = new Guid("36473164-c9e4-4cdf-b266-a0b287c85623");
            var guid2 = new Guid("bc5c4c4a-ca57-4b61-8b53-9fd4673528b6");

            var obj1 = new TestObj() { Id = guid1, Text = "First Guid Object" };
            var obj2 = new TestObj() { Id = guid2, Text = "Second Guid Object" };

            var numIn1 = db.Insert(obj1);
            var numIn2 = db.Insert(obj2);
            obj1.Id.Should().Be(guid1);
            obj2.Id.Should().Be(guid2);

            db.Close();
        }

        [Fact]
        public void AutoGuid_EmptyGuid()
        {
            using var db = new TestDb<GuidUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>(CreateFlags.AutoIncPK);

            var guid1 = new Guid("36473164-c9e4-4cdf-b266-a0b287c85623");
            var guid2 = new Guid("bc5c4c4a-ca57-4b61-8b53-9fd4673528b6");

            var obj1 = new TestObj() { Text = "First Guid Object" };
            var obj2 = new TestObj() { Text = "Second Guid Object" };

            obj1.Id.Should().Be(Guid.Empty);
            obj2.Id.Should().Be(Guid.Empty);

            var numIn1 = db.Insert(obj1);
            var numIn2 = db.Insert(obj2);
            obj1.Id.Should().NotBe(Guid.Empty);
            obj2.Id.Should().NotBe(Guid.Empty);
            obj2.Id.Should().NotBe(obj1.Id);

            db.Close();
        }

        public class TestObj
        {
            [PrimaryKey]
            public Guid Id { get; set; }

            public string? Text { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}, Text={this.Text}]";
            }
        }
    }
}
