// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="IgnoreUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
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
    /// Class CoreSQLiteIgnoreUnitTests.
    /// </summary>
    [PlatformTrait(typeof(IgnoreUnitTests))]

    public class IgnoreUnitTests : CoreTestCaseBase
    {
        private const string DefaultSQLiteDateTimeString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";

        /// <summary>
        /// Initializes a new instance of the <see cref="IgnoreUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public IgnoreUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void MappingIgnoreColumn()
        {
            using var db = new TestDb<IgnoreUnitTests>(this.TestFileSystem);
            TableMapping? m = db.GetMapping<TestObj>();
            m.Should().NotBeNull();

            m!.Columns.Length.Should().Be(2);
        }

        [Fact]
        public void CreateTableSucceeds()
        {
            using var db = new TestDb<IgnoreUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>();
        }

        [Fact]
        public void InsertSucceeds()
        {
            using var db = new TestDb<IgnoreUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>();

            var o = new TestObj
            {
                Text = "Hello",
                IgnoredText = "World",
            };

            db.Insert(o);

            o.Id.Should().Be(1);
        }

        [Fact]
        public void GetDoesntHaveIgnores()
        {
            using var db = new TestDb<IgnoreUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>();

            var o = new TestObj
            {
                Text = "Hello",
                IgnoredText = "World",
            };

            db.Insert(o);

            TestObj? oo = db.Get<TestObj>(o.Id);

            oo.Should().NotBeNull();
            oo!.Text.Should().Be("Hello");
            oo.IgnoredText.Should().BeNull();
        }

        [Fact]
        public void BaseIgnores()
        {
            using var db = new TestDb<IgnoreUnitTests>(this.TestFileSystem);
            db.CreateTable<TableClass>();

            var o = new TableClass
            {
                ToIgnore = "Hello",
                Name = "World",
            };

            db.Insert(o);

            TableClass oo = db.Table<TableClass>().First();

            oo.ToIgnore.Should().BeNull();
            oo.Name.Should().Be("World");
        }

        [Fact]
        public void RedefinedIgnores()
        {
            using var db = new TestDb<IgnoreUnitTests>(this.TestFileSystem);
            db.CreateTable<RedefinedClass>();

            var o = new RedefinedClass
            {
                Name = "Foo",
                Value = "Bar",
                Values = ["hello", "world"],
            };

            db.Insert(o);

            RedefinedClass oo = db.Table<RedefinedClass>().First();

            oo.Name.Should().Be("Foo");
            oo.Value.Should().Be("Bar");
            oo.Values.Should().BeNull();
        }

        [Fact]
        public void DerivedIgnore()
        {
            using var db = new TestDb<IgnoreUnitTests>(this.TestFileSystem);
            db.CreateTable<DerivedIgnoreClass>();

            var o = new DerivedIgnoreClass
            {
                Ignored = "Hello",
                NotIgnored = "World",
            };

            db.Insert(o);

            DerivedIgnoreClass oo = db.Table<DerivedIgnoreClass>().First();

            oo.Ignored.Should().BeNull();
            oo.NotIgnored.Should().Be("World");
        }

        public class BaseClass
        {
            [Ignore]
            public string? ToIgnore
            {
                get;
                set;
            }
        }

        public class TableClass : BaseClass
        {
            public string? Name { get; set; }
        }

        public class TestObj
        {
            protected Dictionary<int, string> _edibles = [];

            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public string? Text { get; set; }

            [Ignore]
            public Dictionary<int, string> Edibles
            {
                get { return this._edibles; }
                set { this._edibles = value; }
            }

            [Ignore]
            public string? IgnoredText { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}]";
            }
        }

        public class RedefinedBaseClass
        {
            public string? Name { get; set; }

            public List<string>? Values { get; set; }
        }

        public class RedefinedClass : RedefinedBaseClass
        {
            [Ignore]
            public new List<string>? Values { get; set; }

            public string? Value { get; set; }
        }

        [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
        private class DerivedIgnoreAttribute : IgnoreAttribute
        {
        }

        private class DerivedIgnoreClass
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string? NotIgnored { get; set; }

            [DerivedIgnore]
            public string? Ignored { get; set; }
        }
    }
}
