// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="InsertUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Diagnostics;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Exceptions;
using NetworkVisor.Core.Database.Providers.SQLite.Logging;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Helpers;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteInsertUnitTests.
    /// </summary>
    [PlatformTrait(typeof(InsertUnitTests))]

    public class InsertUnitTests : CoreTestCaseBase
    {
        private ICoreSQLiteTransactionLogger _sqLiteTransactionLogger;
        private TestDb<InsertUnitTests> _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="InsertUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public InsertUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._sqLiteTransactionLogger = new CoreSQLiteTransactionLogger<InsertUnitTests>(new CoreSQLiteTransactionLoggerProvider(this.TestFileSystem, LogLevel.Trace), null);
            this._db = new TestDb<InsertUnitTests>(this.TestFileSystem, true, null, true, this._sqLiteTransactionLogger);
            this._db.CreateTable<TestObj>();
            this._db.CreateTable<TestObj2>();
            this._db.CreateTable<OneColumnObj>();
            this._db.CreateTable<UniqueObj>();
        }

        [Fact]
        public void InsertALot()
        {
            int n = 10000;
            IEnumerable<TestObj> q = from i in Enumerable.Range(1, n)
                                     select new TestObj()
                                     {
                                         Text = "I am",
                                     };
            TestObj[] objs = q.ToArray();

            var sw = new Stopwatch();
            sw.Start();

            int numIn = this._db.InsertAll(objs);

            sw.Stop();

            numIn.Should().Be(n, "Num inserted must = num objects");

            TestObj[] inObjs = this._db.CreateCommand("select * from TestObj").ExecuteQuery<TestObj>().ToArray();

            for (int i = 0; i < inObjs.Length; i++)
            {
                objs[i].Id.Should().Be(i + 1);
                inObjs[i].Id.Should().Be(i + 1);
                inObjs[i].Text.Should().Be("I am");
            }

            int numCount = this._db.CreateCommand("select count(*) from TestObj").ExecuteScalar<int>();

            numCount.Should().Be(n, "Num counted must = num objects");
        }

        [Fact]
        public void InsertTraces()
        {
            Action<string>? oldTracer = this._sqLiteTransactionLogger.Tracer;
            LogLevel oldTrace = this._sqLiteTransactionLogger.ProviderLogLevel.Current;

            var traces = new List<string>();
            this._sqLiteTransactionLogger.Tracer = traces.Add;
            this._sqLiteTransactionLogger.ProviderLogLevel.Current = LogLevel.Trace;

            var obj1 = new TestObj() { Text = "GLaDOS loves tracing!" };
            int numIn1 = this._db.Insert(obj1);

            numIn1.Should().Be(1);
            traces.Count.Should().Be(1);

            this._sqLiteTransactionLogger.Tracer = oldTracer;
            this._sqLiteTransactionLogger.ProviderLogLevel.Current = oldTrace;
        }

        [Fact]
        public void InsertTwoTimes()
        {
            var obj1 = new TestObj() { Text = "GLaDOS loves testing!" };
            var obj2 = new TestObj() { Text = "Keep testing, just keep testing" };

            int numIn1 = this._db.Insert(obj1);
            int numIn2 = this._db.Insert(obj2);
            numIn1.Should().Be(1);
            numIn2.Should().Be(1);

            var result = this._db.Query<TestObj>("select * from TestObj").ToList();
            result.Count.Should().Be(2);
            obj1.Text.Should().Be(result[0].Text);
            obj2.Text.Should().Be(result[1].Text);
        }

        [Fact]
        public void InsertIntoTwoTables()
        {
            var obj1 = new TestObj() { Text = "GLaDOS loves testing!" };
            var obj2 = new TestObj2() { Text = "Keep testing, just keep testing" };

            int numIn1 = this._db.Insert(obj1);
            numIn1.Should().Be(1);
            int numIn2 = this._db.Insert(obj2);
            numIn2.Should().Be(1);

            var result1 = this._db.Query<TestObj>("select * from TestObj").ToList();
            result1.Count.Should().Be(numIn1);
            result1.First().Text.Should().Be(obj1.Text);

            var result2 = this._db.Query<TestObj>("select * from TestObj2").ToList();
            result2.Count.Should().Be(numIn2);
        }

        [Fact]
        public void InsertWithExtra()
        {
            var obj1 = new TestObj2() { Id = 1, Text = "GLaDOS loves testing!" };
            var obj2 = new TestObj2() { Id = 1, Text = "Keep testing, just keep testing" };
            var obj3 = new TestObj2() { Id = 1, Text = "Done testing" };

            this._db.Insert(obj1);

            try
            {
                this._db.Insert(obj2);
                Assert.Fail("Expected unique constraint violation");
            }
            catch (CoreSQLiteException)
            {
            }

            this._db.Insert(obj2, "OR REPLACE");

            try
            {
                this._db.Insert(obj3);
                Assert.Fail("Expected unique constraint violation");
            }
            catch (CoreSQLiteException)
            {
            }

            this._db.Insert(obj3, "OR IGNORE");

            var result = this._db.Query<TestObj>("select * from TestObj2").ToList();
            result.Count.Should().Be(1);
            result.First().Text.Should().Be(obj2.Text);
        }

        [Fact]
        public void InsertIntoOneColumnAutoIncrementTable()
        {
            var obj = new OneColumnObj();
            this._db.Insert(obj);

            OneColumnObj? result = this._db.Get<OneColumnObj>(1);
            result.Should().NotBeNull();
            result!.Id.Should().Be(1);
        }

        [Fact]
        public void InsertAllSuccessOutsideTransaction()
        {
            var testObjects = Enumerable.Range(1, 20).Select(i => new UniqueObj { Id = i }).ToList();

            this._db.InsertAll(testObjects);

            this._db.Table<UniqueObj>().Count().Should().Be(testObjects.Count);
        }

        [Fact]
        public void InsertAllFailureOutsideTransaction()
        {
            var testObjects = Enumerable.Range(1, 20).Select(i => new UniqueObj { Id = i }).ToList();
            testObjects[testObjects.Count - 1].Id = 1; // causes the insert to fail because of duplicate key

            ExceptionAssert.Throws<CoreSQLiteException>(() => this._db.InsertAll(testObjects));

            this._db.Table<UniqueObj>().Count().Should().Be(0);
        }

        [Fact]
        public void InsertAllSuccessInsideTransaction()
        {
            var testObjects = Enumerable.Range(1, 20).Select(i => new UniqueObj { Id = i }).ToList();

            this._db.RunInTransaction(() =>
            {
                this._db.InsertAll(testObjects);
            });

            this._db.Table<UniqueObj>().Count().Should().Be(testObjects.Count);
        }

        [Fact]
        public void InsertAllFailureInsideTransaction()
        {
            var testObjects = Enumerable.Range(1, 20).Select(i => new UniqueObj { Id = i }).ToList();
            testObjects[testObjects.Count - 1].Id = 1; // causes the insert to fail because of duplicate key

            ExceptionAssert.Throws<CoreSQLiteException>(() => this._db.RunInTransaction(() =>
            {
                this._db.InsertAll(testObjects);
            }));

            this._db.Table<UniqueObj>().Count().Should().Be(0);
        }

        [Fact]
        public void InsertOrReplace()
        {
            this._db.InsertAll(from i in Enumerable.Range(1, 20) select new TestObj { Text = "#" + i });

            this._db.Table<TestObj>().Count().Should().Be(20);

            var t = new TestObj { Id = 5, Text = "Foo", };
            this._db.InsertOrReplace(t);

            var r = (from x in this._db.Table<TestObj>() orderby x.Id select x).ToList();
            r.Count.Should().Be(20);
            r[4].Text.Should().Be("Foo");
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

            public string? Text { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}, Text={this.Text}]";
            }
        }

        public class TestObj2
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string? Text { get; set; }

            public override string ToString()
            {
                return $"[TestObj: Id={this.Id}, Text={this.Text}]";
            }
        }

        public class OneColumnObj
        {
            [AutoIncrement, PrimaryKey]
            public int Id { get; set; }
        }

        public class UniqueObj
        {
            [PrimaryKey]
            public int Id { get; set; }
        }
    }
}
