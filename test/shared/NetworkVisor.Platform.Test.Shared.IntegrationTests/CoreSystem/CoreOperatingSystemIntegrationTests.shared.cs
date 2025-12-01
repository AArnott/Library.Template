// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreOperatingSystemIntegrationTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Platform.Test.Extensions;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.CoreSystem
{
    /// <summary>
    /// Class CoreOperatingSystemIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreOperatingSystemIntegrationTests))]

    public class CoreOperatingSystemIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreOperatingSystemIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreOperatingSystemIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreOperatingSystemIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void CoreOperatingSystem_Ctor()
        {
            this.TestOperatingSystem.Should().NotBeNull().And.BeOfType<CoreOperatingSystem>();
        }

        [Fact]
        public void CoreOperatingSystem_OSArchitecture_Output()
        {
            this.TestOutputHelper.WriteLine($"OSArchitecture: {this.TestOperatingSystem.OSArchitecture}");
            this.TestOperatingSystem.OSArchitecture.Should().Be(RuntimeInformation.OSArchitecture);
        }

        [Fact]
        public void CoreOperatingSystem_OSVersionForUserAgent_Output()
        {
            this.TestOutputHelper.WriteLine($"OSVersionForUserAgent: {this.TestOperatingSystem.OSVersionForUserAgent}");
        }

        /// <summary>
        /// Validates the OS architecture of the current operating system.
        /// </summary>
        /// <remarks>
        /// This test method checks the OS architecture of the current operating system and ensures it matches
        /// the expected architecture based on the platform-specific compilation symbols.
        /// </remarks>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the OS architecture does not match any of the expected values for the defined platform.
        /// </exception>
        [Fact]
        public void CoreOperatingSystem_OSArchitecture()
        {
            this.TestOutputHelper.WriteLine($"OSArchitecture: {this.TestOperatingSystem.OSArchitecture}");
#if NV_PLAT_ANDROID
            this.TestOperatingSystem.OSArchitecture.Should().BeOneOf(Architecture.X86, Architecture.X64, Architecture.Arm, Architecture.Arm64);
#elif NV_PLAT_IOS
            this.TestOperatingSystem.OSArchitecture.Should().BeOneOf(Architecture.X64, Architecture.Arm64);
#elif NV_PLAT_LINUX
            this.TestOperatingSystem.OSArchitecture.Should().BeOneOf(Architecture.X86, Architecture.X64, Architecture.Arm, Architecture.Arm64);
#elif NV_PLAT_MACCATALYST
            this.TestOperatingSystem.OSArchitecture.Should().BeOneOf(Architecture.X64, Architecture.Arm64);
#elif NV_PLAT_MACOS
            this.TestOperatingSystem.OSArchitecture.Should().BeOneOf(Architecture.X64, Architecture.Arm64);
#elif NV_PLAT_WPF
            this.TestOperatingSystem.OSArchitecture.Should().BeOneOf(Architecture.X86, Architecture.X64, Architecture.Arm, Architecture.Arm64);
#elif NV_PLAT_WINUI
            this.TestOperatingSystem.OSArchitecture.Should().BeOneOf(Architecture.X86, Architecture.X64, Architecture.Arm, Architecture.Arm64);
#elif NV_PLAT_WINDOWS || NV_PLAT_WPF
            this.TestOperatingSystem.OSArchitecture.Should().BeOneOf(Architecture.X86, Architecture.X64, Architecture.Arm, Architecture.Arm64);
#elif NV_PLAT_NETCORE
            this.TestOperatingSystem.OSArchitecture.Should().BeOneOf(Architecture.X86, Architecture.X64, Architecture.Arm, Architecture.Arm64);
#else
#error NV_PLAT_XXXX is undefined
#endif
        }

        /// <summary>
        /// Defines the test method IsAndroid.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsAndroid()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsAndroid)}: {this.TestOperatingSystem.IsAndroid}");

#if NV_PLAT_ANDROID
            this.TestOperatingSystem.IsAndroid.Should().BeTrue();
            this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.Android);
            System.OperatingSystem.IsAndroid().Should().BeTrue();
            System.OperatingSystem.IsAndroidVersionAtLeast(21).Should().BeTrue();
            this.TestOperatingSystem.IsAndroidVersionAtLeast(21).Should().BeTrue();
#else
            this.TestOperatingSystem.IsAndroid.Should().BeFalse();
