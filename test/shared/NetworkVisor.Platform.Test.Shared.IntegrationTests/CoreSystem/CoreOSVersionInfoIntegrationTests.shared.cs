// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreOSVersionInfoIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Runtime.InteropServices;
using NetworkVisor.Core.Extensions;

#if NV_PLAT_ANDROID
using Android.OS;
#endif

#if NV_PLAT_IOS || NV_PLAT_MACCATALYST
using ObjCRuntime;
#endif

using System.Diagnostics;
using System.Linq;
using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.LogProvider;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.CoreSystem
{
    /// <summary>
    /// Class CoreOSVersionInfoIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreOSVersionInfoIntegrationTests))]

    public class CoreOSVersionInfoIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreOSVersionInfoIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreOSVersionInfoIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreOSVersionInfo_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void CoreOSVersionInfo_Ctor()
        {
            this.TestOperatingSystem.Should().NotBeNull().And.BeOfType<CoreOperatingSystem>();
        }

        [Fact]
        public void CoreOSVersionInfo_All_Output()
        {
            Version osVersion = this.TestOperatingSystem.OSVersion;
            this.TestOutputHelper.WriteLine($"OSVersionString: [{this.TestOperatingSystem.OSVersionString}]");
            this.TestOutputHelper.WriteLine($"OSVersion: [{osVersion}]");
            this.TestOutputHelper.WriteLine($"OSVersion.Major: [{osVersion.Major}]");
            this.TestOutputHelper.WriteLine($"OSVersion.Minor: [{osVersion.Minor}]");
            this.TestOutputHelper.WriteLine($"OSVersion.Build: [{osVersion.Build}]");
            this.TestOutputHelper.WriteLine($"OSVersion.Revision: [{osVersion.Revision}]");
            this.TestOutputHelper.WriteLine($"OSVersionStringWithPlatform: [{this.TestOperatingSystem.OSVersionWithPlatform}]");
            this.TestOutputHelper.WriteLine($"OSVersionPlatform: [{this.TestOperatingSystem.OSVersionPlatform}]");
            this.TestOutputHelper.WriteLine($"OSVersionServicePack: [{this.TestOperatingSystem.OSVersionServicePack}]");

            this.TestOutputHelper.WriteLine();

            Version systemOSVersion = System.Environment.OSVersion.Version;
            this.TestOutputHelper.WriteLine($"SystemVersionString: [{System.Environment.OSVersion.VersionString}]");
            this.TestOutputHelper.WriteLine($"SystemVersion: [{systemOSVersion}]");
            this.TestOutputHelper.WriteLine($"SystemVersion.Major: [{systemOSVersion.Major}]");
            this.TestOutputHelper.WriteLine($"SystemVersion.Minor: [{systemOSVersion.Minor}]");
            this.TestOutputHelper.WriteLine($"SystemVersion.Build: [{systemOSVersion.Build}]");
            this.TestOutputHelper.WriteLine($"SystemVersion.Revision: [{systemOSVersion.Revision}]");

#if NV_PLAT_MOBILE
            this.TestOutputHelper.WriteLine();

            Version deviceInfoVersion = DeviceInfo.Version;
            this.TestOutputHelper.WriteLine($"DeviceInfo.VersionString: [{DeviceInfo.VersionString}]");
            this.TestOutputHelper.WriteLine($"DeviceInfo.Version: [{deviceInfoVersion}]");
            this.TestOutputHelper.WriteLine($"DeviceInfo.Major: [{deviceInfoVersion.Major}]");
            this.TestOutputHelper.WriteLine($"DeviceInfo.Minor: [{deviceInfoVersion.Minor}]");
            this.TestOutputHelper.WriteLine($"DeviceInfo.Build: [{deviceInfoVersion.Build}]");
            this.TestOutputHelper.WriteLine($"DeviceInfo.Revision: [{deviceInfoVersion.Revision}]");
#endif

            this.TestOutputHelper.WriteLine();

            Version deviceVersion = this.TestOperatingSystem.DeviceVersion;
            this.TestOutputHelper.WriteLine($"DeviceVersionString: [{this.TestOperatingSystem.DeviceVersionString}]");
            this.TestOutputHelper.WriteLine($"DeviceVersion: [{deviceVersion}]");
            this.TestOutputHelper.WriteLine($"DeviceVersion.Major: [{deviceVersion.Major}]");
            this.TestOutputHelper.WriteLine($"DeviceVersion.Minor: [{deviceVersion.Minor}]");
            this.TestOutputHelper.WriteLine($"DeviceVersion.Build: [{deviceVersion.Build}]");
            this.TestOutputHelper.WriteLine($"DeviceVersion.Revision: [{deviceVersion.Revision}]");
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersion_Output()
        {
            Version osVersion = this.TestOperatingSystem.OSVersion;
            this.TestOutputHelper.WriteLine($"OS Version: [{osVersion}]");
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersion_OutputParts()
        {
            Version osVersion = this.TestOperatingSystem.OSVersion;
            this.TestOutputHelper.WriteLine($"OS Version: [{osVersion}]");
            this.TestOutputHelper.WriteLine($"Major: [{osVersion.Major}]");
            this.TestOutputHelper.WriteLine($"Minor: [{osVersion.Minor}]");
            this.TestOutputHelper.WriteLine($"Build: [{osVersion.Build}]");
            this.TestOutputHelper.WriteLine($"Revision: [{osVersion.Revision}]");
        }

        [Fact]
        public void CoreOSVersionInfo_DeviceVersion_Output()
        {
            Version deviceVersion = this.TestOperatingSystem.DeviceVersion;
            this.TestOutputHelper.WriteLine($"Device Version: [{deviceVersion}]");
        }

        [Fact]
        public void CoreOSVersionInfo_DeviceVersion_OutputParts()
        {
            Version deviceVersion = this.TestOperatingSystem.DeviceVersion;
            this.TestOutputHelper.WriteLine($"DeviceVersion: [{deviceVersion}]");
            this.TestOutputHelper.WriteLine($"DeviceVersion.Major: [{deviceVersion.Major}]");
            this.TestOutputHelper.WriteLine($"DeviceVersion.Minor: [{deviceVersion.Minor}]");
            this.TestOutputHelper.WriteLine($"DeviceVersion.Build: [{deviceVersion.Build}]");
            this.TestOutputHelper.WriteLine($"DeviceVersion.Revision: [{deviceVersion.Revision}]");
        }

        [Fact]
        public void CoreOSVersionInfo_DeviceVersionString_Output()
        {
            string deviceVersionString = this.TestOperatingSystem.DeviceVersionString;
#if NV_PLAT_MOBILE
            string systemDeviceVersionString = DeviceInfo.VersionString;
#else
            string systemDeviceVersionString = System.Environment.OSVersion.Version.ToString();
#endif

            this.TestOutputHelper.WriteLine($"SystemDeviceVersionString: [{systemDeviceVersionString}]\nOSDeviceVersionString: [{deviceVersionString}]");

            deviceVersionString.Should().Be(systemDeviceVersionString);

            Version deviceVersion = deviceVersionString.ParseVersion();
            Version deviceVersionSystem = systemDeviceVersionString.ParseVersion();
            deviceVersion.Should().Be(deviceVersionSystem);
        }

        [Fact]
        public void CoreOSVersionInfo_DeviceVersion_SameAsOS()
        {
            Version deviceVersion = this.TestOperatingSystem.DeviceVersion;
            this.TestOutputHelper.WriteLine($"Device Version: [{deviceVersion}]");
        }

        [Fact]
        public void CoreOSVersionInfo_DeviceVersionString_SameAsOS()
        {
            Version deviceVersion = this.TestOperatingSystem.DeviceVersion;

#if NV_PLAT_MOBILE
            Version systemDeviceVersion = DeviceInfo.Version;
#else
            Version systemDeviceVersion = System.Environment.OSVersion.Version;
#endif
            this.TestOutputHelper.WriteLine($"System Device Version: [{systemDeviceVersion}]\nOS Device Version: [{deviceVersion}]");

            deviceVersion.Should().Be(systemDeviceVersion);
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersionSystem_OutputParts()
        {
            Version systemOSVersion = System.Environment.OSVersion.Version;
            this.TestOutputHelper.WriteLine($"System OS Version: [{systemOSVersion}]");
            this.TestOutputHelper.WriteLine($"Major: [{systemOSVersion.Major}]");
            this.TestOutputHelper.WriteLine($"Minor: [{systemOSVersion.Minor}]");
            this.TestOutputHelper.WriteLine($"Build: [{systemOSVersion.Build}]");
            this.TestOutputHelper.WriteLine($"Revision: [{systemOSVersion.Revision}]");
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersionString_Output()
        {
            string osVersionString = this.TestOperatingSystem.OSVersionString;
            this.TestOutputHelper.WriteLine($"OSVersionString: [{osVersionString}]");
            osVersionString.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersionPlatform_Output()
        {
            string? osVersionPlatform = this.TestOperatingSystem.OSVersionPlatform;
            this.TestOutputHelper.WriteLine($"OSVersionPlatform: [{osVersionPlatform}]");
            osVersionPlatform.Should().NotBeNullOrEmpty();

            if (!this.TestOperatingSystem.IsWindowsPlatform)
            {
                osVersionPlatform.Should().Be("Unix");
            }
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersionServicePack_Output()
        {
            string? osVersionServicePack = this.TestOperatingSystem.OSVersionServicePack;
            this.TestOutputHelper.WriteLine($"OSVersionServicePack: [{osVersionServicePack}]");
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersionStringWithPlatform_Output()
        {
            string osVersionStringWithPlatform = this.TestOperatingSystem.OSVersionWithPlatform;
            this.TestOutputHelper.WriteLine($"OSVersionStringWithPlatform: [{osVersionStringWithPlatform}]");

            osVersionStringWithPlatform.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersion_SameAsOS()
        {
            Version osVersion = this.TestOperatingSystem.OSVersion;
            Version systemOSVersion = System.Environment.OSVersion.Version;
            this.TestOutputHelper.WriteLine($"System OS Version: [{systemOSVersion}]\nOS Version: [{osVersion}]");

            osVersion.Should().Be(systemOSVersion);
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersionString_SameAsOS()
        {
            string osVersionString = this.TestOperatingSystem.OSVersionString;
            string systemOSVersionString = System.Environment.OSVersion.Version.ToString();

            this.TestOutputHelper.WriteLine($"SystemVersionString: [{systemOSVersionString}]\nOS VersionString: [{osVersionString}]");

            osVersionString.Should().Be(systemOSVersionString);

            Version osVersion = osVersionString.ParseVersion();
            osVersion.Should().Be(System.Environment.OSVersion.Version);
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersionWithPlatform_SameAsOS()
        {
            string osVersionWithPlatform = this.TestOperatingSystem.OSVersionWithPlatform;
            string systemOSVersionString = System.Environment.OSVersion.VersionString;

            this.TestOutputHelper.WriteLine($"SystemVersionString: [{systemOSVersionString}]\nOSVersionWithPlatform: [{osVersionWithPlatform}]");

            osVersionWithPlatform.Should().Be(systemOSVersionString);
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersionPlatform_SameAsOS()
        {
            string? osVersionPlatformString = this.TestOperatingSystem.OSVersionPlatform;
            osVersionPlatformString.Should().NotBeNullOrEmpty();
            string systemOSVersionString = System.Environment.OSVersion.VersionString;

            this.TestOutputHelper.WriteLine($"SystemVersionString: [{systemOSVersionString}]\nOSVersionPlatform: [{osVersionPlatformString}]");

            // Remove any platform information before comparison.
            (Version Version, string? Platform, string? ServicePack)? result = VersionExtensions.ParseOSVersionString(systemOSVersionString);
            result.Should().NotBeNull();
            result!.Value.Platform.Should().NotBeNullOrEmpty();

            osVersionPlatformString.Should().Be(result!.Value.Platform);
            this.TestOutputHelper.WriteLine($"SystemOSVersionPlatformString: [{result!.Value.Platform}]");
        }

        [Fact]
        public void CoreOSVersionInfo_OSVersionServicePack_SameAsOS()
        {
            string? osVersionServicePack = this.TestOperatingSystem.OSVersionServicePack;
            string systemOSVersionString = System.Environment.OSVersion.VersionString;

            this.TestOutputHelper.WriteLine($"SystemVersionString: [{systemOSVersionString}]\nOSVersionServicePack: [{osVersionServicePack}]");

            // Remove any platform information before comparison.
            (Version Version, string? Platform, string? ServicePack)? result = VersionExtensions.ParseOSVersionString(systemOSVersionString);
            result.Should().NotBeNull();
            result!.Value.Platform.Should().NotBeNullOrEmpty();

            osVersionServicePack.Should().Be(result!.Value.ServicePack);
        }

        [Theory]
        [InlineData("Microsoft Windows NT 10.0.19041.0", "10.0.19041.0", "Microsoft Windows NT", null)]
        [InlineData("Microsoft Windows NT 10.0.19041.0 Service Pack 1", "10.0.19041.0", "Microsoft Windows NT", "Service Pack 1")]
        [InlineData("Microsoft Windows NT 6.1.7601.65536", "6.1.7601.65536", "Microsoft Windows NT", null)]
        [InlineData("Microsoft Win32S 3.1", "3.1", "Microsoft Windows", null)]
        [InlineData("Microsoft Windows 95 4.0.0", "4.0.0", "Microsoft Windows 95", null)]
        [InlineData("Microsoft Windows 98 4.1.0", "4.1.0", "Microsoft Windows 98", null)]
        [InlineData("Microsoft Windows CE 2.12", "2.12", "Microsoft Windows CE", null)]
        [InlineData("Unix 5.4.0", "5.4.0", "Unix", null)]
        [InlineData("Mac OS X 10.15.7", "10.15.7", "Mac OS X", null)]
        [InlineData("Xbox 6.2.9781.0", "6.2.9781.0", "Xbox", null)]
        [InlineData("Xbox 10.0.26100.1969", "10.0.26100.1969", "Xbox", null)]
        [InlineData("Other 1", "1.0", "Other", null)]
        [InlineData("Other 1.2", "1.2", "Other", null)]
        [InlineData("Other 1.2.3", "1.2.3", "Other", null)]
        [InlineData("Other 1.2.3.4", "1.2.3.4", "Other", null)]
        public void ParseVersionString_ValidInput_ReturnsCorrectVersionAndPlatform(string input, string expectedVersion, string? expectedPlatformPrefix, string? expectedPlatformSuffix)
        {
            (Version Version, string? PlatformPrefix, string? PlatformSuffix)? result = VersionExtensions.ParseOSVersionString(input);

            if (result is not null)
            {
                this.TestOutputHelper.WriteLine($"Version: {result!.Value.Version}\nPlatformPrefix: {result!.Value.PlatformPrefix}\nPlatformSuffix: {result!.Value.PlatformSuffix}");
            }

            result.Should().NotBeNull();
            Assert.Equal(new Version(expectedVersion), result!.Value.Version);
            Assert.Equal(expectedPlatformPrefix, result.Value.PlatformPrefix);
            Assert.Equal(expectedPlatformSuffix, result.Value.PlatformSuffix);
        }

        [Fact]
        public void ParseVersionString_NullOrEmptyInput_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => VersionExtensions.ParseOSVersionString(null!));
            Assert.Throws<ArgumentException>(() => VersionExtensions.ParseOSVersionString(string.Empty));
        }
    }
}
