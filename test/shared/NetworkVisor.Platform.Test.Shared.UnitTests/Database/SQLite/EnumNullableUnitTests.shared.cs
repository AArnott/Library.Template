// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="EnumNullableUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreSQLiteEnumNullableUnitTests.
    /// </summary>
    [PlatformTrait(typeof(EnumNullableUnitTests))]

    public class EnumNullableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EnumNullableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public EnumNullableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public enum TestEnum
        {
            Value1,

            Value2,

            Value3,
        }

        [Fact]
        public void ShouldPersistAndReadEnum()
        {
            using var db = new TestDb<EnumNullableUnitTests>(this.TestFileSystem);
            db.CreateTable<TestObj>();

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

        public class TestObj
        {
            [PrimaryKey]
            public int Id { get; set; }

            public TestEnum? Value { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}, Value={this.Value}]";
            }
        }
    }
}
