// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.MacCatalyst.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="MacCatalystUnitTests.maccatalyst.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>MacCatalyst Unit Tests.</summary>
// ***********************************************************************

using System.Reflection;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.MacCatalyst.UnitTests
{
    /// <summary>
    /// Class MacCatalystUnitTests.
    /// </summary>
    [PlatformTrait(typeof(MacCatalystUnitTests))]

    public class MacCatalystUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MacCatalystUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public MacCatalystUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void MacCatalystUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.MacCatalyst, TraitTestType.Unit);
        }

        [Fact]
        public void MacCatalystUnit_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.MacCatalyst);
        }

        /// <summary>
        /// Defines the test method MacCatalystUnit_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void MacCatalystUnit_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(MacCatalystUnitTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.MacCatalyst.UnitTests");
        }
    }
}
