// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Linux.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="LinuxUnitTests.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Reflection;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Linux.UnitTests
{
    /// <summary>
    /// Class LinuxUnitTests.
    /// </summary>
    [PlatformTrait(typeof(LinuxUnitTests))]

    public class LinuxUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinuxUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public LinuxUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void LinuxUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Linux, TraitTestType.Unit);
        }

        [Fact]
        public void LinuxUnit_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.Linux);
        }

        /// <summary>
        /// Defines the test method LinuxUnit_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void LinuxUnit_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(LinuxUnitTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.Linux.UnitTests");
        }
    }
}
