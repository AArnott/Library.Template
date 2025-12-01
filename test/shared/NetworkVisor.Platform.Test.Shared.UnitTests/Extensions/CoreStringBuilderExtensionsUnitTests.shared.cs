// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreStringBuilderExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Text;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CoreStringBuilderExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreStringBuilderExtensionsUnitTests))]

    public class CoreStringBuilderExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreStringBuilderExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreStringBuilderExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreStringBuilderExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

#pragma warning disable SA1118 // Parameter should not span multiple lines
        /// <summary>
        /// Defines the test method StringBuilderExtensions_RemoveEndOfLineIfExists.
        /// </summary>
        /// <param name="testString">String to test.</param>
        /// <param name="expectedString">Expected string result.</param>
        [Theory]
        [InlineData("", "")]
        [InlineData("Test", "Test")]
        [InlineData(
            @"Test
",
            "Test")]
        [InlineData(
            @"
",
            "")]
        [InlineData(
            @"

",
            @"
")]
#pragma warning restore SA1118 // Parameter should not span multiple lines
        public void StringBuilderExtensions_RemoveEndOfLineIfExists(string testString, string expectedString)
        {
            new StringBuilder(testString).RemoveEndOfLineIfExists()!.ToString().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringBuilderExtensions_RemoveEndOfLineIfExists_Null.
        /// </summary>
        [Fact]
        public void StringBuilderExtensions_RemoveEndOfLineIfExists_Null()
        {
            ((StringBuilder?)null).RemoveEndOfLineIfExists().Should().BeNull();
        }
    }
}
