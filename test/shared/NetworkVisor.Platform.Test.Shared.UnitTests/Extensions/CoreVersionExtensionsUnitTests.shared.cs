// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreVersionExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CoreVersionExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreVersionExtensionsUnitTests))]

    public class CoreVersionExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreVersionExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreVersionExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreVersionExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        [Theory]
        [InlineData("", 0, 0, -1, -1)]
        [InlineData("14", 14, 0, -1, -1)]
        [InlineData("14.1", 14, 1, -1, -1)]
        [InlineData("14.1.2", 14, 1, 2, -1)]
        [InlineData("14.1.2.3", 14, 1, 2, 3)]
        [InlineData("Unix 14.1.2.3", 0, 0, -1, -1)]
        public void CoreVersionExtensionsUnit_ParseVersion(string versionString, int major, int minor, int build, int revision)
        {
            Version version = versionString.ParseVersion();
            version.Major.Should().Be(major);
            version.Minor.Should().Be(minor);
            version.Build.Should().Be(build);
            version.Revision.Should().Be(revision);
        }

        [Theory]
        [InlineData("", 0, 0, -1, -1, true)]
        [InlineData("14", 14, 0, -1, -1, true)]
        [InlineData("14.1", 14, 1, -1, -1, true)]
        [InlineData("14.1.2", 14, 1, 2, -1, true)]
        [InlineData("14.1.2.3", 14, 1, 2, 3, true)]
        [InlineData("14", 14, 0, 0, 0, true)]
        [InlineData("14.1", 14, 1, 0, 0, true)]
        [InlineData("14.1.2", 14, 1, 2, 0, true)]
        public void CoreVersionExtensionsUnit_IsOSVersionAtLeast(string versionString, int major, int minor, int build, int revision, bool expectedResult)
        {
            versionString.ParseVersion().IsOSVersionAtLeast(major, minor, build, revision).Should().Be(expectedResult);
        }
    }
}
