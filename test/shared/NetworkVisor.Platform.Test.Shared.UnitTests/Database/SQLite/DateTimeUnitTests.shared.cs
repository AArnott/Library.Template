// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="DateTimeUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreSQLiteNullableUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DateTimeUnitTests))]

    public class DateTimeUnitTests : CoreTestCaseBase
    {
        private const string DefaultSQLiteDateTimeString = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fff";

        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DateTimeUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void AsTicks()
        {
            var dateTime = new DateTime(2012, 1, 14, 3, 2, 1, 234);
            using var db = new TestDb<DateTimeUnitTests>(this.TestFileSystem, storeDateTimeAsTicks: true);
            this.TestDateTime(db, dateTime, dateTime.Ticks.ToString());
        }

        [Fact]
        public void AsStrings()
        {
            var dateTime = new DateTime(2012, 1, 14, 3, 2, 1, 234);
            using var db = new TestDb<DateTimeUnitTests>(this.TestFileSystem, storeDateTimeAsTicks: false);
            this.TestDateTime(db, dateTime, dateTime.ToString(DefaultSQLiteDateTimeString));
        }

        [Theory]
        [InlineData("o")]
        [InlineData("MMM'-'dd'-'yyyy' 'HH':'mm':'ss'.'fffffff")]
        public void AsCustomStrings(string format)
        {
            var dateTime = new DateTime(2012, 1, 14, 3, 2, 1, 234);
            using var db = new TestDb<DateTimeUnitTests>(this.TestFileSystem, this.CustomDateTimeConnectionString(format));
            this.TestDateTime(db, dateTime, dateTime.ToString(format, System.Globalization.CultureInfo.InvariantCulture));
        }

        [Fact]
        public async Task AsTicksAsync()
        {
            var dateTime = new DateTime(2012, 1, 14, 3, 2, 1, 234);
            using var db = new TestDbAsync<DateTimeUnitTests>(this.TestFileSystem, true);
            await this.TestDateTimeAsync(db, dateTime, dateTime.Ticks.ToString());
        }

        [Fact]
        public async Task AsStringAsync()
        {
            var dateTime = new DateTime(2012, 1, 14, 3, 2, 1, 234);
            using var db = new TestDbAsync<DateTimeUnitTests>(this.TestFileSystem, false);
            await this.TestDateTimeAsync(db, dateTime, dateTime.ToString(DefaultSQLiteDateTimeString));
        }

        [Theory]
        [InlineData("o")]
        [InlineData("MMM'-'dd'-'yyyy' 'HH':'mm':'ss'.'fffffff")]
        public async Task AsCustomStringsAsync(string format)
        {
            var dateTime = new DateTime(2012, 1, 14, 3, 2, 1, 234);
            using var db = new TestDbAsync<DateTimeUnitTests>(this.TestFileSystem, this.CustomDateTimeConnectionString(format));
            await this.TestDateTimeAsync(db, dateTime, dateTime.ToString(format, System.Globalization.CultureInfo.InvariantCulture));
        }

        [Fact]
        public async Task LinqNullable()
        {
            foreach (var option in new[] { true, false })
            {
                using var db = new TestDbAsync<DateTimeUnitTests>(this.TestFileSystem, option);
                await db.CreateTableAsync<NullableDateObj>();

                var epochTime = new DateTime(1970, 1, 1);

                await db.InsertAsync(new NullableDateObj { Time = epochTime });
                await db.InsertAsync(new NullableDateObj { Time = new DateTime(1980, 7, 23) });
                await db.InsertAsync(new NullableDateObj { Time = null });
                await db.InsertAsync(new NullableDateObj { Time = new DateTime(2019, 1, 23) });

                List<NullableDateObj> res = await db.Table<NullableDateObj>().Where(x => x.Time == epochTime).ToListAsync();
                res.Count.Should().Be(1);

                res = await db.Table<NullableDateObj>().Where(x => x.Time > epochTime).ToListAsync();
                res.Count.Should().Be(2);
            }
        }

        private async Task TestDateTimeAsync(CoreSQLiteAsyncConnection db, DateTime dateTime, string expected)
        {
            await db.CreateTableAsync<TestObj>();

            TestObj? o, o2;

            // Ticks
            o = new TestObj
            {
                ModifiedTime = dateTime,
            };

            await db.InsertAsync(o);
            o2 = await db.GetAsync<TestObj>(o.Id);
            o2.Should().NotBeNull();
            o2!.ModifiedTime.Should().Be(o.ModifiedTime);

            var stored = await db.ExecuteScalarAsync<string>("SELECT ModifiedTime FROM TestObj;");
            stored.Should().Be(expected);
        }

        private CoreSQLiteConnectionString CustomDateTimeConnectionString(string dateTimeFormat) => new(nameof(DateTimeUnitTests), this.TestFileSystem, CoreSQLiteOpenFlags.Create | CoreSQLiteOpenFlags.ReadWrite, false, dateTimeStringFormat: dateTimeFormat);

        private void TestDateTime(TestDb<DateTimeUnitTests> db, DateTime dateTime, string expected)
        {
            db.CreateTable<TestObj>();

            TestObj? o, o2;

            // Ticks
            o = new TestObj
            {
                ModifiedTime = dateTime,
            };

            db.Insert(o);
            o2 = db.Get<TestObj>(o.Id);
            o2.Should().NotBeNull();
            o2!.ModifiedTime.Should().Be(o.ModifiedTime);

            var stored = db.ExecuteScalar<string>("SELECT ModifiedTime FROM TestObj;");
            stored.Should().Be(expected);
        }

        private class TestObj
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string? Name { get; set; }

            public DateTime ModifiedTime { get; set; }
        }

        private class NullableDateObj
        {
            public DateTime? Time { get; set; }
        }
    }
}
