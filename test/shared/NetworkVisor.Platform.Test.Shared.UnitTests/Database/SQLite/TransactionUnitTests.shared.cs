// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TransactionUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Collections;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Exceptions;
using NetworkVisor.Core.Database.Providers.SQLite.Interop;
using NetworkVisor.Core.Database.Providers.SQLite.Logging;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteTransactionUnitTests.
    /// </summary>
    [PlatformTrait(typeof(TransactionUnitTests))]

    public class TransactionUnitTests : CoreTestCaseBase
    {
        private ICoreSQLiteTransactionLogger _sQLiteTransactionLogger;
        private TestDb<TransactionUnitTests> _db;
        private List<TestObj> testObjects;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransactionUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public TransactionUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.testObjects = Enumerable.Range(1, 20).Select(i => new TestObj()).ToList();
            this._sQLiteTransactionLogger =
                new CoreSQLiteTransactionLogger<TransactionUnitTests>(new CoreSQLiteTransactionLoggerProvider(this.TestFileSystem, LogLevel.Trace), null);

            this._db = new TestDb<TransactionUnitTests>(this.TestFileSystem, true, null, true, this._sQLiteTransactionLogger);
            this._db.CreateTable<TestObj>();
            this._db.InsertAll(this.testObjects);
        }

        [Fact]
        public void SuccessfulSavepointTransaction()
        {
            this._db.RunInTransaction(() =>
            {
                this._db.Delete(this.testObjects[0]);
                this._db.Delete(this.testObjects[1]);
                this._db.Insert(new TestObj());
            });

            this._db.Table<TestObj>().Count().Should().Be(this.testObjects.Count - 1);
        }

        [Fact]
        public void FailSavepointTransaction()
        {
            try
            {
                this._db.RunInTransaction(() =>
                {
                    this._db.Delete(this.testObjects[0]);

                    throw new TransactionTestException();
                });
            }
            catch (TransactionTestException)
            {
                // ignore
            }

            this._db.Table<TestObj>().Count().Should().Be(this.testObjects.Count);
        }

        [Fact]
        public void SuccessfulNestedSavepointTransaction()
        {
            this._db.RunInTransaction(() =>
            {
                this._db.Delete(this.testObjects[0]);

                this._db.RunInTransaction(() =>
                {
                    this._db.Delete(this.testObjects[1]);
                });
            });

            this._db.Table<TestObj>().Count().Should().Be(this.testObjects.Count - 2);
        }

        [Fact]
        public void FailNestedSavepointTransaction()
        {
            try
            {
                this._db.RunInTransaction(() =>
                {
                    this._db.Delete(this.testObjects[0]);

                    this._db.RunInTransaction(() =>
                    {
                        this._db.Delete(this.testObjects[1]);

                        throw new TransactionTestException();
                    });
                });
            }
            catch (TransactionTestException)
            {
                // ignore
            }

            this._db.Table<TestObj>().Count().Should().Be(this.testObjects.Count);
        }

        [Fact]
        public async Task Issue329_TransactionFailuresShouldRollbackAsync()
        {
            using var adb = new TestDbAsync<TransactionUnitTests>(this.TestFileSystem, true, null, this._sQLiteTransactionLogger);
            await adb.CreateTableAsync<TestObj>();
            var initialCount = await adb.Table<TestObj>().CountAsync();
            var rollbacks = 0;

            // Fail a commit
            this._sQLiteTransactionLogger.Tracer = m =>
            {
                this.TestOutputHelper.WriteLine(m);

                if (m == "Executing: rollback")
                {
                    rollbacks++;
                }
            };

            try
            {
                await adb.RunInTransactionAsync(dbt =>
                {
                    dbt.Insert(new TestObj());
                    throw new Exception("User exception");
                });

                Assert.Fail("Should have thrown");
            }
            catch (Exception ex) when (ex.Message == "User exception")
            {
                // Expected
            }

            rollbacks.Should().Be(1);
        }

        [Fact]
        public async Task Issue604_RunInTransactionAsync()
        {
            using var adb = new TestDbAsync<TransactionUnitTests>(this.TestFileSystem, true, null, this._sQLiteTransactionLogger);
            await adb.CreateTableAsync<TestObj>();
            var initialCount = await adb.Table<TestObj>().CountAsync();

            // Fail a commit
            this._sQLiteTransactionLogger.Tracer = m =>
            {
                this.TestOutputHelper.WriteLine(m);

                if (m.Trim().EndsWith("commit"))
                {
                    throw CoreSQLiteException.New(SQLite3.Result.Busy, "Make commit fail");
                }
            };

            try
            {
                await adb.RunInTransactionAsync(db =>
                {
                    db.Insert(new TestObj());
                });

                Assert.Fail("Should have thrown");
            }
            catch (CoreSQLiteException ex) when (ex.Result == SQLite3.Result.Busy)
            {
                // Expected
            }

            // Are we stuck?
            this._sQLiteTransactionLogger.Tracer = null;
            await adb.RunInTransactionAsync(db =>
            {
                db.Insert(new TestObj());
            });

            (await adb.Table<TestObj>().CountAsync()).Should().Be(initialCount + 1);
        }

        [Fact]
        public void Issue604_RecoversFromFailedCommit()
        {
            var initialCount = this._db.Table<TestObj>().Count();

            // Well this is an issue because there is an internal variable called _transactionDepth
            // that tries to track if we are in an active transaction.
            // The problem is, _transactionDepth is set to 0 and then commit is executed on the database.
            // Well, the commit fails and "When COMMIT fails in this way, the transaction remains active and
            // the COMMIT can be retried later after the reader has had a chance to clear"
            var rollbacks = 0;

            this._sQLiteTransactionLogger.Tracer = m =>
            {
                if (m == "Executing: commit")
                {
                    throw CoreSQLiteException.New(SQLite3.Result.Busy, "Make commit fail");
                }

                if (m == "Executing: rollback")
                {
                    rollbacks++;
                }
            };
            this._db.BeginTransaction();
            this._db.Insert(new TestObj());

            try
            {
                this._db.Commit();
                Assert.Fail("Should have thrown");
            }
            catch (CoreSQLiteException ex) when (ex.Result == SQLite3.Result.Busy)
            {
                this._sQLiteTransactionLogger.Tracer = null!;
            }

            Assert.False(this._db.IsInTransaction);
            rollbacks.Should().Be(1);

            // The catch statements in the RunInTransaction family of functions catch this and call rollback,
            // but since _transactionDepth is 0, the transaction isn't actually rolled back.
            //
            // So the next time begin transaction is called on the same connection,
            // sqlite-net attempts to begin a new transaction (because _transactionDepth is 0),
            // which promptly fails because there is still an active transaction on the connection.
            //
            // Well now we are in big trouble because _transactionDepth got set to 1,
            // and when begin transaction fails in this manner, the transaction isn't rolled back
            // (which would have set _transactionDepth to 0)
            this._db.BeginTransaction();
            this._db.Insert(new TestObj());
            this._db.Commit();
            this._db.Table<TestObj>().Count().Should().Be(initialCount + 1);
        }

        [Fact]
        public void Issue604_RecoversFromFailedRelease()
        {
            var initialCount = this._db.Table<TestObj>().Count();

            var rollbacks = 0;

            this._sQLiteTransactionLogger.Tracer = m =>
            {
                this.TestOutputHelper.WriteLine(m);

                if (m.StartsWith("Executing: release"))
                {
                    throw CoreSQLiteException.New(SQLite3.Result.Busy, "Make release fail");
                }

                if (m == "Executing: rollback")
                {
                    rollbacks++;
                }
            };

            var sp0 = this._db.SaveTransactionPoint();

            this._db.Insert(new TestObj());

            try
            {
                this._db.Release(sp0);
                Assert.Fail("Should have thrown");
            }
            catch (CoreSQLiteException ex) when (ex.Result == SQLite3.Result.Busy)
            {
                this._sQLiteTransactionLogger.Tracer = null!;
            }

            Assert.False(this._db.IsInTransaction);
            rollbacks.Should().Be(1);

            this._db.BeginTransaction();
            this._db.Insert(new TestObj());
            this._db.Commit();
            this._db.Table<TestObj>().Count().Should().Be(initialCount + 1);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._db?.Close();
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

        public class TransactionTestException : Exception
        {
        }
    }
}
