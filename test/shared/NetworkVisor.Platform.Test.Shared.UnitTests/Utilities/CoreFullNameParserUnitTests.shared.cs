// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreFullNameParserUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Full Name Parser Unit Tests.</summary>
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
    /// Class CoreFullNameParserUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreFullNameParserUnitTests))]

    public class CoreFullNameParserUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreFullNameParserUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreFullNameParserUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method FullNameParser_FullName.
        /// </summary>
        /// <param name="fullName">Full name to test.</param>
        /// <param name="expectedFullName">Expected full name.</param>
        /// <param name="expectedPreferredName">Expected preferred name.</param>
        /// <param name="expectedFirstName">Expected first name.</param>
        /// <param name="expectedLastName">Expected last name.</param>
        [Theory]
        [InlineData("Steve Bush", "Steve Bush", null, "Steve", "Bush")]
        [InlineData("Bush,Steve", "Steve Bush", null, "Steve", "Bush")]
        [InlineData("Bush,Steven M", "Steven M Bush", null, "Steven M", "Bush")]
        [InlineData("Bush,Steve,M", "M Bush,Steve", null, "M", "Bush,Steve")]
        [InlineData("Bush,", "Bush", null, "", "Bush")]
        [InlineData(",,,", ",,", null, "", ",,")]
        [InlineData(" , ", "", null, "", "")]
        [InlineData("Bush,Steven (Steve)", "Steven (Steve) Bush", "Steve", "Steven", "Bush")]
        [InlineData("Steven (Steve) Bush", "Steven (Steve) Bush", "Steve", "Steven", "Bush")]
        [InlineData("Steven(Steve) Bush", "Steven(Steve) Bush", "Steve", "Steven", "Bush")]
        [InlineData("(Steve) Bush", "(Steve) Bush", "Steve", "", "Bush")]
        [InlineData("(Steve Bush", "(Steve Bush", null, "(Steve", "Bush")]
        [InlineData("(Steve) Steven Bush", "(Steve) Steven Bush", "Steve", "", "Steven Bush")]
        public void FullNameParser_FullName(string fullName, string expectedFullName, string? expectedPreferredName, string expectedFirstName, string expectedLastName)
        {
            var fullNameParserParser = new FullNameParser(fullName);
            fullNameParserParser.Should().NotBeNull();
            fullNameParserParser.FirstName.Should().Be(expectedFirstName);
            fullNameParserParser.LastName.Should().Be(expectedLastName);
            fullNameParserParser.FullName.Should().Be(expectedFullName);
            fullNameParserParser.PreferredName.Should().Be(expectedPreferredName);
        }
    }
}
