// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Android.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="AndroidUnitTests.android.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.Android.UnitTests
{
    /// <summary>
    /// Class AndroidUnitTests.
    /// </summary>
    [PlatformTrait(typeof(AndroidUnitTests))]

    public class AndroidUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public AndroidUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void AndroidUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Android, TraitTestType.Unit);
        }

        [Fact]
        public void AndroidUnit_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.Android);
        }

        /// <summary>
        /// Defines the test method AndroidUnit_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void AndroidUnit_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(AndroidUnitTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.Android.UnitTests");
        }
    }
}
