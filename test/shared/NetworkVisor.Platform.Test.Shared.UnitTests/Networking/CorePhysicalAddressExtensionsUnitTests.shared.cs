// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests.Networking
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CorePhysicalAddressExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking
{
    /// <summary>
    /// Class CorePhysicalAddressExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CorePhysicalAddressExtensionsUnitTests))]

    public class CorePhysicalAddressExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CorePhysicalAddressExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CorePhysicalAddressExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_Ctor.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_Ctor()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            ((PhysicalAddress)null).ToDashString().Should().Be(PhysicalAddressExtensions.PhysicalAddressNoneColonString.Replace(':', '-'));
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_NormalizedParse_Empty.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_NormalizedParse_Empty()
        {
            PhysicalAddressExtensions.NormalizedParse(string.Empty).Should().Be(PhysicalAddress.None);
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            PhysicalAddressExtensions.NormalizedParse(null).Should().Be(PhysicalAddress.None);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_NormalizedParse_Empty_Delim.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_NormalizedParse_Empty_Delim()
        {
            PhysicalAddressExtensions.NormalizedParse("::").Should().Be(PhysicalAddress.None);
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_NormalizedParse_SinglePart.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_NormalizedParse_SinglePart()
        {
            PhysicalAddressExtensions.NormalizedParse("00").ToString().Should().Be("00");
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_IsNone_Null.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_IsNone_Null()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            ((PhysicalAddress)null).IsNone().Should().BeFalse();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type
        }

        [Fact]
        public void PhysicalAddressExtensions_ToDelimString_NoLength()
        {
            new PhysicalAddress([]).ToDashString().Should().Be(PhysicalAddressExtensions.PhysicalAddressNoneColonString.Replace(':', '-'));
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_ToDelimString_Null.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_None_IsNone()
        {
            PhysicalAddress.None.IsNullOrNone().Should().BeTrue();
            PhysicalAddress.None.IsNone().Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_NormalizedParse_SingleDigits.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_NormalizedParse_SingleDigits()
        {
            PhysicalAddressExtensions.NormalizedParse("1:1:1:1:1:1").Should()
                .Be(PhysicalAddress.Parse("01-01-01-01-01-01"));
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_NormalizedParse_Short.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_NormalizedParse_Short()
        {
            PhysicalAddressExtensions.NormalizedParse("1:1:1:1:1").Should()
                .Be(PhysicalAddress.Parse("01-01-01-01-01"));
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_ToArray.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_ToArray()
        {
            PhysicalAddress.Parse("01-02-03-04-05-06").ToArray().Should().BeEquivalentTo(new byte[] { 1, 2, 3, 4, 5, 6 });
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_ToColonString.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_ToColonString()
        {
            PhysicalAddress.Parse("01-02-03-04-05-06").ToColonString().Should().Be("01:02:03:04:05:06");
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_IsBroadcast.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_IsBroadcast()
        {
            PhysicalAddressExtensions.Broadcast.IsBroadcastAddress().Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method PhysicalAddressExtensions_ToArray_Null.
        /// </summary>
        [Fact]
        public void PhysicalAddressExtensions_ToArray_Null()
        {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            Func<byte[]> fx = () => ((PhysicalAddress)null).ToArray();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("physicalAddress");
        }
    }
}
