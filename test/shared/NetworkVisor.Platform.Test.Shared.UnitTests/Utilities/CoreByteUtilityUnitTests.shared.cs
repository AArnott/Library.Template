// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 06-06-2020
//
// Last Modified By : SteveBu
// Last Modified On : 06-06-2020
// ***********************************************************************
// <copyright file="CoreByteUtilityUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Byte Utility Unit Tests.</summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Core.Utilities;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Utilities
{
    /// <summary>
    /// Class CoreByteUtilityUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreByteUtilityUnitTests))]

    public class CoreByteUtilityUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreByteUtilityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreByteUtilityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method ByteUtility_Equality.
        /// </summary>
        /// <param name="a1">Byte array to compare.</param>
        /// <param name="b1">Byte array to compare to.</param>
        /// <param name="expectedResult">Expected result.</param>
        [Theory]
        [InlineData(new byte[] { }, new byte[] { }, true)]
        [InlineData(null, new byte[] { }, false)]
        [InlineData(new byte[] { }, null, false)]
        [InlineData(null, null, true)]
        [InlineData(new byte[] { 1, 2 }, new byte[] { 1, 2 }, true)]
        [InlineData(new byte[] { 1 }, new byte[] { 1, 2 }, false)]

        public void ByteUtility_Equality(byte[]? a1, byte[]? b1, bool expectedResult)
        {
            ByteUtility.Equality(a1, b1).Should().Be(expectedResult);
        }

        /// <summary>
        /// Defines the test method ByteUtility_Equality_Reference.
        /// </summary>
        [Fact]
        public void ByteUtility_Equality_Reference()
        {
            byte[] testBytes = new byte[] { 1, 3 };

            ByteUtility.Equality(testBytes, testBytes).Should().BeTrue();
        }
    }
}
