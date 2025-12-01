// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 06-04-2020
//
// Last Modified By : SteveBu
// Last Modified On : 06-04-2020
// ***********************************************************************
// <copyright file="CoreByteExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Assembly Extensions Unit Tests.</summary>
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
    /// Class ByteExtensionsUnitTests. Byte Extensions Unit Tests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreByteExtensionsUnitTests))]

    public class CoreByteExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreByteExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreByteExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreByteExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method ByteExtensions_GetBits_Byte.
        /// </summary>
        /// <param name="testItem">The number to test.</param>
        /// <param name="position">Bit position.</param>
        /// <param name="length">Length of bits to test.</param>
        /// <param name="expectedResult">Expected result.</param>
        [Theory]
        [InlineData(0, 0, 1, 0)]
        [InlineData(0, 1, 1, 0)]
        [InlineData(0, 2, 1, 0)]
        [InlineData(0, 3, 1, 0)]
        [InlineData(0, 4, 1, 0)]
        [InlineData(0, 5, 1, 0)]
        [InlineData(0, 6, 1, 0)]
        [InlineData(0, 7, 1, 0)]
        [InlineData(1, 0, 1, 1)]
        [InlineData(1, 1, 1, 0)]
        [InlineData(1, 2, 1, 0)]
        [InlineData(1, 3, 1, 0)]
        [InlineData(1, 4, 1, 0)]
        [InlineData(1, 5, 1, 0)]
        [InlineData(1, 6, 1, 0)]
        [InlineData(1, 7, 1, 0)]
        [InlineData(3, 0, 1, 1)]
        [InlineData(3, 1, 1, 1)]
        [InlineData(3, 2, 1, 0)]
        [InlineData(3, 3, 1, 0)]
        [InlineData(3, 4, 1, 0)]
        [InlineData(3, 5, 1, 0)]
        [InlineData(3, 6, 1, 0)]
        [InlineData(3, 7, 1, 0)]
        [InlineData(255, 0, 1, 1)]
        [InlineData(255, 1, 1, 1)]
        [InlineData(255, 2, 1, 1)]
        [InlineData(255, 3, 1, 1)]
        [InlineData(255, 4, 1, 1)]
        [InlineData(255, 5, 1, 1)]
        [InlineData(255, 6, 1, 1)]
        [InlineData(255, 7, 1, 1)]
        [InlineData(255, 8, 1, 0)]
        [InlineData(255, -1, 1, 0)]
        public void ByteExtensions_GetBits_Byte(byte testItem, int position, int length, byte expectedResult)
        {
            testItem.GetBits(position, length).Should().Be(expectedResult);
        }

        /// <summary>
        /// Defines the test method ByteExtensions_GetBit_Byte.
        /// </summary>
        [Fact]
        public void ByteExtensions_GetBit_Byte()
        {
            ((byte)1).GetBit(0).Should().BeTrue();
            ((byte)1).GetBit(1).Should().BeFalse();
            ((byte)1).GetBit(7).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method ByteExtensions_GetBits_UShort.
        /// </summary>
        /// <param name="testItem">The number to test.</param>
        /// <param name="position">Bit position.</param>
        /// <param name="length">Length of bits to test.</param>
        /// <param name="expectedResult">Expected result.</param>
        [Theory]
        [InlineData(0, 0, 1, 0)]
        [InlineData(3, 0, 2, 3)]
        [InlineData(0xFFFF, 1, 2, 3)]
        [InlineData(0xFFFF, -1, 1, 0)]
        [InlineData(0xFFFF, 15, 1, 1)]
        [InlineData(0xFFFF, 16, 1, 0)]
        [InlineData(0xFFFF, 17, 1, 0)]
        [InlineData(0xFFFF, 14, 3, 3)]
        [InlineData(0xFFFF, 0, 16, 0xFFFF)]
        public void ByteExtensions_GetBits_UShort(ushort testItem, int position, int length, ushort expectedResult)
        {
            testItem.GetBits(position, length).Should().Be(expectedResult);
        }

        /// <summary>
        /// Defines the test method ByteExtensions_GetBit_UShort.
        /// </summary>
        [Fact]
        public void ByteExtensions_GetBit_UShort()
        {
            ((ushort)1).GetBit(0).Should().BeTrue();
            ((ushort)1).GetBit(1).Should().BeFalse();
            ((ushort)0xFFFF).GetBit(15).Should().BeTrue();
            ((ushort)0xFFFF).GetBit(16).Should().BeFalse();
        }
    }
}
