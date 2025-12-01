// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="DateTimeOffsetUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
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
    /// Class CoreSQLiteDateTimeOffsetUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DateTimeOffsetUnitTests))]

    public class DateTimeOffsetUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeOffsetUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DateTimeOffsetUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void AsTicks()
        {
            using var db = new TestDb<DateTimeOffsetUnitTests>(this.TestFileSystem);
            this.TestDateTimeOffset(db);
        }

        [Fact]
        public async Task AsTicksAsync()
        {
            using var db = new TestDbAsync<DateTimeOffsetUnitTests>(this.TestFileSystem);
            await this.TestAsyncDateTimeOffset(db);
        }

        private async Task TestAsyncDateTimeOffset(CoreSQLiteAsyncConnection db)
        {
            await db.CreateTableAsync<TestObj>();

            TestObj? o, o2;

            // Ticks
            o = new TestObj
            {
                ModifiedTime = new DateTimeOffset(2012, 1, 14, 3, 2, 1, TimeSpan.Zero),
            };

            await db.InsertAsync(o);
            o2 = await db.GetAsync<TestObj>(o.Id);
            o2.Should().NotBeNull();
            o2!.ModifiedTime.Should().Be(o.ModifiedTime);
        }

        private void TestDateTimeOffset(CoreSQLiteConnection db)
        {
            db.CreateTable<TestObj>();

            TestObj? o, o2;

            // Ticks
            o = new TestObj
            {
                ModifiedTime = new DateTimeOffset(2012, 1, 14, 3, 2, 1, TimeSpan.Zero),
            };

            db.Insert(o);
            o2 = db.Get<TestObj>(o.Id);
            o2.Should().NotBeNull();
            o2!.ModifiedTime.Should().Be(o.ModifiedTime);
        }

        private class TestObj
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string? Name { get; set; }

            public DateTimeOffset ModifiedTime { get; set; }
        }
    }
}
