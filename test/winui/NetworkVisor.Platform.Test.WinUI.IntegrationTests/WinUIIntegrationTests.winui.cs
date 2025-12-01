// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.WinUI.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="WinUIIntegrationTests.winui.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.WinUI.IntegrationTests
{
    /// <summary>
    /// Class WinUIIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(WinUIIntegrationTests))]

    public class WinUIIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WinUIIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public WinUIIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void WinUIIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.WinUI, TraitTestType.Integration);
        }

        [Fact]
        public void WinUIIntegration_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.WinUI);
        }

        /// <summary>
        /// Defines the test method WinUIIntegration_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void WinUIIntegration_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(WinUIIntegrationTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.WinUI.IntegrationTests");
        }
    }
}
