// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.MacOS.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="MacOSIntegrationTests.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.MacOS.IntegrationTests
{
    /// <summary>
    /// Class MacOSIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(MacOSIntegrationTests))]

    public class MacOSIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MacOSIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public MacOSIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void MacOSIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.MacOS, TraitTestType.Integration);
        }

        [Fact]
        public void MacOSIntegration_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.MacOS);
        }

        /// <summary>
        /// Defines the test method MacOSIntegration_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void MacOSIntegration_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(MacOSIntegrationTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.MacOS.IntegrationTests");
        }
    }
}
