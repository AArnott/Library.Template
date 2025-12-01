// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreNetworkDeviceIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Networking.DeviceInfo;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Firewall;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Services.Ping;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Devices
{
    /// <summary>
    /// Class CoreNetworkDeviceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkDeviceIntegrationTests))]

    public class CoreNetworkDeviceIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkDeviceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkDeviceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public ICoreLocalNetworkDevice? LocalNetworkDevice => this.TestNetworkServices.LocalNetworkDevice;

        /// <summary>
        /// Defines the test method NetworkDevice_Ctor.
        /// </summary>
        [Fact]
        public void NetworkDevice_Ctor()
        {
            this.LocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this.LocalNetworkDevice!.LocalNetworkAgent.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkAgent>();
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.UnknownDevice);

            deviceTest.NetworkServices.NetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            deviceTest.NetworkServices.NetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            deviceTest.NetworkServices.NetworkingSystem.Logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();
            deviceTest.NetworkServices.NetworkFirewall.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkFirewall>();
            deviceTest.NetworkServices.NetworkFirewall.Should().BeSameAs(this.TestNetworkFirewall);
            deviceTest.NetworkServices.NetworkPing.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkPing>();

            deviceTest.NetworkServices.FileSystem.Should().BeSameAs(this.TestFileSystem);
            deviceTest.NetworkServices.OperatingSystem.Should().BeSameAs(this.TestOperatingSystem);
            deviceTest.NetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            deviceTest.NetworkServices.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);

            deviceTest.NetworkServices.IsServiceSupported(CoreNetworkServiceTypes.None).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method NetworkDevice_CompareTo_DeviceBase.
        /// </summary>
        [Fact]
        public void NetworkDevice_CompareTo_DeviceBase()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.UnknownDevice);
            ICoreNetworkDevice deviceTest1 = this.CreateTestNetworkDevice(CoreDeviceType.UnknownDevice);

            deviceTest.Should().NotBeNull();
            deviceTest1.Should().NotBeNull();

            deviceTest.CompareTo(deviceTest).Should().Be(0);
            deviceTest.CompareTo(deviceTest1).Should().Be(0);
        }

        /// <summary>
        /// Defines the test method NetworkDevice_CompareTo_ICoreObject.
        /// </summary>
        [Fact]
        public void NetworkDevice_CompareTo_ICoreObject()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.UnknownDevice);
            ICoreNetworkDevice deviceTest1 = this.CreateTestNetworkDevice(CoreDeviceType.UnknownDevice);

            deviceTest.CompareTo((ICoreObject)deviceTest).Should().Be(0);
            deviceTest.CompareTo((ICoreObject)deviceTest1).Should().Be(0);

            var coreObject = new ObjectTest(this.TestCaseLogger);

            Func<int> fx = () => deviceTest.CompareTo(coreObject);
            fx.Should().Throw<ArgumentException>().And.Message.Should().Be("Object must be of type ICoreDevice.");
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.

        /// <summary>
        /// Defines the test method NetworkDevice_CompareTo_Null.
        /// </summary>
        [Fact]
        public void NetworkDevice_CompareTo_Null()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.UnknownDevice);

            deviceTest.CompareTo(null!).Should().Be(1);
