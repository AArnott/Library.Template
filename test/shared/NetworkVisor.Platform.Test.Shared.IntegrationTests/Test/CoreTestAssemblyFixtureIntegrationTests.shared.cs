// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreTestAssemblyFixtureIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.TestApp;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestApp;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;
using Xunit.v3;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Test
{
    /// <summary>
    /// Class CoreTestAssemblyFixtureIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreTestAssemblyFixtureIntegrationTests))]

    public class CoreTestAssemblyFixtureIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTestAssemblyFixtureIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTestAssemblyFixtureIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void TestAssemblyFixture_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void TestAssemblyFixture_ActiveTestContext()
        {
            CoreTestAssemblyFixtureBase.ActiveTestContext.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ITestContext>();
            this.TestAssemblyFixture.XunitTestContext.Should().BeSameAs(CoreTestAssemblyFixtureBase.ActiveTestContext);
            CoreTestAssemblyFixtureBase.ActiveTestContext.TestAssembly.Should().BeSameAs(CoreTestAssemblyFixtureBase.ActiveXunitTestAssembly);
        }

        [Fact]
        public void TestAssemblyFixture_ActiveTestApplication()
        {
            CoreTestAssemblyFixtureBase.ActiveTestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            this.TestAssemblyFixture.TestApplication.Should().BeSameAs(CoreTestAssemblyFixtureBase.ActiveTestApplication);
        }

        [Fact]
        public void TestAssemblyFixture_ActiveXunitTestAssembly()
        {
            CoreTestAssemblyFixtureBase.ActiveXunitTestAssembly.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IXunitTestAssembly>();
            this.TestAssemblyFixture.XunitTestAssembly.Should().BeSameAs(CoreTestAssemblyFixtureBase.ActiveXunitTestAssembly);
        }

        [Fact]
        public void TestAssemblyFixture_ActiveTestAssemblyNamespace()
        {
            CoreTestAssemblyFixtureBase.ActiveTestAssemblyNamespace.Should().NotBeNull().And.Subject.Should().BeAssignableTo<string>();
            this.TestAssemblyFixture.TestAssemblyNamespace.Should().Be(CoreTestAssemblyFixtureBase.ActiveTestAssemblyNamespace);
            CoreTestAssemblyFixtureBase.ActiveXunitTestAssembly!.Assembly.GetNamespace().Should().Be(this.TestAssemblyFixture.TestAssemblyNamespace);
        }

        [Fact]
        public void TestAssemblyFixture_ActiveTestLoggerCategory()
        {
            CoreTestAssemblyFixtureBase.ActiveTestLoggerCategory.Should().NotBeNull().And.Subject.Should().BeAssignableTo<string>();
            this.TestOutputHelper.WriteLine($"ActiveTestLoggerCategory: {CoreTestAssemblyFixtureBase.ActiveTestLoggerCategory}");
            this.TestOutputHelper.WriteLine($"TestAssemblyNamespace: {this.TestAssemblyFixture.TestAssemblyNamespace}");
        }

        [Fact]
        public void TestAssemblyFixture_ActiveTestAssembly()
        {
            CoreTestAssemblyFixtureBase.ActiveTestAssembly.Should().NotBeNull().And.Subject.Should().BeAssignableTo<Assembly>();
            this.TestAssemblyFixture.TestAssembly.Should().BeSameAs(CoreTestAssemblyFixtureBase.ActiveTestAssembly);
            CoreTestAssemblyFixtureBase.ActiveXunitTestAssembly!.Assembly.Should().BeSameAs(this.TestAssemblyFixture.TestAssembly);
        }

        [Fact]
        public void TestAssemblyFixture_TestFileSystem()
        {
            this.TestAssemblyFixture.TestFileSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreFileSystem>();
            this.TestFileSystem.Should().BeSameAs(this.TestAssemblyFixture.TestFileSystem);
            this.TestFileSystem.Should().BeSameAs(this.TestApplication.ServiceProvider.GetRequiredService<ICoreFileSystem>());
            this.TestFileSystem.GlobalLogger.IsNullLogger.Should().BeFalse();
        }

        [Fact]
        public void TestAssemblyFixture_TestAppServiceProvider()
        {
            this.TestAssemblyFixture.TestAppServiceProvider.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IServiceProvider>();
            this.TestAppServiceProvider.Should().BeSameAs(this.TestAssemblyFixture.TestAppServiceProvider);
            this.TestFileSystem.Should().BeSameAs(this.TestAssemblyFixture.TestAppServiceProvider.GetRequiredService<ICoreFileSystem>());
        }

        [Fact]
        public void TestAssemblyFixture_Configuration()
        {
            this.TestAssemblyFixture.Configuration.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IConfiguration>();
            this.Configuration.Should().BeSameAs(this.TestAssemblyFixture.Configuration);
        }

        [Fact]
        public void TestAssemblyFixture_AppSettings()
        {
            var testAssemblyFixture = CoreTestAssemblyFixture.Create();

            // Validate the ActiveTestApplication is did not change.
            CoreTestAssemblyFixtureBase.ActiveTestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            this.TestAssemblyFixture.TestApplication.Should().BeSameAs(CoreTestAssemblyFixtureBase.ActiveTestApplication);

            testAssemblyFixture.CleanUpAppSessionFolderOnDispose.Should().BeTrue();

            this.TestOutputHelper.WriteLine($"Test FileSystem: {testAssemblyFixture.TestFileSystem.ToJsonString<CoreFileSystem>(CoreSerializationFormatFlags.JsonFormatted)}");
            this.TestOutputHelper.WriteLine($"\nAssembly FileSystem: {this.TestFileSystem.ToJsonString<CoreFileSystem>(CoreSerializationFormatFlags.JsonFormatted)}");

            // Validate the AppSettings is not the same.
            testAssemblyFixture.TestFileSystem.AppSettings.Should().NotBe(this.TestFileSystem.AppSettings);
            testAssemblyFixture.TestFileSystem.AppSettings.AppSessionID.Should().NotBe(this.TestFileSystem.AppSettings.AppSessionID);
            testAssemblyFixture.TestFileSystem.LocalUserAppDataFolderPath.Should().NotBe(this.TestFileSystem.LocalUserAppDataFolderPath);
            testAssemblyFixture.TestFileSystem.RoamingUserAppDataFolderPath.Should().NotBe(this.TestFileSystem.RoamingUserAppDataFolderPath);

            testAssemblyFixture.Dispose();

            // Validate the ActiveTestApplication is did not change after dispose
            CoreTestAssemblyFixtureBase.ActiveTestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            this.TestAssemblyFixture.TestApplication.Should().BeSameAs(CoreTestAssemblyFixtureBase.ActiveTestApplication);
        }

        [Fact]
        public void TestAssemblyFixture_DisposeTwice()
        {
            var testAssemblyFixture = CoreTestAssemblyFixture.Create();

            // Validate the ActiveTestApplication is did not change.
            CoreTestAssemblyFixtureBase.ActiveTestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            this.TestAssemblyFixture.TestApplication.Should().BeSameAs(CoreTestAssemblyFixtureBase.ActiveTestApplication);

            testAssemblyFixture.CleanUpAppSessionFolderOnDispose.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"New LocalUserAppDataFolderPath: {testAssemblyFixture.TestFileSystem.LocalUserAppDataFolderPath}");
            this.TestOutputHelper.WriteLine($"Test LocalUserAppDataFolderPath: {this.TestFileSystem.LocalUserAppDataFolderPath}");
            testAssemblyFixture.TestFileSystem.LocalUserAppDataFolderPath.Should().NotBe(this.TestFileSystem.LocalUserAppDataFolderPath);

            this.TestOutputHelper.WriteLine($"New RoamingUserAppDataFolderPath: {testAssemblyFixture.TestFileSystem.RoamingUserAppDataFolderPath}");
            this.TestOutputHelper.WriteLine($"Test RoamingUserAppDataFolderPath: {this.TestFileSystem.RoamingUserAppDataFolderPath}");
            testAssemblyFixture.TestFileSystem.RoamingUserAppDataFolderPath.Should().NotBe(this.TestFileSystem.RoamingUserAppDataFolderPath);

            testAssemblyFixture.Dispose();

            // Validate the ActiveTestApplication is did not change after dispose
            CoreTestAssemblyFixtureBase.ActiveTestApplication.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestApplication>();
            this.TestAssemblyFixture.TestApplication.Should().BeSameAs(CoreTestAssemblyFixtureBase.ActiveTestApplication);
            this.TestCaseLoggerFactory.Should().NotBeNull();

            testAssemblyFixture.Dispose();
        }
    }
}
