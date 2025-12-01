// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreObservableDatabaseCacheUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Cache.Database;
using NetworkVisor.Core.Cache.Observables;
using NetworkVisor.Core.Cache.Tables;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.DeviceInfo;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestDevices;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Cache
{
    /// <summary>
    /// Class CoreObservableDatabaseCacheUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreObservableDatabaseCacheUnitTests))]

    public class CoreObservableDatabaseCacheUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreObservableDatabaseCacheUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreObservableDatabaseCacheUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.CoreNetworkDevices = new CoreObservableDatabaseCache<CoreNetworkDeviceTable>();
        }

        public ICoreObservableDatabaseCache<CoreNetworkDeviceTable> CoreNetworkDevices { get; set; }

        [Fact]
        public void CoreObservableDatabaseCacheUnit_Ctor()
        {
            this.CoreNetworkDevices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreObservableDatabaseCache<CoreNetworkDeviceTable>>();
        }

        [Fact]
        public void CoreObservableDatabaseCacheUnit_AddNetworkDevice()
        {
            this.CoreNetworkDevices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreObservableDatabaseCache<CoreNetworkDeviceTable>>();
            CoreNetworkDeviceTable networkDeviceTable = this.CreateCoreNetworkDeviceTable();
            networkDeviceTable.WrappedNetworkDevice.Should().NotBeNull().And.BeAssignableTo<ICoreTestNetworkDevice>();
            this.OutputNetworkDeviceTable(networkDeviceTable, "Network Device Table");
            this.CoreNetworkDevices.Add(networkDeviceTable);

            ICoreNetworkDevice? wrappedNetworkDevice = networkDeviceTable.ToNetworkDevice<CoreTestNetworkDevice<CoreObservableDatabaseCacheUnitTests>>(this.TestCaseServiceProvider);
            wrappedNetworkDevice.Should().NotBeNull().And.BeAssignableTo<ICoreTestNetworkDevice>();
            wrappedNetworkDevice.SynchronizeObjectVersionInfo(networkDeviceTable.WrappedNetworkDevice);
            wrappedNetworkDevice.Should().Be(networkDeviceTable.WrappedNetworkDevice);
        }

        private CoreNetworkDeviceTable CreateCoreNetworkDeviceTable(CoreDeviceType deviceType = CoreDeviceType.NetworkDevice)
        {
            return CoreNetworkDeviceTable.Create<CoreTestNetworkDevice<CoreObservableDatabaseCacheUnitTests>>(new CoreTestNetworkDevice<CoreObservableDatabaseCacheUnitTests>(this.TestNetworkServices, this.TestNetworkServices.PreferredLocalNetworkAddress, deviceType));
        }

        private void OutputNetworkDeviceTable(CoreNetworkDeviceTable networkDeviceTable, string title)
        {
            this.TestOutputHelper.WriteLine(title.CenterTitle());
            this.TestOutputHelper.WriteLine(networkDeviceTable.NetworkDevice);
        }
    }
}
