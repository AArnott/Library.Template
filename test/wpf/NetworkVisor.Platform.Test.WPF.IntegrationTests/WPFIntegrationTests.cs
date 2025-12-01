// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.WPF.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="WPFIntegrationTests.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.WPF.IntegrationTests
{
    /// <summary>
    /// Class WPFIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(WPFIntegrationTests))]

    public class WPFIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WPFIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public WPFIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void WPFIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.WPF, TraitTestType.Integration);
        }

        [Fact]
        public void WPFIntegration_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.WPF);
        }

        /// <summary>
        /// Defines the test method WPF_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void WPFIntegration_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(WPFIntegrationTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.WPF.IntegrationTests");
        }
    }
}