#endif
        }

        /// <summary>
        /// Defines the test method IsIOS.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsIOS()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsIOS)}: {this.TestOperatingSystem.IsIOS}");

#if NV_PLAT_IOS
            this.TestOperatingSystem.IsIOS.Should().BeTrue();
            this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.IOS);
            System.OperatingSystem.IsIOS().Should().BeTrue();
            System.OperatingSystem.IsIOSVersionAtLeast(11).Should().BeTrue();
            this.TestOperatingSystem.IsIOSVersionAtLeast(11).Should().BeTrue();
#else
            this.TestOperatingSystem.IsIOS.Should().BeFalse();
#endif
        }

        /// <summary>
        /// Defines the test method IsMacCatalyst.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsMacCatalyst()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsMacCatalyst)}: {this.TestOperatingSystem.IsMacCatalyst}");

#if NV_PLAT_MACCATALYST
            this.TestOperatingSystem.IsMacCatalyst.Should().BeTrue();
            this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.MacCatalyst);
            System.OperatingSystem.IsMacCatalyst().Should().BeTrue();
            System.OperatingSystem.IsMacCatalystVersionAtLeast(13, 1).Should().BeTrue();
            this.TestOperatingSystem.IsMacCatalystVersionAtLeast(13, 1).Should().BeTrue();
#else
            this.TestOperatingSystem.IsMacCatalyst.Should().BeFalse();
#endif
        }

        /// <summary>
        /// Defines the test method IsMacOS.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsMacOS()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsMacOS)}: {this.TestOperatingSystem.IsMacOS}");
            this.TestOperatingSystem.IsMacOS.Should().Be(RuntimeInformation.IsOSPlatform(OSPlatform.OSX));

#if NV_HOST_MACOS
            if (this.TestOperatingSystem.IsMacOS)
            {
                this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.MacOS);
                System.OperatingSystem.IsMacOS().Should().BeTrue();
                System.OperatingSystem.IsMacOSVersionAtLeast(10, 15).Should().BeTrue();
                this.TestOperatingSystem.IsMacOSVersionAtLeast(10, 15).Should().BeTrue();
            }
#endif
        }

        /// <summary>
        /// Defines the test method IsWindows.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsWindows()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsWindows)}: {this.TestOperatingSystem.IsWindows}");

#if NV_PLAT_WINUI
            this.TestOperatingSystem.IsWindows.Should().BeFalse();
            System.OperatingSystem.IsWindows().Should().BeTrue();
            System.OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763, 0).Should().BeTrue();
            this.TestOperatingSystem.IsWinUIVersionAtLeast(10, 0, 17763, 0).Should().BeTrue();
#else
            this.TestOperatingSystem.IsWindows.Should().Be(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
#endif

#if NV_HOST_WINDOWS

            if (this.TestOperatingSystem.IsWindows)
            {
                this.TestOperatingSystem.OperatingSystemID.Should().Be(this.TestOperatingSystem.IsWPF
                    ? CoreOperatingSystemID.WPF
                    : CoreOperatingSystemID.Windows);
#if !NET472_OR_GREATER
                System.OperatingSystem.IsWindows().Should().BeTrue();
                System.OperatingSystem.IsWindowsVersionAtLeast(6, 1, 7601).Should().BeTrue();

                if (this.TestOperatingSystem.IsWPF)
                {
                    this.TestOperatingSystem.IsWPFVersionAtLeast(6, 1, 7601).Should().BeTrue();
                }
                else
                {
                    this.TestOperatingSystem.IsWindowsVersionAtLeast(6, 1, 7601).Should().BeTrue();
                }

                System.OperatingSystem.IsWindowsVersionAtLeast(6, 1, 7601).Should().BeTrue();
#endif
                this.TestOperatingSystem.IsWindowsVersionAtLeast(6, 1, 7601).Should().BeTrue();
            }
#endif
        }

        /// <summary>
        /// Defines the test method IsWindows.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsWindowsPackagedApp()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsWindowsPackagedApp)}: {this.TestOperatingSystem.IsWindowsPackagedApp}");

#if NV_PLAT_WINUI
            this.TestOperatingSystem.IsWindowsPackagedApp.Should()
                .Be(global::Windows.ApplicationModel.Package.Current is not null);
#else
            this.TestOperatingSystem.IsWindowsPackagedApp.Should().BeFalse();
#endif
        }

        /// <summary>
        /// Defines the test method IsWinUI.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsWinUI()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsWinUI)}: {this.TestOperatingSystem.IsWinUI}");

