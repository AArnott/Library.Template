// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="ReadmeUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteReadmeUnitTests.
    /// </summary>
    [PlatformTrait(typeof(ReadmeUnitTests))]

    public class ReadmeUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadmeUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public ReadmeUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public static IEnumerable<Valuation> QueryValuations(CoreSQLiteConnection db, Stock stock)
        {
            return db.Query<Valuation>("select * from Valuation where StockId = ?", stock.Id);
        }

        public static IEnumerable<Val> QueryVals(CoreSQLiteConnection db, Stock stock)
        {
            return db.Query<Val>("select \"Price\" as \"Money\", \"Time\" as \"Date\" from Valuation where StockId = ?", stock.Id);
        }

        [Fact]
        public void Synchronous()
        {
            using var db = new TestDb<ReadmeUnitTests>(this.TestFileSystem);

            try
            {
                db.CreateTable<Stock>();
                db.CreateTable<Valuation>();

                AddStock(db, "A1", this.TestOutputHelper);
                AddStock(db, "A2", this.TestOutputHelper);
                AddStock(db, "A3", this.TestOutputHelper);
                AddStock(db, "B1", this.TestOutputHelper);
                AddStock(db, "B2", this.TestOutputHelper);
                AddStock(db, "B3", this.TestOutputHelper);

                TableQuery<Stock> query = db.Table<Stock>().Where(v => !string.IsNullOrEmpty(v.Symbol) && v.Symbol!.StartsWith("A"));

                foreach (Stock stock in query)
                {
                    this.TestOutputHelper.WriteLine("Stock: " + stock.Symbol);
                }

                query.ToList().Count.Should().Be(3);
            }
            finally
            {
                db.Close();
            }
        }

        [Fact]
        public async Task Asynchronous()
        {
            var db = new TestDbAsync<ReadmeUnitTests>(this.TestFileSystem);

            try
            {
                await db.CreateTableAsync<Stock>();

                this.TestOutputHelper.WriteLine("Table created!");

                var stock = new Stock()
                {
                    Symbol = "AAPL",
                };

                await db.InsertAsync(stock);

                this.TestOutputHelper.WriteLine("New sti ID: {0}", stock.Id);

                AsyncTableQuery<Stock> query = db.Table<Stock>().Where(s => !string.IsNullOrEmpty(s.Symbol) && s.Symbol!.StartsWith("A"));

                List<Stock> result = await query.ToListAsync();

                foreach (Stock s in result)
                {
                    this.TestOutputHelper.WriteLine("Stock: " + s.Symbol);
                }

                result.Count.Should().Be(1);

                int count = await db.ExecuteScalarAsync<int>("select count(*) from Stock");

                this.TestOutputHelper.WriteLine($"Found '{count}' stock items.");

                count.Should().Be(1);
            }
            finally
            {
                db.Dispose();
            }
        }

        [Fact]
        public void Cipher()
        {
            string databasePath = CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath(this.TestFileSystem, "CipherMyData");
            File.Delete(databasePath);

            var options = new CoreSQLiteConnectionString(databasePath, true, key: "password");
            using var encryptedDb = new TestDb<ReadmeUnitTests>(this.TestFileSystem, options);

            var options2 = new CoreSQLiteConnectionString(
                databasePath,
                true,
                key: "password",
                preKeyAction: db => db.Execute("PRAGMA cipher_default_use_hmac = OFF;"),
                postKeyAction: db => db.Execute("PRAGMA kdf_iter = 128000;"));

            encryptedDb.Close();
            TestDbAsync<ReadmeUnitTests>? encryptedDb2 = null;

            try
            {
                encryptedDb2 = new TestDbAsync<ReadmeUnitTests>(this.TestFileSystem, options2);
            }
            finally
            {
                encryptedDb2?.Dispose();
            }
        }

        [Fact]
        public void Manual()
        {
            using var db = new TestDb<ReadmeUnitTests>(this.TestFileSystem, ":memory:");

            try
            {
                db.Execute("create table Stock(Symbol varchar(100) not null)");
                db.Execute("insert into Stock(Symbol) values (?)", "MSFT");
                List<Stock> stocks = db.Query<Stock>("select * from Stock");

                stocks.Count.Should().Be(1);
                stocks[0].Symbol.Should().Be("MSFT");
            }
            finally
            {
                db.Close();
            }
        }

        private static void AddStock(CoreSQLiteConnection db, string symbol, ICoreTestOutputHelper testOutputHelper)
        {
            var stock = new Stock()
            {
                Symbol = symbol,
            };

            db.Insert(stock); // Returns the number of rows added to the table
            testOutputHelper.WriteLine("{0} == {1}", stock.Symbol, stock.Id);
        }

        public class Val
        {
            public decimal Money { get; set; }

            public DateTime Date { get; set; }
        }

        public class Stock
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public string? Symbol { get; set; }
        }

        public class Valuation
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            [Indexed]
            public int StockId { get; set; }

            public DateTime Time { get; set; }

            public decimal Price { get; set; }
        }
    }
}