#pragma warning disable IDE0079 // Remove unnecessary suppression
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            deviceTest.CompareTo((ICoreObject)null).Should().Be(1);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        /// <summary>
        /// Defines the test method NetworkDevice_Equals_Null.
        /// </summary>
        [Fact]
        public void NetworkDevice_Equals_Null()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.UnknownDevice);

            deviceTest.Should().NotBeNull();

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            deviceTest.Equals((ICoreNetworkDevice?)null).Should().BeFalse();
            deviceTest!.Equals((ICoreObject)null).Should().BeFalse();
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore IDE0079 // Remove unnecessary suppression

        /// <summary>
        /// Defines the test method NetworkDevice_Equals_ICoreObject.
        /// </summary>
        [Fact]
        public void NetworkDevice_Equals_ICoreObject()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.UnknownDevice);
            ICoreNetworkDevice deviceTest1 = this.CreateTestNetworkDevice(CoreDeviceType.UnknownDevice);

            deviceTest.Should().NotBeNull();
            deviceTest1.Should().NotBeNull();

            deviceTest.Equals((ICoreObject)deviceTest).Should().BeTrue();
            deviceTest.Equals((ICoreObject)deviceTest1).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method NetworkDevice_ToLog.
        /// </summary>
        [Fact]
        public void NetworkDevice_ToLog()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.NetworkDevice);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToLog, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method NetworkDevice_ToLogParents.
        /// </summary>
        [Fact]
        public void NetworkDevice_ToLogParents()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.NetworkDevice);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToLogWithParents, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method NetworkDevice_ToString.
        /// </summary>
        [Fact]
        public void NetworkDevice_ToString()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.NetworkDevice);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToString, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method NetworkDevice_ToStringParents.
        /// </summary>
        [Fact]
        public void NetworkDevice_ToStringParents()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.NetworkDevice);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithParents, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method NetworkDevice_ToStringMultiLine.
        /// </summary>
        [Fact]
        public void NetworkDevice_ToStringMultiLine()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.NetworkDevice);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithMultiLine, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method NetworkDevice_ToStringParentsMultiLine.
        /// </summary>
        [Fact]
        public void NetworkDevice_ToStringParentsMultiLine()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.NetworkDevice);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithParentsMultiLine, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method NetworkDevice_ToStringWithPropNameMultiLine.
        /// </summary>
        [Fact]
        public void NetworkDevice_ToStringWithPropNameMultiLine()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.NetworkDevice);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method NetworkDevice_ToStringWithParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void NetworkDevice_ToStringWithParentsPropNameMultiLine()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.NetworkDevice);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine, LogLevel.Debug));
        }

        /// <summary>
        /// Defines the test method NetworkDevice_ToStringWithParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void NetworkDevice_ToStringWithParentsPropNameMultiLine_LogTrace()
        {
            ICoreNetworkDevice deviceTest = this.CreateTestNetworkDevice(CoreDeviceType.NetworkDevice);

            this.TestOutputHelper.WriteLine(deviceTest.ToString(CoreLoggableFormatFlags.ToStringWithParentsPropNameMultiLine, LogLevel.Trace));
        }

        /// <summary>
        /// Creates the test network device.
        /// </summary>
        /// <param name="deviceType">Type of the device.</param>
        /// <returns>ICoreNetworkDevice.</returns>
        private ICoreNetworkDevice CreateTestNetworkDevice(CoreDeviceType deviceType)
        {
            this.TestNetworkingSystem.PreferredLocalNetworkAddress.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetworkAddress>();

            return new CoreNetworkDeviceTest(this.TestNetworkServices, this.TestNetworkingSystem.PreferredLocalNetworkAddress, deviceType);
        }

        /// <summary>
        /// Class NetworkDeviceTest.
        /// </summary>
        public class CoreNetworkDeviceTest : CoreNetworkDeviceBase<CoreNetworkDeviceTest>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="CoreNetworkDeviceTest"/> class.
            /// </summary>
            /// <param name="networkServices">Network Services interface.</param>
            /// <param name="preferredNetworkAddress">Preferred network. </param>
            /// <param name="deviceType">Type of the device.</param>
            public CoreNetworkDeviceTest(ICoreNetworkServices networkServices, ICorePreferredNetworkAddress? preferredNetworkAddress, CoreDeviceType deviceType = CoreDeviceType.NetworkDevice)
                : base(networkServices, Guid.NewGuid(), preferredNetworkAddress?.IPAddress, preferredNetworkAddress?.SubnetMask, preferredNetworkAddress?.PhysicalAddress, deviceType)
            {
                this.TestPreferredNetworkAddress = preferredNetworkAddress;
            }

            /// <summary>
            /// Gets the test PreferredNetworkAddress.
            /// </summary>
            private ICorePreferredNetworkAddress? TestPreferredNetworkAddress { get; }

            /// <inheritdoc />
            protected override ICorePreferredNetworkAddress? CalculatePreferredNetworkAddress()
            {
                return this.TestPreferredNetworkAddress;
            }

            /// <inheritdoc />
            protected override IPAddress? GetPlatformIPAddress()
            {
                return this.TestPreferredNetworkAddress?.IPAddress;
            }

            /// <inheritdoc />
            protected override IPAddress? GetPlatformSubnetMask()
            {
                return this.TestPreferredNetworkAddress?.SubnetMask;
            }

            /// <inheritdoc />
            protected override PhysicalAddress? GetPlatformPhysicalAddress()
            {
                return this.TestPreferredNetworkAddress?.PhysicalAddress;
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

            /// <inheritdoc />
            protected override bool GetPlatformIsTestDevice() => true;
        }

        /// <summary>
        /// Class ObjectTest.
        /// </summary>
        private class ObjectTest : CoreObjectBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ObjectTest"/> class.
            /// </summary>
            /// <param name="logger">The logger.</param>
            public ObjectTest(ICoreLogger logger)
            : base(logger)
            {
            }
        }
    }
}
