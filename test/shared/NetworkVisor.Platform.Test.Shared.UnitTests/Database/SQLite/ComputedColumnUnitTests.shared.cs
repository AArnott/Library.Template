// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="ComputedColumnUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class ComputedColumnUnitTests.
    /// </summary>
    [PlatformTrait(typeof(ComputedColumnUnitTests))]

    public class ComputedColumnUnitTests : CoreTestCaseBase
    {
        private const string ComputedColumnExpressionText = "json_extract(Body, '$.id')";
        private const string ComputedColumnExpressionBlob = "jsonb_extract(Body, '$.id')";
        private const string ComputedColumnExpressionGuidBlob = "jsonb_extract(Body, '$.Guid')";

        /// <summary>
        /// Initializes a new instance of the <see cref="ComputedColumnUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public ComputedColumnUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void SQLiteComputedColumn_Virtual_Text_String()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<NodesVirtualText>();

            List<TableColumnInfo> colInfos = db.GetTableInfo("NodesVirtualText");
            TableColumnInfo? idColumnInfo = colInfos.FirstOrDefault(col => col.Name?.Equals("Id") ?? false);
            idColumnInfo.Should().NotBeNull();
            idColumnInfo!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);

            TableColumn? idCol = db.Table<NodesVirtualText>().Table.FindColumnWithPropertyName("Id");
            idCol.Should().NotBeNull();
            idCol!.ComputedColumn.Should().NotBeNull();
            idCol.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.IsComputedColumn.Should().BeTrue();
            idCol.ComputedColumn!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.ComputedColumn!.Expression.Should().Be(ComputedColumnExpressionText);

            // Insert record with Id=1
            db.Insert(new NodesVirtualText()
            {
                Body = "{\"id\": \"1\"}",
            });

            NodesVirtualText? node = db.Get<NodesVirtualText>(1);
            node.Should().NotBeNull();
            node!.Id.Should().Be("1");

            var newNode = new NodesVirtualText() { Body = "{\"id\": \"3\"}", };

            // Insert record with Id=3
            db.Insert(newNode);

            node = db.Get<NodesVirtualText>(3);
            node.Should().NotBeNull();
            node!.Id.Should().Be("3");

            // Update Id=3 to Id=4
            newNode.Id = "3";
            newNode.Body = "{\"id\": \"4\"}";
            db.Update(newNode);

            newNode = db.Get<NodesVirtualText>(4);
            newNode.Should().NotBeNull();
            newNode!.Id.Should().Be("4");
            db.Table<NodesVirtualText>().Count().Should().Be(2);

            // Delete Id=4
            db.Delete(newNode).Should().Be(1);
            db.Table<NodesVirtualText>().Count().Should().Be(1);
        }

        [Fact]
        public void SQLiteComputedColumn_Stored_Text_String()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<NodesStoredText>();

            List<TableColumnInfo> colInfos = db.GetTableInfo("NodesStoredText");
            TableColumnInfo? idColumnInfo = colInfos.FirstOrDefault(col => col.Name?.Equals("Id") ?? false);
            idColumnInfo.Should().NotBeNull();
            idColumnInfo!.HiddenColumnType.Should().Be(HiddenColumnType.StoredComputedColumn);

            TableColumn? idCol = db.Table<NodesStoredText>().Table.FindColumnWithPropertyName("Id");
            idCol.Should().NotBeNull();
            idCol!.ComputedColumn.Should().NotBeNull();
            idCol.HiddenColumnType.Should().Be(HiddenColumnType.StoredComputedColumn);
            idCol.IsComputedColumn.Should().BeTrue();
            idCol.ComputedColumn!.HiddenColumnType.Should().Be(HiddenColumnType.StoredComputedColumn);
            idCol.ComputedColumn!.Expression.Should().Be(ComputedColumnExpressionText);

            // Insert record with Id=1
            db.Insert(new NodesStoredText()
            {
                Body = "{\"id\": \"1\"}",
            });

            NodesStoredText? node = db.Get<NodesStoredText>(1);
            node.Should().NotBeNull();
            node!.Id.Should().Be("1");

            var newNode = new NodesStoredText() { Body = "{\"id\": \"3\"}", };

            // Insert record with Id=3
            db.Insert(newNode);

            node = db.Get<NodesStoredText>(3);
            node.Should().NotBeNull();
            node!.Id.Should().Be("3");

            // Update Id=3 to Id=4
            newNode.Id = "3";
            newNode.Body = "{\"id\": \"4\"}";
            db.Update(newNode);

            newNode = db.Get<NodesStoredText>(4);
            newNode.Should().NotBeNull();
            newNode!.Id.Should().Be("4");
            db.Table<NodesStoredText>().Count().Should().Be(2);

            // Delete Id=4
            db.Delete(newNode).Should().Be(1);
            db.Table<NodesStoredText>().Count().Should().Be(1);
        }

        [Fact]
        public void SQLiteComputedColumn_Virtual_Blob_String()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<NodesVirtualBlob>();

            List<TableColumnInfo> colInfos = db.GetTableInfo("NodesVirtualBlob");
            TableColumnInfo? idColumnInfo = colInfos.FirstOrDefault(col => col.Name?.Equals("Id") ?? false);
            idColumnInfo.Should().NotBeNull();
            idColumnInfo!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);

            TableColumn? idCol = db.Table<NodesVirtualBlob>().Table.FindColumnWithPropertyName("Id");
            idCol.Should().NotBeNull();
            idCol!.ComputedColumn.Should().NotBeNull();
            idCol.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.IsComputedColumn.Should().BeTrue();
            idCol.ComputedColumn!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.ComputedColumn!.Expression.Should().Be(ComputedColumnExpressionBlob);

            // Insert record with Id=1
            db.Insert(new NodesVirtualBlob()
            {
                Body = "{\"id\": \"1\"}",
            });

            NodesVirtualBlob? node = db.Get<NodesVirtualBlob>(1);
            node.Should().NotBeNull();
            node!.Id.Should().Be("1");

            var newNode = new NodesVirtualBlob() { Body = "{\"id\": \"3\"}", };

            // Insert record with Id=3
            db.Insert(newNode);

            node = db.Get<NodesVirtualBlob>(3);
            node.Should().NotBeNull();
            node!.Id.Should().Be("3");

            // Update Id=3 to Id=4
            newNode.Id = "3";
            newNode.Body = "{\"id\": \"4\"}";
            db.Update(newNode);

            newNode = db.Get<NodesVirtualBlob>(4);
            newNode.Should().NotBeNull();
            newNode!.Id.Should().Be("4");
            db.Table<NodesVirtualBlob>().Count().Should().Be(2);

            // Delete Id=4
            db.Delete(newNode).Should().Be(1);
            db.Table<NodesVirtualBlob>().Count().Should().Be(1);
        }

        [Fact]
        public void SQLiteComputedColumn_Stored_Blob_String()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<NodesStoredBlob>();

            List<TableColumnInfo> colInfos = db.GetTableInfo("NodesStoredBlob");
            TableColumnInfo? idColumnInfo = colInfos.FirstOrDefault(col => col.Name?.Equals("Id") ?? false);
            idColumnInfo.Should().NotBeNull();
            idColumnInfo!.HiddenColumnType.Should().Be(HiddenColumnType.StoredComputedColumn);

            TableColumn? idCol = db.Table<NodesStoredBlob>().Table.FindColumnWithPropertyName("Id");
            idCol.Should().NotBeNull();
            idCol!.ComputedColumn.Should().NotBeNull();
            idCol.HiddenColumnType.Should().Be(HiddenColumnType.StoredComputedColumn);
            idCol.IsComputedColumn.Should().BeTrue();
            idCol.ComputedColumn!.HiddenColumnType.Should().Be(HiddenColumnType.StoredComputedColumn);
            idCol.ComputedColumn!.Expression.Should().Be(ComputedColumnExpressionBlob);

            // Insert record with Id=1
            db.Insert(new NodesStoredBlob()
            {
                Body = "{\"id\": \"1\"}",
            });

            NodesStoredBlob? node = db.Get<NodesStoredBlob>(1);
            node.Should().NotBeNull();
            node!.Id.Should().Be("1");

            var newNode = new NodesStoredBlob() { Body = "{\"id\": \"3\"}", };

            // Insert record with Id=3
            db.Insert(newNode);

            node = db.Get<NodesStoredBlob>(3);
            node.Should().NotBeNull();
            node!.Id.Should().Be("3");

            // Update Id=3 to Id=4
            newNode.Id = "3";
            newNode.Body = "{\"id\": \"4\"}";
            db.Update(newNode);

            newNode = db.Get<NodesStoredBlob>(4);
            newNode.Should().NotBeNull();
            newNode!.Id.Should().Be("4");
            db.Table<NodesStoredBlob>().Count().Should().Be(2);

            // Delete Id=4
            db.Delete(newNode).Should().Be(1);
            db.Table<NodesStoredBlob>().Count().Should().Be(1);
        }

        [Fact]
        public void SQLiteComputedColumn_Virtual_Text_Guid()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<NodesGuidVirtualText>();
            db.CreateIndex<NodesGuidVirtualText>(x => x.Guid, true);

            List<TableColumnInfo> colInfos = db.GetTableInfo("NodesGuidVirtualText");
            TableColumnInfo? idColumnInfo = colInfos.FirstOrDefault(col => col.Name?.Equals("Guid") ?? false);
            idColumnInfo.Should().NotBeNull();
            idColumnInfo!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);

            TableColumn? idCol = db.Table<NodesGuidVirtualText>().Table.FindColumnWithPropertyName("Guid");
            idCol.Should().NotBeNull();
            idCol!.ComputedColumn.Should().NotBeNull();
            idCol.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.IsComputedColumn.Should().BeTrue();
            idCol.ComputedColumn!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.ComputedColumn!.Expression.Should().Be(ComputedColumnExpressionGuidBlob);

            Guid guidID = Guid.NewGuid();

            // Insert record with guidID
            db.Insert(new NodesGuidVirtualText() { Body = $"{{\"Guid\": \"{guidID}\"}}", });

            NodesGuidVirtualText? node = db.Get<NodesGuidVirtualText>(guidID);
            node.Should().NotBeNull();
            node!.Guid.Should().Be(guidID);

            Guid guidID2 = Guid.NewGuid();
            var newNode = new NodesGuidVirtualText() { Body = $"{{\"Guid\": \"{guidID2}\"}}", };

            // Insert record with guidID2
            db.Insert(newNode);

            node = db.Get<NodesGuidVirtualText>(guidID2);
            node.Should().NotBeNull();
            node!.Guid.Should().Be(guidID2);

            // Update guidID2 to guidID3
            Guid guidID3 = Guid.NewGuid();
            newNode.Guid = guidID2;
            newNode.Body = $"{{\"Guid\": \"{guidID3}\"}}";
            db.Update(newNode);

            newNode = db.Get<NodesGuidVirtualText>(guidID3);
            newNode.Should().NotBeNull();
            newNode!.Guid.Should().Be(guidID3);
            db.Table<NodesGuidVirtualText>().Count().Should().Be(2);

            // Delete guidID3
            db.Delete(newNode).Should().Be(1);
            db.Table<NodesGuidVirtualText>().Count().Should().Be(1);
        }

        [Fact]
        public void SQLiteComputedColumn_Virtual_Blob_Guid()
        {
            using var db = new TestDb<MappingUnitTests>(this.TestFileSystem);
            db.CreateTable<NodesGuidVirtualBlob>();
            db.CreateIndex<NodesGuidVirtualBlob>(x => x.Guid, true);

            List<TableColumnInfo> colInfos = db.GetTableInfo("NodesGuidVirtualBlob");
            TableColumnInfo? idColumnInfo = colInfos.FirstOrDefault(col => col.Name?.Equals("Guid") ?? false);
            idColumnInfo.Should().NotBeNull();
            idColumnInfo!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);

            TableColumn? idCol = db.Table<NodesGuidVirtualBlob>().Table.FindColumnWithPropertyName("Guid");
            idCol.Should().NotBeNull();
            idCol!.ComputedColumn.Should().NotBeNull();
            idCol.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.IsComputedColumn.Should().BeTrue();
            idCol.ComputedColumn!.HiddenColumnType.Should().Be(HiddenColumnType.VirtualComputedColumn);
            idCol.ComputedColumn!.Expression.Should().Be(ComputedColumnExpressionGuidBlob);

            Guid guidID = Guid.NewGuid();

            // Insert record with guidID
            db.Insert(new NodesGuidVirtualBlob() { Body = $"{{\"Guid\": \"{guidID}\"}}", });

            NodesGuidVirtualBlob? node = db.Get<NodesGuidVirtualBlob>(guidID);
            node.Should().NotBeNull();
            node!.Guid.Should().Be(guidID);

            Guid guidID2 = Guid.NewGuid();
            var newNode = new NodesGuidVirtualBlob() { Body = $"{{\"Guid\": \"{guidID2}\"}}", };

            // Insert record with guidID2
            db.Insert(newNode);

            node = db.Get<NodesGuidVirtualBlob>(guidID2);
            node.Should().NotBeNull();
            node!.Guid.Should().Be(guidID2);

            // Update guidID2 to guidID3
            Guid guidID3 = Guid.NewGuid();
            newNode.Guid = guidID2;
            newNode.Body = $"{{\"Guid\": \"{guidID3}\"}}";
            db.Update(newNode);

            newNode = db.Get<NodesGuidVirtualBlob>(guidID3);
            newNode.Should().NotBeNull();
            newNode!.Guid.Should().Be(guidID3);
            db.Table<NodesGuidVirtualBlob>().Count().Should().Be(2);

            // Delete guidID3
            db.Delete(newNode).Should().Be(1);
            db.Table<NodesGuidVirtualBlob>().Count().Should().Be(1);
        }

        private class NodesVirtualText
        {
            [Computed(ComputedColumnExpressionText, HiddenColumnType.VirtualComputedColumn), Core.Database.Providers.SQLite.Attributes.NotNull, PrimaryKey]
            public string? Id { get; set; }

            [JsonText]
            public string? Body { get; set; }
        }

        private class NodesStoredText
        {
            [Computed(ComputedColumnExpressionText, HiddenColumnType.StoredComputedColumn), Core.Database.Providers.SQLite.Attributes.NotNull, PrimaryKey]
            public string? Id { get; set; }

            [JsonText]
            public string? Body { get; set; }
        }

        private class NodesVirtualBlob
        {
            [Computed(ComputedColumnExpressionBlob, HiddenColumnType.VirtualComputedColumn), Core.Database.Providers.SQLite.Attributes.NotNull, PrimaryKey]
            public string? Id { get; set; }

            [JsonBlob]
            public string? Body { get; set; }
        }

        private class NodesStoredBlob
        {
            [Computed(ComputedColumnExpressionBlob, HiddenColumnType.StoredComputedColumn), Core.Database.Providers.SQLite.Attributes.NotNull, PrimaryKey]
            public string? Id { get; set; }

            [JsonBlob]
            public string? Body { get; set; }
        }

        private class NodesGuidVirtualText
        {
            [Computed(ComputedColumnExpressionGuidBlob, HiddenColumnType.VirtualComputedColumn), Core.Database.Providers.SQLite.Attributes.NotNull, PrimaryKey]
            public Guid Guid { get; set; }

            [JsonText]
            public string? Body { get; set; }
        }

        private class NodesGuidVirtualBlob
        {
            [Computed(ComputedColumnExpressionGuidBlob, HiddenColumnType.VirtualComputedColumn), Core.Database.Providers.SQLite.Attributes.NotNull, PrimaryKey]
            public Guid Guid { get; set; }

            [JsonBlob]
            public string? Body { get; set; }
        }
    }
}
