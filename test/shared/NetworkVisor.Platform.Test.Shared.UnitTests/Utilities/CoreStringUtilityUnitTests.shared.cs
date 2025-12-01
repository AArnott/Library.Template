// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreStringUtilityUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Assembly Extensions Unit Tests.</summary>
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
    /// Class CoreStringUtilityUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreStringUtilityUnitTests))]

    public class CoreStringUtilityUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreStringUtilityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreStringUtilityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method StringUtility_FormatDiskSize.
        /// </summary>
        /// <param name="diskSize">Disk size to test.</param>
        /// <param name="expectedResult">Expected result.</param>
        [Theory]
        [InlineData(-1, "Unavailable")]
        [InlineData(0, "0 Bytes")]
        [InlineData(1, "1 Byte")]
        [InlineData(1023, "1023 Bytes")]
        [InlineData(1024, "1 KB")]
        [InlineData(1025, "1 KB")]
        [InlineData(37888, "37 KB")]
        [InlineData(37889, "37 KB")]
        [InlineData(38912, "38 KB")]
        [InlineData(65534, "64 KB")]
        [InlineData(65535, "64 KB")]
        [InlineData(65536, "64 KB")]
        [InlineData(16777216, "16 MB")]
        [InlineData(4294967296, "4 GB")]
        [InlineData(1025899906842624, "933 TB")]
        [InlineData(long.MaxValue, "8192 PB")]
        public void StringUtility_FormatDiskSize(long diskSize, string expectedResult)
        {
            StringUtility.FormatDiskSize(diskSize).Should().Be(expectedResult);
        }

        /// <summary>
        /// Defines the test method StringUtility_FormatLinkSpeed.
        /// </summary>
        /// <param name="linkSpeed">Link speed to test.</param>
        /// <param name="expectedResult">Expected result.</param>
        [Theory]
        [InlineData(-1, "Unavailable")]
        [InlineData(0, "0.0 Bps")]
        [InlineData(1, "1.0 Bps")]
        [InlineData(1023, "1.0 Kbps")]
        [InlineData(1024, "1.0 Kbps")]
        [InlineData(1025, "1.0 Kbps")]
        [InlineData(65536, "65.5 Kbps")]
        [InlineData(16777216, "16.8 Mbps")]
        [InlineData(4294967296, "4.3 Gbps")]
        [InlineData(1025899906842624, "1025.9 Tbps")]
        [InlineData(long.MaxValue, "9223372.0 Tbps")]
        public void StringUtility_FormatLinkSpeed(long linkSpeed, string expectedResult)
        {
            StringUtility.FormatLinkSpeed(linkSpeed).Should().Be(expectedResult);
        }
    }
}
