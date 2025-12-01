// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreStartupIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Startup;
using NetworkVisor.Core.Test.TestApp;
using NetworkVisor.Core.Test.TestStartup;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Logging.Extensions;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Startup
{
    /// <summary>
    /// Class CoreStartupIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreStartupIntegrationTests))]

    public class CoreStartupIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreStartupIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreStartupIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            AssertionConfiguration.Current.Formatting.MaxLines = 2000;
        }

        [Fact]
        public void StartupIntegration_StartupServices_Ctor()
        {
            this.TestStartupServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreStartupServices>();
            this.TestCaseServiceProvider.GetRequiredService<ICoreStartupServices>().Should().BeSameAs(this.TestStartupServices);
            this.TestDeviceStartupServices.Should().Be(CoreStartupServices.Instance);
        }

        [Fact]
        public void StartupIntegration_TestDeviceStartupServices_Mobile()
        {
            if (CoreTestAssemblyFixture.IsMobileTestApp)
            {
                this.TestDeviceStartupServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreStartupServices>();
                this.TestDeviceStartupServices.Should().Be(CoreStartupServices.Instance);
            }
        }

        [Fact]
        public void StartupIntegration_TestDeviceStartupServices()
        {
            this.TestDeviceStartupServices.Should().Be(CoreStartupServices.Instance);
        }

        [Fact]
        public void StartupIntegration_TestStartupServices_Ctor()
        {
            this.TestStartupServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestStartupServices>();
            this.TestCaseServiceProvider.GetRequiredService<ICoreTestStartupServices>().Should().BeSameAs(this.TestStartupServices);
        }

        [Fact]
        public void StartupIntegration_TestStartupServices_AppSettings()
        {
            this.TestStartupServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestStartupServices>();
            this.TestCaseServiceProvider.GetRequiredService<ICoreAppSettings>().Should().BeSameAs(this.AppSettings);

            var jsonString = JsonSerializer.Serialize(this.TestStartupServices.AppSettings, typeof(CoreAppSettings), CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));
            this.TestOutputHelper.WriteLine($"{"StartupServices AppSettings".CenterTitle()}\n{jsonString}");
        }

        [Fact]
        public void StartupIntegration_TestDeviceStartupServices_AppSettings()
        {
            this.TestDeviceStartupServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreStartupServices>();

            var jsonString = JsonSerializer.Serialize(this.TestDeviceStartupServices.AppSettings, typeof(CoreAppSettings), CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));
            this.TestOutputHelper.WriteLine($"{"TestDeviceStartupServices AppSettings".CenterTitle()}\n{jsonString}");
        }

        [Fact]
        public void StartupIntegration_TestStartupServices_AppFolderName()
        {
            this.TestStartupServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestStartupServices>();
            this.TestCaseServiceProvider.GetRequiredService<ICoreAppSettings>().Should().BeSameAs(this.AppSettings);

            this.TestOutputHelper.WriteLine($"{"AppFolderName".CenterTitle()}\n{this.TestStartupServices.AppSettings.AppFolderName}");

            this.TestStartupServices.AppSettings.AppFolderName.Should().Be(this.TestCaseServiceProvider.GetRequiredService<ICoreAppSettings>().AppFolderName);
            this.TestDeviceStartupServices.AppSettings.AppFolderName.Should().Be(CoreStartupServices.Instance.AppSettings.AppFolderName);
        }

        [Fact]
        public void StartupIntegration_TestStartupServices_AppSessionID()
        {
            this.TestStartupServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestStartupServices>();
            this.TestCaseServiceProvider.GetRequiredService<ICoreAppSettings>().Should().BeSameAs(this.AppSettings);

            this.TestOutputHelper.WriteLine($"{"AppSessionID".CenterTitle()}\n{this.TestStartupServices.AppSettings.AppSessionID}");

            this.TestStartupServices.AppSettings.AppSessionID.Should().Be(this.TestCaseServiceProvider.GetRequiredService<ICoreAppSettings>().AppSessionID);
            this.TestDeviceStartupServices.AppSettings.AppSessionID.Should().Be(CoreStartupServices.Instance.AppSettings.AppSessionID);
        }

        [Fact]
        public void StartupIntegration_TestStartupServices_AppSettings_SupportedNetworkServices()
        {
            this.TestStartupServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestStartupServices>();
            this.TestStartupServices.AppSettings.AppHostSettings
                .IsServiceSupported(this.TestStartupServices.AppSettings.AppHostSettings.SupportedNetworkServices)
                .Should().Be(this.TestNetworkingSystem.IsServiceSupported(this.TestStartupServices.AppSettings
                    .AppHostSettings.SupportedNetworkServices));
        }
    }
}
