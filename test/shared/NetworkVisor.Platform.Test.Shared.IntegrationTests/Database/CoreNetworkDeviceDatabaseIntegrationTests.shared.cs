// Assembly         : NetworkVisor.Platform.Test.Shared.Messaging.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreNetworkDeviceDatabaseIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.Cache.Database;
using NetworkVisor.Core.Cache.Tables;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.DeviceInfo;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.Devices;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Database
{
    /// <summary>
    /// Class CoreNetworkDeviceDatabaseIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkDeviceDatabaseIntegrationTests))]
    public class CoreNetworkDeviceDatabaseIntegrationTests : CoreTestCaseBase
    {
        private bool _disposedValue;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkDeviceDatabaseIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreNetworkDeviceDatabaseIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.TemporaryDatabasePath = CoreSQLiteConnection.GetLocalUserAppDatabaseTempFilePath(this.TestFileSystem, CoreNetworkDeviceDatabase.NetworkDeviceDatabaseName);
            this.NetworkDeviceDatabase = new CoreNetworkDeviceDatabase(this.TestCaseServiceProvider, this.TestFileSystem, this.TemporaryDatabasePath);
        }

        public string TemporaryDatabasePath { get; }

        public ICoreNetworkDeviceDatabase NetworkDeviceDatabase { get; }

        [Fact]
        public void NetworkDeviceDatabaseIntegration_NetworkDeviceDatabase()
        {
            this.NetworkDeviceDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkDeviceDatabase>();
            _ = this.NetworkDeviceDatabase.NetworkDeviceTable.Should().NotBeNull().And.Subject.Should().BeAssignableTo<AsyncTableQuery<CoreNetworkDeviceTable>>();
            this.TestFileSystem.FileExists(this.TemporaryDatabasePath).Should().BeTrue();
        }

        [Fact]
        public async Task NetworkDeviceDatabaseIntegration_InsertLocalNetworkDeviceTask()
        {
            (await this.NetworkDeviceDatabase.InsertOrReplaceNetworkDeviceAsync<CoreLocalNetworkDevice>(this.TestNetworkServices.LocalNetworkDevice)).Should().Be(1);
            this.NetworkDeviceDatabase.GetNetworkDeviceAsync<CoreCachedNetworkDevice>(this.TestNetworkServices.LocalNetworkDevice.ObjectId).Should().NotBeNull();
            ICoreCachedNetworkDevice? cachedNetworkDevice = (ICoreCachedNetworkDevice?)await this.NetworkDeviceDatabase.GetNetworkDeviceAsync<CoreCachedNetworkDevice>(this.TestNetworkServices.LocalNetworkDevice.ObjectId);
            cachedNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCachedNetworkDevice>();

            List<CoreNetworkDeviceTable> items = await this.NetworkDeviceDatabase.NetworkDeviceTablesAsync();
            items.Should().HaveCount(1);
            CoreNetworkDeviceTable? networkDeviceTable = items.FirstOrDefault();
            networkDeviceTable.Should().NotBeNull();
            networkDeviceTable.CreatedTimestamp.Should().Be(this.TestNetworkServices.LocalNetworkDevice.CreatedTimestamp);
            networkDeviceTable.ModifiedTimestamp.Should().Be(this.TestNetworkServices.LocalNetworkDevice.ModifiedTimestamp);

            networkDeviceTable.DeviceType.Should().Be(this.TestNetworkServices.LocalNetworkDevice.DeviceType);
            networkDeviceTable.IPAddress.Should().Be(this.TestNetworkServices.LocalNetworkDevice.IPAddress);
            networkDeviceTable.SubnetMask.Should().Be(this.TestNetworkServices.LocalNetworkDevice.SubnetMask);
            networkDeviceTable.PhysicalAddress.Should().Be(this.TestNetworkServices.LocalNetworkDevice.PhysicalAddress);

            this.TestOutputHelper.WriteLine($"{"CoreNetworkDeviceTable".CenterTitle()}\n{items.FirstOrDefault()?.ToString()}\n");

            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider);

            string jsonString = JsonSerializer.Serialize(cachedNetworkDevice, typeof(ICoreNetworkDeviceInfo), options);
            this.TestOutputHelper.WriteLine($"{"Serialized".CenterTitle()}\n{jsonString}");
        }

        [Fact]
        public async Task NetworkDeviceDatabaseIntegration_ReplaceLocalNetworkDeviceTask()
        {
            (await this.NetworkDeviceDatabase.InsertOrReplaceNetworkDeviceAsync<CoreLocalNetworkDevice>(this.TestNetworkServices.LocalNetworkDevice)).Should().Be(1);
            this.NetworkDeviceDatabase.GetNetworkDeviceAsync<CoreCachedNetworkDevice>(this.TestNetworkServices.LocalNetworkDevice.ObjectId).Should().NotBeNull();
            ICoreCachedNetworkDevice? cachedNetworkDevice = (ICoreCachedNetworkDevice?)await this.NetworkDeviceDatabase.GetNetworkDeviceAsync<CoreCachedNetworkDevice>(this.TestNetworkServices.LocalNetworkDevice.ObjectId);

            (await this.NetworkDeviceDatabase.InsertOrReplaceNetworkDeviceAsync<CoreLocalNetworkDevice>(this.TestNetworkServices.LocalNetworkDevice)).Should().Be(1);

            List<CoreNetworkDeviceTable> items = await this.NetworkDeviceDatabase.NetworkDeviceTablesAsync();
            items.Should().HaveCount(1);

            this.TestOutputHelper.WriteLine($"{"CoreNetworkDeviceTable".CenterTitle()}\n{items.FirstOrDefault()?.ToString()}\n");

            cachedNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCachedNetworkDevice>();
            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider);

            string jsonString = JsonSerializer.Serialize(cachedNetworkDevice, typeof(ICoreNetworkDeviceInfo), options);
            this.TestOutputHelper.WriteLine($"{"Serialized".CenterTitle()}\n{jsonString}");
        }

        protected override void Dispose(bool disposing)
        {
            if (this._disposedValue)
            {
                return;
            }

            try
            {
                this.NetworkDeviceDatabase?.Dispose();
                this.TestFileSystem.WaitToDeleteLockedFile(this.TemporaryDatabasePath);
            }
            finally
            {
                this._disposedValue = true;
                base.Dispose(disposing);
            }
        }
    }
}