#if NV_PLAT_WINUI
            this.TestOperatingSystem.IsWinUI.Should().BeTrue();
            this.TestOperatingSystem.IsWindows.Should().BeFalse();
            System.OperatingSystem.IsWindows().Should().BeTrue();
            System.OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763, 0).Should().BeTrue();
            this.TestOperatingSystem.IsWinUIVersionAtLeast(10, 0, 17763, 0).Should().BeTrue();
#else
            this.TestOperatingSystem.IsWinUI.Should().BeFalse();
#endif

#if NV_HOST_WINDOWS
            if (this.TestOperatingSystem.IsWinUI)
            {
                this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.WinUI);
            }
#endif
        }

        /// <summary>
        /// Defines the test method IsWPF.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsWPF()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsWPF)}: {this.TestOperatingSystem.IsWPF}");

#if NV_PLAT_WINUI
            this.TestOperatingSystem.IsWindows.Should().BeFalse();
#elif NV_PLAT_WPF
            this.TestOperatingSystem.IsWPF.Should().BeTrue();
            this.TestOperatingSystem.IsWindows.Should().BeTrue();
            this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.WPF);
            this.TestOperatingSystem.IsWPFVersionAtLeast(6, 1, 7601).Should().BeTrue();
#if !NET472_OR_GREATER
            System.OperatingSystem.IsWindows().Should().BeTrue();
            System.OperatingSystem.IsWindowsVersionAtLeast(6, 1, 7601).Should().BeTrue();
#endif
#else
            this.TestOperatingSystem.IsWindows.Should().Be(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
#endif

#if NV_PLAT_WPF
            if (this.TestOperatingSystem.IsWPF)
            {
                this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.WPF);
            }

#elif NV_HOST_WINDOWS
            if (this.TestOperatingSystem.IsWindows)
            {
                this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.Windows);
            }
#endif
        }

        /// <summary>
        /// Defines the test method IsWindowsPlatform.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsWindowsPlatform()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsWindowsPlatform)}: {this.TestOperatingSystem.IsWindowsPlatform}");

#if NV_PLAT_WINUI
            this.TestOperatingSystem.IsWindowsPlatform.Should().BeTrue();
#else
            this.TestOperatingSystem.IsWindowsPlatform.Should().Be(RuntimeInformation.IsOSPlatform(OSPlatform.Windows));
#endif

#if NV_HOST_WINDOWS
            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                this.TestOperatingSystem.OperatingSystemID.Should().Be(this.TestOperatingSystem.IsWinUI
                    ?
                    CoreOperatingSystemID.WinUI
                    : this.TestOperatingSystem.IsWPF
                        ? CoreOperatingSystemID.WPF
                        : CoreOperatingSystemID.Windows);
            }
#endif
        }

        /// <summary>
        /// Defines the test method IsMacOSPlatform.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsMacOSPlatform()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsMacOSPlatform)}: {this.TestOperatingSystem.IsMacOSPlatform}");

#if NV_PLAT_MACCATALYST
            this.TestOperatingSystem.IsMacOSPlatform.Should().BeTrue();
            System.OperatingSystem.IsMacCatalyst().Should().BeTrue();
            System.OperatingSystem.IsMacCatalystVersionAtLeast(13, 1).Should().BeTrue();
            this.TestOperatingSystem.IsMacCatalystVersionAtLeast(13, 1).Should().BeTrue();
#else
            this.TestOperatingSystem.IsMacOSPlatform.Should().Be(RuntimeInformation.IsOSPlatform(OSPlatform.OSX));
#endif

#if NV_HOST_MACOS
            if (this.TestOperatingSystem.IsMacOSPlatform)
            {
                if (this.TestOperatingSystem.IsMacCatalyst)
                {
                    this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.MacCatalyst);
                    this.TestOperatingSystem.IsMacCatalystVersionAtLeast(13, 1);
                }
                else
                {
                    this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.MacOS);
                    this.TestOperatingSystem.IsMacOSVersionAtLeast(10, 15);
                }
            }
#endif
        }

        /// <summary>
        /// Defines the test method IsLinux.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsLinux()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsLinux)}: {this.TestOperatingSystem.IsLinux}");

            this.TestOperatingSystem.IsLinux.Should().Be(RuntimeInformation.IsOSPlatform(OSPlatform.Linux));

