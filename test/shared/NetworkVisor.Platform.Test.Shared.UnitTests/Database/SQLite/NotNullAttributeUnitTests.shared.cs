// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="NotNullAttributeUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Reflection;
using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Exceptions;
using NetworkVisor.Core.Database.Providers.SQLite.Interop;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteNotNullAttributeUnitTests.
    /// </summary>
    [PlatformTrait(typeof(NotNullAttributeUnitTests))]

    public class NotNullAttributeUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotNullAttributeUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public NotNullAttributeUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void PrimaryKeyHasNotNullConstraint()
        {
            using var db = new TestDb<NotNullAttributeUnitTests>(this.TestFileSystem);

            db.CreateTable<ClassWithPK>();
            List<TableColumnInfo> cols = db.GetTableInfo("ClassWithPK");

            IList<string> joined = (from expected in this.GetExpectedColumnInfos(typeof(ClassWithPK))
                                    join actual in cols on expected.Name equals actual.Name
                                    where actual.NotNull != expected.NotNull
                                    select actual.Name).ToList();

            cols.Count().Should().NotBe(0, "Failed to get table info");
            (!joined.Any()).Should().BeTrue($"not null constraint was not created for the following properties: {string.Join(", ", joined.ToArray())}");
        }

        [Fact]
        public void CreateTableWithNotNullConstraints()
        {
            using var db = new TestDb<NotNullAttributeUnitTests>(this.TestFileSystem);
            db.CreateTable<NotNullNoPK>();
            List<TableColumnInfo> cols = db.GetTableInfo("NotNullNoPK");

            IList<string> joined = (from expected in this.GetExpectedColumnInfos(typeof(NotNullNoPK))
                                    join actual in cols on expected.Name equals actual.Name
                                    where actual.NotNull != expected.NotNull
                                    select actual.Name).ToList();

            cols.Count().Should().NotBe(0, "Failed to get table info");
            joined.Count().Should().Be(0, $"not null constraint was not created for the following properties: {string.Join(", ", joined.ToArray())}");
        }

        [Fact]
        public void InsertWithNullsThrowsException()
        {
            using (var db = new TestDb<NotNullAttributeUnitTests>(this.TestFileSystem))
            {
                db.CreateTable<NotNullNoPK>();

                try
                {
                    var obj = new NotNullNoPK();
                    db.Insert(obj);
                }
                catch (NotNullConstraintViolationException)
                {
                    return;
                }
                catch (CoreSQLiteException ex)
                {
                    if (SQLite3.LibVersionNumber() < 3007017 && ex.Result == SQLite3.Result.Constraint)
                    {
                        this.Inconclusive();
                        return;
                    }
                }
            }

            Assert.Fail("Expected an exception of type NotNullConstraintViolationException to be thrown. No exception was thrown.");
        }

        [Fact]
        public void UpdateWithNullThrowsException()
        {
            using (var db = new TestDb<NotNullAttributeUnitTests>(this.TestFileSystem))
            {
                db.CreateTable<NotNullNoPK>();

                try
                {
                    var obj = new NotNullNoPK()
                    {
                        AnotherRequiredStringProp = "Another required string",
                        RequiredIntProp = 123,
                        RequiredStringProp = "Required string",
                    };
                    db.Insert(obj);
                    obj.RequiredStringProp = null;
                    db.Update(obj);
                }
                catch (NotNullConstraintViolationException)
                {
                    return;
                }
                catch (CoreSQLiteException ex)
                {
                    if (SQLite3.LibVersionNumber() < 3007017 && ex.Result == SQLite3.Result.Constraint)
                    {
                        this.Inconclusive();
                        return;
                    }
                }
            }

            Assert.Fail("Expected an exception of type NotNullConstraintViolationException to be thrown. No exception was thrown.");
        }

        [Fact]
        public void NotNullConstraintExceptionListsOffendingColumnsOnInsert()
        {
            using var db = new TestDb<NotNullAttributeUnitTests>(this.TestFileSystem);
            db.CreateTable<NotNullNoPK>();

            try
            {
                var obj = new NotNullNoPK() { RequiredStringProp = "Some value" };
                db.Insert(obj);
            }
            catch (NotNullConstraintViolationException ex)
            {
                string expected = "AnotherRequiredStringProp, RequiredIntProp";
                string actual = string.Join(", ", ex.Columns.Where(c => !c.IsPK).OrderBy(p => p.PropertyName).Select(c => c.PropertyName));

                actual.Should().Be(expected, "NotNullConstraintViolationException did not correctly list the columns that violated the constraint");
                return;
            }
            catch (CoreSQLiteException ex)
            {
                if (SQLite3.LibVersionNumber() < 3007017 && ex.Result == SQLite3.Result.Constraint)
                {
                    this.Inconclusive();
                    return;
                }
            }

            Assert.Fail("Expected an exception of type NotNullConstraintViolationException to be thrown. No exception was thrown.");
        }

        [Fact]
        public void NotNullConstraintExceptionListsOffendingColumnsOnUpdate()
        {
            // Skip this test if the Dll doesn't support the extended SQLITE_CONSTRAINT codes
            using var db = new TestDb<NotNullAttributeUnitTests>(this.TestFileSystem);
            db.CreateTable<NotNullNoPK>();

            try
            {
                var obj = new NotNullNoPK()
                {
                    AnotherRequiredStringProp = "Another required string",
                    RequiredIntProp = 123,
                    RequiredStringProp = "Required string",
                };
                db.Insert(obj);
                obj.RequiredStringProp = null;
                db.Update(obj);
            }
            catch (NotNullConstraintViolationException ex)
            {
                string expected = "RequiredStringProp";
                string actual = string.Join(", ", ex.Columns.Where(c => !c.IsPK).OrderBy(p => p.PropertyName).Select(c => c.PropertyName));

                actual.Should().Be(expected, "NotNullConstraintViolationException did not correctly list the columns that violated the constraint");

                return;
            }
            catch (CoreSQLiteException ex)
            {
                if (SQLite3.LibVersionNumber() < 3007017 && ex.Result == SQLite3.Result.Constraint)
                {
                    this.Inconclusive();
                    return;
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected an exception of type NotNullConstraintViolationException to be thrown. An exception of type {ex.GetType().Name} was thrown instead.");
            }

            Assert.Fail("Expected an exception of type NotNullConstraintViolationException to be thrown. No exception was thrown.");
        }

        [Fact]
        public void InsertQueryWithNullThrowsException()
        {
            // Skip this test if the Dll doesn't support the extended SQLITE_CONSTRAINT codes
            if (SQLite3.LibVersionNumber() >= 3007017)
            {
                using (var db = new TestDb<NotNullAttributeUnitTests>(this.TestFileSystem))
                {
                    db.CreateTable<NotNullNoPK>();

                    try
                    {
                        db.Execute("insert into \"NotNullNoPK\" (AnotherRequiredStringProp, RequiredIntProp, RequiredStringProp) values(?, ?, ?)", new object?[] { "Another required string", 123, null });
                    }
                    catch (NotNullConstraintViolationException)
                    {
                        return;
                    }
                    catch (CoreSQLiteException ex)
                    {
                        if (SQLite3.LibVersionNumber() < 3007017 && ex.Result == SQLite3.Result.Constraint)
                        {
                            this.Inconclusive();
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        Assert.Fail($"Expected an exception of type NotNullConstraintViolationException to be thrown. An exception of type {ex.GetType().Name} was thrown instead.");
                    }
                }

                Assert.Fail("Expected an exception of type NotNullConstraintViolationException to be thrown. No exception was thrown.");
            }
        }

        [Fact]
        public void UpdateQueryWithNullThrowsException()
        {
            // Skip this test if the Dll doesn't support the extended SQLITE_CONSTRAINT codes
            using var db = new TestDb<NotNullAttributeUnitTests>(this.TestFileSystem);
            db.CreateTable<NotNullNoPK>();

            try
            {
                db.Execute(
                    "insert into \"NotNullNoPK\" (AnotherRequiredStringProp, RequiredIntProp, RequiredStringProp) values(?, ?, ?)",
                    new object[] { "Another required string", 123, "Required string" });

                db.Execute(
                    "update \"NotNullNoPK\" set AnotherRequiredStringProp=?, RequiredIntProp=?, RequiredStringProp=? where ObjectId=?", new object?[] { "Another required string", 123, null, 1 });
            }
            catch (NotNullConstraintViolationException)
            {
                return;
            }
            catch (CoreSQLiteException ex)
            {
                if (SQLite3.LibVersionNumber() < 3007017 && ex.Result == SQLite3.Result.Constraint)
                {
                    this.Inconclusive();
                    return;
                }
            }
            catch (Exception ex)
            {
                Assert.Fail($"Expected an exception of type NotNullConstraintViolationException to be thrown. An exception of type {ex.GetType().Name} was thrown instead.");
            }

            Assert.Fail("Expected an exception of type NotNullConstraintViolationException to be thrown. No exception was thrown.");
        }

        [Fact]
        public void ExecuteNonQueryWithNullThrowsException()
        {
            using (var db = new TestDb<NotNullAttributeUnitTests>(this.TestFileSystem))
            {
                db.CreateTable<NotNullNoPK>();

                try
                {
                    var obj = new NotNullNoPK()
                    {
                        AnotherRequiredStringProp = "Another required prop",
                        RequiredIntProp = 123,
                        RequiredStringProp = "Required string prop",
                    };
                    db.Insert(obj);

                    var obj2 = new NotNullNoPK()
                    {
                        ObjectId = 1,
                        OptionalIntProp = 123,
                    };
                    db.InsertOrReplace(obj2);
                }
                catch (NotNullConstraintViolationException)
                {
                    return;
                }
                catch (CoreSQLiteException ex)
                {
                    if (SQLite3.LibVersionNumber() < 3007017 && ex.Result == SQLite3.Result.Constraint)
                    {
                        this.Inconclusive();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Assert.Fail($"Expected an exception of type NotNullConstraintViolationException to be thrown. An exception of type {ex.GetType().Name} was thrown instead.");
                }
            }

            Assert.Fail("Expected an exception of type NotNullConstraintViolationException to be thrown. No exception was thrown.");
        }

        private void Inconclusive()
        {
            this.TestOutputHelper.WriteLine("Detailed constraint information is only available in SQLite3 version 3.7.17 and above.");
        }

        private IEnumerable<TableColumnInfo> GetExpectedColumnInfos(Type type)
        {
            IEnumerable<TableColumnInfo> expectedValues = from prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty)
                                                          select new TableColumnInfo
                                                          {
                                                              Name = prop.Name,
                                                              NotNull = prop.GetCustomAttributes(typeof(NotNullAttribute), true).Length != 0 || prop.GetCustomAttributes(typeof(PrimaryKeyAttribute), true).Length != 0,
                                                          };

            return expectedValues;
        }

        private class NotNullNoPK
        {
            [PrimaryKey, AutoIncrement]
            public int? ObjectId { get; set; }

            [NotNull]
            public int? RequiredIntProp { get; set; }

            public int? OptionalIntProp { get; set; }

            [NotNull]
            public string? RequiredStringProp { get; set; }

            public string? OptionalStringProp { get; set; }

            [NotNull]
            public string? AnotherRequiredStringProp { get; set; }
        }

        private class ClassWithPK
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }
        }
    }
}
