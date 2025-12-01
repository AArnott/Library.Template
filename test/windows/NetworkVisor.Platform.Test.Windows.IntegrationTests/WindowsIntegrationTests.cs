// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Windows.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="WindowsIntegrationTests.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.Windows.IntegrationTests
{
    /// <summary>
    /// Class WindowsIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(WindowsIntegrationTests))]

    public class WindowsIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="WindowsIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public WindowsIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void WindowsIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Windows, TraitTestType.Integration);
        }

        [Fact]
        public void WindowsIntegration_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.Windows);
        }

        /// <summary>
        /// Defines the test method Windows_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void WindowsIntegration_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(WindowsIntegrationTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.Windows.IntegrationTests");
        }
    }
}
