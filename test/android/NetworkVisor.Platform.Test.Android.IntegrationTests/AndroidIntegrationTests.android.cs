// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Android.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="AndroidIntegrationTests.android.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.Android.IntegrationTests
{
    /// <summary>
    /// Class AndroidIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(AndroidIntegrationTests))]

    public class AndroidIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AndroidIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public AndroidIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void AndroidIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Android, TraitTestType.Integration);
        }

        [Fact]
        public void AndroidIntegration_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.Android);
        }

        /// <summary>
        /// Defines the test method AndroidIntegration_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void AndroidIntegration_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(AndroidIntegrationTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.Android.IntegrationTests");
        }
    }
}
