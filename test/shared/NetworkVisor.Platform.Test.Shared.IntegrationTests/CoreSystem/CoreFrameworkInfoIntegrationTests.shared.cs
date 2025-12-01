// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreFrameworkInfoIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.CoreSystem
{
    /// <summary>
    /// Class CoreFrameworkInfoIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreFrameworkInfoIntegrationTests))]

    public class CoreFrameworkInfoIntegrationTests : CoreTestCaseBase
    {
        private readonly ICoreFrameworkInfo frameworkInfo;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreFrameworkInfoIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreFrameworkInfoIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.frameworkInfo = new CoreFrameworkInfo();
        }

        [Fact]
        public void CoreFrameworkInfoIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void CoreFrameworkInfoIntegration_OperatingSystem_Ctor()
        {
            this.TestOperatingSystem.Should().NotBeNull().And.BeOfType<CoreOperatingSystem>();
        }

        [Fact]
        public void CoreFrameworkInfoIntegration_OperatingSystem_FrameworkInfo()
        {
            this.TestOperatingSystem.FrameworkInfo.Should().NotBeNull().And.BeAssignableTo<ICoreFrameworkInfo>();
            this.TestOutputHelper.WriteLine($"FrameworkInfo: {this.TestOperatingSystem.FrameworkInfo.ToStringWithPropNameMultiLine()}");
        }

        [Fact]
        public void CoreFrameworkInfoIntegration_BuiltFrameworkName()
        {
            this.frameworkInfo.BuiltFrameworkName.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Built Framework Name: {this.frameworkInfo.BuiltFrameworkName}");
        }

        [Fact]
        public void CoreFrameworkInfoIntegration_BuiltFrameworkVersion()
        {
            this.frameworkInfo.BuiltFrameworkVersion.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Built Framework Version: {this.frameworkInfo.BuiltFrameworkVersion}");

            this.frameworkInfo.BuiltFrameworkVersion!.Major.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CoreFrameworkInfoIntegration_BuiltFrameworkType()
        {
            this.TestOutputHelper.WriteLine($"Built Framework Type: {this.frameworkInfo.BuiltFrameworkType}");

            this.frameworkInfo.BuiltFrameworkType.Should().NotBe(CoreFrameworkType.Unknown);
        }

        [Fact]
        public void CoreFrameworkInfoIntegration_RuntimeFrameworkName()
        {
            this.frameworkInfo.RuntimeFrameworkName.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Runtime Framework Name: {this.frameworkInfo.RuntimeFrameworkName}");
        }

        [Fact]
        public void CoreFrameworkInfoIntegration_RuntimeFrameworkVersion()
        {
            this.frameworkInfo.RuntimeFrameworkVersion.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Runtime Framework Version: {this.frameworkInfo.RuntimeFrameworkVersion}");

            this.frameworkInfo.RuntimeFrameworkVersion!.Major.Should().BeGreaterThan(0);
        }

        [Fact]
        public void CoreFrameworkInfoIntegration_RuntimeFrameworkType()
        {
            this.TestOutputHelper.WriteLine($"Runtime Framework Type: {this.frameworkInfo.RuntimeFrameworkType}");

            this.frameworkInfo.RuntimeFrameworkType.Should().NotBe(CoreFrameworkType.Unknown);
        }

        [Fact]
        [ExcludeFromCodeCoverage]
        public void CoreFrameworkInfoIntegration_RuntimeFrameworkCheck()
        {
            Version versionExpected;
            string frameworkNameExpected;
            CoreFrameworkType frameworkTypeExpected = CoreFrameworkType.NetCoreApp;

            // Resharper uses NetFramework 4.6.2, otherwise use NetCoreApp 3.0
            if (CoreTestConstants.IsReSharperTestRunner)
            {
#if NET472_OR_GREATER
                // Resharper uses NetFramework4.6.2
                versionExpected = new Version(4, 6, 2);
                frameworkNameExpected = ".NETFramework,Version=v4.6.2";
                frameworkTypeExpected = CoreFrameworkType.NetFramework;
#else
                // ReSharper uses NetCoreApp3.0
                versionExpected = new Version(3, 0);
                frameworkNameExpected = ".NETCoreApp,Version=v3.0";
#endif
            }
            else
            {
                frameworkTypeExpected = CoreFrameworkType.NetCore;
#if NV_PLAT_ANDROID
                // Test runtime for Android uses NetFramework 4.6.2
                versionExpected = new Version(4, 6, 2);
                frameworkNameExpected = ".NETFramework,Version=v4.6.2";
                frameworkTypeExpected = CoreFrameworkType.NetFramework;
#elif NET472_OR_GREATER
                versionExpected = new Version(4, 7, 2);
                frameworkNameExpected = ".NETFramework,Version=v4.7.2";
                frameworkTypeExpected = CoreFrameworkType.NetFramework;
#elif NET10_0
                versionExpected = new Version(10, 0);
                frameworkNameExpected = ".NETCoreApp,Version=v10.0";
#elif NET9_0
                versionExpected = new Version(9, 0);
                frameworkNameExpected = ".NETCoreApp,Version=v9.0";
#elif NET8_0
                versionExpected = new Version(8, 0);
                frameworkNameExpected = ".NETCoreApp,Version=v8.0";
#elif NET7_0
                versionExpected = new Version(7, 0);
                frameworkNameExpected = ".NETCoreApp,Version=v7.0";
#elif NET6_0
                versionExpected = new Version(6, 0);
                frameworkNameExpected = ".NETCoreApp,Version=v6.0";
#else
                versionExpected = new Version(3, 1);
                frameworkNameExpected = ".NETCoreApp,Version=v3.1";
#endif
            }

            this.frameworkInfo.RuntimeFrameworkName.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Runtime Framework Name: {this.frameworkInfo.RuntimeFrameworkName}");
            this.frameworkInfo.RuntimeFrameworkName.Should().Be(frameworkNameExpected);

            this.frameworkInfo.RuntimeFrameworkVersion.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Runtime Framework Version: {this.frameworkInfo.RuntimeFrameworkVersion}");
            this.frameworkInfo.RuntimeFrameworkVersion!.Major.Should().Be(versionExpected.Major);
            this.frameworkInfo.RuntimeFrameworkVersion!.Minor.Should().Be(versionExpected.Minor);
            this.frameworkInfo.RuntimeFrameworkVersion!.Build.Should().Be(versionExpected.Build);
            this.frameworkInfo.RuntimeFrameworkVersion!.Revision.Should().Be(versionExpected.Revision);

            this.TestOutputHelper.WriteLine($"Runtime Framework Type: {this.frameworkInfo.RuntimeFrameworkType}");
            this.frameworkInfo.RuntimeFrameworkType.Should().Be(frameworkTypeExpected);

#pragma warning disable CA1508 // Avoid dead conditional code
            this.frameworkInfo.IsRunningNetCoreApp.Should().Be(frameworkTypeExpected == CoreFrameworkType.NetCoreApp);
            this.frameworkInfo.IsRunningNetFramework.Should().Be(frameworkTypeExpected == CoreFrameworkType.NetFramework);
            this.frameworkInfo.IsRunningNetCore.Should().Be(frameworkTypeExpected == CoreFrameworkType.NetCore);
#pragma warning restore CA1508 // Avoid dead conditional code
        }
    }
}
