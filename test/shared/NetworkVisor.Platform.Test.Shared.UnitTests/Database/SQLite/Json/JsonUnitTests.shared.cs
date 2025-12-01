// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="JsonUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Net.NetworkInformation;
using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestDevices;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Json
{
    /// <summary>
    /// Class JsonUnitTests.
    /// </summary>
    [PlatformTrait(typeof(JsonUnitTests))]

    public partial class JsonUnitTests : CoreTestCaseBase
    {
        private const string ComputedColumnExpressionDeviceId = "jsonb_extract(\"NetworkDevice\", '$.ObjectId')";
        private const string ComputedColumnExpressionCreatedTimestamp = "jsonb_extract(\"NetworkDevice\", '$.CreatedTimestamp')";
        private const string ComputedColumnExpressionModifiedTimestamp = "jsonb_extract(\"NetworkDevice\", '$.ModifiedTimestamp')";
        private const string ComputedColumnExpressionIPAddress = "jsonb_extract(\"NetworkDevice\", '$.IPAddress')";
        private const string ComputedColumnExpressionPhysicalAddress = "jsonb_extract(\"NetworkDevice\", '$.PhysicalAddress')";
        private const string ComputedColumnExpressionDeviceType = "jsonb_extract(\"NetworkDevice\", '$.DeviceType')";
        private const string _defaultPassword = "TestPassword";

        private string userSecureStorageFilePath;

        /// <summary>
        /// Initializes a new instance of the <see cref="JsonUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public JsonUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.userSecureStorageFilePath = this.TestFileSystem.GetLocalUserAppDatabaseTempFilePath("dbtest_sqlite_", ".db");
        }

        [Fact]
        public void SQLiteJsonUnit_Insert_JsonText()
        {
            using var jsonDb = new TestJsonDb(this.TestFileSystem);

            jsonDb.Insert(new TestJsonNode()
            {
                JsonText = @"
                {
                ""id"": ""2""
                }",
            });

            jsonDb.Table<TestJsonNode>().Count().Should().Be(1);
            TableColumn colJsonText = jsonDb.Table<TestJsonNode>().Table.Columns[1];
            colJsonText.Name.Should().Be("JsonText");
            colJsonText.IsJsonText.Should().BeTrue();
            colJsonText.IsJsonBlob.Should().BeFalse();
            colJsonText.IsJsonPretty.Should().BeFalse();

            TableColumn colJsonBlob = jsonDb.Table<TestJsonNode>().Table.Columns[2];
            colJsonBlob.Name.Should().Be("JsonBlob");
            colJsonBlob.IsJsonText.Should().BeFalse();
            colJsonBlob.IsJsonBlob.Should().BeTrue();
            colJsonBlob.IsJsonPretty.Should().BeFalse();

            // Before Update
            this.TestOutputHelper.WriteLine("Original");
            TestJsonNode? resultRow = jsonDb.Table<TestJsonNode>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonText.Should().Be("""{"id":"2"}""");
        }

        [Fact]
        public void SQLiteJsonUnit_Update_JsonText()
        {
            using var jsonDb = new TestJsonDb(this.TestFileSystem);
            var jsonNode = new TestJsonNode()
            {
                JsonText = @"
                {
                ""id"": ""2""
                }",
            };

            jsonDb.Insert(jsonNode);

            jsonDb.Table<TestJsonNode>().Count().Should().Be(1);
            TableColumn colJsonText = jsonDb.Table<TestJsonNode>().Table.Columns[1];
            colJsonText.Name.Should().Be("JsonText");
            colJsonText.IsJsonText.Should().BeTrue();
            colJsonText.IsJsonBlob.Should().BeFalse();
            colJsonText.IsJsonPretty.Should().BeFalse();

            TableColumn colJsonBlob = jsonDb.Table<TestJsonNode>().Table.Columns[2];
            colJsonBlob.Name.Should().Be("JsonBlob");
            colJsonBlob.IsJsonText.Should().BeFalse();
            colJsonBlob.IsJsonBlob.Should().BeTrue();
            colJsonBlob.IsJsonPretty.Should().BeFalse();

            // Before Update
            this.TestOutputHelper.WriteLine("Original");
            TestJsonNode? resultRow = jsonDb.Table<TestJsonNode>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonText.Should().Be("""{"id":"2"}""");

            jsonNode.JsonText = @"
                {
                ""id"": ""3""
                }";

            jsonDb.Update(jsonNode);

            // Before Update
            this.TestOutputHelper.WriteLine("\nAfter Update");
            resultRow = jsonDb.Table<TestJsonNode>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonText.Should().Be("""{"id":"3"}""");
        }

        [Fact]
        public void SQLiteJsonUnit_Insert_JsonBlob()
        {
            using var jsonDb = new TestJsonDb(this.TestFileSystem);

            jsonDb.Insert(new TestJsonNode()
            {
                JsonBlob = @"
                {
                ""id"": ""2""
                }",
            });

            jsonDb.Table<TestJsonNode>().Count().Should().Be(1);
            TableColumn colJsonText = jsonDb.Table<TestJsonNode>().Table.Columns[1];
            colJsonText.Name.Should().Be("JsonText");
            colJsonText.IsJsonText.Should().BeTrue();
            colJsonText.IsJsonBlob.Should().BeFalse();
            colJsonText.IsJsonPretty.Should().BeFalse();

            TableColumn colJsonBlob = jsonDb.Table<TestJsonNode>().Table.Columns[2];
            colJsonBlob.Name.Should().Be("JsonBlob");
            colJsonBlob.IsJsonText.Should().BeFalse();
            colJsonBlob.IsJsonBlob.Should().BeTrue();
            colJsonBlob.IsJsonPretty.Should().BeFalse();

            // Before Update
            this.TestOutputHelper.WriteLine("Original");
            TestJsonNode? resultRow = jsonDb.Table<TestJsonNode>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonBlob.Should().Be("""{"id":"2"}""");
        }

        [Fact]
        public void SQLiteJsonUnit_Update_JsonBlob()
        {
            using var jsonDb = new TestJsonDb(this.TestFileSystem);
            var jsonNode = new TestJsonNode()
            {
                JsonBlob = @"
                {
                ""id"": ""2""
                }",
            };

            jsonDb.Insert(jsonNode);

            jsonDb.Table<TestJsonNode>().Count().Should().Be(1);
            TableColumn colJsonText = jsonDb.Table<TestJsonNode>().Table.Columns[1];
            colJsonText.Name.Should().Be("JsonText");
            colJsonText.IsJsonText.Should().BeTrue();
            colJsonText.IsJsonBlob.Should().BeFalse();
            colJsonText.IsJsonPretty.Should().BeFalse();

            TableColumn colJsonBlob = jsonDb.Table<TestJsonNode>().Table.Columns[2];
            colJsonBlob.Name.Should().Be("JsonBlob");
            colJsonBlob.IsJsonText.Should().BeFalse();
            colJsonBlob.IsJsonBlob.Should().BeTrue();
            colJsonBlob.IsJsonPretty.Should().BeFalse();

            // Before Update
            this.TestOutputHelper.WriteLine("Original");
            TestJsonNode? resultRow = jsonDb.Table<TestJsonNode>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonBlob.Should().Be("""{"id":"2"}""");

            jsonNode.JsonBlob = @"
                {
                ""id"": ""3""
                }";

            jsonDb.Update(jsonNode);

            // Before Update
            this.TestOutputHelper.WriteLine("\nAfter Update");
            resultRow = jsonDb.Table<TestJsonNode>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonBlob.Should().Be("""{"id":"3"}""");
        }

        [Fact]
        public void SQLiteJsonUnit_Insert_JsonTextPretty()
        {
            using var jsonDb = new TestJsonDb(this.TestFileSystem);

            jsonDb.Insert(new TestJsonNodePretty()
            {
                JsonTextPretty = @"
                {
                ""id"": ""2""
                }",
            });

            jsonDb.Table<TestJsonNodePretty>().Count().Should().Be(1);
            TableColumn colJsonText = jsonDb.Table<TestJsonNodePretty>().Table.Columns[1];
            colJsonText.Name.Should().Be("JsonTextPretty");
            colJsonText.IsJsonText.Should().BeTrue();
            colJsonText.IsJsonBlob.Should().BeFalse();
            colJsonText.IsJsonPretty.Should().BeTrue();

            TableColumn colJsonBlob = jsonDb.Table<TestJsonNodePretty>().Table.Columns[2];
            colJsonBlob.Name.Should().Be("JsonBlobPretty");
            colJsonBlob.IsJsonText.Should().BeFalse();
            colJsonBlob.IsJsonBlob.Should().BeTrue();

            // Before Update
            this.TestOutputHelper.WriteLine("Original");
            TestJsonNodePretty? resultRow = jsonDb.Table<TestJsonNodePretty>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonTextPretty.Should().Be("{\n    \"id\": \"2\"\n}");
        }

        [Fact]
        public void SQLiteJsonUnit_Update_JsonTextPretty()
        {
            using var jsonDb = new TestJsonDb(this.TestFileSystem);
            var jsonNode = new TestJsonNodePretty()
            {
                JsonTextPretty = @"
                {
                ""id"": ""2""
                }",
            };

            jsonDb.Insert(jsonNode);

            jsonDb.Table<TestJsonNodePretty>().Count().Should().Be(1);
            TableColumn colJsonText = jsonDb.Table<TestJsonNodePretty>().Table.Columns[1];
            colJsonText.Name.Should().Be("JsonTextPretty");
            colJsonText.IsJsonText.Should().BeTrue();
            colJsonText.IsJsonBlob.Should().BeFalse();
            colJsonText.IsJsonPretty.Should().BeTrue();

            TableColumn colJsonBlob = jsonDb.Table<TestJsonNodePretty>().Table.Columns[2];
            colJsonBlob.Name.Should().Be("JsonBlobPretty");
            colJsonBlob.IsJsonText.Should().BeFalse();
            colJsonBlob.IsJsonBlob.Should().BeTrue();
            colJsonBlob.IsJsonPretty.Should().BeTrue();

            // Before Update
            this.TestOutputHelper.WriteLine("Original");
            TestJsonNodePretty? resultRow = jsonDb.Table<TestJsonNodePretty>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonTextPretty.Should().Be("{\n    \"id\": \"2\"\n}");

            jsonNode.JsonTextPretty = @"
                {
                ""id"": ""3""
                }";

            jsonDb.Update(jsonNode);

            // Before Update
            this.TestOutputHelper.WriteLine("\nAfter Update");
            resultRow = jsonDb.Table<TestJsonNodePretty>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonTextPretty.Should().Be("{\n    \"id\": \"3\"\n}");
        }

        [Fact]
        public void SQLiteJsonUnit_Insert_JsonBlobPretty()
        {
            using var jsonDb = new TestJsonDb(this.TestFileSystem);

            jsonDb.Insert(new TestJsonNodePretty()
            {
                JsonBlobPretty = @"
                {
                ""id"": ""2""
                }",
            });

            jsonDb.Table<TestJsonNodePretty>().Count().Should().Be(1);
            TableColumn colJsonText = jsonDb.Table<TestJsonNodePretty>().Table.Columns[1];
            colJsonText.Name.Should().Be("JsonTextPretty");
            colJsonText.IsJsonText.Should().BeTrue();
            colJsonText.IsJsonBlob.Should().BeFalse();
            colJsonText.IsJsonPretty.Should().BeTrue();

            TableColumn colJsonBlob = jsonDb.Table<TestJsonNodePretty>().Table.Columns[2];
            colJsonBlob.Name.Should().Be("JsonBlobPretty");
            colJsonBlob.IsJsonText.Should().BeFalse();
            colJsonBlob.IsJsonBlob.Should().BeTrue();
            colJsonBlob.IsJsonPretty.Should().BeTrue();

            // Before Update
            this.TestOutputHelper.WriteLine("Original");
            TestJsonNodePretty? resultRow = jsonDb.Table<TestJsonNodePretty>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonBlobPretty.Should().Be("{\n    \"id\": \"2\"\n}");
        }

        [Fact]
        public void SQLiteJsonUnit_Update_JsonBlobPretty()
        {
            using var jsonDb = new TestJsonDb(this.TestFileSystem);
            var jsonNode = new TestJsonNodePretty()
            {
                JsonBlobPretty = @"
                {
                ""id"": ""2""
                }",
            };

            jsonDb.Insert(jsonNode);

            jsonDb.Table<TestJsonNodePretty>().Count().Should().Be(1);
            TableColumn colJsonText = jsonDb.Table<TestJsonNodePretty>().Table.Columns[1];
            colJsonText.Name.Should().Be("JsonTextPretty");
            colJsonText.IsJsonText.Should().BeTrue();
            colJsonText.IsJsonBlob.Should().BeFalse();
            colJsonText.IsJsonPretty.Should().BeTrue();

            TableColumn colJsonBlob = jsonDb.Table<TestJsonNodePretty>().Table.Columns[2];
            colJsonBlob.Name.Should().Be("JsonBlobPretty");
            colJsonBlob.IsJsonText.Should().BeFalse();
            colJsonBlob.IsJsonBlob.Should().BeTrue();
            colJsonBlob.IsJsonPretty.Should().BeTrue();

            // Before Update
            this.TestOutputHelper.WriteLine("Original");
            TestJsonNodePretty? resultRow = jsonDb.Table<TestJsonNodePretty>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonBlobPretty.Should().Be("{\n    \"id\": \"2\"\n}");

            jsonNode.JsonBlobPretty = @"
                {
                ""id"": ""3""
                }";

            jsonDb.Update(jsonNode);

            // Before Update
            this.TestOutputHelper.WriteLine("\nAfter Update");
            resultRow = jsonDb.Table<TestJsonNodePretty>().FirstOrDefault();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);
            resultRow!.JsonBlobPretty.Should().Be("{\n    \"id\": \"3\"\n}");
        }

        [Fact]
        public async Task SQLiteJsonUnit_NetworkDeviceAsync()
        {
            using var networkDeviceDb = new TestNetworkDeviceDb(this.TestCaseServiceProvider, this.TestFileSystem);

            networkDeviceDb.Should().NotBeNull();
            var networkDevice = new CoreTestNetworkDevice<JsonUnitTests>
            {
                // Update default values before inserting into database.
                Manufacturer = "TestManufacturer",
                Model = "TestModel",
                DeviceName = "TestDeviceName",
                DeviceIdiom = CoreDeviceIdiom.Desktop,
                DeviceHostType = CoreDeviceHostType.Physical,
            };

            (await networkDeviceDb.InsertOrReplaceNetworkDeviceAsync(networkDevice)).Should().Be(1);

            (await networkDeviceDb.Table<NetworkDeviceTable>().CountAsync()).Should().Be(1);

            // Before Update
            this.TestOutputHelper.WriteLine("Original".CenterTitle());
            NetworkDeviceTable? resultRow = await networkDeviceDb.Table<NetworkDeviceTable>().FirstOrDefaultAsync();
            resultRow.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(resultRow);

            resultRow!.DeviceID.Should().Be(networkDevice.ObjectId);

            ICoreNetworkDevice? networkDeviceFromDb = await networkDeviceDb.GetNetworkDeviceAsync<CoreTestNetworkDevice<JsonUnitTests>>(resultRow!.DeviceID);
            networkDeviceFromDb.Should().NotBeNull();

            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine("From Database:".CenterTitle());
            this.TestOutputHelper.WriteLine(networkDeviceFromDb.ToStringWithParentsPropNameMultiLine());

            networkDeviceFromDb.SynchronizeObjectVersionInfo(networkDevice);

            networkDeviceFromDb!.Manufacturer.Should().Be("TestManufacturer");
            networkDeviceFromDb.Model.Should().Be("TestModel");
            networkDeviceFromDb.DeviceName.Should().Be("TestDeviceName");
            networkDeviceFromDb.DeviceIdiom.Should().Be(CoreDeviceIdiom.Desktop);
            networkDeviceFromDb.DeviceHostType.Should().Be(CoreDeviceHostType.Physical);

            networkDevice.Should().Be(networkDeviceFromDb);

            (await networkDeviceDb.GetNetworkDeviceAsync<CoreTestNetworkDevice<JsonUnitTests>>(Guid.Empty)).Should().BeNull();

            NetworkDeviceTable? networkDeviceTable = (await networkDeviceDb.Table<NetworkDeviceTable>().ToListAsync()).FirstOrDefault();
            networkDeviceTable.Should().NotBeNull();
            networkDeviceTable.DeviceID.Should().Be(networkDeviceFromDb.ObjectId);
            networkDeviceTable.CreatedTimestamp.Should().Be(networkDeviceFromDb.CreatedTimestamp);

            // networkDeviceTable.ModifiedTimestamp.Should().Be(networkDeviceFromDb.ModifiedTimestamp);
            networkDeviceTable.DeviceType.Should().Be(networkDeviceFromDb.DeviceType);
            networkDeviceTable.IPAddress.Should().Be(networkDeviceFromDb.IPAddress);
            networkDeviceTable.PhysicalAddress.Should().Be(networkDeviceFromDb.PhysicalAddress);

            (await networkDeviceDb.DeleteAsync<NetworkDeviceTable>(resultRow!.DeviceID)).Should().Be(1);

            (await networkDeviceDb.GetNetworkDeviceAsync<CoreTestNetworkDevice<JsonUnitTests>>(resultRow.DeviceID)).Should().BeNull();
            (await networkDeviceDb.Table<NetworkDeviceTable>().CountAsync()).Should().Be(0);
        }

        public class TestJsonNode
        {
            [PrimaryKey]
            [AutoIncrement]
            public Guid Id { get; set; }

            [JsonText]
            public string? JsonText { get; set; }

            [JsonBlob]
            public string? JsonBlob { get; set; }

            public override string ToString()
            {
                return $"[TestJsonNode: Id={this.Id}, JsonText={this.JsonText}, JsonBlob={this.JsonBlob}]";
            }
        }

        public class TestJsonNodePretty
        {
            [PrimaryKey]
            [AutoIncrement]
            public Guid Id { get; set; }

            [JsonText]
            [JsonPretty]
            public string? JsonTextPretty { get; set; }

            [JsonBlob]
            [JsonPretty]
            public string? JsonBlobPretty { get; set; }

            public override string ToString()
            {
                return $"[TestJsonNodePretty: Id={this.Id}, JsonTextPretty={this.JsonTextPretty}, JsonBlobPretty={this.JsonBlobPretty}]";
            }
        }

        public class TestJsonDb : TestDbBase<JsonUnitTests>
        {
            public TestJsonDb(ICoreFileSystem fileSystem)
                : base(fileSystem)
            {
                this.CreateTable<TestJsonNode>();
                this.CreateTable<TestJsonNodePretty>();
            }
        }

        public class NetworkDeviceTable
        {
            [Computed(ComputedColumnExpressionDeviceId, HiddenColumnType.VirtualComputedColumn), Core.Database.Providers.SQLite.Attributes.NotNull, PrimaryKey]
            public Guid DeviceID { get; set; }

            [JsonBlob]
            public string? NetworkDevice { get; set; }

            [Computed(ComputedColumnExpressionCreatedTimestamp, HiddenColumnType.VirtualComputedColumn)]
            public DateTimeOffset? CreatedTimestamp { get; set; }

            [Computed(ComputedColumnExpressionModifiedTimestamp, HiddenColumnType.VirtualComputedColumn)]
            public DateTimeOffset? ModifiedTimestamp { get; set; }

            [Computed(ComputedColumnExpressionIPAddress, HiddenColumnType.VirtualComputedColumn)]
            public IPAddress? IPAddress { get; set; }

            [Computed(ComputedColumnExpressionPhysicalAddress, HiddenColumnType.VirtualComputedColumn)]
            public PhysicalAddress? PhysicalAddress { get; set; }

            [Computed(ComputedColumnExpressionDeviceType, HiddenColumnType.VirtualComputedColumn)]
            public CoreDeviceType? DeviceType { get; set; }

            public override string ToString()
            {
                return $"DeviceID={this.DeviceID}\nIPAddress={this.IPAddress}\nPhysicalAddress={this.PhysicalAddress.ToColonString()}\nDeviceType={this.DeviceType}\nNetworkDevice=\n{this.NetworkDevice}";
            }
        }

        public class TestNetworkDeviceDb : TestDbAsync<JsonUnitTests>
        {
            private readonly IServiceProvider _serviceProvider;

            public TestNetworkDeviceDb(IServiceProvider serviceProvider, ICoreFileSystem fileSystem)
                : base(fileSystem)
            {
                this._serviceProvider = serviceProvider;
                this.CreateTableAsync<NetworkDeviceTable>().ConfigureAwait(false).GetAwaiter().GetResult();
                this.CreateIndexAsync<NetworkDeviceTable>(x => x.DeviceID).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            public Task<int> InsertOrReplaceNetworkDeviceAsync(ICoreNetworkDevice networkDevice, JsonSerializerOptions? options = null)
            {
                options ??= CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this._serviceProvider);
                string json = JsonSerializer.Serialize(networkDevice, typeof(CoreTestNetworkDevice<JsonUnitTests>), options);

                return this.InsertOrReplaceAsync(new NetworkDeviceTable { DeviceID = networkDevice.ObjectId, NetworkDevice = json });
            }

            public async Task<ICoreNetworkDevice?> GetNetworkDeviceAsync<T>(Guid deviceID)
                where T : class, ICoreNetworkDevice, new()
            {
                NetworkDeviceTable? networkDeviceTable = await this.GetAsync<NetworkDeviceTable>(deviceID);
                JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this._serviceProvider);

                return networkDeviceTable is null
                    ? null
                    : (ICoreNetworkDevice?)CoreSerializableObject.CreateFromJson<T>(networkDeviceTable.NetworkDevice, options).SerializedObject;
            }

            public Task<int> DeleteAsync(ICoreNetworkDevice networkDevice)
            {
                return this.DeleteAsync<NetworkDeviceTable>(networkDevice.ObjectId);
            }
        }
    }
}
