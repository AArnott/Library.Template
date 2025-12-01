// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreTestIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Extensions;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Test
{
    /// <summary>
    /// Class CoreTestIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreTestIntegrationTests))]

    public class CoreTestIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTestIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreTestIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void CoreTestIntegration_IsReSharperTestRunner()
        {
            this.TestOutputHelper.WriteLine($"IsReSharperTestRunner: {CoreTestConstants.IsReSharperTestRunner}");
            CoreTestConstants.IsReSharperTestRunner.Should().Be(!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("NV_TESTRUNNER_RESHARPER")));
        }

        [Fact]
        public void CoreTestIntegration_IsRunningInCI()
        {
            this.TestOutputHelper.WriteLine($"IsRunningInCI: {CoreAppConstants.IsRunningInCI}");
            CoreAppConstants.IsRunningInCI.Should().Be(CoreAppConstants.IsRunningOnAzure || CoreAppConstants.IsRunningOnGitHub);
        }

        [Fact]
        public void CoreTestIntegration_IsRunningOnAzure()
        {
            this.TestOutputHelper.WriteLine($"IsRunningOnAzure: {CoreAppConstants.IsRunningOnAzure}");
            CoreAppConstants.IsRunningOnAzure.Should().Be(!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("BUILD_REASON")));
        }

        [Fact]
        public void CoreTestIntegration_IsLocalDevEnvironment()
        {
            this.TestOutputHelper.WriteLine($"IsLocalDevEnvironment: {CoreAppConstants.IsLocalDevEnvironment}");
            CoreAppConstants.IsLocalDevEnvironment.Should().Be(!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("NV_DEV_LOCAL")));
        }

        [Fact]
        public void CoreTestIntegration_IsRunningOnGitHub()
        {
            this.TestOutputHelper.WriteLine($"IsRunningOnGitHub: {CoreAppConstants.IsRunningOnGitHub}");
            CoreAppConstants.IsRunningOnGitHub.Should().Be(!string.IsNullOrEmpty(System.Environment.GetEnvironmentVariable("GITHUB_ACTIONS")));
        }

        [Fact]
        public void CoreTestIntegration_IsLinuxBuildHost()
        {
            this.TestOutputHelper.WriteLine($"IsLinuxBuildHost: {CoreAppConstants.IsLinuxBuildHost}");
        }

        [Fact]
        public void CoreTestIntegration_IsMacOSBuildHost()
        {
            this.TestOutputHelper.WriteLine($"IsMacOSBuildHost: {CoreAppConstants.IsMacOSBuildHost}");
        }

        [Fact]
        public void CoreTestIntegration_IsWindowsBuildHost()
        {
            this.TestOutputHelper.WriteLine($"IsWindowsBuildHost: {CoreAppConstants.IsWindowsBuildHost}");
        }

        [Fact]
        public void CoreTestIntegration_TestClassType()
        {
            this.TestOutputHelper.WriteLine($"TestClassType: {this.TestClassType}");
            this.TestClassType.ToString().Should().Be("NetworkVisor.Platform.Test.Shared.IntegrationTests.Test.CoreTestIntegrationTests");
        }

        [Fact]
        public void CoreTestIntegration_AppBuildHostType()
        {
            this.TestOutputHelper.WriteLine($"AppBuildHostType: {CoreAppConstants.AppBuildHostType}");
        }

#if NV_HOST_WINDOWS
        [Fact]
        public void CoreTestIntegration_WindowsHostTests()
        {
            CoreAppConstants.AppBuildHostType.Should().Be(CoreBuildHostType.Windows);
            CoreAppConstants.IsWindowsBuildHost.Should().BeTrue();
            CoreAppConstants.IsMacOSBuildHost.Should().BeFalse();
            CoreAppConstants.IsLinuxBuildHost.Should().BeFalse();
        }

#elif NV_HOST_MACOS
        [Fact]
        public void CoreTestIntegration_MacOSHostTests()
        {
            CoreAppConstants.AppBuildHostType.Should().Be(CoreBuildHostType.MacOS);
            CoreAppConstants.IsWindowsBuildHost.Should().BeFalse();
            CoreAppConstants.IsMacOSBuildHost.Should().BeTrue();
            CoreAppConstants.IsLinuxBuildHost.Should().BeFalse();
        }

#elif NV_HOST_LINUX
        [Fact]
        public void CoreTestIntegration_LinuxHostTests()
        {
            CoreAppConstants.AppBuildHostType.Should().Be(CoreBuildHostType.Linux);
            CoreAppConstants.IsWindowsBuildHost.Should().BeFalse();
            CoreAppConstants.IsMacOSBuildHost.Should().BeFalse();
            CoreAppConstants.IsLinuxBuildHost.Should().BeTrue();
        }
#else
#error Unknown Build Host
#endif
    }
}
