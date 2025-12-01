// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// // ***********************************************************************
// <copyright file="CoreLocalNetworkDeviceIntegrationTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Devices
{
    /// <summary>
    /// Class CoreLocalNetworkDeviceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLocalNetworkDeviceIntegrationTests))]

    public class CoreLocalNetworkDeviceIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLocalNetworkDeviceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLocalNetworkDeviceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        private ICoreLocalNetworkDevice LocalNetworkDevice => this.TestNetworkServices.LocalNetworkDevice;

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.LocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this.LocalNetworkDevice.Should().BeAssignableTo<ICoreDeviceInfo>();
            this.LocalNetworkDevice.Should().BeAssignableTo<ICoreLocalNetworkAgent>();
            this.LocalNetworkDevice.LocalNetworkAgent.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkAgent>();

            this.LocalNetworkDevice.OperatingSystem.Should().NotBeNull().And.Subject.Should().BeSameAs(this.TestOperatingSystem);
            this.LocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_PreferredNetworkAddress.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_PreferredNetworkAddress()
        {
            this.LocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this.LocalNetworkDevice.PreferredNetworkAddress.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetworkAddress>();
            this.LocalNetworkDevice.PreferredNetworkAddress?.IPAddress.Should().Be(this.LocalNetworkDevice.IPAddress);
            this.LocalNetworkDevice.PreferredNetworkAddress?.SubnetMask.Should().Be(this.LocalNetworkDevice.SubnetMask);
            this.LocalNetworkDevice.PreferredNetworkAddress?.PhysicalAddress.Should().Be(this.LocalNetworkDevice.PhysicalAddress);
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_ObjectBag_Output()
        {
            this.TestOutputHelper.WriteLine($"Object Count: {this.LocalNetworkDevice.Count}");

            foreach (ICoreObjectItem item in this.LocalNetworkDevice.FindAllItems())
            {
                this.TestOutputHelper.WriteLine(item.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_ToStringWithParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_ToStringWithParentsPropNameMultiLine()
        {
            this.LocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this.LocalNetworkDevice.Should().BeAssignableTo<ICoreDeviceInfo>();
            this.LocalNetworkDevice.OperatingSystem.Should().NotBeNull().And.Subject.Should().BeSameAs(this.TestOperatingSystem);
            this.TestOutputHelper.WriteLine(this.LocalNetworkDevice.ToStringWithParentsPropNameMultiLine());
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_IPAddress.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_IPAddress()
        {
            this.TestOutputHelper.WriteLine($"IPAddress: {this.LocalNetworkDevice.IPAddress}");
            this.LocalNetworkDevice.IPAddress.IsNullNoneAnyOrLoopback().Should().BeFalse();
            this.LocalNetworkDevice.IPAddress!.Equals(this.TestNetworkServices.PreferredNetwork?.PreferredLocalNetworkAddress?.IPAddress).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_DeviceID.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceID()
        {
            this.TestOutputHelper.WriteLine($"DeviceID: {this.LocalNetworkDevice.DeviceID}");
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_SystemDeviceID.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_SystemDeviceID()
        {
            this.TestOutputHelper.WriteLine($"SystemDeviceID: {this.TestNetworkingSystem.SystemDeviceID}");
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_DeviceID.
        /// </summary>
        [Fact]
        public async Task LocalNetworkDevice_DeviceIDMatchesSystemDeviceIDAsync()
        {
            this.TestNetworkServices.Preferences.Should().NotBeNull();
            var storedDeviceId = await this.TestNetworkServices.Preferences!.GetPropAsync(CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId.ToVersionedStoragePropName());

            this.TestOutputHelper.WriteLine($"Stored DeviceID: {storedDeviceId}");
            this.TestOutputHelper.WriteLine($"DeviceID: {this.LocalNetworkDevice.DeviceID}");
            this.TestOutputHelper.WriteLine($"SystemDeviceID: {this.TestNetworkingSystem.SystemDeviceID}");
            this.LocalNetworkDevice.DeviceID.Should().Be(this.TestNetworkingSystem.SystemDeviceID);
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_UserID.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_UserID()
        {
            this.TestOutputHelper.WriteLine($"UserID: {this.LocalNetworkDevice.UserID}");
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_SystemUserID.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_SystemUserID()
        {
            this.TestOutputHelper.WriteLine($"SystemUserID: {this.TestNetworkingSystem.SystemUserID}");
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_UserIDMatchesSystemUserID.
        /// </summary>
        [Fact]
        public async Task LocalNetworkDevice_UserIDMatchesSystemUserIDAsync()
        {
            this.TestNetworkServices.Preferences.Should().NotBeNull();
            var storedUserId = await this.TestNetworkServices.Preferences!.GetPropAsync(CoreStoragePropName.SharedRoamingUserNetworkVisorAgentUserId.ToVersionedStoragePropName());

            this.TestOutputHelper.WriteLine($"Stored UserID: {storedUserId}");
            this.TestOutputHelper.WriteLine($"UserID: {this.LocalNetworkDevice.UserID}");
            this.TestOutputHelper.WriteLine($"SystemUserID: {this.TestNetworkingSystem.SystemUserID}");
            this.LocalNetworkDevice.UserID.Should().Be(this.TestNetworkingSystem.SystemUserID);
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_DeviceIDNotMatchUserID.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceIDNotMatchUserID()
        {
            this.TestOutputHelper.WriteLine($"UserID: {this.LocalNetworkDevice.UserID}");
            this.TestOutputHelper.WriteLine($"DeviceID: {this.LocalNetworkDevice.DeviceID}");
            this.LocalNetworkDevice.UserID.Should().NotBe(this.LocalNetworkDevice.DeviceID);
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_Subnet.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_Subnet()
        {
            this.TestOutputHelper.WriteLine($"Subnet: {this.LocalNetworkDevice.SubnetMask}");

            // Cellular connections can have an invalid SubnetMask.
            if (!this.LocalNetworkDevice.PreferredNetworkInterface!.IsCellularConnection)
            {
                this.LocalNetworkDevice.SubnetMask.IsNullNoneAnyOrLoopback().Should().BeFalse();
            }

            this.LocalNetworkDevice.SubnetMask!.Equals(this.TestNetworkServices.PreferredNetwork?.PreferredLocalNetworkAddress?.SubnetMask).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_PhysicalAddress.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_PhysicalAddress()
        {
            this.TestOutputHelper.WriteLine($"PhysicalAddress: {this.LocalNetworkDevice.PhysicalAddress.ToDashString()}");

            PhysicalAddress? physicalAddress = this.LocalNetworkDevice.PhysicalAddress;

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
        public void LocalNetworkDevice_IsLocalDevice()
        {
            this.TestOutputHelper.WriteLine($"IsLocalDevice: {this.LocalNetworkDevice.IsLocalDevice}");
            this.LocalNetworkDevice.IsLocalDevice.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_IsTestDevice.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_IsTestDevice()
        {
            this.TestOutputHelper.WriteLine($"IsTestDevice: {this.LocalNetworkDevice.IsTestDevice}");
            this.LocalNetworkDevice.IsTestDevice.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_IsLoopBackDevice.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_IsLoopBackDevice()
        {
            this.TestOutputHelper.WriteLine($"IsLoopBackDevice: {this.LocalNetworkDevice.IsLoopBackDevice}");
            this.LocalNetworkDevice.IsLoopBackDevice.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_DeviceType.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceType_LocalComputer()
        {
            this.TestOutputHelper.WriteLine($"DeviceType: {this.LocalNetworkDevice.DeviceType}");
            this.LocalNetworkDevice.DeviceType.Should().Be(CoreDeviceType.LocalComputer);
        }

        /// <summary>
        /// Defines the test method LocalNetworkDevice_ToStringWithPropNameMultiLine.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_ToStringWithPropNameMultiLine()
        {
            this.LocalNetworkDevice.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLocalNetworkDevice>();
            this.LocalNetworkDevice.Should().BeAssignableTo<ICoreDeviceInfo>();
            this.LocalNetworkDevice.OperatingSystem.Should().NotBeNull().And.Subject.Should()
                .BeSameAs(this.TestOperatingSystem);
            this.TestOutputHelper.WriteLine(this.LocalNetworkDevice.ToStringWithPropNameMultiLine());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_DeviceIdiom.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_DeviceIdiom()
        {
            this.TestOutputHelper.WriteLine($"CoreDeviceIdiom: {this.LocalNetworkDevice.DeviceIdiom}");

#if NV_PLAT_MACOS || NV_PLAT_MACCATALYST
            if (!this.TestOperatingSystem.IsMacOSPlatform)
            {
                this.LocalNetworkDevice.Model.Should().BeNullOrEmpty();
                return;
            }
#endif
            this.LocalNetworkDevice.DeviceIdiom.Should().Match(di =>
                di == CoreDeviceIdiom.Phone || di == CoreDeviceIdiom.Desktop || di == CoreDeviceIdiom.TV ||
                di == CoreDeviceIdiom.Tablet || di == CoreDeviceIdiom.Watch || di == CoreDeviceIdiom.Laptop);
        }

        /// <summary>
        /// Defines the test method DeviceInfo_DeviceType.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_DeviceHostType()
        {
            this.LocalNetworkDevice.DeviceHostType.Should().Match(dt =>
                dt == CoreDeviceHostType.Physical || dt == CoreDeviceHostType.Virtual);

            this.TestOutputHelper.WriteLine($"CoreDeviceHostType: {this.LocalNetworkDevice.DeviceHostType}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_Manufacturer.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_Manufacturer()
        {
            this.LocalNetworkDevice.Manufacturer.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Manufacturer: {this.LocalNetworkDevice.Manufacturer}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_Model.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public void LocalNetworkDevice_DeviceInfo_Model()
        {
            this.TestOutputHelper.WriteLine($"Model: {this.LocalNetworkDevice.Model}");

#if NV_PLAT_MACOS || NV_PLAT_MACCATALYST
            if (this.TestOperatingSystem.IsMacOSPlatform)
            {
                this.LocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this.LocalNetworkDevice.Model.Should().BeNullOrEmpty();
            }
#elif NV_PLAT_WINDOWS || NV_PLAT_WINUI || NET472_OR_GREATER || NV_PLAT_WPF
            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                this.LocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this.LocalNetworkDevice.Model.Should().BeNullOrEmpty();
            }
#elif NV_PLAT_ANDROID
            if (this.TestOperatingSystem.IsAndroid)
            {
                this.LocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this.LocalNetworkDevice.Model.Should().BeNullOrEmpty();
            }
#elif NV_PLAT_IOS
            if (this.TestOperatingSystem.IsIOS)
            {
                this.LocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this.LocalNetworkDevice.Model.Should().BeNullOrEmpty();
            }
#else
            // NET5.0, NET6.0, NET7.0, NET8.0, NET9.0, NET10.0, NETCOREAPP3.1, NETCOREAPP2.0
            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                if (this.IsWmiSupported)
                {
                    this.LocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
                }
                else
                {
                    this.LocalNetworkDevice.Model.Should().BeEmpty();
                }
            }
            else if (this.TestOperatingSystem.IsLinux || this.TestOperatingSystem.BuildPlatformType == CoreBuildPlatformType.NetCore)
            {
                this.LocalNetworkDevice.Model.Should().BeEmpty();
            }
            else if (this.TestOperatingSystem.IsMacOSPlatform)
            {
                this.LocalNetworkDevice.Model.Should().NotBeNullOrEmpty();
            }
            else
            {
                this.LocalNetworkDevice.Model.Should().BeEmpty();
            }
#endif
        }

        /// <summary>
        /// Defines the test method DeviceInfo_Name.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_Name()
        {
            this.TestOutputHelper.WriteLine($"Name: {this.LocalNetworkDevice.DeviceName}");
#if NV_PLAT_MACOS || NV_PLAT_MACCATALYST
            if (this.TestOperatingSystem.IsMacOSPlatform)
            {
                this.LocalNetworkDevice.DeviceName.Should().NotBeNullOrEmpty();
            }
            else
            {
                this.LocalNetworkDevice.DeviceName.Should().BeNullOrEmpty();
            }
#else
            this.LocalNetworkDevice.DeviceName.Should().NotBeNullOrEmpty();
#endif
        }

        /// <summary>
        /// Defines the test method DeviceInfo_OSVersionString.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_OSVersionString()
        {
            this.LocalNetworkDevice.OSVersionString.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"OSVersionString: {this.LocalNetworkDevice.OSVersionString}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_OSVersion.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_OSVersion()
        {
            this.LocalNetworkDevice.OSVersion.Should().NotBeNull().And.Subject.Should().BeOfType<Version>();
            this.TestOutputHelper.WriteLine($"OSVersion: {this.LocalNetworkDevice.OSVersion}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_OSVersionString.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_DeviceVersionString()
        {
            this.LocalNetworkDevice.DeviceVersionString.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"DeviceVersionString: {this.LocalNetworkDevice.DeviceVersionString}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_DeviceVersion.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_DeviceVersion()
        {
            this.LocalNetworkDevice.DeviceVersion.Should().NotBeNull().And.Subject.Should().BeOfType<Version>();
            this.TestOutputHelper.WriteLine($"DeviceVersion: {this.LocalNetworkDevice.DeviceVersion}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_DeviceInfo.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_DeviceInfo()
        {
            // Do this first as it calls SystemProfiler on MacOSPlatform
            var model = this.LocalNetworkDevice.Model;

            this.TestOutputHelper.WriteLine($"Name: {this.LocalNetworkDevice.DeviceName}");
            this.TestOutputHelper.WriteLine($"Manufacturer: {this.LocalNetworkDevice.Manufacturer}");
            this.TestOutputHelper.WriteLine($"Model: {model}");
            this.TestOutputHelper.WriteLine($"OSVersionString: {this.LocalNetworkDevice.OSVersionString}");
            this.TestOutputHelper.WriteLine($"OSVersion: {this.LocalNetworkDevice.OSVersion}");
            this.TestOutputHelper.WriteLine($"CoreDeviceIdiom: {this.LocalNetworkDevice.DeviceIdiom}");
            this.TestOutputHelper.WriteLine($"CoreDeviceHostType: {this.LocalNetworkDevice.DeviceHostType}");
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToString.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_ToString()
        {
            this.TestOutputHelper.WriteLine(this.LocalNetworkDevice.ToString());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithPropName.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_ToStringWithPropName()
        {
            this.TestOutputHelper.WriteLine(this.LocalNetworkDevice.ToString(CoreLoggableFormatFlags.ToStringWithPropName));
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithParentsPropName.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_ToStringWithParentsPropName()
        {
            this.TestOutputHelper.WriteLine(this.LocalNetworkDevice.ToStringWithParentsPropName());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringMultiLine.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_ToStringMultiLine()
        {
            this.TestOutputHelper.WriteLine(this.LocalNetworkDevice.ToStringWithMultiLine());
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithPropNameMultiLine.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_ToStringWithPropNameMultiLine()
        {
            this.TestOutputHelper.WriteLine(this.LocalNetworkDevice.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine));
        }

        /// <summary>
        /// Defines the test method DeviceInfo_ToStringWithParentsPropNameMultiLine.
        /// </summary>
        [Fact]
        public void LocalNetworkDevice_DeviceInfo_ToStringWithParentsPropNameMultiLine()
        {
            this.TestOutputHelper.WriteLine(this.LocalNetworkDevice.ToStringWithParentsPropNameMultiLine());
        }
    }
}
