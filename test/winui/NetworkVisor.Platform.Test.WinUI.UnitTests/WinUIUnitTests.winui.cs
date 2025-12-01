// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.WinUI.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="WinUIUnitTests.winui.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.WinUI.UnitTests
{
    /// <summary>
    /// Class WinUIUnitTests.
    /// </summary>
    [PlatformTrait(typeof(WinUIUnitTests))]

    public class WinUIUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WinUIUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public WinUIUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void WinUIUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.WinUI, TraitTestType.Unit);
        }

        [Fact]
        public void WinUIUnit_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.WinUI);
        }

        /// <summary>
        /// Defines the test method WinUIUnit_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void WinUIUnit_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(WinUIUnitTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.WinUI.UnitTests");
        }
    }
}
