// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// ***********************************************************************
// <copyright file="CoreFrameworkInfoUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.CoreSystem
{
    /// <summary>
    /// Class CoreFrameworkInfoUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreFrameworkInfoUnitTests))]

    public class CoreFrameworkInfoUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreFrameworkInfoUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreFrameworkInfoUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreFrameworkInfoUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        [Fact]
        public void CoreFrameworkInfoUnit_Ctor()
        {
            this.TestOperatingSystem.Should().NotBeNull().And.BeOfType<CoreOperatingSystem>();
        }

        [Theory]
        [InlineData(".NETCoreApp,Version=v2.0", 2, 0, -1, -1)]
        [InlineData(".NETCoreApp,Version=v5.0", 5, 0, -1, -1)]
        [InlineData(".NETCoreApp,Version=v6.0", 6, 0, -1, -1)]
        [InlineData(".NETCoreApp,Version=v7.0", 7, 0, -1, -1)]
        [InlineData(".NETStandard,Version=v2.0", 2, 0, -1, -1)]
        [InlineData(".NETStandard,Version=v2.1", 2, 1, -1, -1)]
        [InlineData(".NETFramework,Version=v4.6.2", 4, 6, 2, -1)]
        [InlineData(".NETFramework,Version=v4.7.2", 4, 7, 2, -1)]
        [InlineData(null, 0, 0, -1, -1)]
        [InlineData("", 0, 0, -1, -1)]
        [InlineData(".NETFramework,Version=4.7.2", 0, 0, -1, -1)]
        [InlineData(".NETFramework", 0, 0, -1, -1)]
        public void CoreFrameworkInfoUnit_ToFrameworkVersion(string? frameworkName, int major, int minor, int build, int revision)
        {
            Version version = frameworkName.ToFrameworkVersion();

            this.TestOutputHelper.WriteLine($"Framework Name: {frameworkName}");
            this.TestOutputHelper.WriteLine($"Framework Version: {version}");

            version.Major.Should().Be(major);
            version.Minor.Should().Be(minor);
            version.Build.Should().Be(build);
            version.Revision.Should().Be(revision);
        }

        [Theory]
        [InlineData(".NETCoreApp,Version=v2.0", CoreFrameworkType.NetCoreApp)]
        [InlineData(".NETCoreApp,Version=v5.0", CoreFrameworkType.NetCore)]
        [InlineData(".NETCoreApp,Version=v6.0", CoreFrameworkType.NetCore)]
        [InlineData(".NETCoreApp,Version=v7.0", CoreFrameworkType.NetCore)]
        [InlineData(".NETStandard,Version=v2.0", CoreFrameworkType.NetStandard)]
        [InlineData(".NETStandard,Version=v2.1", CoreFrameworkType.NetStandard)]
        [InlineData(".NETFramework,Version=v4.6.2", CoreFrameworkType.NetFramework)]
        [InlineData(".NETFramework,Version=v4.7.2", CoreFrameworkType.NetFramework)]
        [InlineData(null, CoreFrameworkType.Unknown)]
        [InlineData("", CoreFrameworkType.Unknown)]
        [InlineData(".NETFramework,Version=4.7.2", CoreFrameworkType.NetFramework)]
        [InlineData(".NETFramework", CoreFrameworkType.NetFramework)]
        [InlineData("Foobar", CoreFrameworkType.Unknown)]
        public void CoreFrameworkInfoUnit_ToFrameworkType(string? frameworkName, CoreFrameworkType frameworkTypeTest)
        {
            CoreFrameworkType frameworkType = frameworkName.ToFrameworkType();

            this.TestOutputHelper.WriteLine($"Framework Name: {frameworkName}");
            this.TestOutputHelper.WriteLine($"Framework Type: {frameworkType}");

            frameworkType.Should().Be(frameworkTypeTest);
        }
    }
}
