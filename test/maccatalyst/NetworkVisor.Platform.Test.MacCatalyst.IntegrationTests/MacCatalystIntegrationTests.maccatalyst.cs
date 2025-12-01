// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.MacCatalyst.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="MacCatalystIntegrationTests.maccatalyst.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>MacCatalyst Integration Tests.</summary>
// ***********************************************************************

using System.Reflection;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.MacCatalyst.IntegrationTests
{
    /// <summary>
    /// Class MacCatalystIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(MacCatalystIntegrationTests))]

    public class MacCatalystIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MacCatalystIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public MacCatalystIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void MacCatalystIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.MacCatalyst, TraitTestType.Integration);
        }

        [Fact]
        public void MacCatalystIntegration_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.MacCatalyst);
        }

        /// <summary>
        /// Defines the test method MacCatalystIntegration_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void MacCatalystIntegration_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(MacCatalystIntegrationTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.MacCatalyst.IntegrationTests");
        }
    }
}
