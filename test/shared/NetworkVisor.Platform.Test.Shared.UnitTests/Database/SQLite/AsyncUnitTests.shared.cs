// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="AsyncUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Database.Providers.SQLite.Commands;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Interop;
using NetworkVisor.Core.Database.Providers.SQLite.Logging;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.TestCase;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Helpers;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class AsyncUnitTests.
    /// </summary>
    [PlatformTrait(typeof(AsyncUnitTests))]

    public class AsyncUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AsyncUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public AsyncUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task EnableWalAsync()
        {
            string? path = this.TestFileSystem.GetLocalUserAppDatabaseTempFilePath("EnableWalAsync", ".db");
            path.Should().NotBeNull();
            using var db = new TestDbAsync<AsyncUnitTests>(this.TestFileSystem, path!);
            await db.EnableWriteAheadLoggingAsync();
        }

        [Fact]
        public async Task EnableLoadExtensionAsync()
        {
            if (SQLite3.IsFeatureSupported(this.TestFileSystem, CoreSQLiteFeature.LoadExtensions))
            {
                using var env = new TestEnvironment(this, this.TestCaseLogger);
                CoreSQLiteAsyncConnection connection = env.Connection;

                await connection.EnableLoadExtensionAsync(true);
            }
            else
            {
                this.TestOutputHelper.WriteLine("LoadExtensions not supported");
                this.TestOperatingSystem.IsIOS.Should().BeTrue();
            }
        }

        [Fact]
        public async Task InsertAsyncWithType()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection connection = env.Connection;
            await connection.CreateTableAsync<TestCustomer>();

            var testCustomer = new TestCustomer
            {
                FirstName = "Joe",
            };

            await connection.InsertAsync(testCustomer, typeof(TestCustomer));
            List<TestCustomer> c = await connection.QueryAsync<TestCustomer>("select * from TestCustomer");

            c[0].FirstName.Should().Be("Joe");
        }

        [Fact]
        public async Task InsertAsyncWithExtra()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection connection = env.Connection;
            await connection.CreateTableAsync<TestCustomer>();

            var testCustomer = new TestCustomer
            {
                FirstName = "Joe",
            };
            await connection.InsertAsync(testCustomer, "or replace");

            List<TestCustomer> c = await connection.QueryAsync<TestCustomer>("select * from TestCustomer");

            c[0].FirstName.Should().Be("Joe");
        }

        [Fact]
        public async Task InsertAsyncWithTypeAndExtra()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection connection = env.Connection;
            await connection.CreateTableAsync<TestCustomer>();

            var testCustomer = new TestCustomer
            {
                FirstName = "Joe",
            };
            await connection.InsertAsync(testCustomer, "or replace", typeof(TestCustomer));

            List<TestCustomer> c = await connection.QueryAsync<TestCustomer>("select * from TestCustomer");
            c[0].FirstName.Should().Be("Joe");
        }

        [Fact]
        public async Task InsertOrReplaceAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection connection = env.Connection;
            await connection.CreateTableAsync<TestCustomer>();

            var testCustomer = new TestCustomer
            {
                FirstName = "Joe",
            };
            await connection.InsertOrReplaceAsync(testCustomer);
            List<TestCustomer> c = await connection.QueryAsync<TestCustomer>("select * from TestCustomer");
            c[0].FirstName.Should().Be("Joe");
        }

        [Fact]
        public async Task InsertOrReplaceAsyncWithType()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection connection = env.Connection;
            await connection.CreateTableAsync<TestCustomer>();

            var testCustomer = new TestCustomer
            {
                FirstName = "Joe",
            };
            await connection.InsertOrReplaceAsync(testCustomer, typeof(TestCustomer));
            List<TestCustomer> c = await connection.QueryAsync<TestCustomer>("select * from TestCustomer");
            c[0].FirstName.Should().Be("Joe");
        }

        [Fact]
        public async Task QueryAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection connection = env.Connection;
            await connection.CreateTableAsync<TestCustomer>();
            var testCustomer = new TestCustomer
            {
                FirstName = "Joe",
            };
            await connection.InsertAsync(testCustomer);
            await connection.QueryAsync<TestCustomer>("select * from TestCustomer");
        }

        [Fact]
        public async Task MemoryQueryAsync()
        {
            var connection = new CoreSQLiteAsyncConnection(this.TestFileSystem, ":memory:", false, new CoreQueryLogger(this.TestCaseLogger));
            try
            {
                await connection.CreateTableAsync<TestCustomer>();

                var testCustomer = new TestCustomer
                {
                    FirstName = "Joe",
                };

                await connection.InsertAsync(testCustomer);

                await connection.QueryAsync<TestCustomer>("select * from TestCustomer");
            }
            finally
            {
                await connection.CloseAsync();
            }
        }

        [Fact]
        public async Task BusyTime()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            TimeSpan defaultBusyTime = conn.GetBusyTimeout();
            Assert.True(defaultBusyTime > TimeSpan.FromMilliseconds(999));

            await conn.SetBusyTimeoutAsync(TimeSpan.FromSeconds(10));
            TimeSpan newBusyTime = conn.GetBusyTimeout();
            Assert.True(newBusyTime > TimeSpan.FromMilliseconds(9999));
        }

        [Fact]
        public async Task StressAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            await conn.SetBusyTimeoutAsync(TimeSpan.FromSeconds(1));

            int n = 500;
            var errors = new ConcurrentBag<string>();
            var tasks = new List<Task>();
            for (int i = 0; i < n; i++)
            {
                int ii = i;

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        var obj = new TestCustomer
                        {
                            FirstName = ii.ToString(),
                        };

                        await conn.InsertAsync(obj).ConfigureAwait(false);

                        if (obj.Id == 0)
                        {
                            errors.Add("Bad Id");
                        }

                        List<TestCustomer> query = await (from c in conn.Table<TestCustomer>() where c.Id == obj.Id select c).ToListAsync().ConfigureAwait(false);

                        TestCustomer? obj2 = query.FirstOrDefault();

                        if (obj2 is null)
                        {
                            errors.Add("Failed query");
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"[{ii}] {ex}");
                    }
                }));
            }

            await Task.WhenAll(tasks.ToArray());

            int count = await conn.Table<TestCustomer>().CountAsync();

            foreach (string e in errors)
            {
                this.TestOutputHelper.WriteLine("ERROR " + e);
            }

            errors.Count.Should().Be(0, string.Join(", ", errors));
            n.Should().Be(count);
        }

        [Fact]
        public async Task TestCreateTableAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            // drop the TestCustomer table...
            await conn.ExecuteAsync("drop table if exists TestCustomer");

            // run...
            await conn.CreateTableAsync<TestCustomer>();

            // check...
            using CoreSQLiteConnection check = new CoreSQLiteConnection(this.TestFileSystem, env.DatabaseFilePath, true, new CoreQueryLogger(this.TestCaseLogger));

            // run it - if it's missing we'll get a failure...
            check.Execute("select * from TestCustomer");
        }

        [Fact]
        public async Task DropTableAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // drop it...
            await conn.DropTableAsync<TestCustomer>();

            // check...
            using CoreSQLiteConnection check = new CoreSQLiteConnection(this.TestFileSystem, env.DatabaseFilePath, true, new CoreQueryLogger(this.TestCaseLogger));

            // load it back and check - should be missing
            CoreSQLiteCommand command = check.CreateCommand("select name from sqlite_master where type='table' and name='TestCustomer'");
            command.ExecuteScalar<string>().Should().BeNull();
        }

        [Fact]
        public async Task DropTableAsyncNonGeneric()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // drop it...
            await conn.DropTableAsync(await conn.GetMappingAsync(typeof(TestCustomer)));

            // check...
            using CoreSQLiteConnection check = new CoreSQLiteConnection(this.TestFileSystem, env.DatabaseFilePath, true, new CoreQueryLogger(this.TestCaseLogger));

            // load it back and check - should be missing
            CoreSQLiteCommand command = check.CreateCommand("select name from sqlite_master where type='table' and name='TestCustomer'");
            command.ExecuteScalar<string>().Should().BeNull();
        }

        [Fact]
        public async Task TestInsertAsync()
        {
            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();

            // connect...
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // run...
            await conn.InsertAsync(testCustomer);

            // check that we got an id...
            testCustomer.Id.Should().NotBe(0);

            // check...
            using CoreSQLiteConnection check = new CoreSQLiteConnection(this.TestFileSystem, env.DatabaseFilePath, true, new CoreQueryLogger(this.TestCaseLogger));

            // load it back...
            TestCustomer? loaded = check.Get<TestCustomer>(testCustomer.Id);
            loaded.Should().NotBeNull();
            loaded!.Id.Should().Be(testCustomer.Id);
        }

        [Fact]
        public async Task UpdateAsync()
        {
            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();

            // connect...
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // run...
            await conn.InsertAsync(testCustomer);

            // change it...
            string newEmail = Guid.NewGuid().ToString();
            testCustomer.Email = newEmail;

            // save it...
            await conn.UpdateAsync(testCustomer);

            // check...
            using CoreSQLiteConnection check = new CoreSQLiteConnection(this.TestFileSystem, env.DatabaseFilePath, true, new CoreQueryLogger(this.TestCaseLogger));

            // load it back - should be changed...
            TestCustomer? loaded = check.Get<TestCustomer>(testCustomer.Id);
            loaded.Should().NotBeNull();
            newEmail.Should().Be(loaded?.Email);
        }

        [Fact]
        public async Task UpdateAsyncWithType()
        {
            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();

            // connect...
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // run...
            await conn.InsertAsync(testCustomer);

            // change it...
            string newEmail = Guid.NewGuid().ToString();
            testCustomer.Email = newEmail;

            // save it...
            await conn.UpdateAsync(testCustomer, typeof(TestCustomer));

            // check...
            TestCustomer? loaded = await conn.GetAsync<TestCustomer>(testCustomer.Id);
            loaded.Should().NotBeNull();
            newEmail.Should().Be(loaded?.Email);
        }

        [Fact]
        public async Task UpdateAllAsync()
        {
            // create...
            TestCustomer testCustomer1 = this.CreateTestCustomer(firstName: "Frank");
            TestCustomer testCustomer2 = this.CreateTestCustomer(country: "Mexico");
            TestCustomer[] testCustomers = new[] { testCustomer1, testCustomer2 };

            // connect...
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // run...
            await conn.InsertAllAsync(testCustomers);

            // change it...
            string newEmail1 = Guid.NewGuid().ToString();
            string newEmail2 = Guid.NewGuid().ToString();
            testCustomer1.Email = newEmail1;
            testCustomer2.Email = newEmail2;

            // save it...
            await conn.UpdateAllAsync(testCustomers);

            // check...
            List<TestCustomer> loaded = await conn.Table<TestCustomer>().ToListAsync();
            loaded.Count(x => x.Email == newEmail1).Should().Be(1);
            loaded.Count(x => x.Email == newEmail2).Should().Be(1);
        }

        [Fact]
        public async Task TestDeleteAsync()
        {
            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();

            // connect...
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // run...
            await conn.InsertAsync(testCustomer);

            // delete it...
            await conn.DeleteAsync(testCustomer);

            // check...
            using CoreSQLiteConnection check = new CoreSQLiteConnection(this.TestFileSystem, env.DatabaseFilePath, true, new CoreQueryLogger(this.TestCaseLogger));

            // load it back - should be null...
            var loaded = check.Table<TestCustomer>().Where(v => v.Id == testCustomer.Id).ToList();
            loaded.Count.Should().Be(0);
        }

        [Fact]
        public async Task GetAsync()
        {
            // create...
            TestCustomer testCustomer = new TestCustomer
            {
                FirstName = "foo",
                LastName = "bar",
                Email = Guid.NewGuid().ToString(),
            };

            // connect and insert...
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.InsertAsync(testCustomer);

            // check...
            testCustomer.Id.Should().NotBe(0);

            // get it back...
            Task<TestCustomer?> task = conn.GetAsync<TestCustomer>(testCustomer.Id);
            TestCustomer? loaded = await task;
            loaded.Should().NotBeNull();

            // check...
            loaded!.Id.Should().Be(testCustomer.Id);
        }

        [Fact]
        public async Task FindAsyncWithExpressionAsync()
        {
            // create...
            TestCustomer testCustomer = new TestCustomer
            {
                FirstName = "foo",
                LastName = "bar",
                Email = Guid.NewGuid().ToString(),
            };

            // connect and insert...
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.InsertAsync(testCustomer);

            // check...
            testCustomer.Id.Should().NotBe(0);

            // get it back...
            Task<TestCustomer?> task = conn.FindAsync<TestCustomer>(x => x.Id == testCustomer.Id);
            TestCustomer? loaded = await task;
            loaded.Should().NotBeNull();

            // check...
            loaded!.Id.Should().Be(testCustomer.Id);
        }

        [Fact]
        public async Task FindAsyncWithExpressionNullAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // get it back...
            TestCustomer? loaded = await conn.FindAsync<TestCustomer>(x => x.Id == 1);

            // check...
            loaded.Should().BeNull();
        }

        [Fact]
        public async Task TestFindAsyncItemPresentAsync()
        {
            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();

            // connect and insert...
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.InsertAsync(testCustomer);

            // check...
            testCustomer.Id.Should().NotBe(0);

            // get it back...
            TestCustomer? loaded = await conn.FindAsync<TestCustomer>(testCustomer.Id);
            loaded.Should().NotBeNull();

            // check...
            loaded!.Id.Should().Be(testCustomer.Id);
        }

        [Fact]
        public async Task TestFindAsyncItemMissingAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // now get one that doesn't exist...
            (await conn.FindAsync<TestCustomer>(-1)).Should().BeNull();
        }

        [Fact]
        public async Task TestQueryAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // insert some...
            List<TestCustomer> testCustomers = [];
            for (int index = 0; index < 5; index++)
            {
                TestCustomer testCustomer = this.CreateTestCustomer();

                // insert...
                await conn.InsertAsync(testCustomer);

                // add...
                testCustomers.Add(testCustomer);
            }

            // return the third one...
            Task<List<TestCustomer>> task = conn.QueryAsync<TestCustomer>("select * from TestCustomer where id=?", testCustomers[2].Id);
            await task;
            List<TestCustomer> loaded = await task;

            // check...
            loaded.Count.Should().Be(1);
            testCustomers[2].Email.Should().Be(loaded[0].Email);
        }

        [Fact]
        public async Task TestSingleQueryAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // insert some...
            List<TestCustomer> testCustomers = [];
            for (int index = 0; index < 5; index++)
            {
                TestCustomer testCustomer = this.CreateTestCustomer();

                // insert...
                await conn.InsertAsync(testCustomer);

                // add...
                testCustomers.Add(testCustomer);
            }

            // return the third one...
            Task<List<string>> task = conn.QueryScalarsAsync<string>("select Email from TestCustomer where id=?", testCustomers[2].Id);
            await task;
            List<string> loaded = await task;

            // check...
            loaded.Count.Should().Be(1);
            testCustomers[2].Email.Should().Be(loaded[0]);

            // return the third one...
            Task<List<int>> inttest = conn.QueryScalarsAsync<int>("select Id from TestCustomer where id=?", testCustomers[2].Id);
            await task;
            List<int> intloaded = await inttest;

            // check...
            loaded.Count.Should().Be(1);
            testCustomers[2].Id.Should().Be(intloaded[0]);

            // return string list
            Task<List<string>> listtask = conn.QueryScalarsAsync<string>("select Email from TestCustomer order by Id");
            List<string> listloaded = await listtask;

            // check...
            listloaded.Count.Should().Be(5);
            testCustomers[2].Email.Should().Be(listloaded[2]);

            // select columns
            Task<List<string>> columnstask = conn.QueryScalarsAsync<string>("select FirstName, LastName from TestCustomer");
            (await columnstask).Count.Should().Be(5);
        }

        [Fact]
        public async Task TestTableAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.ExecuteAsync("delete from TestCustomer");

            // insert some...
            List<TestCustomer> testCustomers = [];
            for (int index = 0; index < 5; index++)
            {
                TestCustomer testCustomer = new TestCustomer
                {
                    FirstName = "foo",
                    LastName = "bar",
                    Email = Guid.NewGuid().ToString(),
                };

                // insert...
                await conn.InsertAsync(testCustomer);

                // add...
                testCustomers.Add(testCustomer);
            }

            // run the table operation...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>();
            List<TestCustomer> loaded = await query.ToListAsync();

            // check that we got them all back...
            loaded.Count.Should().Be(5);
            loaded.Where(v => v.Id == testCustomers[0].Id).Should().NotBeNull();
            loaded.Where(v => v.Id == testCustomers[1].Id).Should().NotBeNull();
            loaded.Where(v => v.Id == testCustomers[2].Id).Should().NotBeNull();
            loaded.Where(v => v.Id == testCustomers[3].Id).Should().NotBeNull();
            loaded.Where(v => v.Id == testCustomers[4].Id).Should().NotBeNull();
        }

        [Fact]
        public async Task TestExecuteAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // do a manual insert...
            string email = Guid.NewGuid().ToString();
            await conn.ExecuteAsync("insert into TestCustomer (firstname, lastname, email) values (?, ?, ?)", "foo", "bar", email);

            // check...
            using CoreSQLiteConnection check = new CoreSQLiteConnection(this.TestFileSystem, env.DatabaseFilePath, true, new CoreQueryLogger(this.TestCaseLogger));

            // load it back - should be null...
            TableQuery<TestCustomer> result = check.Table<TestCustomer>().Where(v => v.Email == email);
            result.Should().NotBeNull();
        }

        [Fact]
        public async Task TestInsertAllAsync()
        {
            // create a bunch of TestCustomers...
            List<TestCustomer> testCustomers = [];
            for (int index = 0; index < 100; index++)
            {
                TestCustomer testCustomer = new TestCustomer
                {
                    FirstName = "foo",
                    LastName = "bar",
                    Email = Guid.NewGuid().ToString(),
                };
                testCustomers.Add(testCustomer);
            }

            // connect...
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // insert them all...
            await conn.InsertAllAsync(testCustomers);

            // check...
            using CoreSQLiteConnection check = new CoreSQLiteConnection(this.TestFileSystem, env.DatabaseFilePath, true, new CoreQueryLogger(this.TestCaseLogger));

            for (int index = 0; index < testCustomers.Count; index++)
            {
                // load it back and check...
                TestCustomer? loaded = check.Get<TestCustomer>(testCustomers[index].Id);
                loaded.Should().NotBeNull();
                loaded?.Email.Should().Be(testCustomers[index].Email);
            }
        }

        [Fact]
        public async Task TestRunInTransactionAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            bool transactionCompleted = false;

            // run...
            TestCustomer testCustomer = new TestCustomer();

            await conn.RunInTransactionAsync((c) =>
            {
                // insert...
                testCustomer.FirstName = "foo";
                testCustomer.LastName = "bar";
                testCustomer.Email = Guid.NewGuid().ToString();
                c.Insert(testCustomer);

                // delete it again...
                c.Execute("delete from TestCustomer where id=?", testCustomer.Id);

                // set completion flag
                transactionCompleted = true;
            });

            // check...
            transactionCompleted.Should().BeTrue();
            using CoreSQLiteConnection check = new CoreSQLiteConnection(this.TestFileSystem, env.DatabaseFilePath, true, new CoreQueryLogger(this.TestCaseLogger));

            // load it back and check - should be deleted...
            var loaded = check.Table<TestCustomer>().Where(v => v.Id == testCustomer.Id).ToList();
            loaded.Count.Should().Be(0);
        }

        [Fact]
        public async Task TestExecuteScalarAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // check...
            string? name = await conn.ExecuteScalarAsync<string>("select name from sqlite_master where type='table' and name='TestCustomer'");
            name.Should().Be("TestCustomer");

            // delete
            await conn.DeleteAllAsync<TestCustomer>();

            // check...
            int noDataResult = await conn.ExecuteScalarAsync<int>("select Max(Id) from TestCustomer where FirstName='hfiueyf8374fhi'");
            noDataResult.Should().Be(0);
        }

        [Fact]
        public async Task TestAsyncTableQueryToListAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();
            await conn.InsertAsync(testCustomer);

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>();
            Task<List<TestCustomer>> task = query.ToListAsync();
            await task;
            List<TestCustomer> items = await task;

            // check...
            TestCustomer loaded = items.Where(v => v.Id == testCustomer.Id).First();
            testCustomer.Email.Should().Be(loaded.Email);
        }

        [Fact]
        public async Task TestAsyncTableQueryToFirstAsyncFoundAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();
            await conn.InsertAsync(testCustomer);

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>().Where(v => v.Id == testCustomer.Id);
            Task<TestCustomer> task = query.FirstAsync();
            await task;
            TestCustomer loaded = await task;

            // check...
            testCustomer.Email.Should().Be(loaded.Email);
        }

        [Fact]
        public async Task TestAsyncTableQueryToFirstAsyncMissing()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();
            await conn.InsertAsync(testCustomer);

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>().Where(v => v.Id == -1);
            Task<TestCustomer> task = query.FirstAsync();
            ExceptionAssert.Throws<AggregateException>(task.Wait);
        }

        [Fact]
        public async Task TestAsyncTableQueryToFirstOrDefaultAsyncFoundAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();
            await conn.InsertAsync(testCustomer);

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>().Where(v => v.Id == testCustomer.Id);
            Task<TestCustomer?> task = query.FirstOrDefaultAsync();
            TestCustomer? loaded = await task;
            loaded.Should().NotBeNull();

            // check...
            testCustomer.Email.Should().Be(loaded?.Email);
        }

        [Fact]
        public async Task TestAsyncTableQueryToFirstOrDefaultAsyncMissingAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // create...
            TestCustomer testCustomer = this.CreateTestCustomer();
            await conn.InsertAsync(testCustomer);

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>().Where(v => v.Id == -1);
            Task<TestCustomer?> task = query.FirstOrDefaultAsync();
            TestCustomer? loaded = await task;

            // check...
            loaded.Should().BeNull();
        }

        [Fact]
        public async Task TestAsyncTableQueryWhereOperation()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();

            // create...
            TestCustomer testCustomer1 = this.CreateTestCustomer(string.Empty, "country");
            await conn.InsertAsync(testCustomer1);
            TestCustomer testCustomer2 = this.CreateTestCustomer("address");
            await conn.InsertAsync(testCustomer2);

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>();

            // check...
            TestCustomer loaded = (await query.Where(v => v.Id == testCustomer1.Id).ToListAsync()).First();
            testCustomer1.Email.Should().Be(loaded.Email);

            // check...
            TestCustomer emptyAddress = (await query.Where(v => string.IsNullOrEmpty(v.Address)).ToListAsync()).First();
            string.IsNullOrEmpty(emptyAddress.Address).Should().BeTrue();
            testCustomer1.Email.Should().Be(emptyAddress.Email);

            // check...
            TestCustomer nullCountry = (await query.Where(v => string.IsNullOrEmpty(v.Country)).ToListAsync()).First();
            string.IsNullOrEmpty(nullCountry.Country).Should().BeTrue();
            testCustomer2.Email.Should().Be(nullCountry.Email);

            // check...
            TestCustomer isNotNullorEmpty = (await query.Where(v => !string.IsNullOrEmpty(v.Country)).ToListAsync()).First();
            string.IsNullOrEmpty(isNotNullorEmpty.Country).Should().BeFalse();
            testCustomer1.Email.Should().Be(isNotNullorEmpty.Email);
        }

        [Fact]
        public async Task TestAsyncTableQueryCountAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.ExecuteAsync("delete from TestCustomer");

            // create...
            for (int index = 0; index < 10; index++)
            {
                await conn.InsertAsync(this.CreateTestCustomer());
            }

            // load...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>();
            Task<int> task = query.CountAsync();

            // check...
            (await task).Should().Be(10);
        }

        [Fact]
        public async Task TestAsyncTableOrderByAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.ExecuteAsync("delete from TestCustomer");

            // create...
            for (int index = 0; index < 10; index++)
            {
                await conn.InsertAsync(this.CreateTestCustomer());
            }

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>().OrderBy(v => v.Email);
            Task<List<TestCustomer>> task = query.ToListAsync();
            await task;
            List<TestCustomer> items = await task;

            // check...
            string.Compare(items[0].Email, items[9].Email).Should().Be(-1);
        }

        [Fact]
        public async Task TestAsyncTableOrderByDescendingAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.ExecuteAsync("delete from TestCustomer");

            // create...
            for (int index = 0; index < 10; index++)
            {
                await conn.InsertAsync(this.CreateTestCustomer());
            }

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>().OrderByDescending(v => v.Email);
            Task<List<TestCustomer>> task = query.ToListAsync();
            await task;
            List<TestCustomer> items = await task;

            // check...
            string.Compare(items[0].Email, items[9].Email).Should().Be(1);
        }

        [Fact]
        public async Task TestAsyncTableQueryTakeAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.ExecuteAsync("delete from TestCustomer");

            // create...
            for (int index = 0; index < 10; index++)
            {
                TestCustomer testCustomer = this.CreateTestCustomer();
                testCustomer.FirstName = index.ToString();
                await conn.InsertAsync(testCustomer);
            }

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>().OrderBy(v => v.FirstName).Take(1);
            Task<List<TestCustomer>> task = query.ToListAsync();
            await task;
            List<TestCustomer> items = await task;

            // check...
            items.Count.Should().Be(1);
            items[0].FirstName.Should().Be("0");
        }

        [Fact]
        public async Task TestAsyncTableQuerySkipAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.ExecuteAsync("delete from TestCustomer");

            // create...
            for (int index = 0; index < 10; index++)
            {
                TestCustomer testCustomer = this.CreateTestCustomer();
                testCustomer.FirstName = index.ToString();
                await conn.InsertAsync(testCustomer);
            }

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>().OrderBy(v => v.FirstName).Skip(5);
            Task<List<TestCustomer>> task = query.ToListAsync();
            await task;
            List<TestCustomer> items = await task;

            // check...
            items.Count.Should().Be(5);
            items[0].FirstName.Should().Be("5");
        }

        [Fact]
        public async Task TestAsyncTableElementAtAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.ExecuteAsync("delete from TestCustomer");

            // create...
            for (int index = 0; index < 10; index++)
            {
                TestCustomer testCustomer = this.CreateTestCustomer();
                testCustomer.FirstName = index.ToString();
                await conn.InsertAsync(testCustomer);
            }

            // query...
            AsyncTableQuery<TestCustomer> query = conn.Table<TestCustomer>().OrderBy(v => v.FirstName);
            Task<TestCustomer> task = query.ElementAtAsync(7);
            await task;
            TestCustomer loaded = await task;

            // check...
            loaded.FirstName.Should().Be("7");
        }

        [Fact]
        public async Task TestAsyncGetWithExpression()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;
            await conn.CreateTableAsync<TestCustomer>();
            await conn.ExecuteAsync("delete from TestCustomer");

            // create...
            for (int index = 0; index < 10; index++)
            {
                TestCustomer testCustomer = this.CreateTestCustomer();
                testCustomer.FirstName = index.ToString();
                await conn.InsertAsync(testCustomer);
            }

            // get...
            TestCustomer? loaded = await conn.GetAsync<TestCustomer>(x => x.FirstName == "7");
            loaded.Should().NotBeNull();

            // check...
            loaded!.FirstName.Should().Be("7");
        }

        [Fact]
        public async Task CreateTable()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTableResult r0 = await conn.CreateTableAsync<TestCustomer>();

            r0.Should().Be(CreateTableResult.Created);

            CreateTableResult r1 = await conn.CreateTableAsync<TestCustomer>();

            r1.Should().Be(CreateTableResult.Migrated);
            CreateTableResult r2 = await conn.CreateTableAsync<TestCustomer>();

            r2.Should().Be(CreateTableResult.Migrated);

            env.TraceBag.Count.Should().Be((4 * 3) + 1);
        }

        [Fact]
        public async Task CreateTableNonGeneric()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTableResult r0 = await conn.CreateTableAsync(typeof(TestCustomer));

            r0.Should().Be(CreateTableResult.Created);

            CreateTableResult r1 = await conn.CreateTableAsync(typeof(TestCustomer));

            r1.Should().Be(CreateTableResult.Migrated);

            CreateTableResult r2 = await conn.CreateTableAsync(typeof(TestCustomer));

            r2.Should().Be(CreateTableResult.Migrated);

            env.TraceBag.Count.Should().Be((4 * 3) + 1);
        }

        [Fact]
        public async Task CreateTablesNonGeneric()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTablesResult r0 = await conn.CreateTablesAsync(CreateFlags.None, typeof(TestCustomer), typeof(TestOrder));

            r0.Results[typeof(TestCustomer)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestOrder)].Should().Be(CreateTableResult.Created);
        }

        [Fact]
        public async Task CreateTables2()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTablesResult r0 = await conn.CreateTablesAsync<TestCustomer, TestOrder>();

            r0.Results[typeof(TestCustomer)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestOrder)].Should().Be(CreateTableResult.Created);
        }

        [Fact]
        public async Task CreateTables3()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTablesResult r0 = await conn.CreateTablesAsync<TestCustomer, TestOrder, TestOrderHistory>();
            r0.Results[typeof(TestCustomer)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestOrder)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestOrderHistory)].Should().Be(CreateTableResult.Created);
        }

        [Fact]
        public async Task CreateTables4()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTablesResult r0 = await conn.CreateTablesAsync<TestCustomer, TestOrder, TestOrderHistory, TestProduct>();
            r0.Results[typeof(TestCustomer)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestOrder)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestOrderHistory)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestProduct)].Should().Be(CreateTableResult.Created);
        }

        [Fact]
        public async Task CreateTables5()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTablesResult r0 = await conn.CreateTablesAsync<TestCustomer, TestOrder, TestOrderHistory, TestProduct, TestOrderLine>();
            r0.Results[typeof(TestCustomer)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestOrder)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestOrderHistory)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestProduct)].Should().Be(CreateTableResult.Created);
            r0.Results[typeof(TestOrderLine)].Should().Be(CreateTableResult.Created);
        }

        [Fact]
        public async Task CreateIndexAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTableResult r0 = await conn.CreateTableAsync<TestCustomer>();
            r0.Should().Be(CreateTableResult.Created);

            int ri = await conn.CreateIndexAsync<TestCustomer>(x => x.FirstName!);
            ri.Should().Be(0);
        }

        [Fact]
        public async Task CreateIndexAsyncNonGeneric()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTableResult r0 = await conn.CreateTableAsync<TestCustomer>();
            r0.Should().Be(CreateTableResult.Created);

            int ri = await conn.CreateIndexAsync(nameof(TestCustomer), nameof(TestCustomer.FirstName));
            ri.Should().Be(0);
        }

        [Fact]
        public async Task CreateIndexAsyncWithName()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTableResult r0 = await conn.CreateTableAsync<TestCustomer>();
            r0.Should().Be(CreateTableResult.Created);

            int ri = await conn.CreateIndexAsync("Foofoo", nameof(TestCustomer), nameof(TestCustomer.FirstName));
            ri.Should().Be(0);
        }

        [Fact]
        public async Task CreateIndexAsyncWithManyColumns()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTableResult r0 = await conn.CreateTableAsync<TestCustomer>();
            r0.Should().Be(CreateTableResult.Created);

            int ri = await conn.CreateIndexAsync(
                nameof(TestCustomer),
                new[] { nameof(TestCustomer.FirstName), nameof(TestCustomer.LastName) });

            ri.Should().Be(0);
        }

        [Fact]
        public async Task CreateIndexAsyncWithManyColumnsWithName()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTableResult r0 = await conn.CreateTableAsync<TestCustomer>();
            r0.Should().Be(CreateTableResult.Created);

            int ri = await conn.CreateIndexAsync("Foofoo", nameof(TestCustomer), new[] { nameof(TestCustomer.FirstName), nameof(TestCustomer.LastName) });

            ri.Should().Be(0);
        }

        [Fact]
        public async Task CloseAsync()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection conn = env.Connection;

            CreateTableResult r0 = await conn.CreateTableAsync<TestCustomer>();

            r0.Should().Be(CreateTableResult.Created);
        }

        [Fact]
        public async Task Issue881()
        {
            using var env = new TestEnvironment(this, this.TestCaseLogger);
            CoreSQLiteAsyncConnection connection = env.Connection;

            var t1 = Task.Run(async () =>
            await connection.RunInTransactionAsync(db => Thread.Sleep(TimeSpan.FromSeconds(0.2))));

            var t2 = Task.Run(async () =>
           {
               Thread.Sleep(TimeSpan.FromSeconds(0.1));
               await connection.RunInTransactionAsync(db => Thread.Sleep(TimeSpan.FromSeconds(0.1)));
           });

            await Task.WhenAll(t1, t2);
        }

        private TestCustomer CreateTestCustomer(string? address = null, string? country = null, string firstName = "")
        {
            var testCustomer = new TestCustomer()
            {
                FirstName = string.IsNullOrEmpty(firstName) ? "foo" : firstName,
                LastName = "bar",
                Email = Guid.NewGuid().ToString(),
                Address = address,
                Country = country,
            };
            return testCustomer;
        }

        // introduced to allow tests run each one with its own private database file, so they can be run in total isolation and in parallel
        // (by test Nunit runners that allow it)
        public class TestEnvironment : IDisposable
        {
            private readonly ICoreSQLiteTransactionLogger _sqLiteTransactionLogger;
            private ConcurrentBag<string> _traceBag = [];
            private bool alreadyDisposed = false; // To detect redundant calls

            public TestEnvironment(ICoreTestCase testCase, ICoreTestCaseLogger testCaseLogger, LogLevel tracerLogLevel = LogLevel.Trace, [CallerMemberName] string testName = "")
            {
                // each test gets its own database file named after the test itself.. and we include also a random number in the name
                // so it is possible to run concurrently the same test on multiple threads (some task runners allow this "stress testing" execution mode
                this.DatabaseFilePath = CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath<AsyncUnitTests>(testCase.TestFileSystem);
                this.NetworkingSystem = testCase.TestNetworkingSystem;

                this._sqLiteTransactionLogger = new CoreSQLiteTransactionLogger<AsyncUnitTests>(new CoreSQLiteTransactionLoggerProvider(testCase.TestFileSystem, tracerLogLevel), line =>
                {
                    this._traceBag.Add(line);
                    testCaseLogger.LogTrace(line);
                });

                testCase.TestNetworkingSystem.FileSystem.WaitToDeleteLockedFile(this.DatabaseFilePath);

                this.Connection = new CoreSQLiteAsyncConnection(testCase.TestFileSystem, this.DatabaseFilePath, true, this._sqLiteTransactionLogger);
            }

            ~TestEnvironment()
            {
                this.Dispose();
            }

            public ICoreNetworkingSystem NetworkingSystem { get; }

            public CoreSQLiteAsyncConnection Connection { get; }

            public string DatabaseFilePath { get; }

            public IReadOnlyCollection<string> TraceBag => this._traceBag;

            public void ClearTraceList()
            {
#if NET472_OR_GREATER || NETSTANDARD2_0_OR_GREATER
                this._traceBag = new();
#else
                this._traceBag.Clear();
#endif
            }

            // This code added to correctly implement the disposable pattern.
            void IDisposable.Dispose()
            {
                this.Dispose();
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose()
            {
                if (this.alreadyDisposed)
                {
                    return;
                }

                this.alreadyDisposed = true;

                this.Connection?.CloseAsync().GetAwaiter().GetResult();
            }
        }
    }
}
