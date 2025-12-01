// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="MappingUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteMappingUnitTests.
    /// </summary>
    [PlatformTrait(typeof(MappingUnitTests))]

    public class MappingUnitTests : CoreTestCaseBase
    {
        private const string ComputedColumnExpression = "json_extract(Body, '$.id')";

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public MappingUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void HasGoodNames()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);

            db.CreateTable<AFunnyTableName>();

            TableMapping? mapping = db.GetMapping<AFunnyTableName>();

            mapping.Should().NotBeNull();
            mapping!.TableName.Should().Be("AGoodTableName");

            mapping.Columns[0].Name.Should().Be("Id");
            mapping.Columns[1].Name.Should().Be("AGoodColumnName");
        }

        [Fact]
        public void OverrideNames()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<OverrideNamesClass>();

            List<TableColumnInfo> cols = db.GetTableInfo("OverrideNamesClass");
            cols.Count.Should().Be(3);
            cols.Exists(x => x.Name == "n").Should().BeTrue();
            cols.Exists(x => x.Name == "v").Should().BeTrue();

            var o = new OverrideNamesClass
            {
                Name = "Foo",
                Value = "Bar",
            };

            db.Insert(o);

            OverrideNamesClass oo = db.Table<OverrideNamesClass>().First();

            oo.Name.Should().Be("Foo");
            oo.Value.Should().Be("Bar");
        }

        [Fact]
        public void HiddenColumnType_VirtualComputedColumn()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<NodesVirtual>();

            List<TableColumnInfo> colInfos = db.GetTableInfo("NodesVirtual");
            TableColumnInfo? idColumnInfo = colInfos.FirstOrDefault(col => col.Name?.Equals("Id") ?? false);
            idColumnInfo.Should().NotBeNull();
            idColumnInfo!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);

            TableColumn? idCol = db.Table<NodesVirtual>().Table.FindColumnWithPropertyName("Id");
            idCol.Should().NotBeNull();
            idCol!.ComputedColumn.Should().NotBeNull();
            idCol.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.IsComputedColumn.Should().BeTrue();
            idCol.ComputedColumn!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.ComputedColumn!.Expression.Should().Be(ComputedColumnExpression);

            db.Insert(new NodesVirtual()
            {
                Body = "{\"id\": \"1\"}",
            });

            NodesVirtual? node = db.Get<NodesVirtual>(1);
            node.Should().NotBeNull();
            node!.Id.Should().Be("1");

            db.Insert(new NodesVirtual()
            {
                Body = "{\"id\": \"3\"}",
            });

            node = db.Get<NodesVirtual>(3);
            node.Should().NotBeNull();
            node!.Id.Should().Be("3");
        }

        [Fact]
        public void HiddenColumnType_StoredComputedColumn()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<NodesStored>();

            List<TableColumnInfo> colInfos = db.GetTableInfo("NodesStored");
            TableColumnInfo? idColumnInfo = colInfos.FirstOrDefault(col => col.Name?.Equals("Id") ?? false);
            idColumnInfo.Should().NotBeNull();
            idColumnInfo!.HiddenColumnType.Should().Be(HiddenColumnType.StoredComputedColumn);

            TableColumn? idCol = db.Table<NodesStored>().Table.FindColumnWithPropertyName("Id");
            idCol.Should().NotBeNull();
            idCol!.ComputedColumn.Should().NotBeNull();
            idCol.HiddenColumnType.Should().Be(HiddenColumnType.StoredComputedColumn);
            idCol.IsComputedColumn.Should().BeTrue();
            idCol.ComputedColumn!.HiddenColumnType.Should().Be(HiddenColumnType.StoredComputedColumn);
            idCol.ComputedColumn!.Expression.Should().Be(ComputedColumnExpression);

            db.Insert(new NodesStored()
            {
                Body = "{\"id\": \"1\"}",
            });

            NodesStored? node = db.Get<NodesStored>(1);
            node.Should().NotBeNull();
            node!.Id.Should().Be("1");

            db.Insert(new NodesStored()
            {
                Body = "{\"id\": \"3\"}",
            });

            node = db.Get<NodesStored>(3);
            node.Should().NotBeNull();
            node!.Id.Should().Be("3");
        }

        [Fact]
        public void Issue86()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<Foo>();

            db.Insert(new Foo { Bar = 42 });
            db.Insert(new Foo { Bar = 69 });

            Foo? found42 = db.Table<Foo>().FirstOrDefault(f => f.Bar == 42);
            found42.Should().NotBeNull();

            var ordered = new List<Foo>(db.Table<Foo>().OrderByDescending(f => f.Bar));
            ordered.Count.Should().Be(2);
            ordered[0].Bar.Should().Be(69);
            ordered[1].Bar.Should().Be(42);
        }

        [Fact]
        public void OnlyKey()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<OnlyKeyModel>();

            db.InsertOrReplace(new OnlyKeyModel { MyModelId = "Foo" });
            OnlyKeyModel? foo = db.Get<OnlyKeyModel>("Foo");
            foo.Should().NotBeNull();
            foo!.MyModelId.Should().Be("Foo");

            db.Insert(new OnlyKeyModel { MyModelId = "Bar" });
            OnlyKeyModel? bar = db.Get<OnlyKeyModel>("Bar");
            bar.Should().NotBeNull();
            bar!.MyModelId.Should().Be("Bar");

            db.Update(new OnlyKeyModel { MyModelId = "Foo" });
            OnlyKeyModel? foo2 = db.Get<OnlyKeyModel>("Foo");
            foo2.Should().NotBeNull();
            foo2!.MyModelId.Should().Be("Foo");
        }

        [Fact]
        public void TableMapping_MapsValueTypes()
        {
            var mapping = new TableMapping(typeof((int A, string B, double? C)));

            mapping.Columns.Length.Should().Be(3);
            mapping.Columns[0].Name.Should().Be("Item1");
            mapping.Columns[1].Name.Should().Be("Item2");
            mapping.Columns[2].Name.Should().Be("Item3");
        }

        public class OnlyKeyModel
        {
            [PrimaryKey]
            public string? MyModelId { get; set; }
        }

        [Table("foo")]
        public class Foo
        {
            [Column("baz")]
            public int Bar { get; set; }
        }

        [Table("AGoodTableName")]
        private class AFunnyTableName
        {
            [PrimaryKey]
            public int Id { get; set; }

            [Column("AGoodColumnName")]
            public string? AFunnyColumnName { get; set; }
        }

        private class OverrideNamesBase
        {
            [PrimaryKey, AutoIncrement]
            public int Id { get; set; }

            public virtual string? Name { get; set; }

            public virtual string? Value { get; set; }
        }

        private class OverrideNamesClass : OverrideNamesBase
        {
            [Column("n")]
            public override string? Name { get; set; }

            [Column("v")]
            public override string? Value { get; set; }
        }

        private class NodesVirtual
        {
            [Computed("json_extract(Body, '$.id')", HiddenColumnType.VirtualComputedColumn), NotNull, PrimaryKey]
            public string? Id { get; set; }

            public string? Body { get; set; }
        }

        private class NodesStored
        {
            [Computed("json_extract(Body, '$.id')", HiddenColumnType.StoredComputedColumn), NotNull, PrimaryKey]
            public string? Id { get; set; }

            public string? Body { get; set; }
        }
    }
}
