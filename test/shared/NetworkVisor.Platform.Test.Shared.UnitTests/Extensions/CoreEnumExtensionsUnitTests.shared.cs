// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreEnumExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.ComponentModel;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CoreEnumExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreEnumExtensionsUnitTests))]

    public class CoreEnumExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEnumExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreEnumExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Enum TestEnum.
        /// </summary>
        private enum TestEnum
        {
            /// <summary>
            /// The first second.
            /// </summary>
            [Description("First Second")]
            First_Second,

            /// <summary>
            /// The first second third.
            /// </summary>
            [Description("First Second Third")]
            First_Second_Third,
        }

        [Fact]
        public void CoreEnumExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method EnumExtensions_ToEnumString.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("First Second", "First_Second")]
        [InlineData(" ", null)]
        [InlineData("\t", null)]
        [InlineData("\r\n", null)]
        public void EnumExtensions_ToEnumString(string testString, string? expectedString)
        {
            testString.ToEnumString().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method EnumExtensions_FromEnumString.
        /// </summary>
        /// <param name="testEnum">The test enum.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData(TestEnum.First_Second, "First Second")]
        [InlineData(TestEnum.First_Second_Third, "First Second Third")]
        public void EnumExtensions_FromEnumString(Enum testEnum, string expectedString)
        {
            testEnum.FromEnumValue().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method EnumExtensions_GetDescription.
        /// </summary>
        /// <param name="testEnum">The test enum.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData(TestEnum.First_Second, "First Second")]
        [InlineData(TestEnum.First_Second_Third, "First Second Third")]
        public void EnumExtensions_GetDescription(Enum testEnum, string expectedString)
        {
            testEnum.GetDescription().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method EnumExtensions_GetDescription_Null.
        /// </summary>
        [Fact]
        public void EnumExtensions_GetDescription_Null()
        {
            Func<string?> fx = () => ((TestEnum?)null).GetDescription();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("value");
        }

        /// <summary>
        /// Defines the test method EnumExtensions_FromEnumValue_Null.
        /// </summary>
        [Fact]
        public void EnumExtensions_FromEnumValue_Null()
        {
            Func<string?> fx = () => ((TestEnum?)null).FromEnumValue();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("value");
        }
    }
}
