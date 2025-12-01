// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreAssemblyExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Assembly Extensions Unit Tests.</summary>
// ***********************************************************************

using System.Reflection;
using System.Runtime.Versioning;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CoreAssemblyExtensionsUnitTests. Assembly Extensions Unit Tests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAssemblyExtensionsUnitTests))]

    public class CoreAssemblyExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAssemblyExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAssemblyExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Gets or sets the test property value.
        /// </summary>
        public ICoreFrameworkInfo? FrameworkInfo { get; set; } = new CoreFrameworkInfo();

        [Fact]
        public void CoreAssemblyExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_AssemblyName.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_AssemblyName()
        {
            this.TestOutputHelper.WriteLine(typeof(CoreAssemblyExtensionsUnitTests).AssemblyQualifiedName);
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_TestRunner_FrameworkDescription.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_TestRunner_FrameworkDescription()
        {
            foreach (object? customAttribute in Assembly.GetExecutingAssembly()
                .GetCustomAttributes(typeof(TargetFrameworkAttribute), false))
            {
                if (customAttribute is TargetFrameworkAttribute targetFrameworkAttribute)
                {
                    this.TestOutputHelper.WriteLine($"Framework DisplayName: {targetFrameworkAttribute.FrameworkDisplayName}");
                    this.TestOutputHelper.WriteLine($"Framework FrameworkName: {targetFrameworkAttribute.FrameworkName}");
                    this.TestOutputHelper.WriteLine();
                }
            }
        }

        [Theory]
        [InlineData("", "")]
        [InlineData(".NETCoreApp,Version=v3.1", ".NET Core 3.1")]
        [InlineData(".NETStandard,Version=v2.0", ".NET Standard 2.0")]
        [InlineData(".NETStandard,Version=v2.1", ".NET Standard 2.1")]
        [InlineData(".NETCoreApp,Version=v5.0", ".NET 5.0")]
        [InlineData(".NETCoreApp,Version=v6.0", ".NET 6.0")]
        [InlineData(".NETCoreApp,Version=v7.0", ".NET 7.0")]
        [InlineData(".NETFramework,Version=v4.6", ".NET Framework 4.6")]
        [InlineData(".NETFramework,Version=v4.6.2", ".NET Framework 4.6.2")]
        [InlineData(".NETFramework,Version=v4.7", ".NET Framework 4.7")]
        [InlineData(".NETFramework,Version=v4.7.2", ".NET Framework 4.7.2")]

        public void AssemblyExtensions_ToFrameworkDisplayName(string frameworkName, string frameworkDisplayName)
        {
            var targetFramework = new TargetFrameworkAttribute(frameworkName);

            targetFramework.FrameworkName.Should().Be(frameworkName);
            targetFramework.FrameworkDisplayName.Should().BeNull();
            targetFramework.ToFrameworkDisplayName().Should().Be(frameworkDisplayName);
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_BuiltTargetFrameworkAttribute.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_BuiltTargetFrameworkAttribute()
        {
            TargetFrameworkAttribute? targetFrameworkAttribute = NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltTargetFrameworkAttribute;

            targetFrameworkAttribute.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Built Framework DisplayName: {targetFrameworkAttribute!.FrameworkDisplayName}");
            this.TestOutputHelper.WriteLine($"Built Framework FrameworkName: {targetFrameworkAttribute.FrameworkName}");
            this.TestOutputHelper.WriteLine();

            targetFrameworkAttribute.FrameworkName.Should().Be(NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkName);

            if (!string.IsNullOrEmpty(targetFrameworkAttribute.FrameworkDisplayName))
            {
                targetFrameworkAttribute.FrameworkDisplayName.Should().Be(NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkDisplayName);
            }
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_RuntimeTargetFrameworkAttribute.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_RuntimeTargetFrameworkAttribute()
        {
            TargetFrameworkAttribute? targetFrameworkAttribute = NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeTargetFrameworkAttribute;

            targetFrameworkAttribute.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Entry Framework DisplayName: {targetFrameworkAttribute!.FrameworkDisplayName}");
            this.TestOutputHelper.WriteLine($"Entry Framework FrameworkName: {targetFrameworkAttribute.FrameworkName}");
            this.TestOutputHelper.WriteLine();

            targetFrameworkAttribute.FrameworkName.Should().Be(NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName);

            if (!string.IsNullOrEmpty(targetFrameworkAttribute.FrameworkDisplayName))
            {
                targetFrameworkAttribute.FrameworkDisplayName.Should().Be(NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkDisplayName);
            }
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_RuntimeFrameworkDisplayName.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_RuntimeFrameworkDisplayName()
        {
            this.TestOutputHelper.WriteLine($"Runtime Framework DisplayName: {NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkDisplayName}");
            NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkDisplayName.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_BuiltFrameworkDisplayName.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_BuiltFrameworkDisplayName()
        {
            this.TestOutputHelper.WriteLine($"Built Framework DisplayName: {NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkDisplayName}");
            NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkDisplayName.Should().NotBeNullOrWhiteSpace();
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_RuntimeFrameworkName.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_RuntimeFrameworkName()
        {
            // Resharper uses NetFramework 4.6.2, otherwise use NetCoreApp 3.0
            if (CoreTestConstants.IsReSharperTestRunner)
            {
#if NET472_OR_GREATER
                // Resharper uses NetFramework4.6.2
                NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName.Should().Be(".NETFramework,Version=v4.6.2");
#else
                // Resharper uses NetCoreApp3.1
                NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName.Should().Be(".NETCoreApp,Version=v3.0");
#endif
            }
            else
            {
#if NV_PLAT_ANDROID
                NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName.Should().Be(".NETFramework,Version=v4.6.2");
#elif NET472_OR_GREATER
                NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName.Should().Be(".NETFramework,Version=v4.7.2");
#elif NET10_0
                NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName.Should().Be(".NETCoreApp,Version=v10.0");
#elif NET9_0
                NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName.Should().Be(".NETCoreApp,Version=v9.0");

#elif NET8_0
                NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName.Should().Be(".NETCoreApp,Version=v8.0");
#elif NET7_0
                NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName.Should().Be(".NETCoreApp,Version=v7.0");
#elif NET6_0
                NetworkVisor.Core.Extensions.CoreAssemblyExtensions.RuntimeFrameworkName.Should().Be(".NETCoreApp,Version=v6.0");
#else
#error Framework is undedfined
#endif
            }
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_BuildFrameworkName.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_BuiltFrameworkName()
        {
#if NET6_0
            NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkName.Should().Be(".NETCoreApp,Version=v6.0");
#elif NET7_0
            NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkName.Should().Be(".NETCoreApp,Version=v7.0");
#elif NET8_0
            NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkName.Should().Be(".NETCoreApp,Version=v8.0");
#elif NET9_0
            NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkName.Should().Be(".NETCoreApp,Version=v9.0");
#elif NET10_0
            NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkName.Should().Be(".NETCoreApp,Version=v10.0");
#elif NETSTANDARD2_0_OR_GREATER || NET472_OR_GREATER
            NetworkVisor.Core.Extensions.CoreAssemblyExtensions.BuiltFrameworkName.Should().Be(".NETStandard,Version=v2.0");
#else
#error Framework is undedfined
#endif
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_GetNamespace_Null.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_GetNamespace_Null()
        {
            Func<string> fx = () => NetworkVisor.Core.Extensions.CoreAssemblyExtensions.GetNamespace(null);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("assembly");
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_ExtractEmbeddedResource.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_ExtractEmbeddedResource()
        {
            var configFileName = FileExtensions.GetRandomFileName($"{CoreAppConstants.DefaultAppSettingsFileNameRoot}.", ".json");
            var configFile = CoreAssemblyExtensions.ExtractEmbeddedResourceFromType<CoreAssemblyExtensionsUnitTests>($"Settings.{CoreAppConstants.DefaultAppSettingsFileName}", this.TestFileSystem.LocalUserAppSettingsFolderPath, configFileName);
            configFile.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine(configFile);
            this.TestFileSystem.FileExists(configFile).Should().BeTrue();

            var configFileSharedName = FileExtensions.GetRandomFileName($"{CoreAppConstants.DefaultSharedSettingsFileNameRoot}.", ".json");
            var configFileShared = CoreAssemblyExtensions.ExtractEmbeddedResourceFromType<CoreAssemblyExtensionsUnitTests>($"Settings.{CoreAppConstants.DefaultSharedSettingsFileName}", this.TestFileSystem.LocalUserAppSettingsFolderPath, configFileSharedName);
            configFileShared.Should().NotBeNullOrEmpty();
            this.TestFileSystem.FileExists(configFileShared).Should().BeTrue();
            this.TestOutputHelper.WriteLine(configFileShared);

            // Test Configuration Shared then Application
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile(configFileShared)
                .AddJsonFile(configFile)
                .Build();

            ICoreAppSettings appSettings = CoreAppSettings.CreateFromAssemblyOrDefault(this.TestAssembly, null, CoreHostEnvironment.Testing);

            // Validate AppSettings
            configuration[CoreAppConstants.SchemaVersionPropertyName].Should().Be(appSettings.SchemaVersion.ToString());

            configuration[CoreAppConstants.AppFolderNamePropertyName].Should().Be(this.TestAssemblyNamespace);
            configuration[CoreAppConstants.AppGuidPropertyName].Should().Be(appSettings.AppGuid.ToString());
            configuration[CoreAppConstants.AppPackageNamePropertyName].Should().Be(appSettings.AppPackageName);
            configuration[CoreAppConstants.AppNamePropertyName].Should().Be(appSettings.AppName);

            configuration[CoreAppConstants.AppHostEnvironmentPropertyName].Should().Be(appSettings.AppHostSettings.AppHostEnvironment.ToString());
            configuration[CoreAppConstants.CloudHostEnvironmentPropertyName].Should().Be(appSettings.AppHostSettings.CloudHostEnvironment.ToString());

            configuration[CoreAppConstants.MinimumLogLevelPropertyName].Should().Be(appSettings.AppLoggingSettings.MinimumLogLevel.ToString());
            configuration[CoreAppConstants.LoggerProvidersPropertyName].Should().Be(appSettings.AppLoggingSettings.LoggerProviders.ToString());
            configuration[CoreAppConstants.OpenTelemetryConnectionStringPropertyName].Should().Be(appSettings.AppLoggingSettings.OpenTelemetryConnectionString);

            // Clean up
            this.TestFileSystem.WaitToDeleteLockedFile(configFile);
            this.TestFileSystem.WaitToDeleteLockedFile(configFileShared);
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_ExtractEmbeddedResource_ResourceName_Null.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_ExtractEmbeddedResource_ResourceName_Null()
        {
            var configFileName = FileExtensions.GetRandomFileName($"{CoreAppConstants.DefaultAppSettingsFileNameRoot}.", ".json");
            Func<string> fx = () =>
                CoreAssemblyExtensions.ExtractEmbeddedResourceFromType<CoreAssemblyExtensionsUnitTests>(
                    null!,
                    this.TestFileSystem.LocalUserAppSettingsFolderPath,
                    configFileName);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("resourceName");
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_ExtractEmbeddedResource_DestFolderPath_Null.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_ExtractEmbeddedResource_DestFolderPath_Null()
        {
            var configFileName = FileExtensions.GetRandomFileName($"{CoreAppConstants.DefaultAppSettingsFileNameRoot}.", ".json");
            Func<string> fx = () =>
                CoreAssemblyExtensions.ExtractEmbeddedResourceFromType<CoreAssemblyExtensionsUnitTests>($"Settings.{CoreAppConstants.DefaultAppSettingsFileName}", null!, configFileName);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("destFolderPath");
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_ExtractEmbeddedResource_DestFileName_Null.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_ExtractEmbeddedResource_DestFileName_Null()
        {
            Func<string> fx = () =>
                CoreAssemblyExtensions.ExtractEmbeddedResourceFromType<CoreAssemblyExtensionsUnitTests>($"Settings.{CoreAppConstants.DefaultAppSettingsFileName}", this.TestFileSystem.LocalUserAppSettingsFolderPath, null!);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("destFileName");
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_ExtractEmbeddedResource_BadResourceName.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_ExtractEmbeddedResource_BadResourceName()
        {
            var configFileName = FileExtensions.GetRandomFileName($"{CoreAppConstants.DefaultAppSettingsFileNameRoot}.", ".json");
            CoreAssemblyExtensions.ExtractEmbeddedResourceFromType<CoreAssemblyExtensionsUnitTests>("@", this.TestFileSystem.LocalUserAppSettingsFolderPath, configFileName).Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_ExtractEmbeddedResource_BadDestFileName.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_ExtractEmbeddedResource_BadDestFileName()
        {
            CoreAssemblyExtensions.ExtractEmbeddedResourceFromType<CoreAssemblyExtensionsUnitTests>($"Settings.{CoreAppConstants.DefaultAppSettingsFileName}", "/", "/")
                .Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_ReadFileContentsAsync_AppSettings.
        /// </summary>
        [Fact]
        public async Task AssemblyExtensions_ReadFileContentsAsync_AppSettings()
        {
            var configFileName = FileExtensions.GetRandomFileName($"{CoreAppConstants.DefaultAppSettingsFileNameRoot}.", ".json");
            var configFile = CoreAssemblyExtensions.ExtractEmbeddedResourceFromType<CoreAssemblyExtensionsUnitTests>($"Settings.{CoreAppConstants.DefaultAppSettingsFileName}", this.TestFileSystem.LocalUserAppSettingsFolderPath, configFileName);
            configFile.Should().NotBeNullOrEmpty();
            this.TestFileSystem.FileExists(configFile).Should().BeTrue();

            var fileContents = await this.TestFileSystem.ReadFileContentsAsync(configFile);
            fileContents.Should().NotBeNullOrEmpty();
            fileContents.Should().Contain("AppSettings");
            this.TestOutputHelper.WriteLine(fileContents);
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_GetManifestResourceNames.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_GetManifestResourceNames_Output()
        {
            Assembly assembly = typeof(CoreAssemblyExtensionsUnitTests).Assembly;

            foreach (var manifestResourceName in CoreAssemblyExtensions.GetManifestResourceNames<CorePlatformAssemblyTypeExtensionsUnitTests>().ToList())
            {
                string fullResourceName = $"{assembly.GetNamespace()}.{manifestResourceName}";
                using Stream? resFileStream = assembly.GetManifestResourceStream(fullResourceName) ?? assembly.GetManifestResourceStream(manifestResourceName);
                resFileStream.Should().NotBeNull();
                this.TestOutputHelper.WriteLine(manifestResourceName.CenterTitle('*', '*', 4));

                using (StreamReader reader = new StreamReader(resFileStream!))
                {
                    this.TestOutputHelper.WriteLine(reader.ReadToEnd());
                }

                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method AssemblyExtensions_GetManifestResourceNames.
        /// </summary>
        [Fact]
        public void AssemblyExtensions_GetManifestResourceNames()
        {
            var manifestResourceNames = CoreAssemblyExtensions.GetManifestResourceNames<CorePlatformAssemblyTypeExtensionsUnitTests>().ToList();

            manifestResourceNames.Should().ContainMatch($"*Settings.{CoreAppConstants.DefaultAppSettingsFileName}");
            manifestResourceNames.Should().ContainMatch($"*Settings.{CoreAppConstants.DefaultSharedSettingsFileName}");
        }
    }
}