#if NV_HOST_LINUX
            if (this.TestOperatingSystem.IsLinux)
            {
                this.TestOperatingSystem.OperatingSystemID.Should().Be(CoreOperatingSystemID.Linux);
            }
#endif
        }

        /// <summary>
        /// Defines the test method IsMobileOS.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsMobileOS()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsMobileOS)}: {this.TestOperatingSystem.IsMobileOS}");

#if NV_PLAT_MOBILE
            this.TestOperatingSystem.IsMobileOS.Should().BeTrue();
#else
            this.TestOperatingSystem.IsMobileOS.Should().BeFalse();
#endif
        }

        /// <summary>
        /// Defines the test method IsDesktopOS.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_IsDesktopOS()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsDesktopOS)}: {this.TestOperatingSystem.IsDesktopOS}");

#if NV_PLAT_MOBILE
            this.TestOperatingSystem.IsDesktopOS.Should().BeFalse();
#else
            this.TestOperatingSystem.IsDesktopOS.Should().BeTrue();
#endif
        }

        /// <summary>
        /// Defines the test method Mobile Device Platform is not null.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_MobileDevicePlatform_NotNull()
        {
#if NV_PLAT_MOBILE
            this.TestOperatingSystem.MobileDevicePlatform().Should().NotBeNull().And.BeOfType<DevicePlatform>();
#endif
        }

        /// <summary>
        /// Defines the test method Mobile Device Platform is not null.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_MobileDevicePlatform()
        {
#if NV_PLAT_MOBILE
            this.TestOutputHelper.WriteLine($"MobileDevicePlatform: {this.TestOperatingSystem.MobileDevicePlatform()}");
            DevicePlatform? mobileDevicePlatform = this.TestOperatingSystem.MobileDevicePlatform();
            mobileDevicePlatform.Should().NotBeNull().And.BeOfType<DevicePlatform>();

#if NV_PLAT_ANDROID
            mobileDevicePlatform.Equals(DevicePlatform.Android).Should().BeTrue();
            mobileDevicePlatform.Equals(DevicePlatform.iOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.macOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.MacCatalyst).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.tvOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.Tizen).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.WinUI).Should().BeFalse();
#elif NV_PLAT_IOS
            mobileDevicePlatform.Equals(DevicePlatform.Android).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.iOS).Should().BeTrue();
            mobileDevicePlatform.Equals(DevicePlatform.macOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.MacCatalyst).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.tvOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.Tizen).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.WinUI).Should().BeFalse();
#elif NV_PLAT_MACCATALYST
            mobileDevicePlatform.Equals(DevicePlatform.Android).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.iOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.macOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.MacCatalyst).Should().BeTrue();
            mobileDevicePlatform.Equals(DevicePlatform.tvOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.Tizen).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.WinUI).Should().BeFalse();
#elif NV_PLAT_WINUI
            mobileDevicePlatform.Equals(DevicePlatform.Android).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.iOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.macOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.MacCatalyst).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.tvOS).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.Tizen).Should().BeFalse();
            mobileDevicePlatform.Equals(DevicePlatform.WinUI).Should().BeTrue();
#endif
#endif
        }

        // Works on NetCore (Windows and MacOS)

        /// <summary>
        /// Defines the test method CoreOperatingSystem_IsLoggerProviderSupportedOnDevice.
        /// </summary>
        /// <param name="loggingOutput">The logging output.</param>
        /// <param name="isSupported">if set to <see langword="true"/> [is supported].</param>
        [Theory]
#if NV_PLAT_ANDROID
        [InlineData(LoggerProviderFlags.AndroidLog, true)]
#else
        [InlineData(LoggerProviderFlags.AndroidLog, false)]
#endif

#if NV_PLAT_IOS || NV_PLAT_MACCATALYST
        [InlineData(LoggerProviderFlags.AppleLog, true)]
#else
        [InlineData(LoggerProviderFlags.AppleLog, false)]
#endif

#if !NV_PLAT_IOS && !NV_PLAT_ANDROID && !NV_PLAT_WINUI
        [InlineData(LoggerProviderFlags.Console, true)]
#else
        [InlineData(LoggerProviderFlags.Console, false)]
#endif

#if NV_PLAT_IOS || NV_PLAT_ANDROID || NV_PLAT_WINUI || NV_PLAT_MACCATALYST
        [InlineData(LoggerProviderFlags.Maui, true)]
#else
        [InlineData(LoggerProviderFlags.Maui, false)]
#endif

