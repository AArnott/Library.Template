// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreAppIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreApp;
using NetworkVisor.Core.CoreApp.Context;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Startup;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.CoreApp
{
    /// <summary>
    /// Class CoreAppIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreAppIntegrationTests))]

    public class CoreAppIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAppIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAppIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method AppIntegration_Ctor.
        /// </summary>
        [Fact]
        public void AppIntegration_Ctor()
        {
            // Force the creation of the lazy CoreApplication instance
            _ = this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreApplication>();
        }

        [Fact]
        public void AppIntegration_ApplicationContext()
        {
            // Force the creation of the lazy CoreApplication instance
            _ = this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreApplication>();

            ICoreContext appContext = this.TestApplication.ApplicationContext;
            _ = appContext.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreContext>();

            _ = appContext.ServiceProvider.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IServiceProvider>();
            _ = appContext.ServiceProvider.Should().BeSameAs(this.TestAppServiceProvider);
        }

        [Fact]
        public void AppIntegration_GlobalLogLevel()
        {
            // Force the creation of the lazy CoreApplication instance
            _ = this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreApplication>();

            _ = this.TestApplication.GlobalLogLevel.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreGlobalLogLevel>();
            _ = this.TestApplication.GlobalLogLevel.Should().BeSameAs(this.TestCaseServiceProvider.GetRequiredService<ICoreGlobalLogLevel>());
        }

        [Fact]
        public void AppIntegration_GlobalLogger()
        {
            // Force the creation of the lazy CoreApplication instance
            _ = this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreApplication>();

            _ = this.TestApplication.GlobalLogger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreGlobalLogger>();
            _ = this.TestApplication.GlobalLogger.Should().BeSameAs(this.TestCaseServiceProvider.GetRequiredService<ICoreGlobalLogger>());
            this.TestApplication.GlobalLogger.LogDebug($"Test of Global Logger ({this.TestApplication.GlobalLogger.CategoryName})");
            _ = this.TestApplication.GlobalLogger.IsNullLogger.Should().BeFalse();
            _ = this.TestApplication.GlobalLogger.CategoryName.Should().Be(this.TestAssemblyNamespace);
        }

        [Fact]
        public void AppIntegration_ApplicationAssembly()
        {
            // Force the creation of the lazy CoreApplication instance
            _ = this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreApplication>();

            _ = this.TestApplication.ApplicationAssembly.Should().NotBeNull().And.Subject.Should().BeAssignableTo<Assembly>();
            _ = this.TestApplication.ApplicationAssembly.Should().BeSameAs(this.TestAssembly);
        }

        [Fact]
        public void AppIntegration_StartupServices()
        {
            // Force the creation of the lazy CoreApplication instance
            _ = this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreApplication>();

            _ = this.TestApplication.StartupServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreStartupServices>();
            _ = this.TestApplication.StartupServices.Should().BeSameAs(this.TestStartupServices);
        }

        [Fact]
        public void AppIntegration_PopulateTestServices()
        {
            var services = new ServiceCollection();
            this.PopulateTestServices(services);

            IServiceProvider serviceProvider = services.BuildServiceProvider();

            foreach (ServiceDescriptor service in this.TestServiceCollection)
            {
                this.TestOutputHelper.WriteLine(service.ToString());

                if (service.ServiceType.IsGenericType)
                {
                    this.TestOutputHelper.WriteLine($"*** Skipping comparison of generic type {service.ServiceType.FullName!} ***");
                    continue;
                }

                _ = serviceProvider.GetRequiredService(service.ServiceType).Should().NotBeNull().And.Subject.Should().BeAssignableTo(service.ServiceType);
            }
        }

        [Fact]
        public void AppIntegration_Configuration()
        {
            // Force the creation of the lazy CoreApplication instance
            _ = this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreApplication>();

            _ = this.TestApplication.Configuration.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IConfiguration>();
            _ = this.TestApplication.Configuration.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IConfigurationManager>();
            _ = this.TestApplication.Configuration.Should().BeSameAs(this.Configuration);
        }

        [Fact]
        public void AppIntegration_ServiceProvider()
        {
            // Force the creation of the lazy CoreApplication instance
            _ = this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreApplication>();

            _ = this.TestApplication.ServiceProvider.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IServiceProvider>();
            _ = this.TestApplication.ServiceProvider.Should().BeSameAs(this.TestAppServiceProvider);
            _ = this.TestApplication.ServiceProvider.GetRequiredService<ICoreFileSystem>().Should().BeSameAs(this.TestAppServiceProvider.GetRequiredService<ICoreFileSystem>());
            _ = this.TestApplication.ServiceProvider.GetRequiredService<ICoreFileSystem>().Should().BeSameAs(this.TestCaseServiceProvider.GetRequiredService<ICoreFileSystem>());
            _ = this.TestApplication.ServiceProvider.GetRequiredService<ICoreNetworkServices>().Should().BeSameAs(this.TestAppServiceProvider.GetRequiredService<ICoreNetworkServices>());
            _ = this.TestApplication.ServiceProvider.GetRequiredService<ICoreNetworkServices>().Should().BeSameAs(this.TestCaseServiceProvider.GetRequiredService<ICoreNetworkServices>());
        }

        [Fact]
        public void AppIntegration_ServiceProvider_Singletons()
        {
            // Force the creation of the lazy CoreApplication instance
            _ = this.TestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreApplication>();

            _ = this.TestApplication.ServiceProvider.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IServiceProvider>();

            ICoreFileSystem fileSystem1 = this.TestApplication.ServiceProvider.GetRequiredService<ICoreFileSystem>();
            _ = fileSystem1.Should().BeSameAs(this.TestApplication.ServiceProvider.GetRequiredService<ICoreFileSystem>());
            _ = fileSystem1.Should().BeSameAs(this.TestCaseServiceProvider.GetRequiredService<ICoreFileSystem>());

            ICoreNetworkingSystem networkingSystem1 = this.TestApplication.ServiceProvider.GetRequiredService<ICoreNetworkingSystem>();
            _ = networkingSystem1.Should().BeSameAs(this.TestApplication.ServiceProvider.GetRequiredService<ICoreNetworkingSystem>());
            _ = networkingSystem1.Should().BeSameAs(this.TestCaseServiceProvider.GetRequiredService<ICoreNetworkingSystem>());

            ICoreGlobalLogger globalLogger1 = this.TestApplication.ServiceProvider.GetRequiredService<ICoreGlobalLogger>();
            _ = globalLogger1.Should().BeSameAs(this.TestApplication.ServiceProvider.GetRequiredService<ICoreGlobalLogger>());
            _ = globalLogger1.Should().BeSameAs(this.TestCaseServiceProvider.GetRequiredService<ICoreGlobalLogger>());
        }
    }
}
