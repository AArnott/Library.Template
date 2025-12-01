// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="CoreTestApplicationIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Hosting;
using NetworkVisor.Core.Test.Logging;
using NetworkVisor.Core.Test.TestApp;
using NetworkVisor.Core.Test.TestCase;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestApp;
using NetworkVisor.Platform.Test.TestApp.Extensions;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;
using Xunit.Abstractions;
using static NetworkVisor.Platform.Test.TestApp.Extensions.CoreTestApplicationBuilder;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests
{
    /// <summary>
    /// Class CoreTestApplicationIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreTestApplicationIntegrationTests))]
    [Collection("TestRun")]
    public class CoreTestApplicationIntegrationTests : CoreTestCaseBase<CoreTestApplicationIntegrationTests>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestApplicationIntegrationTests"/> class.
        /// </summary>
        /// <param name="testOutputHelper">The test output helper used to output to tests.</param>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        /// <param name="testRunFixture">Test run fixture common to all tests in the collection.</param>
        public CoreTestApplicationIntegrationTests(ITestOutputHelper testOutputHelper, CoreTestClassFixture<CoreTestApplicationIntegrationTests> testClassFixture, CoreTestRunFixture testRunFixture)
            : base(testOutputHelper, testClassFixture, testRunFixture)
        {
        }

        [Fact]
        public void CoreTestApplicationIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_ServiceHost()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            var jsonString = JsonSerializer.Serialize(this.ServiceHost, typeof(CoreServiceHost<CoreTestApplication>), CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted));
            this.TestOutputHelper.WriteLine(jsonString);
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestRunTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this._testRunFixture.TraitOperatingSystem}");
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestRunTraitTestType()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this._testRunFixture.TraitTestType}");
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestRunAssembly()
        {
            this.TestOutputHelper.WriteLine($"TestRunAssembly: {this._testRunFixture.TestRunAssembly?.FullName}");
            this.TestOutputHelper.WriteLine($"TestRunAssembly Type: {this._testRunFixture.TestRunAssembly?.GetType()}");
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_Current()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            CurrentCoreApplication.Current.Should().NotBeNull();
            this.TestApplication.Application.Should().BeSameAs(CurrentCoreApplication.Current!.Application);
            this.TestApplication.ServiceProvider.Should().BeSameAs(CurrentCoreApplication.Current.ServiceProvider);
            this.TestApplication.LoggerFactory.Should().NotBeNull();
            this.TestApplication.LoggerFactory.Should().BeSameAs(CurrentCoreApplication.Current!.LoggerFactory);
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_CurrentCoreApplication()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();

            ICurrentCoreApplication? currentApplication = CurrentCoreApplication.Current;
            Assert.NotNull(currentApplication);
            Assert.NotNull(currentApplication.ServiceProvider);
            Assert.NotNull(currentApplication.Application);

            currentApplication.Application.Should().BeSameAs(this.TestApplication.Application);
            currentApplication.ServiceProvider.Should().BeSameAs(this.TestApplication.ServiceProvider);
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_TestOperatingSystem()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ICoreOperatingSystem operatingSystem = this.GetRequiredService<ICoreOperatingSystem>();
            operatingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreOperatingSystem>();

            var jsonString = JsonSerializer.Serialize(operatingSystem, typeof(CoreTestApplicationBuilder.TestCoreOperatingSystem), CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted));
            this.TestOutputHelper.WriteLine(jsonString);
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_TestCase()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ICoreTestCase testCase = this.GetRequiredService<ICoreTestCase>();
            testCase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCase>();
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_TestFileSystem()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ICoreFileSystem fileSystem = this.GetRequiredService<ICoreFileSystem>();
            fileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();

            var jsonString = JsonSerializer.Serialize(fileSystem, typeof(CoreTestApplicationBuilder.TestCoreFileSystem), CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted));
            this.TestOutputHelper.WriteLine(jsonString);
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_TestLoggerFactory()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ICoreTestLoggerFactory testLoggerFactory = this.GetRequiredService<ICoreTestLoggerFactory>();
            testLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestLoggerFactory>();
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_TestLoggerFactory_TestCaseProviders()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ICoreTestLoggerFactory testLoggerFactory = this.GetRequiredService<ICoreTestLoggerFactory>();
            testLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestLoggerFactory>();
            testLoggerFactory.TestCaseProviders.Should().HaveCount(1);
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_CoreLogger()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ICoreLogger logger = this.GetRequiredService<ICoreLogger>();
            logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();
            logger.LogDebug("Debug Message");
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_CoreLoggerT()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ICoreLogger logger = this.GetRequiredService<ICoreLogger<CoreTestApplicationIntegrationTests>>();
            logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();
            logger.LogDebug("Debug Message");
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_Logger()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ILogger logger = this.GetRequiredService<ILogger>();
            logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILogger>();
            logger.LogDebug("Debug Message");
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_LoggerT()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ILogger logger = this.GetRequiredService<ILogger<CoreTestApplicationIntegrationTests>>();
            logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILogger>();
            logger.LogDebug("Debug Message");
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestApplication_LoggerTWithScope()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ILogger logger = this.GetRequiredService<ILogger<CoreTestApplicationIntegrationTests>>();
            logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ILogger>();

            using IDisposable? scopeString = logger.BeginScope("ScopeString");
            using IDisposable? scopeParams = logger.BeginScope("ScopeParams: {ParamInt} {ParamString}", 1, "ParamStringValue");
            logger.LogDebug("Test {MsgParamProp}", "MsgParam1Value");
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestScopedWithLogger()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ITestScopedWithLogger testScopedWithLogger = this.GetRequiredService<ITestScopedWithLogger>();
            testScopedWithLogger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestScopedWithLogger>();

            ICoreLogger logger = testScopedWithLogger.Logger;
            logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();

            using IDisposable? scopeString = logger.BeginScope("ScopeString");
            using IDisposable? scopeParams = logger.BeginScope("ScopeParams: {ParamInt} {ParamString}", 1, "ParamStringValue");
            logger.LogDebug("Test {MsgParamProp}", "MsgParam1Value");
        }

        [Fact]
        public void CoreTestApplicationIntegration_TestSingletonWithLoggerFactory()
        {
            this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            ITestSingletonWithLoggerFactory testScopedWithLoggerFactory = this.GetRequiredService<ITestSingletonWithLoggerFactory>();
            testScopedWithLoggerFactory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestSingletonWithLoggerFactory>();

            ICoreLogger logger = testScopedWithLoggerFactory.LoggerFactory.CreateCoreLogger();
            logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();

            using IDisposable? scopeString = logger.BeginScope("ScopeString");
            using IDisposable? scopeParams = logger.BeginScope("ScopeParams: {ParamInt} {ParamString}", 1, "ParamStringValue");
            logger.LogDebug("Logger 1 {MsgParamProp}", "MsgParam1Value");
            testScopedWithLoggerFactory.Logger.LogDebug("Logger 2 {MsgParamProp}", "MsgParam1Value");
        }
    }
}
