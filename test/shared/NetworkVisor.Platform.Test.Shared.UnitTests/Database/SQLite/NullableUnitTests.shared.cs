// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="NullableUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.ComponentModel;
using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteNullableUnitTests.
    /// </summary>
    [PlatformTrait(typeof(NullableUnitTests))]

    public class NullableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NullableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public NullableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public enum TestIntEnum
        {
            One = 1,
            Two = 2,
        }

        [StoreAsText]
        public enum TestTextEnum
        {
            Alpha,
            Beta,
        }

        [Fact]
        [Description("Create a table with a nullable int column then insert and select against it")]
        public void NullableInt()
        {
            using var db = new TestDb<NullableUnitTests>(this.TestFileSystem);
            db.CreateTable<NullableIntClass>();

            var withNull = new NullableIntClass() { NullableInt = null };
            var with0 = new NullableIntClass() { NullableInt = 0 };
            var with1 = new NullableIntClass() { NullableInt = 1 };
            var withMinus1 = new NullableIntClass() { NullableInt = -1 };

            db.Insert(withNull);
            db.Insert(with0);
            db.Insert(with1);
            db.Insert(withMinus1);

            NullableIntClass[] results = db.Table<NullableIntClass>().OrderBy(x => x.ID).ToArray();

            results.Length.Should().Be(4);

            results[0].Should().Be(withNull);
            results[1].Should().Be(with0);
            results[2].Should().Be(with1);
            results[3].Should().Be(withMinus1);
        }

        [Fact]
        [Description("Create a table with a nullable int column then insert and select against it")]
        public void NullableFloat()
        {
            using var db = new TestDb<NullableUnitTests>(this.TestFileSystem);
            db.CreateTable<NullableFloatClass>();

            var withNull = new NullableFloatClass() { NullableFloat = null };
            var with0 = new NullableFloatClass() { NullableFloat = 0 };
            var with1 = new NullableFloatClass() { NullableFloat = 1 };
            var withMinus1 = new NullableFloatClass() { NullableFloat = -1 };

            db.Insert(withNull);
            db.Insert(with0);
            db.Insert(with1);
            db.Insert(withMinus1);

            NullableFloatClass[] results = db.Table<NullableFloatClass>().OrderBy(x => x.ID).ToArray();

            results.Length.Should().Be(4);

            results[0].Should().Be(withNull);
            results[1].Should().Be(with0);
            results[2].Should().Be(with1);
            results[3].Should().Be(withMinus1);
        }

        [Fact]
        public void NullableString()
        {
            using var db = new TestDb<NullableUnitTests>(this.TestFileSystem);
            db.CreateTable<StringClass>();

            var withNull = new StringClass() { StringData = null };
            var withEmpty = new StringClass() { StringData = string.Empty };
            var withData = new StringClass() { StringData = "data" };

            db.Insert(withNull);
            db.Insert(withEmpty);
            db.Insert(withData);

            StringClass[] results = db.Table<StringClass>().OrderBy(x => x.ID).ToArray();

            results.Length.Should().Be(3);

            results[0].Should().Be(withNull);
            results[1].Should().Be(withEmpty);
            results[2].Should().Be(withData);
        }

        [Fact]
        public void WhereNotNull()
        {
            using var db = new TestDb<NullableUnitTests>(this.TestFileSystem);
            db.CreateTable<NullableIntClass>();

            var withNull = new NullableIntClass() { NullableInt = null };
            var with0 = new NullableIntClass() { NullableInt = 0 };
            var with1 = new NullableIntClass() { NullableInt = 1 };
            var withMinus1 = new NullableIntClass() { NullableInt = -1 };

            db.Insert(withNull);
            db.Insert(with0);
            db.Insert(with1);
            db.Insert(withMinus1);

            NullableIntClass[] results = db.Table<NullableIntClass>().Where(x => x.NullableInt != null).OrderBy(x => x.ID).ToArray();

            results.Length.Should().Be(3);

            results[0].Should().Be(with0);
            results[1].Should().Be(with1);
            results[2].Should().Be(withMinus1);
        }

        [Fact]
        public void WhereNull()
        {
            using var db = new TestDb<NullableUnitTests>(this.TestFileSystem);
            db.CreateTable<NullableIntClass>();

            var withNull = new NullableIntClass() { NullableInt = null };
            var with0 = new NullableIntClass() { NullableInt = 0 };
            var with1 = new NullableIntClass() { NullableInt = 1 };
            var withMinus1 = new NullableIntClass() { NullableInt = -1 };

            db.Insert(withNull);
            db.Insert(with0);
            db.Insert(with1);
            db.Insert(withMinus1);

            NullableIntClass[] results = db.Table<NullableIntClass>().Where(x => x.NullableInt == null).OrderBy(x => x.ID).ToArray();

            results.Length.Should().Be(1);
            results[0].Should().Be(withNull);
        }

        [Fact]
        public void StringWhereNull()
        {
            using var db = new TestDb<NullableUnitTests>(this.TestFileSystem);
            db.CreateTable<StringClass>();

            var withNull = new StringClass() { StringData = null };
            var withEmpty = new StringClass() { StringData = string.Empty };
            var withData = new StringClass() { StringData = "data" };

            db.Insert(withNull);
            db.Insert(withEmpty);
            db.Insert(withData);

            StringClass[] results = db.Table<StringClass>().Where(x => x.StringData == null).OrderBy(x => x.ID).ToArray();
            results.Length.Should().Be(1);
            results[0].Should().Be(withNull);
        }

        [Fact]
        public void StringWhereNotNull()
        {
            using var db = new TestDb<NullableUnitTests>(this.TestFileSystem);
            db.CreateTable<StringClass>();

            var withNull = new StringClass() { StringData = null };
            var withEmpty = new StringClass() { StringData = string.Empty };
            var withData = new StringClass() { StringData = "data" };

            db.Insert(withNull);
            db.Insert(withEmpty);
            db.Insert(withData);

            StringClass[] results = db.Table<StringClass>().Where(x => x.StringData != null).OrderBy(x => x.ID).ToArray();
            results.Length.Should().Be(2);
            results[0].Should().Be(withEmpty);
            results[1].Should().Be(withData);
        }

        [Fact]
        [Description("Create a table with a nullable enum column then insert and select against it")]
        public void NullableEnum()
        {
            using var db = new TestDb<NullableUnitTests>(this.TestFileSystem);
            db.CreateTable<NullableEnumClass>();

            var withNull = new NullableEnumClass { NullableIntEnum = null, NullableTextEnum = null };
            var with1 = new NullableEnumClass { NullableIntEnum = TestIntEnum.One, NullableTextEnum = null };
            var with2 = new NullableEnumClass { NullableIntEnum = TestIntEnum.Two, NullableTextEnum = null };
            var withNullA = new NullableEnumClass { NullableIntEnum = null, NullableTextEnum = TestTextEnum.Alpha };
            var with1B = new NullableEnumClass { NullableIntEnum = TestIntEnum.One, NullableTextEnum = TestTextEnum.Beta };

            db.Insert(withNull);
            db.Insert(with1);
            db.Insert(with2);
            db.Insert(withNullA);
            db.Insert(with1B);

            NullableEnumClass[] results = db.Table<NullableEnumClass>().OrderBy(x => x.ID).ToArray();

            results.Length.Should().Be(5);

            results[0].Should().Be(withNull);
            results[1].Should().Be(with1);
            results[2].Should().Be(with2);
            results[3].Should().Be(withNullA);
            results[4].Should().Be(with1B);
        }

        public class NullableIntClass
        {
            [PrimaryKey, AutoIncrement]
            public int ID { get; set; }

            public int? NullableInt { get; set; }

            public override bool Equals(object? obj)
            {
                var other = (NullableIntClass?)obj;
                return this.ID == other?.ID && this.NullableInt == other.NullableInt;
            }

            public override int GetHashCode()
            {
                return this.ID.GetHashCode() + this.NullableInt.GetHashCode();
            }
        }

        public class NullableFloatClass
        {
            [PrimaryKey, AutoIncrement]
            public int ID { get; set; }

            public float? NullableFloat { get; set; }

            public override bool Equals(object? obj)
            {
                var other = (NullableFloatClass?)obj;
                return this.ID == other?.ID && this.NullableFloat == other.NullableFloat;
            }

            public override int GetHashCode()
            {
                return this.ID.GetHashCode() + this.NullableFloat?.GetHashCode() ?? 0;
            }
        }

        public class StringClass
        {
            [PrimaryKey, AutoIncrement]
            public int ID { get; set; }

            // Strings are allowed to be null by default
            public string? StringData { get; set; }

            public override bool Equals(object? obj)
            {
                var other = (StringClass?)obj;
                return this.ID == other?.ID && this.StringData == other.StringData;
            }

            public override int GetHashCode()
            {
                return this.ID.GetHashCode() + (this.StringData?.GetHashCode() ?? 0);
            }
        }

        public class NullableEnumClass
        {
            [PrimaryKey, AutoIncrement]
            public int ID { get; set; }

            public TestIntEnum? NullableIntEnum { get; set; }

            public TestTextEnum? NullableTextEnum { get; set; }

            public override bool Equals(object? obj)
            {
                var other = (NullableEnumClass?)obj;
                return this.ID == other?.ID && this.NullableIntEnum == other.NullableIntEnum && this.NullableTextEnum == other.NullableTextEnum;
            }

            public override int GetHashCode()
            {
                return this.ID.GetHashCode() + this.NullableIntEnum?.GetHashCode() ?? 0 + this.NullableTextEnum?.GetHashCode() ?? 0;
            }

            public override string ToString()
            {
                return $"[NullableEnumClass: ID={this.ID}, NullableIntEnum={this.NullableIntEnum}, NullableTextEnum={this.NullableTextEnum}]";
            }
        }
    }
}
