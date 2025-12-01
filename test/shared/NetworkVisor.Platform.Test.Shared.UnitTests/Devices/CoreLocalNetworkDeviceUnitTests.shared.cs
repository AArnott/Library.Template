// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// // ***********************************************************************
// <copyright file="CoreLocalNetworkDeviceUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Networking.DeviceInfo;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.Devices;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Devices
{
    /// <summary>
    /// Class CoreLocalNetworkDeviceUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLocalNetworkDeviceUnitTests))]

    public class CoreLocalNetworkDeviceUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// The device information.
        /// </summary>
        private readonly ICoreLocalNetworkDevice testLocalNetworkDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLocalNetworkDeviceUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLocalNetworkDeviceUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.testLocalNetworkDevice = new TestLocalNetworkDeviceLocal(this.TestNetworkServices);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.testLocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this.testLocalNetworkDevice.Should().BeAssignableTo<ICoreDeviceInfo>();
            this.testLocalNetworkDevice.OperatingSystem.Should().NotBeNull().And.Subject.Should()
                .BeSameAs(this.TestOperatingSystem);
        }

        /// <summary>
        /// Defines the test method DeviceInfo_DeviceIdiom.
        /// </summary>
        [Fact]
        public void DeviceInfo_DeviceIdiom()
        {
            this.testLocalNetworkDevice.DeviceIdiom.Should().Be(CoreDeviceIdiom.Unknown);
            this.testLocalNetworkDevice.DeviceName.Should().BeEmpty();
            this.testLocalNetworkDevice.DeviceHostType.Should().Be(CoreDeviceHostType.Unknown);
            this.testLocalNetworkDevice.Manufacturer.Should().BeEmpty();
            this.testLocalNetworkDevice.Model.Should().BeEmpty();
            this.testLocalNetworkDevice.OSVersionString.Should().BeEmpty();
            this.testLocalNetworkDevice.OSVersion.Should().BeNull();
            this.testLocalNetworkDevice.DeviceVersionString.Should().BeEmpty();
            this.testLocalNetworkDevice.DeviceVersion.Should().BeNull();
        }

        [Fact]
        public void DeviceInfo_DeviceId_SameAs_ObjectId()
        {
            this.testLocalNetworkDevice.DeviceID.Should().Be(this.testLocalNetworkDevice.ObjectId);
        }

        /// <summary>
        /// Defines the test method DeviceInfo_GetLogPropertyListLevel_Null.
        /// </summary>
        [Fact]
        public void DeviceInfo_GetLogPropertyListLevel_Null()
        {
            Func<IEnumerable<ICoreLogPropertyLevel>> fx = () => this.testLocalNetworkDevice.GetLogPropertyListLevel(null!);

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("logPropertyListFormatter");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToString.
        /// </summary>
        [Fact]
        public void DeviceInfo_ToString()
        {
            this.TestOutputHelper.WriteLine(this.testLocalNetworkDevice.ToString());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithPropName.
        /// </summary>
        [Fact]
        public void DeviceInfo_ToStringWithPropName()
        {
            this.TestOutputHelper.WriteLine(this.testLocalNetworkDevice.ToStringWithPropName());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithParentsPropName.
        /// </summary>
        [Fact]
        public void DeviceInfo_ToStringWithParentsPropName()
        {
            this.TestOutputHelper.WriteLine(this.testLocalNetworkDevice.ToStringWithParentsPropName());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithPropNameMultiLine.
        /// </summary>
        [Fact]
        public void DeviceInfo_ToStringWithPropNameMultiLine()
        {
            this.TestOutputHelper.WriteLine(this.testLocalNetworkDevice.ToStringWithPropNameMultiLine());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void DeviceInfo_ToStringWithParentsPropNameMultiLine()
        {
            this.TestOutputHelper.WriteLine(this.testLocalNetworkDevice.ToStringWithParentsPropNameMultiLine());
        }

        /// <summary>
        /// Private class to Test LocalNetworkDevice.
        /// </summary>
        [ExcludeFromCodeCoverage]
        private class TestLocalNetworkDeviceLocal : CoreLocalNetworkDevice
        {
            public TestLocalNetworkDeviceLocal(ICoreNetworkServices networkServices)
                : base(networkServices)
            {
            }

            /// <inheritdoc />
            protected override string GetPlatformModel() => string.Empty;

            /// <inheritdoc />
            protected override string GetPlatformManufacturer() => string.Empty;

            /// <inheritdoc />
            protected override string GetPlatformDeviceName() => string.Empty;

            /// <inheritdoc />
            protected override CoreDeviceIdiom? GetPlatformDeviceIdiom() => null;

            /// <inheritdoc />
            protected override CoreDeviceHostType? GetPlatformDeviceHostType() => null;

            /// <inheritdoc />
            protected override ICorePreferredNetwork GetPlatformPreferredNetwork() => this.NetworkServices.PreferredNetwork;

            /// <inheritdoc />
            protected override string? GetPlatformOSVersionString()
            {
                base.GetPlatformOSVersionString().Should().NotBeNull().And.Subject.Should().Be(this.NetworkingSystem.OperatingSystem.OSVersionString);
                return string.Empty;
            }

            /// <inheritdoc />
            protected override Version? GetPlatformOSVersion()
            {
                base.GetPlatformOSVersion().Should().NotBeNull().And.Subject.Should().Be(this.NetworkingSystem.OperatingSystem.OSVersion);
                return null;
            }

            /// <inheritdoc />
            protected override string? GetPlatformDeviceVersionString()
            {
                base.GetPlatformDeviceVersionString().Should().NotBeNull().And.Subject.Should().Be(this.NetworkingSystem.OperatingSystem.DeviceVersionString);
                return string.Empty;
            }

            /// <inheritdoc />
            protected override Version? GetPlatformDeviceVersion()
            {
                base.GetPlatformDeviceVersion().Should().NotBeNull().And.Subject.Should().Be(this.NetworkingSystem.OperatingSystem.DeviceVersion);
                return null;
            }
        }
    }
}
