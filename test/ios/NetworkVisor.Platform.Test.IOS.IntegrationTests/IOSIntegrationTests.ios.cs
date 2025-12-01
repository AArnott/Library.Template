// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.IOS.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="IOSIntegrationTests.ios.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.IOS.IntegrationTests
{
    /// <summary>
    /// Class IOSIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(IOSIntegrationTests))]

    public class IOSIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IOSIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public IOSIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void IOSIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.IOS, TraitTestType.Integration);
        }

        [Fact]
        public void IOSIntegration_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.IOS);
        }

        /// <summary>
        /// Defines the test method IOSIntegration_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void IOSIntegration_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(IOSIntegrationTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.IOS.IntegrationTests");
        }
    }
}
