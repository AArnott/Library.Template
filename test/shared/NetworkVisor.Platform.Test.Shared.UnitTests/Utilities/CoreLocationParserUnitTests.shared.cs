// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreLocationParserUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Location Parser Unit Tests.</summary>
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
    /// Class CoreLocationParserUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreLocationParserUnitTests))]

    public class CoreLocationParserUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLocationParserUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLocationParserUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method LocationParser_Location.
        /// </summary>
        /// <param name="location">The location to test.</param>
        /// <param name="expectedLocation">Expected location result.</param>
        /// <param name="expectedCity">Expected city.</param>
        /// <param name="expectedState">Expected state.</param>
        [Theory]
        [InlineData("Seattle, WA", "Seattle, WA", "Seattle", "WA")]
        [InlineData("Atlanta , GA", "Atlanta, GA", "Atlanta", "GA")]
        [InlineData("New York ,, NY", "New York, NY", "New York", "NY")]
        [InlineData("City Name, with Comma, State", "City Name, with Comma, State", "City Name, with Comma", "State")]
        [InlineData("", "", null, null)]
        [InlineData(null, null, null, null)]
        public void LocationParser_Location(string? location, string? expectedLocation, string? expectedCity, string? expectedState)
        {
            var locationParser = new LocationParser(location);
            locationParser.Should().NotBeNull();
            locationParser.City.Should().Be(expectedCity);
            locationParser.State.Should().Be(expectedState);
            locationParser.Location.Should().Be(expectedLocation);
        }
    }
}
