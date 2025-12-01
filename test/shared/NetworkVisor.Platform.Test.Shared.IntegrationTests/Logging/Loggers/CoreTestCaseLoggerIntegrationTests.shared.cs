// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreTestCaseLoggerIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Factory;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.LogProvider;
using NetworkVisor.Core.Test.Logging.Factory;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests
{
    /// <summary>
    /// Class CoreTestCaseLoggerIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreTestCaseLoggerIntegrationTests))]
    public class CoreTestCaseLoggerIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestCaseLoggerIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Test fixture common across all tests.</param>
        public CoreTestCaseLoggerIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void TestCaseLoggerIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void TestCaseLoggerIntegration_CoreLoggerFactory_ILogger()
        {
            ICoreTestCaseLoggerFactory loggerFactory = this.CreateCoreTestCaseLoggerFactory(CoreAppConstants.GetMinimumLogLevel(), LoggerProviderFlags.Testing | LoggerProviderFlags.OpenTelemetry);

            ILogger logger = loggerFactory.CreateLogger(this.LoggerCategoryName);
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_DI_ILogger()
        {
            ILogger logger = this.GetTestCaseScopedRequiredService<ILogger>();
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_CoreLoggerFactory_ILogger_Type()
        {
            ICoreTestCaseLoggerFactory loggerFactory = this.CreateCoreTestCaseLoggerFactory(CoreAppConstants.GetMinimumLogLevel(), LoggerProviderFlags.Testing | LoggerProviderFlags.OpenTelemetry);

            ILogger<CoreTestCaseLoggerIntegrationTests> logger = loggerFactory.CreateLogger<CoreTestCaseLoggerIntegrationTests>();
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_DI_ILogger_Type()
        {
            ILogger<CoreTestCaseLoggerIntegrationTests> logger = this.GetTestCaseScopedRequiredService<ILogger<CoreTestCaseLoggerIntegrationTests>>();
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_CoreLoggerFactory_ICoreLogger()
        {
            ICoreTestCaseLoggerFactory loggerFactory = this.CreateCoreTestCaseLoggerFactory(CoreAppConstants.GetMinimumLogLevel(), LoggerProviderFlags.Testing | LoggerProviderFlags.OpenTelemetry);

            ICoreLogger logger = loggerFactory.CreateCoreLogger(this.LoggerCategoryName);
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_DI_ICoreLogger()
        {
            ICoreLogger logger = this.GetTestCaseScopedRequiredService<ICoreLogger>();
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_CoreLoggerFactory_ICoreLogger_Type()
        {
            ICoreTestCaseLoggerFactory loggerFactory = this.CreateCoreTestCaseLoggerFactory(CoreAppConstants.GetMinimumLogLevel(), LoggerProviderFlags.Testing | LoggerProviderFlags.OpenTelemetry);

            ICoreLogger<CoreTestCaseLoggerIntegrationTests> logger = loggerFactory.CreateCoreLogger<CoreTestCaseLoggerIntegrationTests>();
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_DI_ICoreLogger_Type()
        {
            ICoreLogger<CoreTestCaseLoggerIntegrationTests> logger = this.GetTestCaseScopedRequiredService<ICoreLogger<CoreTestCaseLoggerIntegrationTests>>();
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_DI_ICoreTestCaseLogger()
        {
            ICoreTestCaseLogger logger = this.GetTestCaseScopedRequiredService<ICoreTestCaseLogger>();
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_CoreLoggerFactory_ICoreTestCaseLogger_TestCase()
        {
            ICoreTestCaseLoggerFactory loggerFactory = this.CreateCoreTestCaseLoggerFactory(CoreAppConstants.GetMinimumLogLevel(), LoggerProviderFlags.Testing | LoggerProviderFlags.OpenTelemetry);

            ICoreTestCaseLogger logger = loggerFactory.CreateTestCaseLogger(this);
            logger.LogDebug("Test");
        }

        [Fact]
        public void TestCaseLoggerIntegration_ILogger_Scope()
        {
            ICoreTestCaseLoggerFactory loggerFactory = this.CreateCoreTestCaseLoggerFactory(CoreAppConstants.GetMinimumLogLevel(), LoggerProviderFlags.Testing | LoggerProviderFlags.OpenTelemetry);

            ILogger<CoreTestCaseLoggerIntegrationTests> logger = loggerFactory.CreateLogger<CoreTestCaseLoggerIntegrationTests>();

            using IDisposable? scopeString = logger.BeginScope("ScopeString");
            using IDisposable? scopeParams = logger.BeginScope("ScopeParams: {ParamInt} {ParamString}", 1, "ParamStringValue");
            logger.LogDebug("Test {MsgParamProp}", "MsgParam1Value");
        }

        [Fact]
        public void TestCaseLoggerIntegration_ICoreLogger_Scope()
        {
            ICoreTestCaseLoggerFactory loggerFactory = this.CreateCoreTestCaseLoggerFactory(CoreAppConstants.GetMinimumLogLevel(), LoggerProviderFlags.Testing | LoggerProviderFlags.OpenTelemetry);
            ICoreLogger logger = loggerFactory.CreateCoreLogger(this.LoggerCategoryName);

            using IDisposable? scopeString = logger.BeginScope("ScopeString");
            using IDisposable? scopeParams = logger.BeginScope("ScopeParams: {ParamInt} {ParamString}", 1, "ParamStringValue");
            logger.LogDebug("Test {MsgParamProp}", "MsgParam1Value");
        }

        [Fact]
        public void TestCaseLoggerIntegration_ICoreTestCaseLogger_IsNullLogger()
        {
            using ICoreTestCaseLogger testCaseLogger = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            testCaseLogger.Should().NotBeNull().And.BeAssignableTo<ICoreTestCaseLogger>();
            testCaseLogger.IsNullLogger.Should().BeFalse();
        }

        [Fact]
        public void TestCaseLoggerIntegration_ICoreTestCaseLogger_IsSilentLogger()
        {
            using ICoreTestCaseLogger testCaseLogger = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            testCaseLogger.Should().NotBeNull().And.BeAssignableTo<ICoreTestCaseLogger>();
            testCaseLogger.IsSilentLogger.Should().BeFalse();
        }

        [Fact]
        public void TestCaseLoggerIntegration_ICoreLogger_IsNullLogger()
        {
            ICoreLogger logger = this.GetTestCaseScopedRequiredService<ICoreLogger>();
            logger.Should().NotBeNull().And.BeAssignableTo<ICoreLogger>();
            logger.IsNullLogger.Should().BeFalse();
        }

        [Fact]
        public void TestCaseLoggerIntegration_ICoreLogger_IsSilentLogger()
        {
            ICoreLogger logger = this.GetTestCaseScopedRequiredService<ICoreLogger>();
            logger.Should().NotBeNull().And.BeAssignableTo<ICoreLogger>();
            logger.IsSilentLogger.Should().BeFalse();
        }

        private ICoreTestCaseLoggerFactory CreateCoreTestCaseLoggerFactory(LogLevel? logLevel = null, LoggerProviderFlags supportedLogProviders = LoggerProviderFlags.Testing)
        {
            logLevel ??= CoreAppConstants.GetMinimumLogLevel();
            return this.TestCaseLoggerFactory;
        }
    }
}
