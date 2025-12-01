// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="ConcurrencyUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class ConcurrencyUnitTests.
    /// </summary>
    [PlatformTrait(typeof(ConcurrencyUnitTests))]

    public class ConcurrencyUnitTests : CoreTestCaseBase
    {
        private string _databaseFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrencyUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public ConcurrencyUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._databaseFilePath = CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath<ConcurrencyUnitTests>(this.TestFileSystem);
            using var dbConnection = new TestDbConcurrency(this.TestFileSystem, this._databaseFilePath, CoreSQLiteOpenFlags.FullMutex | CoreSQLiteOpenFlags.ReadWrite | CoreSQLiteOpenFlags.Create);
            dbConnection.CreateTables();
        }

        [Fact]
        public async Task TestLoadAsync()
        {
            try
            {
                var tokenSource = new CancellationTokenSource();
                var tasks = new List<Task>
                {
                    new DbReader(this.TestFileSystem, tokenSource.Token, this.TestOutputHelper, this._databaseFilePath).Run(),
                    new DbWriter(this.TestFileSystem, tokenSource.Token, this.TestOutputHelper, this._databaseFilePath).Run(),
                };

                // Wait 5sec
                tokenSource.CancelAfter(5000);

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                Assert.Fail(ex.ToString());
            }
        }

        /// <summary>
        /// Test for issue #761. Because the nature of this test is a race condition,
        /// it is not guaranteed to fail if the issue is present. It does appear to
        /// fail most of the time, though.
        /// </summary>
        [Fact]
        public async Task TestInsertCommandCreationAsync()
        {
            using var dbConnection = new TestDbConcurrency(this.TestFileSystem, this._databaseFilePath, CoreSQLiteOpenFlags.FullMutex | CoreSQLiteOpenFlags.ReadWrite | CoreSQLiteOpenFlags.Create);
            var obj1 = new TestObj();
            var obj2 = new TestObj();

            var taskA = Task.Run(() =>
            {
                dbConnection.Insert(obj1);
            });

            var taskB = Task.Run(() =>
            {
                dbConnection.Insert(obj2);
            });

            await Task.WhenAll(taskA, taskB);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    if (File.Exists(this._databaseFilePath))
                    {
                        File.Delete(this._databaseFilePath);
                    }
                }
                catch (Exception ex)
                {
                    this.TestNetworkingSystem.Logger.LogError(ex, "Failed to delete database {DatabaseFilePath}", this._databaseFilePath);
                }
            }

            base.Dispose(disposing);
        }

        public class TestObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}]";
            }
        }

        public class DbReader
        {
            private CancellationToken cancellationToken;
            private ICoreTestOutputHelper testOutputHelper;
            private string databaseFilePath;
            private ICoreFileSystem fileSystem;

            public DbReader(ICoreFileSystem fileSystem, CancellationToken cancellationToken, ICoreTestOutputHelper testOutputHelper, string databaseFilePath)
            {
                this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
                this.cancellationToken = cancellationToken;
                this.testOutputHelper = testOutputHelper;
                this.databaseFilePath = databaseFilePath;
            }

            public Task Run()
            {
                var t = Task.Run(async () =>
                {
                    try
                    {
                        while (true)
                        {
                            // NOTE: Change this to readwrite and then it does work ???
                            // No more IOERROR
                            CoreSQLiteOpenFlags flags = CoreSQLiteOpenFlags.FullMutex | CoreSQLiteOpenFlags.ReadOnly;
#if NV_PLAT_IOS
                            flags = CoreSQLiteOpenFlags.FullMutex | CoreSQLiteOpenFlags.ReadWrite;
#endif
                            using (var dbConnection = new TestDbConcurrency(this.fileSystem, this.databaseFilePath, flags))
                            {
                                var records = dbConnection.Table<TestObj>().ToList();
                                this.testOutputHelper.WriteLine($"{Environment.CurrentManagedThreadId} Read records: {records.Count}");
                            }

                            // No await so we stay on the same thread
                            await Task.Delay(10, this.cancellationToken);

                            this.cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                });

                return t;
            }
        }

        public class DbWriter
        {
            private CancellationToken cancellationToken;
            private ICoreTestOutputHelper testOutputHelper;
            private string databaseFilePath;
            private ICoreFileSystem fileSystem;

            public DbWriter(ICoreFileSystem fileSystem, CancellationToken cancellationToken, ICoreTestOutputHelper testOutputHelper, string databaseFilePath)
            {
                this.fileSystem = fileSystem ?? throw new ArgumentNullException(nameof(fileSystem));
                this.cancellationToken = cancellationToken;
                this.testOutputHelper = testOutputHelper;
                this.databaseFilePath = databaseFilePath;
            }

            public Task Run()
            {
                var t = Task.Run(() =>
                {
                    try
                    {
                        while (true)
                        {
                            using (var dbConnection = new TestDbConcurrency(this.fileSystem, this.databaseFilePath, CoreSQLiteOpenFlags.FullMutex | CoreSQLiteOpenFlags.ReadWrite))
                            {
                                this.testOutputHelper.WriteLine($"{Environment.CurrentManagedThreadId} Start insert");

                                for (var i = 0; i < 50; i++)
                                {
                                    var newRecord = new TestObj()
                                    {
                                    };

                                    dbConnection.Insert(newRecord);
                                }

                                this.testOutputHelper.WriteLine($"{Environment.CurrentManagedThreadId} Inserted records");
                            }

                            // No await so we stay on the same thread
                            Task.Delay(1).GetAwaiter().GetResult();
                            this.cancellationToken.ThrowIfCancellationRequested();
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                });

                return t;
            }
        }

        public class TestDbConcurrency : TestDbBase<ConcurrencyUnitTests>
        {
            public TestDbConcurrency(ICoreFileSystem fileSystem, string databaseFilePath, CoreSQLiteOpenFlags openflags, bool storeDateTimeAsTicks = true)
                : base(fileSystem, databaseFilePath, openflags, storeDateTimeAsTicks)
            {
                this.BusyTimeout = TimeSpan.FromSeconds(5);
                this.CleanupDatabaseOnClose = false;
            }

            public void CreateTables()
            {
                this.CreateTable<TestObj>();
            }
        }
    }
}
