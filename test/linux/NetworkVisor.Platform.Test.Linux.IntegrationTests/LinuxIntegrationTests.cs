// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Linux.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="LinuxIntegrationTests.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.Linux.IntegrationTests
{
    /// <summary>
    /// Class LinuxIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(LinuxIntegrationTests))]

    public class LinuxIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LinuxIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared setup and cleanup.</param>
        public LinuxIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void LinuxIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Linux, TraitTestType.Integration);
            this.TraitTestType.Should().Be(TraitTestType.Integration);
        }

        [Fact]
        public void LinuxIntegration_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.Linux);
            this.TraitOperatingSystem.Should().Be(TraitOperatingSystem.Linux);
        }

        /// <summary>
        /// Defines the test method LinuxIntegration_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void LinuxIntegration_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(LinuxIntegrationTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.Linux.IntegrationTests");
        }
    }
}
