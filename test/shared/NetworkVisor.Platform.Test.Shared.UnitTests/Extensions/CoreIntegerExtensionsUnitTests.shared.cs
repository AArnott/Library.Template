// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreIntegerExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CoreIntegerExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreIntegerExtensionsUnitTests))]

    public class CoreIntegerExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreIntegerExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreIntegerExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreIntegerExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method IntegerExtensions_ToInt16HexString.
        /// </summary>
        [Fact]
        public void IntegerExtensions_ToInt16HexString()
        {
            const short TestInt16 = 256;
            string prefix = "0x";
            string hexString = TestInt16.ToHexString(prefix);

            hexString.Should().Be($"{prefix}{TestInt16:X4}");
        }

        /// <summary>
        /// Defines the test method IntegerExtensions_ToInt16HexStringNullPrefix.
        /// </summary>
        [Fact]
        public void IntegerExtensions_ToInt16HexStringNullPrefix()
        {
            const short TestInt16 = 256;
            string? prefix = null;

#pragma warning disable CS8604 // Possible null reference argument.
            string hexString = TestInt16.ToHexString(prefix);
#pragma warning restore CS8604 // Possible null reference argument.

            hexString.Should().Be($"{prefix}{TestInt16:X4}");
        }

        /// <summary>
        /// Defines the test method IntegerExtensions_ToInt32HexString.
        /// </summary>
        [Fact]
        public void IntegerExtensions_ToInt32HexString()
        {
            const int TestInt32 = 256;
            string prefix = "0x";
            string hexString = TestInt32.ToHexString(prefix);

            hexString.Should().Be($"{prefix}{TestInt32:X8}");
        }

        /// <summary>
        /// Defines the test method IntegerExtensions_ToUInt16HexString.
        /// </summary>
        [Fact]
        public void IntegerExtensions_ToUInt16HexString()
        {
            const ushort TestUint16 = 256;
            string prefix = "0x";
            string hexString = TestUint16.ToHexString(prefix);

            hexString.Should().Be($"{prefix}{TestUint16:X4}");
        }

        /// <summary>
        /// Defines the test method IntegerExtensions_ToUInt32HexString.
        /// </summary>
        [Fact]
        public void IntegerExtensions_ToUInt32HexString()
        {
            const uint TestUint32 = 256;
            string prefix = "0x";
            string hexString = TestUint32.ToHexString(prefix);

            hexString.Should().Be($"{prefix}{TestUint32:X8}");
        }

        /// <summary>
        /// Defines the test method IntegerExtensions_ZeroFillNumber.
        /// </summary>
        /// <param name="number">Number to test.</param>
        /// <param name="minLength">Minimum length.</param>
        /// <param name="expectedValue">Expected value.</param>
        [Theory]
        [InlineData(32, 4, "0032")]
        [InlineData(0, 0, "0")]
        [InlineData(-32, 4, "-0032")]
        [InlineData(-32, 2, "-32")]
        [InlineData(-32, 1, "-32")]
        public void IntegerExtensions_ZeroFillNumber(int number, int minLength, string expectedValue)
        {
            number.ZeroFillNumber(minLength).Should().Be(expectedValue);
        }
    }
}
