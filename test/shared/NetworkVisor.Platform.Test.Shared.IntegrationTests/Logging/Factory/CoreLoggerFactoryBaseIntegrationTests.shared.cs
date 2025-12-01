// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 08-11-2021
//
// Last Modified By : SteveBu
// Last Modified On : 08-11-2021
// ***********************************************************************
// <copyright file="CoreLoggerFactoryBaseIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Logging.Factory;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.Loggers;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Logging.Loggers
{
    /// <summary>
    /// Class CoreLoggerFactoryIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLoggerFactoryBaseIntegrationTests))]

    public class CoreLoggerFactoryBaseIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLoggerFactoryBaseIntegrationTests"/> class.
        /// Initializes a new instance of the <see cref="CoreLoggerFactoryBaseIntegrationTests"/> test class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLoggerFactoryBaseIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void LoggerFactoryBase_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void LoggerFactorBase_Type()
        {
            this.TestCaseLoggerFactory.Should().NotBeNull().And.BeOfType(typeof(CoreTestCaseLoggerFactory));
        }

        [Fact]
        public void LoggerFactorBase_WrappedLoggerFactory()
        {
            this.TestCaseLoggerFactory.WrappedLoggerFactory.Should().NotBeNull().And.BeAssignableTo<ILoggerFactory>();
            this.TestCaseLoggerFactory.WrappedLoggerFactory.Should().BeSameAs(this.TestAppServiceProvider.GetRequiredService<ICoreGlobalLoggerFactory>().WrappedLoggerFactory);
            ILogger<CoreLoggerFactoryBaseIntegrationTests> logger = this.TestCaseLoggerFactory.WrappedLoggerFactory.CreateLogger<CoreLoggerFactoryBaseIntegrationTests>();
            logger.Should().NotBeNull().And.BeAssignableTo<ILogger>();
            logger.LogInformation("Test LogInformation");
        }

        [Fact]
        public void LoggerFactorBase_Logger()
        {
            using ICoreTestCaseLogger testCaseLogger = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            testCaseLogger.Should().NotBeNull().And.BeAssignableTo<ICoreTestCaseLogger>();
        }

        [Fact]
        public void LoggerFactorBase_TestCaseLogger_SameWrappedLogger()
        {
            using ICoreTestCaseLogger testCaseLogger1 = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            using ICoreTestCaseLogger testCaseLogger2 = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            testCaseLogger1.Should().NotBeNull().And.BeAssignableTo<ICoreTestCaseLogger>();
            testCaseLogger2.Should().NotBeNull().And.BeAssignableTo<ICoreTestCaseLogger>();
            testCaseLogger1.WrappedLogger.Should().BeSameAs(testCaseLogger2.WrappedLogger);
        }

        [Fact]
        public void LoggerFactorBase_Logger_IsNullLogger()
        {
            using ICoreTestCaseLogger testCaseLogger = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            testCaseLogger.Should().NotBeNull().And.BeAssignableTo<ICoreTestCaseLogger>();
            testCaseLogger.IsNullLogger.Should().BeFalse();
        }

        [Fact]
        public void LoggerFactorBase_Logger_IsSilentLogger()
        {
            using ICoreTestCaseLogger testCaseLogger = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            testCaseLogger.Should().NotBeNull().And.BeAssignableTo<ICoreTestCaseLogger>();
            testCaseLogger.IsSilentLogger.Should().BeFalse();
        }

        [Fact]
        public void LoggerFactorBase_LogInformation()
        {
            using ICoreTestCaseLogger testCaseLogger = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            testCaseLogger.LogInformation("Information Log");
        }

        [Fact]
        public void LoggerFactorBase_LogInformation_Destructured()
        {
            using ICoreTestCaseLogger testCaseLogger = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            testCaseLogger.LogInformation("Information Log {@TestClass}", new TestClass("Ten", 10));
        }

        [Fact]
        public void LoggerFactorBase_LogInformation_Stringify()
        {
            using ICoreTestCaseLogger testCaseLogger = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            testCaseLogger.LogInformation("Information Log {$TestClass}", new TestClass("Ten", 10));
        }

        [Fact]
        public void LoggerFactorBase_LogInformation_Tuple()
        {
            using ICoreTestCaseLogger testCaseLogger = this.TestCaseLoggerFactory.CreateTestCaseLogger(this);
            var sensorInput = new { Latitude = 25, Longitude = 134 };
            testCaseLogger.LogInformation("Processing {@SensorInput}", sensorInput);
        }

        [Fact]
        public void LoggerFactorBase_AppAssembly()
        {
            this.TestCaseLoggerFactory.AppAssembly.Should().BeSameAs(this.TestAssembly);
        }

        [Fact]
        public void LoggerFactorBase_GlobalLoggerFactory()
        {
            this.TestCaseServiceProvider.GetRequiredService<ICoreGlobalLoggerFactory>().Should().NotBeNull().And.BeAssignableTo<ICoreGlobalLoggerFactory>();
            this.TestCaseServiceProvider.GetRequiredService<ICoreGlobalLoggerFactory>().Should().BeSameAs(this.TestCaseLoggerFactory);
            this.TestCaseServiceProvider.GetRequiredService<ICoreLoggerFactory>().Should().BeSameAs(this.TestCaseLoggerFactory);
            this.TestCaseServiceProvider.GetRequiredService<ILoggerFactory>().Should().BeSameAs(this.TestCaseLoggerFactory.WrappedLoggerFactory);
        }

        [Fact]
        public void LoggerFactorBase_GlobalLogLevel()
        {
            this.TestCaseLoggerFactory.GlobalLogLevel.Should().BeSameAs(this.TestApplication.GlobalLogLevel);
            this.TestCaseLoggerFactory.GlobalLogLevel.Should().BeSameAs(this.TestCaseServiceProvider.GetRequiredService<ICoreGlobalLogLevel>());
            this.TestCaseLoggerFactory.GlobalLogLevel.Current.Should().Be(this.AppSettings.AppLoggingSettings.MinimumLogLevel);
            this.TestCaseLoggerFactory.GlobalLogLevel.Default.Should().Be(this.AppSettings.AppLoggingSettings.MinimumLogLevel);
        }

        [Fact]
        public void LoggerFactorBase_GlobalLogger()
        {
            this.TestCaseLoggerFactory.GlobalLogger.Should().NotBeNull().And.BeAssignableTo<ICoreGlobalLogger>();
            this.TestCaseLoggerFactory.GlobalLogger.Should().BeSameAs(this.TestApplication.GlobalLogger);
            this.TestCaseLoggerFactory.GlobalLogger.IsNullLogger.Should().BeFalse();
            this.TestCaseLoggerFactory.GlobalLogger.IsSilentLogger.Should().BeFalse();
            this.TestCaseLoggerFactory.GlobalLogger.CategoryName.Should().Be(this.TestAssemblyNamespace);
            this.TestCaseLoggerFactory.GlobalLogger.LogDebug($"Test of Global {this.TestCaseLoggerFactory.GlobalLogLevel.Current} Logger [{this.TestCaseLoggerFactory.GlobalLogger.CategoryName}]");

            this.TestCaseLoggerFactory.GlobalLogger.LoggerLogLevel.Should().BeSameAs(this.TestCaseLoggerFactory.GlobalLogLevel);
            this.TestCaseLoggerFactory.GlobalLogger.LoggerLogLevel.Current.Should().Be(this.AppSettings.AppLoggingSettings.MinimumLogLevel);
            this.TestCaseLoggerFactory.GlobalLogger.LoggerLogLevel.Default.Should().Be(this.AppSettings.AppLoggingSettings.MinimumLogLevel);

            this.TestCaseLoggerFactory.GlobalLogger.IsEnabled(LogLevel.Critical).Should().BeTrue();
            this.TestCaseLoggerFactory.GlobalLogger.IsEnabled(LogLevel.Debug).Should().BeTrue();
            this.TestCaseLoggerFactory.GlobalLogger.IsEnabled(LogLevel.Trace).Should().BeFalse();
        }

        [Fact]
        public void LoggerFactorBase_ServiceProvider()
        {
            this.TestCaseLoggerFactory.ServiceProvider.Should().NotBeNull().And.BeAssignableTo<IServiceProvider>();
            this.TestCaseLoggerFactory.ServiceProvider.GetRequiredService<ICoreFileSystem>().Should().NotBeNull().And.BeAssignableTo<ICoreFileSystem>().And.BeSameAs(this.TestFileSystem);
            this.TestCaseLoggerFactory.ServiceProvider.GetRequiredService<ICoreNetworkingSystem>().Should().NotBeNull().And.BeAssignableTo<ICoreNetworkingSystem>().And.BeSameAs(this.TestNetworkingSystem);
            this.TestCaseLoggerFactory.ServiceProvider.GetRequiredService<ICoreNetworkServices>().Should().NotBeNull().And.BeAssignableTo<ICoreNetworkServices>().And.BeSameAs(this.TestNetworkServices);
        }

        public class TestClass
        {
            public TestClass(string stringName, int intName)
            {
                this.StringName = stringName;
                this.IntName = intName;
            }

            public string StringName { get; protected set; }

            public int IntName { get; protected set; }

            public override string ToString()
            {
                return $"{this.StringName}: {this.IntName}";
            }
        }
    }
}