#if NET472_OR_GREATER
        [InlineData(LoggerProviderFlags.RollingFile, false)]
#else
        [InlineData(LoggerProviderFlags.RollingFile, true)]
#endif

        // Works on all platforms
        [InlineData(LoggerProviderFlags.OpenTelemetry, true)]
        [InlineData(LoggerProviderFlags.ReadFromConfig, true)]
        [InlineData(LoggerProviderFlags.TestCorrelator, true)]
        [InlineData(LoggerProviderFlags.TestCaseLogger, true)]
        [InlineData(LoggerProviderFlags.Silent, true)]
        [InlineData(LoggerProviderFlags.SQLiteTransactions, true)]
        public void CoreOperatingSystem_IsLoggerProviderSupportedOnDevice(LoggerProviderFlags loggingOutput, bool isSupported)
        {
            this.TestOperatingSystem.IsLoggerProviderSupportedOnDevice(loggingOutput).Should().Be(isSupported);
        }

        /// <summary>
        /// Defines the test method GetDeviceType.
        /// </summary>
        [Fact]
        public void CoreOperatingSystem_GetDeviceType()
        {
            this.TestOutputHelper.WriteLine($"DeviceHostType: {this.TestOperatingSystem.DeviceHostType}");

#if NV_PLAT_MACCATALYST
            // TODO: Figure out how to set and test CoreDeviceHostType on MacCatalyst
            this.TestOperatingSystem.DeviceHostType.Should().Be(CoreDeviceHostType.Physical);
#elif NV_PLAT_IOS
            this.TestOperatingSystem.DeviceHostType.Should().Be(Runtime.Arch == Arch.DEVICE ? CoreDeviceHostType.Physical : CoreDeviceHostType.Virtual);
#elif NV_PLAT_ANDROID

            // Output Android Build information
            this.TestOutputHelper.WriteLine($"Android Build.Fingerprint: {Build.Fingerprint}");
            this.TestOutputHelper.WriteLine($"Android Build.Model: {Build.Model}");
            this.TestOutputHelper.WriteLine($"Android Build.Manufacturer: {Build.Manufacturer}");
            this.TestOutputHelper.WriteLine($"Android Build.Brand: {Build.Brand}");
            this.TestOutputHelper.WriteLine($"Android Build.Product: {Build.Product}");

            var isEmulator =
                (Build.Brand!.StartsWith("generic", StringComparison.Ordinal) && Build.Device!.StartsWith("generic", StringComparison.Ordinal)) ||
                Build.Fingerprint!.StartsWith("generic", StringComparison.Ordinal) ||
                Build.Fingerprint.StartsWith("unknown", StringComparison.Ordinal) ||
                Build.Hardware!.Contains("goldfish", StringComparison.Ordinal) ||
                Build.Hardware.Contains("ranchu", StringComparison.Ordinal) ||
                Build.Model!.Contains("google_sdk", StringComparison.Ordinal) ||
                Build.Model.Contains("Emulator", StringComparison.Ordinal) ||
                Build.Model.Contains("Android SDK built for x86", StringComparison.Ordinal) ||
                Build.Manufacturer!.Contains("Genymotion", StringComparison.Ordinal) ||
                Build.Manufacturer.Contains("VS Emulator", StringComparison.Ordinal) ||
                Build.Product!.Contains("emulator", StringComparison.Ordinal) ||
                Build.Product.Contains("google_sdk", StringComparison.Ordinal) ||
                Build.Product.Contains("sdk", StringComparison.Ordinal) ||
                Build.Product.Contains("sdk_google", StringComparison.Ordinal) ||
                Build.Product.Contains("sdk_x86", StringComparison.Ordinal) ||
                Build.Product.Contains("simulator", StringComparison.Ordinal) ||
                Build.Product.Contains("vbox86p", StringComparison.Ordinal);

            this.TestOperatingSystem.DeviceHostType.Should().Be(isEmulator ? CoreDeviceHostType.Virtual : CoreDeviceHostType.Physical);
#else
            this.TestOperatingSystem.DeviceHostType.Should().Be(CoreDeviceHostType.Physical);
#endif
        }

        [Fact]
        public void CoreOperatingSystem_BuildHost()
        {
            this.TestOutputHelper.WriteLine($"BuildHostType: {this.TestOperatingSystem.BuildHostType}");

#if NV_HOST_WINDOWS
            this.TestOperatingSystem.BuildHostType.Should().Be(CoreBuildHostType.Windows);
            this.TestOperatingSystem.IsWindowsBuildHost.Should().BeTrue();
            this.TestOperatingSystem.IsLinuxBuildHost.Should().BeFalse();
            this.TestOperatingSystem.IsMacOSBuildHost.Should().BeFalse();
#elif NV_HOST_LINUX
            this.TestOperatingSystem.BuildHostType.Should().Be(CoreBuildHostType.Linux);
            this.TestOperatingSystem.IsWindowsBuildHost.Should().BeFalse();
            this.TestOperatingSystem.IsLinuxBuildHost.Should().BeTrue();
            this.TestOperatingSystem.IsMacOSBuildHost.Should().BeFalse();
#elif NV_HOST_MACOS
            this.TestOperatingSystem.BuildHostType.Should().Be(CoreBuildHostType.MacOS);
            this.TestOperatingSystem.IsWindowsBuildHost.Should().BeFalse();
            this.TestOperatingSystem.IsLinuxBuildHost.Should().BeFalse();
            this.TestOperatingSystem.IsMacOSBuildHost.Should().BeTrue();
#else
#error NV_HOST_XXXX is undefined
#endif
        }

        [Fact]
        public void CoreOperatingSystem_BuildPlatformType()
        {
            this.TestOutputHelper.WriteLine($"BuildPlatformType: {this.TestOperatingSystem.BuildPlatformType}");

#if NV_PLAT_ANDROID
            this.TestOperatingSystem.BuildPlatformType.Should().Be(CoreBuildPlatformType.Android);
#elif NV_PLAT_IOS
            this.TestOperatingSystem.BuildPlatformType.Should().Be(CoreBuildPlatformType.IOS);
#elif NV_PLAT_LINUX
            this.TestOperatingSystem.BuildPlatformType.Should().Be(CoreBuildPlatformType.Linux);
#elif NV_PLAT_MACCATALYST
            this.TestOperatingSystem.BuildPlatformType.Should().Be(CoreBuildPlatformType.MacCatalyst);
#elif NV_PLAT_MACOS
            this.TestOperatingSystem.BuildPlatformType.Should().Be(CoreBuildPlatformType.MacOS);
#elif NV_PLAT_WPF
            this.TestOperatingSystem.BuildPlatformType.Should().Be(CoreBuildPlatformType.WPF);
#elif NV_PLAT_WINUI
            this.TestOperatingSystem.BuildPlatformType.Should().Be(CoreBuildPlatformType.WinUI);
#elif NV_PLAT_WINDOWS || NV_PLAT_WPF
            this.TestOperatingSystem.BuildPlatformType.Should().Be(CoreBuildPlatformType.Windows);
#elif NV_PLAT_NETCORE
            this.TestOperatingSystem.BuildPlatformType.Should().Be(CoreBuildPlatformType.NetCore);
#else
#error NV_PLAT_XXXX is undefined
#endif
        }

        [Fact]
        public void CoreOperatingSystem_BuildSharedPlatformType()
        {
            this.TestOutputHelper.WriteLine(
                $"BuildSharedPlatformType: {this.TestOperatingSystem.BuildSharedPlatformType}");
#if NV_PLAT_LINUX || NV_PLAT_MACOS || NV_PLAT_WINDOWS || NV_PLAT_MACCATALYST || NV_PLAT_WINUI || NV_PLAT_WPF
            this.TestOperatingSystem.BuildSharedPlatformType.Should().Be(CoreBuildSharedPlatformType.Desktop);
#elif NV_PLAT_MOBILE
            this.TestOperatingSystem.BuildSharedPlatformType.Should().Be(CoreBuildSharedPlatformType.Mobile);
#elif NV_PLAT_NETCORE
            this.TestOperatingSystem.BuildSharedPlatformType.Should().Be(CoreBuildSharedPlatformType.Server);
#else
#error NV_PLAT_XXXX is undefined
#endif
        }

        /*
         *         /// <summary>
           /// Unknown Operating System
           /// </summary>
           [Description("Unknown Operating System")]
           Unknown,

           /// <summary>
           /// Windows operating system
           /// </summary>
           [Description("Microsoft Windows")]
           Windows,

           /// <summary>
           /// MacOS operating system
           /// </summary>
           [Description("Apple macOS")]
           MacOS,

           /// <summary>
           /// Android operating system
           /// </summary>
           [Description("Google Android")]
           Android,

           /// <summary>
           /// IOS operating system
           /// </summary>
           [Description("Apple iOS")]
           IOS,

           /// <summary>
           /// MacCatalyst operating system
           /// </summary>
           [Description("Apple Mac Catalyst")]
           MacCatalyst,

           /// <summary>
           /// Linux operating system
           /// </summary>
           [Description("Linux")]
           Linux,
         */

        /// <summary>
        /// Tests the OperatingSystemID enum.
        /// </summary>
        /// <param name="operatingSystemID">OperatingSystemID.</param>
        /// <param name="simpleName">Simple name.</param>
        /// <param name="displayName">Display name.</param>
        [Theory]
        [InlineData(CoreOperatingSystemID.Unknown, "Unknown", "Unknown Operating System")]
        [InlineData(CoreOperatingSystemID.Windows, "Windows", "Microsoft Windows")]
        [InlineData(CoreOperatingSystemID.MacOS, "MacOS", "Apple macOS")]
        [InlineData(CoreOperatingSystemID.Android, "Android", "Google Android")]
        [InlineData(CoreOperatingSystemID.IOS, "IOS", "Apple iOS")]
        [InlineData(CoreOperatingSystemID.MacCatalyst, "MacCatalyst", "Apple Mac Catalyst")]
        [InlineData(CoreOperatingSystemID.Linux, "Linux", "Linux")]
        [InlineData(CoreOperatingSystemID.WPF, "WPF", "Windows Presentation Framework")]
        public void CoreOperatingSystem_ID_Enum(CoreOperatingSystemID operatingSystemID, string simpleName, string displayName)
        {
            operatingSystemID.ToSimpleName().Should().Be(simpleName);
            operatingSystemID.ToDisplayName().Should().Be(displayName);
            operatingSystemID.GetDescription().Should().Be(displayName);
            Enum.Parse(typeof(CoreOperatingSystemID), simpleName).Should().Be(operatingSystemID);
        }

        [Fact]
        public void CoreOperatingSystem_SystemProductVersion()
        {
            this.TestOutputHelper.WriteLine(
                $"Product Version: {FileVersionInfo.GetVersionInfo(typeof(int).Assembly.Location).ProductVersion}");
            this.TestOutputHelper.WriteLine($"Framework Version: {System.Environment.Version}");
        }

        [Fact]
        public void CoreOperatingSystem_FrameworkInfo()
        {
            this.TestOutputHelper.WriteLine(
                $"FrameworkInfo: {this.TestOperatingSystem.FrameworkInfo.ToStringWithPropNameMultiLine()}");
            this.TestOperatingSystem.FrameworkInfo.Should().NotBeNull().And.BeAssignableTo<ICoreFrameworkInfo>();
        }

        [Fact]
        public void CoreOperatingSystem_ProductionBuild_Output()
        {
            this.TestOutputHelper.WriteLine($"Production Build: {this.TestOperatingSystem.ProductionBuildEnvironment}");
        }

        [Fact]
        public void CoreOperatingSystem_NotUnknownBuild()
        {
            this.TestOperatingSystem.ProductionBuildEnvironment.Should().NotBe(CoreHostEnvironment.Default);
        }

        [Fact]
        public void CoreOperatingSystem_IsProductionBuild()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsProductionBuild)}: {this.TestOperatingSystem.IsProductionBuild}");

