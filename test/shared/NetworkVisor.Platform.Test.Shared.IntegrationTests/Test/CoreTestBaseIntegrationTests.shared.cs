// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreTestBaseIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Cloud.Client;
using NetworkVisor.Core.Configuration;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.LogProvider;
using NetworkVisor.Core.Networking.Firewall;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Networking.Services.Ping;
using NetworkVisor.Core.Test.Logging.Factory;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Test
{
    /// <summary>
    /// Class CoreTestBaseIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreTestBaseIntegrationTests))]

    public class CoreTestBaseIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestBaseIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTestBaseIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method TestBase_Ctor.
        /// </summary>
        [Fact]
        public void TestBase_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestCaseLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCaseLoggerFactory>();

            // CoreClientTestBase methods
            this.AppSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreAppSettings>();
            this.TestNetworkConnectivity.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkConnectivity>();
            this.TestCloudClient.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCloudClient>();

            this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
        }

        /// <summary>
        /// Defines the test method TestBase_Dispose.
        /// </summary>
        [Fact]
        public void TestBase_Dispose()
        {
            using var testAssemblyFixture = CoreTestAssemblyFixture.Create();
            using var testClassFixture = new CoreTestClassFixture(testAssemblyFixture);
            using var testClientBase = new TestClientBaseTest(this.TestOutputHelper, testClassFixture);
        }

        /// <summary>
        /// Defines the test method TestBase_FileSystem.
        /// </summary>
        [Fact]
        public void TestBase_FileSystem()
        {
            this.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();
        }

        /// <summary>
        /// Defines the test method TestBase_NetworkServices.
        /// </summary>
        [Fact]
        public void TestBase_NetworkServices()
        {
            this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            this.TestNetworkServices.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);
            this.TestNetworkServices.NetworkPing.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkPing>();
            this.TestNetworkServices.NetworkFirewall.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkFirewall>();
            this.TestNetworkServices.NetworkFirewall.Should().BeSameAs(this.TestNetworkFirewall);
        }

        /// <summary>
        /// Defines the test method TestBase_NetworkConnectivity.
        /// </summary>
        [Fact]
        public void TestBase_NetworkConnectivity()
        {
            this.TestNetworkConnectivity.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkConnectivity>();
            this.TestNetworkConnectivity.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);
        }

        /// <summary>
        /// Defines the test method TestBase_NetworkingSystem.
        /// </summary>
        [Fact]
        public void TestBase_NetworkingSystem()
        {
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
        }

        /// <summary>
        /// Defines the test method TestBase_LoggerFactory.
        /// </summary>
        [Fact]
        public void TestBase_LoggerFactory()
        {
            this.TestCaseLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCaseLoggerFactory>();
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

        [Fact]
        public void TestBase_TestConfiguration_GetLogTestCaseScope()
        {
            this.Configuration.Should().NotBeNull();

            this.Configuration[CoreAppConstants.LogTestCaseScopePropertyName].Should().NotBeNullOrEmpty();
            this.Configuration[CoreAppConstants.LogTestCaseScopePropertyName].Should().Be(this.AddTestCaseScope.ToString());
            this.Configuration.GetLogTestCaseScope(this.AddTestCaseScope).Should().Be(this.AddTestCaseScope);
        }

        [Fact]
        public void TestBase_TestConfiguration_GetLoggerProviders()
        {
            this.Configuration.Should().NotBeNull();

            this.Configuration[CoreAppConstants.LoggerProvidersPropertyName].Should().NotBeNullOrEmpty();
            this.Configuration.GetLoggerProviders(CoreAppLoggingConstants.GetDefaultLoggerProviders(CoreHostEnvironment.Testing, false), false).Should().Be(this.TestOperatingSystem.IsMobileOS ? LoggerProviderFlags.MauiTest : LoggerProviderFlags.Testing);

            this.Configuration[CoreAppConstants.MinimumLogLevelPropertyName].Should().NotBeNullOrEmpty();
            this.Configuration.GetMinimumLogLevel().Should().Be(CoreAppConstants.GetMinimumLogLevel());
        }

        [Fact]
        public void TestBase_TestConfiguration_GetLoggerProviders_AddOpenTelemetry()
        {
            this.Configuration.Should().NotBeNull();

            this.Configuration[CoreAppConstants.LoggerProvidersPropertyName].Should().NotBeNullOrEmpty();
            this.Configuration.GetLoggerProviders(null, true).Should().Be(this.TestOperatingSystem.IsMobileOS ? LoggerProviderFlags.MauiTest | LoggerProviderFlags.OpenTelemetry : LoggerProviderFlags.Testing | LoggerProviderFlags.OpenTelemetry);

            this.Configuration[CoreAppConstants.MinimumLogLevelPropertyName].Should().NotBeNullOrEmpty();
            this.Configuration.GetMinimumLogLevel().Should().Be(CoreAppConstants.GetMinimumLogLevel());
        }

        [Fact]
        public void TestBase_TestConfiguration_GetMinimumLogLevel()
        {
            this.Configuration.Should().NotBeNull();

            this.Configuration[CoreAppConstants.MinimumLogLevelPropertyName].Should().NotBeNullOrEmpty();
            this.Configuration.GetMinimumLogLevel().Should().Be(CoreAppConstants.GetMinimumLogLevel());
        }

        /// <summary>
        /// Test class for CoreClientTestBase.
        /// </summary>
        private class TestClientBaseTest : CoreTestCaseBase
        {
            public TestClientBaseTest(ICoreTestOutputHelper testOutputHelper, CoreTestClassFixture testClassFixture)
                : base(testClassFixture)
            {
            }
        }
    }
}
