// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.WPF.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="WPFUnitTests.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.WPF.UnitTests
{
    /// <summary>
    /// Class WPFUnitTests.
    /// </summary>
    [PlatformTrait(typeof(WPFUnitTests))]

    public class WPFUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WPFUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public WPFUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void WPFUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.WPF, TraitTestType.Unit);
        }

        [Fact]
        public void WPFUnit_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.WPF);
        }

        /// <summary>
        /// Defines the test method WPF_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void WPFUnit_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(WPFUnitTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.WPF.UnitTests");
        }
    }
}
