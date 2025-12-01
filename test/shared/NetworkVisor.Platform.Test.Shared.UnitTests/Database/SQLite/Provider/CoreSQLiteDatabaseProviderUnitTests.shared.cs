// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreSQLiteDatabaseProviderUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Provider;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Encryption.SHA;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Provider
{
    /// <summary>
    /// Class CoreSQLiteDatabaseProviderUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreSQLiteDatabaseProviderUnitTests))]

    public class CoreSQLiteDatabaseProviderUnitTests : CoreTestCaseBase
    {
        private const string _defaultPassword = "TestPassword";
        private string userSecureStorageFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSQLiteDatabaseProviderUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSQLiteDatabaseProviderUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.userSecureStorageFilePath = this.TestFileSystem.GetLocalUserAppDatabaseTempFilePath("dbtest_sqlite_", ".db");
        }

        [Fact]
        public void SQLiteDatabaseProviderUnitTests_NoInitialConnection()
        {
            this.TestFileSystem.FileExists(this.userSecureStorageFilePath).Should().BeFalse();
        }

        [Fact]
        public async Task SQLiteDatabaseProviderUnitTests_CreateTableAsync()
        {
            using ICoreSQLiteDatabaseProvider connection = this.OpenSQLiteConnection();
            connection.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreSQLiteDatabaseProvider>();
            connection.IsOpen.Should().BeTrue();
            CreateTableResult createTableResult = await connection.CreateTableAsync<TestTable>();
            createTableResult.Should().Be(CreateTableResult.Created);
        }

        [Fact]
        public async Task SQLiteDatabaseProviderUnitTests_CreateTableAsync_Encrypted()
        {
            using ICoreSQLiteDatabaseProvider connection = this.OpenSQLiteConnection(CoreDatabaseProviderFlags.Defaults, _defaultPassword);
            connection.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreSQLiteDatabaseProvider>();
            connection.IsOpen.Should().BeTrue();
            CreateTableResult createTableResult = await connection.CreateTableAsync<TestTable>();
            createTableResult.Should().Be(CreateTableResult.Created);
            await connection.InsertAsync(new TestTable("TestName"));
            List<TestTable>? items = await connection.Table<TestTable>().ToListAsync();
            items.Count.Should().Be(1);
            await connection.CloseAsync();
            connection.IsOpen.Should().BeFalse();

            using ICoreSQLiteDatabaseProvider reOpenConnection = this.OpenSQLiteConnection(CoreDatabaseProviderFlags.Defaults | CoreDatabaseProviderFlags.CleanupDatabaseOnClose, _defaultPassword);
            reOpenConnection.Should().NotBeNull();
            reOpenConnection.IsOpen.Should().BeTrue();
            List<TestTable>? reOpenedItems = await reOpenConnection.Table<TestTable>().ToListAsync();
            reOpenedItems.Count.Should().Be(1);
        }

        private ICoreSQLiteDatabaseProvider OpenSQLiteConnection(CoreDatabaseProviderFlags databaseProviderFlags = CoreDatabaseProviderFlags.Defaults | CoreDatabaseProviderFlags.CleanupDatabaseOnClose, string? password = null)
        {
            var connectionString = new CoreSQLiteConnectionString(this.userSecureStorageFilePath, CoreSQLiteDatabaseProvider.DefaultSQLiteOpenFlags, databaseProviderFlags.HasFlag(CoreDatabaseProviderFlags.StoreDateTimeAsTicks), password);

            var connection = new CoreSQLiteDatabaseProvider(this.TestFileSystem, connectionString, databaseProviderFlags);
            connection.Should().NotBeNull();
            connection.IsOpen.Should().BeTrue();

            this.TestFileSystem.FileExists(this.userSecureStorageFilePath).Should().BeTrue();
            connection.DatabaseFilePath.Should().Be(this.userSecureStorageFilePath);

            return connection;
        }

        [Table("test_table")]
        private class TestTable
        {
            public const int NameMaxLength = 32;
            private const int IdLength = 8;

            public TestTable()
            {
                this.Ranking = 0;
            }

            public TestTable(string? name)
            {
                name = name?.Trim().Truncate(NameMaxLength);
                this.Id = Sha1(name).Truncate(IdLength);
                this.Name = name;
                this.Ranking = 0;
            }

            [Column("id")]
            [MaxLength(IdLength)]
            [PrimaryKey]
            public string? Id { get; set; }

            [Column("name")]
            [MaxLength(NameMaxLength)]
            public string? Name { get; set; }

            [Column("ranking")]
            public int Ranking { get; set; }

            /// <summary>
            /// Computes the SHA-1 hash of the specified input string.
            /// </summary>
            /// <param name="input">The input string to hash. If the input is null or empty, it returns the input.</param>
            /// <returns>The SHA-1 hash of the input string in lowercase hexadecimal format, or the input if it is null or empty.</returns>
            private static string? Sha1(string? input)
            {
                return string.IsNullOrEmpty(input) ? input : CoreSHA1.Compute(input!).ToHexString()?.ToLower();
            }
        }
    }
}
