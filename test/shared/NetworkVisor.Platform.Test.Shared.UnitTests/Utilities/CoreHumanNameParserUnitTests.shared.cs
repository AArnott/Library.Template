// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreHumanNameParserUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Forked from https://github.com/jamescurran/HumanNameParser.</summary>
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
    /// Class CoreHumanNameParserUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreHumanNameParserUnitTests))]

    public class CoreHumanNameParserUnitTests : CoreTestCaseBase
    {
        private readonly CoreHumanNameParser _humanNameParser = new();

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreHumanNameParserUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreHumanNameParserUnitTests(CoreTestClassFixture testClassFixture)
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

        [Fact]
        public void HumanNameParser_BasicTest()
        {
            CoreParsedHumanName parsedHumanName = this._humanNameParser.ParseHumanName("Björn O'Malley");

            parsedHumanName.Should().NotBeNull().And.Subject.Should().BeOfType<CoreParsedHumanName>();
            parsedHumanName.First.Should().Be("Björn");
            parsedHumanName.FullName.Should().Be("Björn O'Malley");
            parsedHumanName.Last.Should().Be("O'Malley");
            parsedHumanName.Middle.Should().BeEmpty();
            parsedHumanName.Nicknames.Should().BeEmpty();
            parsedHumanName.Suffix.Should().BeEmpty();
            parsedHumanName.Title.Should().BeEmpty();
            parsedHumanName.LeadingInitial.Should().BeEmpty();
        }

        [Fact]
        public void HumanNameParser_Nickname()
        {
            CoreParsedHumanName parsedHumanName = this._humanNameParser.ParseHumanName("James C. ('Jimmy') O'Dell, Jr.");

            parsedHumanName.Should().NotBeNull().And.Subject.Should().BeOfType<CoreParsedHumanName>();
            parsedHumanName.First.Should().Be("James");
            parsedHumanName.Last.Should().Be("O'Dell");
            parsedHumanName.Middle.Should().Be("C.");
            parsedHumanName.Nicknames.Should().Be("Jimmy");
            parsedHumanName.Suffix.Should().Be("Jr.");
            parsedHumanName.Title.Should().BeEmpty();
            parsedHumanName.LeadingInitial.Should().BeEmpty();
        }

        [Fact]
        public void HumanNameParser_OneName()
        {
            CoreParsedHumanName parsedHumanName = this._humanNameParser.ParseHumanName("Cher");
            parsedHumanName.Should().NotBeNull().And.Subject.Should().BeOfType<CoreParsedHumanName>();

            parsedHumanName.First.Should().BeEmpty();
            parsedHumanName.Last.Should().Be("Cher");   // Should this be First instead of Last?
            parsedHumanName.Middle.Should().BeEmpty();
            parsedHumanName.Nicknames.Should().BeEmpty();
            parsedHumanName.Suffix.Should().BeEmpty();
            parsedHumanName.Title.Should().BeEmpty();
            parsedHumanName.LeadingInitial.Should().BeEmpty();
        }

        [Fact]
        public void HumanNameParser_LeadingInitial()
        {
            CoreParsedHumanName parsedHumanName = this._humanNameParser.ParseHumanName("J. Walter Weatherman");

            parsedHumanName.Should().NotBeNull().And.Subject.Should().BeOfType<CoreParsedHumanName>();
            parsedHumanName.First.Should().Be("Walter");
            parsedHumanName.Last.Should().Be("Weatherman");
            parsedHumanName.Middle.Should().BeEmpty();
            parsedHumanName.Nicknames.Should().BeEmpty();
            parsedHumanName.Suffix.Should().BeEmpty();
            parsedHumanName.Title.Should().BeEmpty();
            parsedHumanName.LeadingInitial.Should().Be("J.");
        }

        [Fact]
        public void HumanNameParser_LeadingInitial_Name_SameLetter()
        {
            CoreParsedHumanName parsedHumanName = this._humanNameParser.ParseHumanName("A Anderson");

            parsedHumanName.Should().NotBeNull().And.Subject.Should().BeOfType<CoreParsedHumanName>();
            parsedHumanName.First.Should().BeEmpty();
            parsedHumanName.Last.Should().Be("Anderson");
            parsedHumanName.Middle.Should().BeEmpty();
            parsedHumanName.Nicknames.Should().BeEmpty();
            parsedHumanName.Suffix.Should().BeEmpty();
            parsedHumanName.Title.Should().BeEmpty();
            parsedHumanName.LeadingInitial.Should().Be("A");
        }

        [Fact]
        public void HumanNameParser_LastComma()
        {
            CoreParsedHumanName parsedHumanName = this._humanNameParser.ParseHumanName("de la Cruz, Ana M.");

            parsedHumanName.Should().NotBeNull().And.Subject.Should().BeOfType<CoreParsedHumanName>();
            parsedHumanName.First.Should().Be("Ana");
            parsedHumanName.Last.Should().Be("de la Cruz");
            parsedHumanName.Middle.Should().Be("M.");
            parsedHumanName.Nicknames.Should().BeEmpty();
            parsedHumanName.Suffix.Should().BeEmpty();
            parsedHumanName.Title.Should().BeEmpty();
            parsedHumanName.LeadingInitial.Should().BeEmpty();
        }

        [Fact]
        public void HumanNameParser_Title()
        {
            CoreParsedHumanName parsedHumanName = this._humanNameParser.ParseHumanName("Mr. William R. De La Cruz III");

            parsedHumanName.Should().NotBeNull().And.Subject.Should().BeOfType<CoreParsedHumanName>();
            parsedHumanName.First.Should().Be("William");
            parsedHumanName.Last.Should().Be("De La Cruz");
            parsedHumanName.Middle.Should().Be("R.");
            parsedHumanName.Nicknames.Should().BeEmpty();
            parsedHumanName.Suffix.Should().Be("III");
            parsedHumanName.Title.Should().Be("Mr.");
            parsedHumanName.LeadingInitial.Should().BeEmpty();
        }

        [Theory]
        [InlineData("", "", "", "", "", "", "", "")]
        [InlineData("Björn O'Malley", "", "", "Björn", "", "", "O'Malley", "")]
        [InlineData("Bin Lin", "", "", "Bin", "", "", "Lin", "")]
        [InlineData("Linda Jones", "", "", "Linda", "", "", "Jones", "")]
        [InlineData("Jason H. Priem", "", "", "Jason", "", "H.", "Priem", "")]
        [InlineData("Björn O'Malley-Muñoz", "", "", "Björn", "", "", "O'Malley-Muñoz", "")]
        [InlineData("Björn C. O'Malley", "", "", "Björn", "", "C.", "O'Malley", "")]
        [InlineData("Björn \"Bill\" O'Malley", "", "", "Björn", "Bill", "", "O'Malley", "")]
        [InlineData("Colin \"Bomber\" Harris", "", "", "Colin", "Bomber", "", "Harris", "")]
        [InlineData("Björn (\"Bill\") O'Malley", "", "", "Björn", "Bill", "", "O'Malley", "")]
        [InlineData("Björn (\"Wild Bill\") O'Malley", "", "", "Björn", "Wild Bill", "", "O'Malley", "")]
        [InlineData("Björn (Bill) O'Malley", "", "", "Björn", "Bill", "", "O'Malley", "")]
        [InlineData("Björn 'Bill' O'Malley", "", "", "Björn", "Bill", "", "O'Malley", "")]
        [InlineData("Björn C O'Malley", "", "", "Björn", "", "C", "O'Malley", "")]
        [InlineData("Björn C. R. O'Malley", "", "", "Björn", "", "C. R.", "O'Malley", "")]
        [InlineData("Björn Charles O'Malley", "", "", "Björn", "", "Charles", "O'Malley", "")]
        [InlineData("Björn Charles R. O'Malley", "", "", "Björn", "", "Charles R.", "O'Malley", "")]
        [InlineData("Björn van O'Malley", "", "", "Björn", "", "", "van O'Malley", "")]
        [InlineData("Björn Charles van der O'Malley", "", "", "Björn", "", "Charles", "van der O'Malley", "")]
        [InlineData("Björn Charles O'Malley y Muñoz", "", "", "Björn", "", "Charles", "O'Malley y Muñoz", "")]
        [InlineData("Björn O'Malley, Jr.", "", "", "Björn", "", "", "O'Malley", "Jr.")]
        [InlineData("Björn O'Malley Jr", "", "", "Björn", "", "", "O'Malley", "Jr")]
        [InlineData("B O'Malley", "", "", "B", "", "", "O'Malley", "")]
        [InlineData("William Carlos Williams", "", "", "William", "", "Carlos", "Williams", "")]
        [InlineData("C. Björn Roger O'Malley", "", "C.", "Björn", "", "Roger", "O'Malley", "")]
        [InlineData("C. Ben O'Malley", "", "C.", "Ben", "", "", "O'Malley", "")]
        [InlineData("B. C. O'Malley", "", "", "B.", "", "C.", "O'Malley", "")]
        [InlineData("B C O'Malley", "", "", "B", "", "C", "O'Malley", "")]
        [InlineData("B.J. Thomas", "", "", "B.J.", "", "", "Thomas", "")]
        [InlineData("O'Malley, Björn", "", "", "Björn", "", "", "O'Malley", "")]
        [InlineData("O'Malley, Björn Jr", "", "", "Björn", "", "", "O'Malley", "Jr")]
        [InlineData("O'Malley, C. Björn", "", "C.", "Björn", "", "", "O'Malley", "")]
        [InlineData("O'Malley, C. Björn III", "", "C.", "Björn", "", "", "O'Malley", "III")]
        [InlineData("O'Malley y Muñoz, C. Björn Roger III", "", "C.", "Björn", "", "Roger", "O'Malley y Muñoz", "III")]
        [InlineData("James Hugh Calum Laurie", "", "", "James", "", "Hugh Calum", "Laurie", "")]

        // [InlineData("O'Malley Jr., Björn", "", "", "Björn", "", "", "O'Malley", "Jr")]
        [InlineData("Mr. William R. Hearst, III", "Mr.", "", "William", "", "R.", "Hearst", "III")]
        [InlineData("BENTLEY, E RANDOLPH", "", "E", "RANDOLPH", "", "", "BENTLEY", "")]
        [InlineData("WARD, C LAVON", "", "C", "LAVON", "", "", "WARD", "")]
        [InlineData("BOWER, N RUSSELL", "", "N", "RUSSELL", "", "", "BOWER", "")]
        public void HumanNameParser_ParseHumanName(string testName, string title, string firstInitial, string firstName, string nickNames, string middleName, string lastName, string suffix)
        {
            CoreParsedHumanName parsedHumanName = this._humanNameParser.ParseHumanName(testName);

            parsedHumanName.Should().NotBeNull().And.Subject.Should().BeOfType<CoreParsedHumanName>();
            parsedHumanName.First.Should().Be(firstName);
            parsedHumanName.Last.Should().Be(lastName);
            parsedHumanName.Middle.Should().Be(middleName);
            parsedHumanName.Nicknames.Should().Be(nickNames);
            parsedHumanName.Suffix.Should().Be(suffix);
            parsedHumanName.Title.Should().Be(title);
            parsedHumanName.LeadingInitial.Should().Be(firstInitial);
        }

        [Theory]
        [InlineData("  abc  def  ghi   ", "abc def ghi")]
        public void HumanNameParser_NormalizeName(string testName, string testResult)
        {
            this._humanNameParser.NormalizeName(testName).Should().Be(testResult);
        }

        [Theory]
        [InlineData("", "", ',')]
        [InlineData(null, "", ',')]
        [InlineData("John Smith", "John Smith", ',')]
        [InlineData("Smith, John", "John Smith", ',')]
        [InlineData("Smith,John", "John Smith", ',')]
        [InlineData("Smith,    John    ", "John Smith", ',')]
        public void HumanNameParser_FlipName(string? testName, string testResult, char flipChar)
        {
            this._humanNameParser.FlipName(testName, flipChar).Should().Be(testResult);
        }

        [Fact]
        public void HumanNameParser_Comma_NoSpace()
        {
            CoreParsedHumanName parsedHumanName = this._humanNameParser.ParseHumanName("Smith,John");

            parsedHumanName.Should().NotBeNull().And.Subject.Should().BeOfType<CoreParsedHumanName>();
            parsedHumanName.First.Should().Be("John");
            parsedHumanName.Last.Should().Be("Smith");
            parsedHumanName.Middle.Should().BeEmpty();
            parsedHumanName.Nicknames.Should().BeEmpty();
            parsedHumanName.Suffix.Should().BeEmpty();
            parsedHumanName.Title.Should().BeEmpty();
            parsedHumanName.LeadingInitial.Should().BeEmpty();
        }
    }
}
