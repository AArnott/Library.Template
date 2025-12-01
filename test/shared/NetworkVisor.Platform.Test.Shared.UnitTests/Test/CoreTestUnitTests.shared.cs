// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreTestUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Formatters;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Extensions;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Test
{
    /// <summary>
    /// Class CoreTestUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreTestUnitTests))]

    public class CoreTestUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTestUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreTestUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        [Fact]
        public void TestBase_OutputTestLogger()
        {
            this.TestCaseLogger.LogInformation("Test");
        }

        [Fact]
        public void TestBase_BeginMethodScope()
        {
            using IDisposable scope = this.TestCaseLogger.BeginMethodScope();

            this.TestCaseLogger.LogInformation("Test");
        }

        [Fact]
        public void TestBase_TestFileSystem()
        {
            this.TestFileSystem.Should().NotBeNull().And.BeAssignableTo<ICoreFileSystem>();
        }

        [Fact]
        public void TestBase_TestOperatingSystem()
        {
            this.TestFileSystem.OperatingSystem.Should().NotBeNull().And.BeAssignableTo<ICoreOperatingSystem>();
            this.TestOperatingSystem.Should().NotBeNull().And.BeAssignableTo<ICoreOperatingSystem>();
            this.TestOperatingSystem.Should().BeSameAs(this.TestFileSystem.OperatingSystem);
        }

        [Fact]
        public void TestBase_CleanUpAppSessionFolderOnDispose()
        {
            this.CleanUpAppSessionFolderOnDispose.Should().Be(this.AppSettings.AppHostSettings.AppHostEnvironment.IsDevTestEnvironment());
        }

        [Fact]
        public void TestBase_TestBuildHost()
        {
#if NV_HOST_WINDOWS
            CoreAppConstants.AppBuildHostType.Should().Be(CoreBuildHostType.Windows);
            CoreAppConstants.IsWindowsBuildHost.Should().BeTrue();
            CoreAppConstants.IsLinuxBuildHost.Should().BeFalse();
            CoreAppConstants.IsMacOSBuildHost.Should().BeFalse();
#elif NV_HOST_LINUX
            CoreAppConstants.AppBuildHostType.Should().Be(CoreBuildHostType.Linux);
            CoreAppConstants.IsWindowsBuildHost.Should().BeFalse();
            CoreAppConstants.IsLinuxBuildHost.Should().BeTrue();
            CoreAppConstants.IsMacOSBuildHost.Should().BeFalse();
#elif NV_HOST_MACOS
            CoreAppConstants.AppBuildHostType.Should().Be(CoreBuildHostType.MacOS);
            CoreAppConstants.IsWindowsBuildHost.Should().BeFalse();
            CoreAppConstants.IsLinuxBuildHost.Should().BeFalse();
            CoreAppConstants.IsMacOSBuildHost.Should().BeTrue();
#else
#error NV_HOST_XXXX is undefined
#endif
        }

        [Fact]
        public void TestBase_IsWmiSupported()
        {
#if NV_PLAT_WINDOWS || NV_PLAT_WINUI || NV_PLAT_WPF
            this.IsWmiSupported.Should().Be(this.TestOperatingSystem.IsWindowsPlatform);
#else
            this.IsWmiSupported.Should().BeFalse();
#endif
        }
    }
}
