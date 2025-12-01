// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.NetCore.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="NetCoreIntegrationTests.netcore.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.NetCore.IntegrationTests
{
    /// <summary>
    /// Class NetCoreIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(NetCoreIntegrationTests))]

    public class NetCoreIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetCoreIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">Class test fixture common across all test cases.</param>
        public NetCoreIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NetCoreIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.NetCore, TraitTestType.Integration);
        }

        [Fact]
        public void NetCoreIntegration_GetTraitOperatingSystem()
        {
            this.TestOutputHelper.WriteLine($"TraitOperatingSystem: {this.TestClassType.GetTraitOperatingSystem()}");
            this.TestClassType.GetTraitOperatingSystem().Should().Be(TraitOperatingSystem.NetCore);
        }

        /// <summary>
        /// Defines the test method  NetCoreIntegration_CoreAssemblyExtensions_GetNamespace.
        /// </summary>
        [Fact]
        public void NetCoreIntegration_CoreAssemblyExtensions_GetNamespace()
        {
            typeof(NetCoreIntegrationTests).GetTypeInfo().Assembly.GetNamespace().Should().Be("NetworkVisor.Platform.Test.NetCore.IntegrationTests");
        }
    }
}
