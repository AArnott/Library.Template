// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreConfigurationIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NetworkVisor.Core.Configuration;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Settings
{
    /// <summary>
    /// Class NetCoreConfigurationIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreConfigurationIntegrationTests))]

    public class CoreConfigurationIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreConfigurationIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreConfigurationIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreConfigurationIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        /// <summary>
        /// Defines the test method  CoreConfigurationIntegration_AddExtractedAppSettings_Environment.
        /// </summary>
        /// <param name="hostEnvironment">Host environment to test.</param>
        /// <param name="expectedResult">Result to expect.</param>
        [Theory]
        [InlineData(CoreHostEnvironment.Default, "")]
        [InlineData(CoreHostEnvironment.Testing, "Testing")]
        [InlineData(CoreHostEnvironment.Development, "Development")]
        [InlineData(CoreHostEnvironment.Staging, "Staging")]
        [InlineData(CoreHostEnvironment.Production, "Production")]
        public void CoreConfigurationIntegration_AddExtractedAppSettings_Environment(CoreHostEnvironment hostEnvironment, string expectedResult)
        {
            var destFolderPath = Path.Combine(this.TestFileSystem.LocalUserAppTestArtifactsFolderPath, Guid.NewGuid().ToStringNoDashes());
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddExtractedAppSettingsFromAssemblyType<CoreConfigurationIntegrationTests>(destFolderPath, true, hostEnvironment)
                .Build();

            CoreHostEnvironment defaultBuildHostEnvironment = CoreEnvironmentSettings.DefaultBuildHostEnvironment;
            this.TestOutputHelper.WriteLine($"DefaultBuildHostEnvironment: {defaultBuildHostEnvironment}");

            // We don't copy development and testing configuration files for staging and production builds.
            if (hostEnvironment == CoreHostEnvironment.Default ||
                defaultBuildHostEnvironment == CoreHostEnvironment.Staging ||
                defaultBuildHostEnvironment == CoreHostEnvironment.Production)
            {
                expectedResult = defaultBuildHostEnvironment.ToString();
            }

            IConfigurationSection appSettings = config.GetSection(CoreAppConstants.AppSettingsPropertyName);
            appSettings.Should().NotBeNull();
            appSettings["AppFolderName"].Should().Be(ThisAssembly.AssemblyName);

            IConfigurationSection appHostSettings = config.GetSection(CoreAppConstants.AppHostSettingsPropertyName);
            appHostSettings["AppHostEnvironment"].Should().Be(expectedResult);
            appHostSettings["CloudHostEnvironment"].Should().Be(expectedResult);
        }

        [Fact]
        public void CoreConfigurationIntegration_AddExtractedAppSettings()
        {
            var destFolderPath = Path.Combine(this.TestFileSystem.LocalUserAppTestArtifactsFolderPath, Guid.NewGuid().ToStringNoDashes());
            IConfigurationRoot config = new ConfigurationBuilder()
                .AddExtractedAppSettingsFromAssemblyType<CoreConfigurationIntegrationTests>(destFolderPath, true, CoreHostEnvironment.Default)
                .Build();

            var expectedResult = CoreEnvironmentSettings.DefaultBuildHostEnvironment.ToString();
            config.Providers.Count().Should().Be(2);

            IConfigurationSection appSettings = config.GetSection(CoreAppConstants.AppSettingsPropertyName);
            appSettings.Should().NotBeNull();
            appSettings["AppFolderName"].Should().Be(ThisAssembly.AssemblyName);

            IConfigurationSection appHostSettings = config.GetSection(CoreAppConstants.AppHostSettingsPropertyName);
            appHostSettings["AppHostEnvironment"].Should().Be(expectedResult);
            appHostSettings["CloudHostEnvironment"].Should().Be(expectedResult);
        }

        [Theory]
        [InlineData(CoreHostEnvironment.Default)]
        [InlineData(CoreHostEnvironment.Development)]
        [InlineData(CoreHostEnvironment.Testing)]
        [InlineData(CoreHostEnvironment.Staging)]
        [InlineData(CoreHostEnvironment.Production)]
        public void CoreConfigurationIntegration_AddExtractedAppSettingsFromAssemblyType(CoreHostEnvironment hostEnvironment)
        {
            string appSessionID = Guid.NewGuid().ToStringNoDashes();
            var destFolderPath = Path.Combine(this.TestFileSystem.LocalUserAppTestArtifactsFolderPath, appSessionID);

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddExtractedAppSettingsFromAssemblyType<CoreConfigurationIntegrationTests>(destFolderPath, false, hostEnvironment)
                .Build();

            ICoreAppSettings? appSettings = config.CreateAndBindAppSettings(this.TestAssembly, appSessionID);
            appSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreAppSettings>();

            var jsonString = JsonSerializer.Serialize(appSettings, typeof(CoreAppSettings), CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));

            this.TestOutputHelper.WriteLine($"HostEnvironment: {hostEnvironment}\n{jsonString}");

            CoreHostEnvironment defaultBuildHostEnvironment = CoreEnvironmentSettings.DefaultBuildHostEnvironment;

            // We don't copy development and testing configuration files for staging and production builds.
            if (defaultBuildHostEnvironment.IsDevTestEnvironment() && hostEnvironment != CoreHostEnvironment.Default)
            {
                appSettings!.AppHostSettings.AppHostEnvironment.Should().Be(hostEnvironment);
                appSettings.AppHostSettings.CloudHostEnvironment.Should().Be(hostEnvironment);
            }
            else
            {
                // AddExtractedAppSettingsFromType will always set the AppHostEnvironment and CloudHostEnvironment to Build Host Environment.
                appSettings!.AppHostSettings.AppHostEnvironment.Should().Be(CoreEnvironmentSettings.DefaultBuildHostEnvironment);
                appSettings.AppHostSettings.CloudHostEnvironment.Should().Be(CoreEnvironmentSettings.DefaultBuildHostEnvironment);
            }
        }

        [Fact]
        public void CoreConfigurationIntegration_AddExtractedAppSettings_AppSettings()
        {
            var destFolderPath = Path.Combine(this.TestFileSystem.LocalUserAppTestArtifactsFolderPath, Guid.NewGuid().ToStringNoDashes());

            IConfigurationRoot config = new ConfigurationBuilder()
                .AddExtractedAppSettingsFromAssemblyType<CoreConfigurationIntegrationTests>(destFolderPath, true, CoreHostEnvironment.Default)
                .Build();

            ICoreAppSettings? appSettings = config.CreateAndBindAppSettings(this.TestAssembly);
            appSettings.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreAppSettings>();

            var jsonString = JsonSerializer.Serialize(appSettings, typeof(CoreAppSettings), CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider));

            this.TestOutputHelper.WriteLine(jsonString);

            appSettings.Should().NotBeNull();
            appSettings!.AppHostSettings.AppHostEnvironment.Should().Be(CoreEnvironmentSettings.DefaultBuildHostEnvironment);
            appSettings.AppHostSettings.CloudHostEnvironment.Should().Be(CoreEnvironmentSettings.DefaultBuildHostEnvironment);
            appSettings.AppFolderName.Should().Be(CoreAppSettings.GetAppFolderNameFromAssemblyName(this.TestAssemblyNamespace, appSettings.AppHostSettings.AppHostEnvironment));
        }
    }
}