#if NV_BUILD_PRODUCTION
            this.TestOperatingSystem.ProductionBuildEnvironment.Should().Be(CoreHostEnvironment.Production);
            this.TestOperatingSystem.IsProductionBuild.Should().BeTrue();
            this.TestOperatingSystem.IsDevelopmentBuild.Should().BeFalse();
            this.TestOperatingSystem.IsStagingBuild.Should().BeFalse();
            this.TestOperatingSystem.IsTestingBuild.Should().BeFalse();
#else
            this.TestOperatingSystem.IsProductionBuild.Should().BeFalse();
            this.TestOperatingSystem.ProductionBuildEnvironment.Should().NotBe(CoreHostEnvironment.Production);
#endif
        }

        [Fact]
        public void CoreOperatingSystem_IsDevelopmentBuild()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsDevelopmentBuild)}: {this.TestOperatingSystem.IsDevelopmentBuild}");

#if NV_BUILD_DEVELOPMENT
            this.TestOperatingSystem.ProductionBuildEnvironment.Should().Be(CoreHostEnvironment.Development);
            this.TestOperatingSystem.IsProductionBuild.Should().BeFalse();
            this.TestOperatingSystem.IsDevelopmentBuild.Should().BeTrue();
            this.TestOperatingSystem.IsStagingBuild.Should().BeFalse();
            this.TestOperatingSystem.IsTestingBuild.Should().BeFalse();
