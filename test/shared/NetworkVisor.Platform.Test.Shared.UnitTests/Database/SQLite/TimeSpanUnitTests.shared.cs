// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TimeSpanUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
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
    /// Class CoreSQLiteTimeSpanUnitTests.
    /// </summary>
    [PlatformTrait(typeof(TimeSpanUnitTests))]

    public class TimeSpanUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TimeSpanUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public TimeSpanUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void AsTicks()
        {
            using var db = new TestDb<TimeSpanUnitTests>(this.TestFileSystem, this.TimeSpanAsTicksConnectionString(true));
            var span = new TimeSpan(42, 12, 33, 20, 501);
            this.TestTimeSpan(db, span, span.Ticks.ToString());
        }

        [Fact]
        public void AsStrings()
        {
            using var db = new TestDb<TimeSpanUnitTests>(this.TestFileSystem, this.TimeSpanAsTicksConnectionString(false));
            var span = new TimeSpan(42, 12, 33, 20, 501);
            this.TestTimeSpan(db, span, span.ToString());
        }

        [Fact]
        public async Task AsTicksAsync()
        {
            using var db = new TestDbAsync<TimeSpanUnitTests>(this.TestFileSystem, this.TimeSpanAsTicksConnectionString(true));
            var span = new TimeSpan(42, 12, 33, 20, 501);
            await this.TestTimeSpanAsync(db, span, span.Ticks.ToString());
        }

        [Fact]
        public async Task AsStringsAsync()
        {
            using var db = new TestDbAsync<TimeSpanUnitTests>(this.TestFileSystem, this.TimeSpanAsTicksConnectionString(false));
            var span = new TimeSpan(42, 12, 33, 20, 501);
            await this.TestTimeSpanAsync(db, span, span.ToString());
        }

        private CoreSQLiteConnectionString TimeSpanAsTicksConnectionString(bool asTicks = true) => new(nameof(TimeSpanUnitTests), this.TestFileSystem, CoreSQLiteOpenFlags.Create | CoreSQLiteOpenFlags.ReadWrite, true, storeTimeSpanAsTicks: asTicks);

        private async Task TestTimeSpanAsync(CoreSQLiteAsyncConnection db, TimeSpan duration, string expected)
        {
            await db.CreateTableAsync<TestObj>();

            TestObj? o, o2;

            o = new TestObj
            {
                Duration = duration,
            };

            await db.InsertAsync(o);
            o2 = await db.GetAsync<TestObj>(o.Id);
            o2.Should().NotBeNull();
            o2!.Duration.Should().Be(o.Duration);

            var stored = await db.ExecuteScalarAsync<string>("SELECT Duration FROM TestObj;");
            stored.Should().Be(expected);
        }

        private void TestTimeSpan(TestDb<TimeSpanUnitTests> db, TimeSpan duration, string expected)
        {
            db.CreateTable<TestObj>();

            TestObj? o, o2;

            o = new TestObj
            {
                Duration = duration,
            };
            db.Insert(o);
            o2 = db.Get<TestObj>(o.Id);
            o2.Should().NotBeNull();
            o2!.Duration.Should().Be(o.Duration);

            var stored = db.ExecuteScalar<string>("SELECT Duration FROM TestObj;");
            stored.Should().Be(expected);
        }

        private class TestObj
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string? Name { get; set; }

            public TimeSpan Duration { get; set; }
        }
    }
}
