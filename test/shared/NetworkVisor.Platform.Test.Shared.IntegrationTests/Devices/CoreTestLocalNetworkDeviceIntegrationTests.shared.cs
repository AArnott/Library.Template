// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// // ***********************************************************************
// <copyright file="CoreTestLocalNetworkDeviceIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Diagnostics.CodeAnalysis;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Networking.DeviceInfo;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Storage;
using NetworkVisor.Core.Storage.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.Devices;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Devices
{
    /// <summary>
    /// Class CoreTestLocalNetworkDeviceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreTestLocalNetworkDeviceIntegrationTests))]

    public class CoreTestLocalNetworkDeviceIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// The device information.
        /// </summary>
        private readonly ICoreLocalNetworkDevice _testLocalNetworkDevice;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestLocalNetworkDeviceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTestLocalNetworkDeviceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._testLocalNetworkDevice = new CoreLocalNetworkDevice(this.TestNetworkServices);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this._testLocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this._testLocalNetworkDevice.Should().BeAssignableTo<ICoreDeviceInfo>();
            this._testLocalNetworkDevice.Should().BeAssignableTo<ICoreLocalNetworkAgent>();
            this._testLocalNetworkDevice.LocalNetworkAgent.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkAgent>();

            this._testLocalNetworkDevice.OperatingSystem.Should().NotBeNull().And.Subject.Should()
                .BeSameAs(this.TestOperatingSystem);
            this._testLocalNetworkDevice.Count.Should().Be(1);
            this._testLocalNetworkDevice.PreferredNetwork.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetwork>();
            this._testLocalNetworkDevice.Count.Should().BeGreaterThan(3);
            this._testLocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_PreferredNetworkAddress.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_PreferredNetworkAddress()
        {
            this._testLocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this._testLocalNetworkDevice.PreferredNetworkAddress.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetworkAddress>();
            this._testLocalNetworkDevice.PreferredNetworkAddress?.IPAddress.Should().Be(this._testLocalNetworkDevice.IPAddress);
            this._testLocalNetworkDevice.PreferredNetworkAddress?.SubnetMask.Should().Be(this._testLocalNetworkDevice.SubnetMask);
            this._testLocalNetworkDevice.PreferredNetworkAddress?.PhysicalAddress.Should().Be(this._testLocalNetworkDevice.PhysicalAddress);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_ObjectBag_Output()
        {
            this.TestOutputHelper.WriteLine($"Object Count: {this._testLocalNetworkDevice.Count}");

            foreach (ICoreObjectItem item in this._testLocalNetworkDevice.FindAllItems())
            {
                this.TestOutputHelper.WriteLine(item.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_ToStringWithParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_ToStringWithParentsPropNameMultiLine()
        {
            this._testLocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this._testLocalNetworkDevice.Should().BeAssignableTo<ICoreDeviceInfo>();
            this._testLocalNetworkDevice.OperatingSystem.Should().NotBeNull().And.Subject.Should()
                .BeSameAs(this.TestOperatingSystem);
            this.TestOutputHelper.WriteLine(this._testLocalNetworkDevice.ToStringWithParentsPropNameMultiLine());
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_IPAddress.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_IPAddress()
        {
            this.TestOutputHelper.WriteLine($"IPAddress: {this._testLocalNetworkDevice.IPAddress}");
            this._testLocalNetworkDevice.IPAddress.IsNullNoneAnyOrLoopback().Should().BeFalse();
            this._testLocalNetworkDevice.IPAddress!.Equals(this.TestNetworkServices.PreferredNetwork?.PreferredLocalNetworkAddress?.IPAddress).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_DeviceID.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceID()
        {
            this.TestOutputHelper.WriteLine($"DeviceID: {this._testLocalNetworkDevice.DeviceID}");
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_SystemDeviceID.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_SystemDeviceID()
        {
            this.TestOutputHelper.WriteLine($"SystemDeviceID: {this.TestNetworkingSystem.SystemDeviceID}");
        }

        [Fact]
        public async Task TestLocalNetworkDevice_DeviceIDMatchesSystemDeviceIDAsync()
        {
            this.TestNetworkServices.Preferences.Should().NotBeNull();
            var storedDeviceId = await this.TestNetworkServices.Preferences!.GetPropAsync(CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId.ToVersionedStoragePropName());

            this.TestOutputHelper.WriteLine($"Stored DeviceID: {storedDeviceId}");
            this.TestOutputHelper.WriteLine($"DeviceID: {this._testLocalNetworkDevice.DeviceID}");
            this.TestOutputHelper.WriteLine($"SystemDeviceID: {this.TestNetworkingSystem.SystemDeviceID}");
            this._testLocalNetworkDevice.DeviceID.Should().Be(this.TestNetworkingSystem.SystemDeviceID);
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_UserID.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_UserID()
        {
            this.TestOutputHelper.WriteLine($"UserID: {this._testLocalNetworkDevice.UserID}");
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_SystemUserID.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_SystemUserID()
        {
            this.TestOutputHelper.WriteLine($"SystemUserID: {this.TestNetworkingSystem.SystemUserID}");
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_UserIDMatchesSystemUserID.
        /// </summary>
        [Fact]
        public async Task TestLocalNetworkDevice_UserIDMatchesSystemUserIDAsync()
        {
            this.TestNetworkServices.Preferences.Should().NotBeNull();
            var storedUserId = await this.TestNetworkServices.Preferences!.GetPropAsync(CoreStoragePropName.SharedRoamingUserNetworkVisorAgentUserId.ToVersionedStoragePropName());

            this.TestOutputHelper.WriteLine($"Stored UserID: {storedUserId}");
            this.TestOutputHelper.WriteLine($"UserID: {this._testLocalNetworkDevice.UserID}");
            this.TestOutputHelper.WriteLine($"SystemUserID: {this.TestNetworkingSystem.SystemUserID}");
            this._testLocalNetworkDevice.UserID.Should().Be(this.TestNetworkingSystem.SystemUserID);
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_DeviceIDNotMatchUserID.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceIDNotMatchUserID()
        {
            this.TestOutputHelper.WriteLine($"UserID: {this._testLocalNetworkDevice.UserID}");
            this.TestOutputHelper.WriteLine($"DeviceID: {this._testLocalNetworkDevice.DeviceID}");
            this._testLocalNetworkDevice.UserID.Should().NotBe(this._testLocalNetworkDevice.DeviceID);
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_Subnet.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_Subnet()
        {
            this.TestOutputHelper.WriteLine($"Subnet: {this._testLocalNetworkDevice.SubnetMask}");

            // Cellular connections can have an invalid SubnetMask.
            if (!this._testLocalNetworkDevice.PreferredNetworkInterface!.IsCellularConnection)
            {
                this._testLocalNetworkDevice.SubnetMask.IsNullNoneAnyOrLoopback().Should().BeFalse();
            }

            this._testLocalNetworkDevice.SubnetMask!.Equals(this.TestNetworkServices.PreferredNetwork?.PreferredLocalNetworkAddress?.SubnetMask).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_PhysicalAddress.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_PhysicalAddress()
        {
            this.TestOutputHelper.WriteLine($"PhysicalAddress: {this._testLocalNetworkDevice.PhysicalAddress.ToDashString()}");

            PhysicalAddress? physicalAddress = this._testLocalNetworkDevice.PhysicalAddress;

            if (this.TestNetworkServices.IsServiceSupported(CoreNetworkServiceTypes.LocalPhysicalAddress))
            {
                physicalAddress.IsNullOrNone().Should().BeFalse();
            }

            physicalAddress!.Equals(this.TestNetworkServices.PreferredNetwork?.PreferredLocalNetworkAddress?.PhysicalAddress).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_IsLocalDevice.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_IsLocalDevice()
        {
            this.TestOutputHelper.WriteLine($"IsLocalDevice: {this._testLocalNetworkDevice.IsLocalDevice}");
            this._testLocalNetworkDevice.IsLocalDevice.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_IsTestDevice.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_IsTestDevice()
        {
            this.TestOutputHelper.WriteLine($"IsTestDevice: {this._testLocalNetworkDevice.IsTestDevice}");
            this._testLocalNetworkDevice.IsTestDevice.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_IsLoopBackDevice.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_IsLoopBackDevice()
        {
            this.TestOutputHelper.WriteLine($"IsLoopBackDevice: {this._testLocalNetworkDevice.IsLoopBackDevice}");
            this._testLocalNetworkDevice.IsLoopBackDevice.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_DeviceType.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceType_LocalComputer()
        {
            this.TestOutputHelper.WriteLine($"DeviceType: {this._testLocalNetworkDevice.DeviceType}");
            this._testLocalNetworkDevice.DeviceType.Should().Be(CoreDeviceType.LocalComputer);
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_ToStringWithPropNameMultiLine.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_ToStringWithPropNameMultiLine()
        {
            this._testLocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this._testLocalNetworkDevice.Should().BeAssignableTo<ICoreDeviceInfo>();
            this._testLocalNetworkDevice.OperatingSystem.Should().NotBeNull().And.Subject.Should()
                .BeSameAs(this.TestOperatingSystem);
            this.TestOutputHelper.WriteLine(this._testLocalNetworkDevice.ToStringWithPropNameMultiLine());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_DeviceIdiom.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_DeviceIdiom()
        {
            this.TestOutputHelper.WriteLine($"CoreDeviceIdiom: {this._testLocalNetworkDevice.DeviceIdiom}");

#if NV_PLAT_MACOS || NV_PLAT_MACCATALYST
            if (!this.TestOperatingSystem.IsMacOSPlatform)
            {
                this._testLocalNetworkDevice.Model.Should().BeNullOrEmpty();
                return;
            }
#endif
            this._testLocalNetworkDevice.DeviceIdiom.Should().Match(di =>
                di == CoreDeviceIdiom.Phone || di == CoreDeviceIdiom.Desktop || di == CoreDeviceIdiom.TV ||
                di == CoreDeviceIdiom.Tablet || di == CoreDeviceIdiom.Watch || di == CoreDeviceIdiom.Laptop);
        }

        /// <summary>
        /// Defines the test method DeviceInfo_DeviceType.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_DeviceHostType()
        {
            this._testLocalNetworkDevice.DeviceHostType.Should().Match(dt =>
                dt == CoreDeviceHostType.Physical || dt == CoreDeviceHostType.Virtual);

            this.TestOutputHelper.WriteLine($"CoreDeviceHostType: {this._testLocalNetworkDevice.DeviceHostType}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_Manufacturer.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_Manufacturer()
        {
            this._testLocalNetworkDevice.Manufacturer.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Manufacturer: {this._testLocalNetworkDevice.Manufacturer}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_Model.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public void TestLocalNetworkDevice_DeviceInfo_Model()
        {
            this.TestOutputHelper.WriteLine($"Model: {this._testLocalNetworkDevice.Model}");

#if NV_PLAT_MACOS || NV_PLAT_MACCATALYST
            if (this.TestOperatingSystem.IsMacOSPlatform)
            {
                this._testLocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this._testLocalNetworkDevice.Model.Should().BeNullOrEmpty();
            }
#elif NV_PLAT_WINDOWS || NV_PLAT_WINUI || NET472_OR_GREATER || NV_PLAT_WPF
            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                this._testLocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this._testLocalNetworkDevice.Model.Should().BeNullOrEmpty();
            }
#elif NV_PLAT_ANDROID
            if (this.TestOperatingSystem.IsAndroid)
            {
                this._testLocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this._testLocalNetworkDevice.Model.Should().BeNullOrEmpty();
            }
#elif NV_PLAT_IOS
            if (this.TestOperatingSystem.IsIOS)
            {
                this._testLocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this._testLocalNetworkDevice.Model.Should().BeNullOrEmpty();
            }
#else
            // NET5.0, NET6.0, NET7.0, NET8.0, NET9.0, NET10.0, NETCOREAPP3.1, NETCOREAPP2.0
            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                if (this.IsWmiSupported)
                {
                    this._testLocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
                }
                else
                {
                    this._testLocalNetworkDevice.Model.Should().BeEmpty();
                }
            }
            else if (this.TestOperatingSystem.IsLinux || this.TestOperatingSystem.BuildPlatformType == CoreBuildPlatformType.NetCore)
            {
                this._testLocalNetworkDevice.Model.Should().BeEmpty();
            }
            else if (this.TestOperatingSystem.IsMacOSPlatform)
            {
                this._testLocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this._testLocalNetworkDevice.Model.Should().BeEmpty();
            }
#endif
        }

        /// <summary>
        /// Defines the test method DeviceInfo_Name.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_Name()
        {
            this.TestOutputHelper.WriteLine($"Name: {this._testLocalNetworkDevice.DeviceName}");
#if NV_PLAT_MACOS || NV_PLAT_MACCATALYST
            if (this.TestOperatingSystem.IsMacOSPlatform)
            {
                this._testLocalNetworkDevice.DeviceName.Should().NotBeNullOrEmpty();
            }
            else
            {
                this._testLocalNetworkDevice.DeviceName.Should().BeNullOrEmpty();
            }
#else
            this._testLocalNetworkDevice.DeviceName.Should().NotBeNullOrEmpty();
#endif
        }

        /// <summary>
        /// Defines the test method DeviceInfo_OSVersionString.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_OSVersionString()
        {
            this._testLocalNetworkDevice.OSVersionString.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"OSVersionString: {this._testLocalNetworkDevice.OSVersionString}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_OSVersion.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_OSVersion()
        {
            this._testLocalNetworkDevice.OSVersion.Should().NotBeNull().And.Subject.Should().BeOfType<Version>();
            this.TestOutputHelper.WriteLine($"OSVersion: {this._testLocalNetworkDevice.OSVersion}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_OSVersionString.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_DeviceVersionString()
        {
            this._testLocalNetworkDevice.DeviceVersionString.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"DeviceVersionString: {this._testLocalNetworkDevice.DeviceVersionString}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_DeviceVersion.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_DeviceVersion()
        {
            this._testLocalNetworkDevice.DeviceVersion.Should().NotBeNull().And.Subject.Should().BeOfType<Version>();
            this.TestOutputHelper.WriteLine($"DeviceVersion: {this._testLocalNetworkDevice.DeviceVersion}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_DeviceInfo.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_DeviceInfo()
        {
            // Do this first as it calls SystemProfiler on MacOSPlatform
            var model = this._testLocalNetworkDevice.Model;

            this.TestOutputHelper.WriteLine($"Name: {this._testLocalNetworkDevice.DeviceName}");
            this.TestOutputHelper.WriteLine($"Manufacturer: {this._testLocalNetworkDevice.Manufacturer}");
            this.TestOutputHelper.WriteLine($"Model: {model}");
            this.TestOutputHelper.WriteLine($"OSVersionString: {this._testLocalNetworkDevice.OSVersionString}");
            this.TestOutputHelper.WriteLine($"OSVersion: {this._testLocalNetworkDevice.OSVersion}");
            this.TestOutputHelper.WriteLine($"CoreDeviceIdiom: {this._testLocalNetworkDevice.DeviceIdiom}");
            this.TestOutputHelper.WriteLine($"CoreDeviceHostType: {this._testLocalNetworkDevice.DeviceHostType}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToString.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_ToString()
        {
            this.TestOutputHelper.WriteLine(this._testLocalNetworkDevice.ToString());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithPropName.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_ToStringWithPropName()
        {
            this.TestOutputHelper.WriteLine(this._testLocalNetworkDevice.ToString(CoreLoggableFormatFlags.ToStringWithPropName));
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithParentsPropName.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_ToStringWithParentsPropName()
        {
            this.TestOutputHelper.WriteLine(this._testLocalNetworkDevice.ToStringWithParentsPropName());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringMultiLine.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_ToStringMultiLine()
        {
            this.TestOutputHelper.WriteLine(this._testLocalNetworkDevice.ToStringWithMultiLine());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithPropNameMultiLine.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_ToStringWithPropNameMultiLine()
        {
            this.TestOutputHelper.WriteLine(this._testLocalNetworkDevice.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine));
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void TestLocalNetworkDevice_DeviceInfo_ToStringWithParentsPropNameMultiLine()
        {
            this.TestOutputHelper.WriteLine(this._testLocalNetworkDevice.ToStringWithParentsPropNameMultiLine());
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._testLocalNetworkDevice.NetworkServices.Preferences?.AppLocalUserSecureStorage?.RemoveAllAsync().GetAwaiter().GetResult();
                this._testLocalNetworkDevice.NetworkServices.Preferences?.AppRoamingUserSecureStorage?.RemoveAllAsync().GetAwaiter().GetResult();

                this._testLocalNetworkDevice.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
