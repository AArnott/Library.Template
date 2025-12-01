// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// // ***********************************************************************
// <copyright file="CoreNetworkDiscoveredDeviceIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Networking.Discovery;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Discovery
{
    /// <summary>
    /// Class CoreNetworkDiscoveredDeviceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkDiscoveredDeviceIntegrationTests))]

    public class CoreNetworkDiscoveredDeviceIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkDiscoveredDeviceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkDiscoveredDeviceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreNetworkDiscoveredDevice_PreferredNetwork_NotNull()
        {
            this.TestNetworkServices.PreferredNetwork.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetwork>();
            this.TestOutputHelper.WriteLine($"{this.TestNetworkServices.PreferredNetwork.ToStringWithParentsPropNameMultiLine()}");
        }

        [Fact]
        public void CoreNetworkDiscoveredDevice_Defaults()
        {
            ICorePreferredNetworkAddress? preferredLocalNetworkAddress = this.TestNetworkingSystem.PreferredLocalNetworkAddress;
            preferredLocalNetworkAddress.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetworkAddress>();

            var testDiscoveredDevice = new TestCoreDiscoveredDevice(this.TestNetworkServices, preferredLocalNetworkAddress);
            testDiscoveredDevice.As<object?>().Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"{testDiscoveredDevice.ToStringWithParentsPropNameMultiLine()}");
            testDiscoveredDevice.IPAddress.Should().Be(preferredLocalNetworkAddress?.IPAddress);
            testDiscoveredDevice.PhysicalAddress.Should().Be(preferredLocalNetworkAddress?.PhysicalAddress);
            testDiscoveredDevice.DeviceType.Should().Be(CoreDeviceType.NetworkDevice);
            testDiscoveredDevice.IsLocalDevice.Should().BeTrue();
            testDiscoveredDevice.IsTestDevice.Should().BeFalse();
            testDiscoveredDevice.IsLoopBackDevice.Should().BeFalse();
            testDiscoveredDevice.ObjectVersion.Should().Be(1);
            testDiscoveredDevice.ObjectCacheVersion.Should().Be(1);
        }

        protected class TestCoreDiscoveredDevice : CoreNetworkDiscoveredDeviceBase
        {
            private readonly ICorePreferredNetworkAddress? preferredNetworkAddress;

            /// <summary>
            /// Initializes a new instance of the <see cref="TestCoreDiscoveredDevice"/> class.
            /// </summary>
            /// <param name="networkServices">Network services interface.</param>
            /// <param name="preferredNetworkAddress">Preferred network address.</param>
            /// <param name="deviceType">Optional device type.</param>
            public TestCoreDiscoveredDevice(ICoreNetworkServices networkServices, ICorePreferredNetworkAddress? preferredNetworkAddress, CoreDeviceType deviceType = CoreDeviceType.NetworkDevice)
                : base(networkServices, Guid.NewGuid(), preferredNetworkAddress?.IPAddress, preferredNetworkAddress?.SubnetMask, preferredNetworkAddress?.PhysicalAddress, networkServices?.Logger, deviceType)
            {
                this.preferredNetworkAddress = preferredNetworkAddress;
            }

            /// <inheritdoc />
            protected override ICorePreferredNetworkAddress? CalculatePreferredNetworkAddress()
            {
                return this.preferredNetworkAddress;
            }

            /// <inheritdoc />
            protected override IPAddress? GetPlatformIPAddress()
            {
                return this.preferredNetworkAddress?.IPAddress;
            }

            /// <inheritdoc />
            protected override IPAddress? GetPlatformSubnetMask()
            {
                return this.preferredNetworkAddress?.SubnetMask;
            }

            /// <inheritdoc />
            protected override PhysicalAddress? GetPlatformPhysicalAddress()
            {
                return this.preferredNetworkAddress?.PhysicalAddress;
            }

            /// <inheritdoc />
            protected override string? GetPlatformModel()
            {
                return null;
            }

            /// <inheritdoc />
            protected override string? GetPlatformManufacturer()
            {
                return null;
            }

            /// <inheritdoc />
            protected override string? GetPlatformDeviceName()
            {
                return null;
            }

            /// <inheritdoc />
            protected override CoreDeviceIdiom? GetPlatformDeviceIdiom()
            {
                return null;
            }

            /// <inheritdoc />
            protected override CoreDeviceHostType? GetPlatformDeviceHostType()
            {
                return null;
            }
        }
    }
}
