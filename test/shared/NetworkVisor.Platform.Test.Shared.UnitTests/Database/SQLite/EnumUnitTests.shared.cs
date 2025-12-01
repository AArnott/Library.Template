// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="EnumUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreSQLiteEnumUnitTests.
    /// </summary>
    [PlatformTrait(typeof(EnumUnitTests))]

    public class EnumUnitTests : CoreTestCaseBase
    {
        private const string DefaultSQLiteDateTimeString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public EnumUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public enum TestEnum
        {
            Value1,

            Value2,

            Value3,
        }

        [StoreAsText]
        public enum StringTestEnum
        {
            Value1,

            Value2,

            Value3,
        }

        public enum ByteTestEnum : byte
        {
            Value1 = 1,

            Value2 = 2,

            Value3 = 3,
        }

        [Fact]
        public void ShouldPersistAndReadEnum()
        {
            using var db = new TestDbEnum(this.TestFileSystem);

            var obj1 = new TestObj() { Id = 1, Value = TestEnum.Value2 };
            var obj2 = new TestObj() { Id = 2, Value = TestEnum.Value3 };

            var numIn1 = db.Insert(obj1);
            var numIn2 = db.Insert(obj2);
            numIn1.Should().Be(1);
            numIn2.Should().Be(1);

            var result = db.Query<TestObj>("select * from TestObj").ToList();
            result.Count.Should().Be(2);
            result[0].Value.Should().Be(obj1.Value);
            result[1].Value.Should().Be(obj2.Value);

            result[0].Id.Should().Be(obj1.Id);
            result[1].Id.Should().Be(obj2.Id);

            db.Close();
        }

        [Fact]
        public void ShouldPersistAndReadStringEnum()
        {
            using var db = new TestDbEnum(this.TestFileSystem);

            var obj1 = new StringTestObj() { Id = 1, Value = StringTestEnum.Value2 };
            var obj2 = new StringTestObj() { Id = 2, Value = StringTestEnum.Value3 };

            var numIn1 = db.Insert(obj1);
            var numIn2 = db.Insert(obj2);
            numIn1.Should().Be(1);
            numIn2.Should().Be(1);

            var result = db.Query<StringTestObj>("select * from StringTestObj").ToList();
            result.Count.Should().Be(2);
            result[0].Value.Should().Be(obj1.Value);
            result[1].Value.Should().Be(obj2.Value);

            result[0].Id.Should().Be(obj1.Id);
            result[1].Id.Should().Be(obj2.Id);

            db.Close();
        }

        [Fact]
        public void Issue33_ShouldPersistAndReadByteEnum()
        {
            using var db = new TestDbEnum(this.TestFileSystem);

            var obj1 = new ByteTestObj() { Id = 1, Value = ByteTestEnum.Value2 };
            var obj2 = new ByteTestObj() { Id = 2, Value = ByteTestEnum.Value3 };

            var numIn1 = db.Insert(obj1);
            var numIn2 = db.Insert(obj2);
            numIn1.Should().Be(1);
            numIn2.Should().Be(1);

            var result = db.Query<ByteTestObj>("select * from ByteTestObj order by Id").ToList();
            result.Count.Should().Be(2);
            result[0].Value.Should().Be(obj1.Value);
            result[1].Value.Should().Be(obj2.Value);

            result[0].Id.Should().Be(obj1.Id);
            result[1].Id.Should().Be(obj2.Id);

            db.Close();
        }

        public class ByteTestObj
        {
            [PrimaryKey]
            public int Id { get; set; }

            public ByteTestEnum Value { get; set; }

            public override string ToString()
            {
                return $"[ByteTestObj: Id={this.Id}, Value={this.Value}]";
            }
        }

        public class TestObj
        {
            [PrimaryKey]
            public int Id { get; set; }

            public TestEnum Value { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}, Value={this.Value}]";
            }
        }

        public class StringTestObj
        {
            [PrimaryKey]
            public int Id { get; set; }

            public StringTestEnum Value { get; set; }

            public override string ToString()
            {
                return $"[StringTestObj: Id={this.Id}, Value={this.Value}]";
            }
        }

        public class TestDbEnum : TestDbBase<EnumUnitTests>
        {
            public TestDbEnum(ICoreFileSystem fileSystem)
                : base(fileSystem)
            {
                this.CreateTable<TestObj>();
                this.CreateTable<StringTestObj>();
                this.CreateTable<ByteTestObj>();
            }
        }
    }
}