#else
            this.TestOperatingSystem.IsDevelopmentBuild.Should().BeFalse();
            this.TestOperatingSystem.ProductionBuildEnvironment.Should().NotBe(CoreHostEnvironment.Development);
#endif
        }

        [Fact]
        public void CoreOperatingSystem_IsStagingBuild()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsStagingBuild)}: {this.TestOperatingSystem.IsStagingBuild}");

#if NV_BUILD_STAGING
            this.TestOperatingSystem.ProductionBuildEnvironment.Should().Be(CoreHostEnvironment.Staging);
            this.TestOperatingSystem.IsProductionBuild.Should().BeFalse();
            this.TestOperatingSystem.IsDevelopmentBuild.Should().BeFalse();
            this.TestOperatingSystem.IsStagingBuild.Should().BeTrue();
            this.TestOperatingSystem.IsTestingBuild.Should().BeFalse();
#else
            this.TestOperatingSystem.IsStagingBuild.Should().BeFalse();
            this.TestOperatingSystem.ProductionBuildEnvironment.Should().NotBe(CoreHostEnvironment.Staging);
#endif
        }

        [Fact]
        public void CoreOperatingSystem_IsTestingBuild()
        {
            this.TestOutputHelper.WriteLine(
                $"{nameof(this.TestOperatingSystem.IsTestingBuild)}: {this.TestOperatingSystem.IsTestingBuild}");

#if NV_BUILD_TESTING
            this.TestOperatingSystem.ProductionBuildEnvironment.Should().Be(CoreHostEnvironment.Testing);
            this.TestOperatingSystem.IsProductionBuild.Should().BeFalse();
            this.TestOperatingSystem.IsDevelopmentBuild.Should().BeFalse();
            this.TestOperatingSystem.IsStagingBuild.Should().BeFalse();
            this.TestOperatingSystem.IsTestingBuild.Should().BeTrue();
#else
            this.TestOperatingSystem.IsTestingBuild.Should().BeFalse();
            this.TestOperatingSystem.ProductionBuildEnvironment.Should().NotBe(CoreHostEnvironment.Testing);
#endif
        }

        [Fact]
        public void CoreOperatingSystem_IsProcessElevated()
        {
            this.TestOutputHelper.WriteLine($"IsProcessElevated: {this.TestOperatingSystem.IsProcessElevated}");
            this.TestOperatingSystem.IsProcessElevated.Should()
                .Be(this.TestOperatingSystem.IsWindows || CoreTestConstants.IsReSharperTestRunner);
        }

        [Fact]
        public void CoreOperatingSystem_PlatformAssemblyType()
        {
#if NV_PLAT_ANDROID
            this.TestOperatingSystem.PlatformAssemblyType.Should().Be(CorePlatformAssemblyType.Android);
#elif NV_PLAT_IOS
            this.TestOperatingSystem.PlatformAssemblyType.Should().Be(CorePlatformAssemblyType.IOS);
#elif NV_PLAT_LINUX
            this.TestOperatingSystem.PlatformAssemblyType.Should().Be(CorePlatformAssemblyType.Linux);
#elif NV_PLAT_MACCATALYST
            this.TestOperatingSystem.PlatformAssemblyType.Should().Be(CorePlatformAssemblyType.MacCatalyst);
#elif NV_PLAT_MACOS
            this.TestOperatingSystem.PlatformAssemblyType.Should().Be(CorePlatformAssemblyType.MacOS);
#elif NV_PLAT_WPF
            this.TestOperatingSystem.PlatformAssemblyType.Should().Be(CorePlatformAssemblyType.WPF);
#elif NV_PLAT_WINUI
            this.TestOperatingSystem.PlatformAssemblyType.Should().Be(CorePlatformAssemblyType.WinUI);
#elif NV_PLAT_WINDOWS || NV_PLAT_WPF
            this.TestOperatingSystem.PlatformAssemblyType.Should().Be(CorePlatformAssemblyType.Windows);
#elif NV_PLAT_NETCORE
            this.TestOperatingSystem.PlatformAssemblyType.Should().Be(CorePlatformAssemblyType.NetCore);
#else
#error NV_PLAT_XXXX is undefined
#endif

        }
    }
}
