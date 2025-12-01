// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreClientTestBaseUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Cloud.Auth;
using NetworkVisor.Core.Cloud.Client;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Identity.Credentials;
using NetworkVisor.Core.Logging.Factory;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Firewall;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Networking.Services.Ping;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Logging.Factory;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.TestCase;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;
using Xunit.v3;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Test
{
    /// <summary>
    /// Class ClientTestBaseUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreClientTestBaseUnitTests))]

    public class CoreClientTestBaseUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreClientTestBaseUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreClientTestBaseUnitTests(CoreTestClassFixture testClassFixture)
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

        [Fact]
        public void TestBase_AppSettings()
        {
            var jsonString = JsonSerializer.Serialize(this.AppSettings, typeof(CoreAppSettings), CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));
            this.TestOutputHelper.WriteLine($"AppSettings:\n{jsonString}");

            this.AppSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreAppSettings>();
            this.AppSettings.AppHostSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreAppHostSettings>();
            this.AppSettings.AppLoggingSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreAppLoggingSettings>();
            this.AppSettings.AppAuthSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreAppAuthSettings>();
            this.AppSettings.CloudAuthSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCloudAuthSettings>();
            this.AppSettings.AppAssembly.Should().BeSameAs(this.TestAssembly);
            this.AppSettings.AppFolderName.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public void TestBase_AppSettings_AppHostSettings()
        {
            this.AppSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreAppSettings>();
            this.AppSettings.AppHostSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreAppHostSettings>();
            this.AppSettings.AppHostSettings.IsServiceSupported(this.AppSettings.AppHostSettings.SupportedNetworkServices).Should().Be(this.TestNetworkingSystem.IsServiceSupported(this.AppSettings.AppHostSettings.SupportedNetworkServices));
        }

        /// <summary>
        /// Defines the test method TestBase_DoubleDispose.
        /// </summary>
        [Fact]
        public void TestBase_DoubleDispose()
        {
            using var testAssemblyFixture = CoreTestAssemblyFixture.Create();
            using var testClassFixture = new CoreTestClassFixture(testAssemblyFixture);
            using var testClientBase = new TestClientBaseTest(this.TestOutputHelper, testClassFixture);
            testClientBase.Dispose();
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
        /// Defines the test method TestBase_ActiveTestCase.
        /// </summary>
        [Fact]
        public void TestBase_ActiveTestCase()
        {
            ActiveTestCase.Should().NotBeNull();
            ActiveTestCase.Should().BeSameAs(this);
        }

        /// <summary>
        /// Defines the test method TestBase_ActiveTestCaseLogger.
        /// </summary>
        [Fact]
        public void TestBase_ActiveTestCaseLogger()
        {
            ActiveTestCase.Should().NotBeNull();
            ActiveTestCase.Should().BeSameAs(this);
            using ICoreTestCaseLogger? testCaseLogger = ActiveTestCaseLogger;
            testCaseLogger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCaseLogger>();

            // testCaseLogger!.TestCase.Should().BeSameAs(this);
            testCaseLogger!.LogDebug("Test Active Logger");
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
        /// Defines the test method TestBase_TestCaseLoggerFactory.
        /// </summary>
        [Fact]
        public void TestBase_TestCaseLoggerFactory()
        {
            this.TestCaseLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCaseLoggerFactory>();
        }

        /// <summary>
        /// Defines the test method TestBase_GlobalLogLevel.
        /// </summary>
        [Fact]
        public void TestBase_GlobalLogLevel()
        {
            LogLevel defaultLogLevel = CoreAppConstants.GetMinimumLogLevel();
            ICoreGlobalLogLevel globalLogLevel = this.TestCaseServiceProvider.GetRequiredService<ICoreGlobalLogLevel>();

            this.TestCaseLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCaseLoggerFactory>();
            this.TestCaseLoggerFactory.GlobalLogLevel.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreGlobalLogLevel>();
            this.TestCaseLoggerFactory.GlobalLogLevel.Should().Be(globalLogLevel);
            this.TestCaseLoggerFactory.GlobalLogLevel.Current.Should().Be(CoreAppConstants.GetMinimumLogLevel());

            this.TestNetworkingSystem.Logger.LoggerLogLevel.Should().Be(globalLogLevel);
            this.TestNetworkServices.Logger.LoggerLogLevel.Should().Be(globalLogLevel);
        }

#if FIX_TESTCASELOGGER
        /// <summary>
        /// Defines the test method TestBase_TestCaseLoggerFactory.
        /// </summary>
        [Fact]
        public void TestBase_GlobalLogLevel_BeginScope()
        {
            LogLevel defaultLogLevel = CoreAppConstants.GetMinimumLogLevel();

            this.TestCaseLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCaseLoggerFactory>();
            this.TestCaseLoggerFactory.GlobalLogLevel.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreGlobalLogLevel>();
            this.TestCaseLoggerFactory.GlobalLogLevel.Current.Should().Be(defaultLogLevel);

            testCaseLoggerDefault.LoggerLogLevel.Should().Be(this.TestCaseLoggerFactory.GlobalLogLevel);
            testCaseLoggerDefault.LogTrace("Default: {LogLevel}", LogLevel.Trace);
            testCaseLoggerDefault.LogDebug("Default: {LogLevel}", LogLevel.Debug);
            testCaseLoggerDefault.LogInformation("Default: {LogLevel}", LogLevel.Information);

            this.TestNetworkServices.Logger.LoggerLogLevel.Current.Should().Be(defaultLogLevel);

            using (IDisposable logLevelScope = this.TestCaseLoggerFactory.GlobalLogLevel.BeginScope(LogLevel.Information))
            {

                this.TestCaseLoggerFactory.GlobalLogLevel.Current.Should().Be(LogLevel.Information);

                testCaseLoggerDefault.LoggerLogLevel.Current.Should().Be(LogLevel.Information);
                testCaseLoggerScope.LoggerLogLevel.Current.Should().Be(LogLevel.Information);
                testCaseLoggerScope.LogTrace("Scope: {LogLevel}", LogLevel.Trace);
                testCaseLoggerScope.LogDebug("Scope: {LogLevel}", LogLevel.Debug);
                testCaseLoggerScope.LogInformation("Scope: {LogLevel}", LogLevel.Information);

                this.TestNetworkServices.Logger.LoggerLogLevel.Current.Should().Be(LogLevel.Information);
            }

            this.TestCaseLoggerFactory.GlobalLogLevel.Current.Should().Be(defaultLogLevel);
        }
#endif

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
