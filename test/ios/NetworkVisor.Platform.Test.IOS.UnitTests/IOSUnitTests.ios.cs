// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.IOS.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="IOSUnitTests.ios.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.IOS.UnitTests
{
    /// <summary>
    /// Class IOSUnitTests.
    /// </summary>
    [PlatformTrait(typeof(IOSUnitTests))]

    public class IOSUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IOSUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public IOSUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void IOSUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.IOS, TraitTestType.Unit);
        }

        [Fact]
        public void IOSUnit_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.IOS);
        }

        /// <summary>
        /// Defines the test method IOSUnit_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void IOSUnit_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(IOSUnitTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.IOS.UnitTests");
        }
    }
}
