// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreStringExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

    /// <summary>
    /// Class StringExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreStringExtensionsUnitTests))]

    public class CoreStringExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreStringExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreStringExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreStringExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToCamelCase.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("Value", "value")]
        [InlineData("V", "v")]
        [InlineData("\t", "\t")]
        public void StringExtensions_ToCamelCase(string testString, string expectedString)
        {
            testString.ToCamelCase().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToPascalCase.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("value", "Value")]
        [InlineData("v", "V")]
        [InlineData("\t", "\t")]
        public void StringExtensions_ToPascalCase(string testString, string expectedString)
        {
            testString.ToPascalCase().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToAlphaNumeric.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("123*1%_ABC", "1231ABC")]
        [InlineData("_", "")]
        [InlineData("abc", "abc")]
        [InlineData("abc\t\n", "abc")]
        public void StringExtensions_ToAlphaNumeric(string testString, string expectedString)
        {
            testString.ToAlphaNumeric().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveWhitespace.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("123 1 ABC", "1231ABC")]
        [InlineData(" ", "")]
        [InlineData("\n", "")]
        [InlineData("\t", "")]
        [InlineData("abc", "abc")]
        [InlineData("abc\t\n", "abc")]
        public void StringExtensions_RemoveWhitespace(string testString, string expectedString)
        {
            testString.RemoveWhitespace().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToNumeric.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("123 1 ABC", "1231")]
        [InlineData(" ", "")]
        [InlineData("\n", "")]
        [InlineData("\t", "")]
        [InlineData("abc", "")]
        [InlineData("123\t\n", "123")]
        public void StringExtensions_ToNumeric(string testString, string expectedString)
        {
            testString.ToNumeric().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_TrimEveryLine.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("1231 \n", "1231")]
        [InlineData("1231 \r\n 42 324 324 ", "1231\n42 324 324")]
        [InlineData(" \r\n ", "")]
        [InlineData(" \r\n 1", "1")]
        [InlineData(" \r\n 1 \r\n\t 2", "1\n2")]
        public void StringExtensions_TrimEveryLine(string testString, string expectedString)
        {
            testString.TrimEveryLine().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_SplitOnFirstDelim.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="chDelim">The ch delimiter.</param>
        /// <param name="expectStrings">The expect strings.</param>
        [Theory]
        [InlineData("First|Last", '|', new[] { "First", "Last" })]
        [InlineData("First|Middle|Last", '|', new[] { "First", "Middle|Last" })]
        [InlineData("First", '|', new[] { "First", "" })]
        [InlineData(" First | Last ", '|', new[] { "First", "Last" })]
        [InlineData(" First |  ", '|', new[] { "First", "" })]
        [InlineData("| Last ", '|', new[] { "", "Last" })]
        public void StringExtensions_SplitOnFirstDelim(string testString, char chDelim, string[] expectStrings)
        {
            string[] result = testString.SplitOnFirstDelim(chDelim);

            result.Should().NotBeNull().And.Subject.Should().AllBeOfType(typeof(string)).And.Subject.Count().Should()
                .Be(2);
            result.Should().ContainInOrder(expectStrings);
        }

        /// <summary>
        /// Defines the test method StringExtensions_SplitOnLastDelim.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="chDelim">The ch delimiter.</param>
        /// <param name="expectStrings">The expect strings.</param>
        [Theory]
        [InlineData("First|Last", '|', new[] { "First", "Last" })]
        [InlineData("First|Middle|Last", '|', new[] { "First|Middle", "Last" })]
        [InlineData("First", '|', new[] { "First", "" })]
        [InlineData(" First | Last ", '|', new[] { "First", "Last" })]
        [InlineData(" First |  ", '|', new[] { "First", "" })]
        [InlineData("| Last ", '|', new[] { "", "Last" })]
        public void StringExtensions_SplitOnLastDelim(string testString, char chDelim, string[] expectStrings)
        {
            string[] result = testString.SplitOnLastDelim(chDelim);

            result.Should().NotBeNull().And.Subject.Should().AllBeOfType(typeof(string)).And.Subject.Count().Should()
                .Be(2);
            result.Should().ContainInOrder(expectStrings);
        }

        /// <summary>
        /// Defines the test method StringExtensions_NullIfWhitespace.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("1231 \n", "1231")]
        [InlineData("1231 \r\n 42 324 324 ", "1231 \r\n 42 324 324")]
        [InlineData(" \r\n ", null)]
        [InlineData(" \t\r\n ", null)]
        [InlineData(" \r\n 1", "1")]
        public void StringExtensions_NullIfWhitespace(string testString, string? expectedString)
        {
            testString.NullIfWhitespace().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_StringBeforeDelim.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="chDelim">The ch delimiter.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("First|Last", '|', "First")]
        [InlineData("First|Last", '*', "First|Last")]
        [InlineData("|Last", '|', "")]
        public void StringExtensions_StringBeforeDelim(string testString, char chDelim, string expectedString)
        {
            testString.StringBeforeDelim(chDelim).Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_StringAfterDelim.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="chDelim">The ch delimiter.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("First|Last", '|', "Last")]
        [InlineData("First|Last", '*', "")]
        [InlineData("|Last", '|', "Last")]
        [InlineData("First|", '|', "")]
        public void StringExtensions_StringAfterDelim(string testString, char chDelim, string expectedString)
        {
            testString.StringAfterDelim(chDelim).Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_StringBeforeLastDelim.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="chDelim">The ch delimiter.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("First|Middle|Last", '|', "First|Middle")]
        [InlineData("First|Last", '*', "First|Last")]
        [InlineData("|Last", '|', "")]
        [InlineData("Last|", '|', "Last")]
        public void StringExtensions_StringBeforeLastDelim(string testString, char chDelim, string expectedString)
        {
            testString.StringBeforeLastDelim(chDelim).Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_StringAfterLastDelim.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="chDelim">The ch delimiter.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("First|Middle|Last", '|', "Last")]
        [InlineData("First|Middle|Last", '*', "")]
        [InlineData("|Last", '|', "Last")]
        [InlineData("First|", '|', "")]
        public void StringExtensions_StringAfterLastDelim(string testString, char chDelim, string expectedString)
        {
            testString.StringAfterLastDelim(chDelim).Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ParseYesNo.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedBool">if set to <see langword="true"/> [expected bool].</param>
        [Theory]
        [InlineData("Yes", true)]
        [InlineData("Y", true)]
        [InlineData("No", false)]
        [InlineData("N", false)]
        [InlineData("Yellow", true)]
        [InlineData("Nope", false)]
        public void StringExtensions_ParseYesNo(string testString, bool? expectedBool)
        {
            testString.ParseYesNo().Should().Be(expectedBool);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ParseYesNoStrict.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedBool">if set to <see langword="true"/> [expected bool].</param>
        [Theory]
        [InlineData("Yes", true)]
        [InlineData("No", false)]
        [InlineData("Y", null)]
        [InlineData("N", null)]
        [InlineData("Yellow", null)]
        [InlineData("Nope", null)]
        public void StringExtensions_ParseYesNoStrict(string testString, bool? expectedBool)
        {
            testString.ParseYesNoStrict().Should().Be(expectedBool);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ParseDoubleOrNull.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedDouble">The expected double.</param>
        [Theory]
        [InlineData("1.0", 1.0)]
        [InlineData(".1", .1)]
        [InlineData("15", 15.0)]
        [InlineData("-1", -1.0)]
        [InlineData(" 15 ", 15.0)]
        [InlineData(" -1 ", -1.0)]
        [InlineData(" - 1 ", null)]
        [InlineData("1/3", null)]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(" ", null)]
        public void StringExtensions_ParseDoubleOrNull(string? testString, double? expectedDouble)
        {
            testString.ParseDoubleOrNull().Should().Be(expectedDouble);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ParseIntOrNull.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedInt">The expected int.</param>
        [Theory]
        [InlineData("1.0", null)]
        [InlineData(".1", null)]
        [InlineData("15", 15)]
        [InlineData("-1", -1)]
        [InlineData("1/3", null)]
        [InlineData(" 15 ", 15)]
        [InlineData(" -1 ", -1)]
        [InlineData(" - 1 ", null)]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(" ", null)]
        public void StringExtensions_ParseIntOrNull(string? testString, int? expectedInt)
        {
            testString.ParseIntOrNull().Should().Be(expectedInt);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ParseLongOrNull.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedLong">The expected long.</param>
        [Theory]
        [InlineData("1.0", null)]
        [InlineData(".1", null)]
        [InlineData("15", 15L)]
        [InlineData("-1", -1L)]
        [InlineData("1/3", null)]
        [InlineData(" 15 ", 15L)]
        [InlineData(" -1 ", -1L)]
        [InlineData(" - 1 ", null)]
        [InlineData(null, null)]
        [InlineData("", null)]
        [InlineData(" ", null)]
        public void StringExtensions_ParseLongOrNull(string? testString, long? expectedLong)
        {
            testString.ParseLongOrNull().Should().Be(expectedLong);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToDoubleQuote.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData(null, "\"\"")]
        [InlineData("", "\"\"")]
        [InlineData(" ", "\" \"")]
        [InlineData("Test", "\"Test\"")]
        public void StringExtensions_ToDoubleQuote(string? testString, string expectedString)
        {
            testString.ToDoubleQuoted().Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_PadWithDelim.
        /// </summary>
        /// <param name="testString">The test string.</param>
        /// <param name="delimString">The delimiter string.</param>
        /// <param name="length">The length.</param>
        /// <param name="expectedString">The expected string.</param>
        [Theory]
        [InlineData("Property", ":", 20, "Property:           ")]
        [InlineData("Property", ":", 5, "Property:")]
        [InlineData(null, ":", 5, ":    ")]
        [InlineData(null, null, 5, "")]
        public void StringExtensions_PadWithDelim(string? testString, string? delimString, int length, string expectedString)
        {
            testString.PadWithDelim(delimString, length).Should().Be(expectedString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_NullIfWhitespace_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_NullIfWhitespace_Empty()
        {
            string.Empty.NullIfWhitespace().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_NullIfWhitespace_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_NullIfWhitespace_Null()
        {
            ((string)null).NullIfWhitespace().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_NullIfWhitespace_Space.
        /// </summary>
        [Fact]
        public void StringExtensions_NullIfWhitespace_Space()
        {
            " ".NullIfWhitespace().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ParseYesNo_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_ParseYesNo_Empty()
        {
            string.Empty.ParseYesNo().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ParseYesNo_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_ParseYesNo_Null()
        {
            ((string)null).ParseYesNo().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ParseYesNoStrict_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_ParseYesNoStrict_Empty()
        {
            string.Empty.ParseYesNoStrict().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ParseYesNoStrict_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_ParseYesNoStrict_Null()
        {
            ((string)null).ParseYesNoStrict().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ProcessSplits_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_ProcessSplits_Null()
        {
            MethodInfo? methodInfo =
                typeof(StringExtensions).GetMethod("ProcessSplits", BindingFlags.NonPublic | BindingFlags.Static);

            object[] parameters = { null, 1 };

            methodInfo.Should().NotBeNull();
            nameof(methodInfo).Should().NotBeNullOrEmpty();

            string[]? result = (string[])methodInfo?.Invoke(null, parameters);

            result.Should().NotBeNull().And.Subject.Should().BeOfType(typeof(string[])).And.Subject.Count().Should()
                .Be(2);
            result?[0].Should().BeNull();
            result?[1].Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveWhitespace_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveWhitespace_Empty()
        {
            string.Empty.RemoveWhitespace().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveWhitespace_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveWhitespace_Null()
        {
            ((string)null).RemoveWhitespace().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_SplitOnFirstDelim_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_SplitOnFirstDelim_Empty()
        {
            string[] result = string.Empty.SplitOnFirstDelim('|');
            result.Should().NotBeNull().And.Subject.Should().BeOfType(typeof(string[])).And.Subject.Count().Should()
                .Be(2);
            result[0].Should().BeNull();
            result[1].Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_SplitOnFirstDelim_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_SplitOnFirstDelim_Null()
        {
            string[] result = ((string)null).SplitOnFirstDelim('|');

            result.Should().NotBeNull().And.Subject.Should().BeOfType(typeof(string[])).And.Subject.Count().Should()
                .Be(2);
            result[0].Should().BeNull();
            result[1].Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_SplitOnLastDelim_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_SplitOnLastDelim_Empty()
        {
            string[] result = string.Empty.SplitOnLastDelim('|');
            result.Should().NotBeNull().And.Subject.Should().BeOfType(typeof(string[])).And.Subject.Count().Should()
                .Be(2);
            result[0].Should().BeNull();
            result[1].Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_SplitOnLastDelim_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_SplitOnLastDelim_Null()
        {
            string[] result = ((string)null).SplitOnLastDelim('|');

            result.Should().NotBeNull().And.Subject.Should().BeOfType(typeof(string[])).And.Subject.Count().Should()
                .Be(2);
            result[0].Should().BeNull();
            result[1].Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_StringAfterDelim_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_StringAfterDelim_Empty()
        {
            string.Empty.StringAfterDelim('|').Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_StringAfterDelim_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_StringAfterDelim_Null()
        {
            ((string)null).StringAfterDelim('|').Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_StringBeforeDelim_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_StringBeforeDelim_Empty()
        {
            string.Empty.StringBeforeDelim('|').Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_StringBeforeDelim_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_StringBeforeDelim_Null()
        {
            ((string)null).StringBeforeDelim('|').Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToAlphaNumeric_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_ToAlphaNumeric_Empty()
        {
            string.Empty.ToAlphaNumeric().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToAlphaNumeric_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_ToAlphaNumeric_Null()
        {
            ((string)null).ToAlphaNumeric().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToCamelCase_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_ToCamelCase_Empty()
        {
            string.Empty.ToCamelCase().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToCamelCase_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_ToCamelCase_Null()
        {
            ((string)null).ToCamelCase().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToNumeric_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_ToNumeric_Empty()
        {
            string.Empty.ToNumeric().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToNumeric_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_ToNumeric_Null()
        {
            ((string)null).ToNumeric().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToPascalCase_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_ToPascalCase_Empty()
        {
            string.Empty.ToPascalCase().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToPascalCase_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_ToPascalCase_Null()
        {
            ((string)null).ToPascalCase().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_TrimEveryLine_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_TrimEveryLine_Empty()
        {
            string.Empty.TrimEveryLine().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_TrimEveryLine_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_TrimEveryLine_Null()
        {
            ((string)null).TrimEveryLine().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ReplaceLastChar.
        /// </summary>
        [Fact]
        public void StringExtensions_ReplaceLastChar()
        {
            "Foo".ReplaceLastChar('x').Should().Be("Fox");
        }

        /// <summary>
        /// Defines the test method StringExtensions_ReplaceLastChar_SingleChar.
        /// </summary>
        [Fact]
        public void StringExtensions_ReplaceLastChar_SingleChar()
        {
            "F".ReplaceLastChar('x').Should().Be("x");
        }

        /// <summary>
        /// Defines the test method StringExtensions_ReplaceLastChar_EmptyString.
        /// </summary>
        [Fact]
        public void StringExtensions_ReplaceLastChar_EmptyString()
        {
            string.Empty.ReplaceLastChar('x').Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ReplaceLastChar_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_ReplaceLastChar_Null()
        {
            ((string)null).ReplaceLastChar('x').Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveLastChar.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveLastChar()
        {
            "Foo".RemoveLastChar().Should().Be("Fo");
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveLastChar_SingleChar.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveLastChar_SingleChar()
        {
            "F".RemoveLastChar().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveLastChar_EmptyString.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveLastChar_EmptyString()
        {
            string.Empty.RemoveLastChar().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveLastChar_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveLastChar_Null()
        {
            ((string)null).RemoveLastChar().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveTrailingPathSeparator_Backslash.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveTrailingPathSeparator_Backslash()
        {
            "Foo\\".RemoveTrailingPathSeparator().Should().Be("Foo");
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveTrailingPathSeparator_Slash.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveTrailingPathSeparator_Slash()
        {
            "Foo/".RemoveTrailingPathSeparator().Should().Be("Foo");
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveTrailingPathSeparator_SingleChar_Backslash.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveTrailingPathSeparator_SingleChar_Backslash()
        {
            "\\".RemoveTrailingPathSeparator().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveTrailingPathSeparator_SingleChar_Slash.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveTrailingPathSeparator_SingleChar_Slash()
        {
            "/".RemoveTrailingPathSeparator().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveTrailingPathSeparator_NoSlash.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveTrailingPathSeparator_NoSlash()
        {
            "Foo".RemoveTrailingPathSeparator().Should().Be("Foo");
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveTrailingPathSeparator_EmptyString.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveTrailingPathSeparator_EmptyString()
        {
            string.Empty.RemoveTrailingPathSeparator().Should().Be(string.Empty);
        }

        /// <summary>
        /// Defines the test method StringExtensions_RemoveTrailingPathSeparator_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_RemoveTrailingPathSeparator_Null()
        {
            ((string)null).RemoveTrailingPathSeparator().Should().BeNull();
        }

        /// <summary>
        /// Defines the test method StringExtensions_Contains.
        /// </summary>
        [Fact]
        public void StringExtensions_Contains()
        {
            StringExtensions.Contains("TestString", "Test", StringComparison.InvariantCulture).Should().BeTrue();
            StringExtensions.Contains("TestString", "test", StringComparison.InvariantCulture).Should().BeFalse();
            StringExtensions.Contains("TestString", "test", StringComparison.InvariantCultureIgnoreCase).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method StringExtensions_Contains_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_Contains_Null()
        {
            StringExtensions.Contains(null, null, StringComparison.InvariantCulture).Should().BeFalse();
            StringExtensions.Contains(null, "TestString", StringComparison.InvariantCulture).Should().BeFalse();
            StringExtensions.Contains("TestString", null, StringComparison.InvariantCulture).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ZeroFillNumber.
        /// </summary>
        /// <param name="numberString">Number string to test.</param>
        /// <param name="minLength">Minimum length.</param>
        /// <param name="expectedValue">Expected result.</param>
        [Theory]
        [InlineData("32", 4, "0032")]
        [InlineData("0", 0, "0")]
        [InlineData("-32", 4, "-0032")]
        [InlineData("-32", 2, "-32")]
        [InlineData("-32", 1, "-32")]
        [InlineData("", 1, "")]
        [InlineData(null, 1, null)]
        public void StringExtensions_ZeroFillNumber(string? numberString, int minLength, string? expectedValue)
        {
            numberString.ZeroFillNumber(minLength).Should().Be(expectedValue);
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToBase64.
        /// </summary>
        /// <param name="numberString">Number string to test.</param>
        /// <param name="encodingString">Encoding type as string.</param>
        /// <param name="expectedValue">Expected result.</param>
        [ExcludeFromCodeCoverage]
        [Theory]
        [InlineData("This is a string", "UTF8", "VGhpcyBpcyBhIHN0cmluZw==")]
        [InlineData("This is a string", "ASCII", "VGhpcyBpcyBhIHN0cmluZw==")]
        [InlineData("This is a string", "BigEndianUnicode", "AFQAaABpAHMAIABpAHMAIABhACAAcwB0AHIAaQBuAGc=")]
        [InlineData("This is a string", "Unicode", "VABoAGkAcwAgAGkAcwAgAGEAIABzAHQAcgBpAG4AZwA=")]
        [InlineData("This is a string", "UTF32", "VAAAAGgAAABpAAAAcwAAACAAAABpAAAAcwAAACAAAABhAAAAIAAAAHMAAAB0AAAAcgAAAGkAAABuAAAAZwAAAA==")]
        [InlineData("", "UTF8", "")]
        [InlineData(null, "UTF8", null)]
        public void StringExtensions_ToBase64(string? numberString, string encodingString, string? expectedValue)
        {
            Encoding encoding = encodingString switch
            {
                "UTF8" => Encoding.UTF8,
                "ASCII" => Encoding.ASCII,
                "BigEndianUnicode" => Encoding.BigEndianUnicode,
                "Unicode" => Encoding.Unicode,
                "UTF32" => Encoding.UTF32,
                _ => Encoding.UTF8,
            };

            numberString.ToBase64(encoding).Should().Be(expectedValue);
            expectedValue.TryParseBase64(encoding, out string? expectedNumberString).Should().Be(!string.IsNullOrEmpty(expectedValue));
            expectedNumberString.Should().Be(numberString);
        }

        /// <summary>
        /// Defines the test method StringExtensions_TryParseBase64_InvalidString.
        /// </summary>
        [Fact]
        public void StringExtensions_TryParseBase64_InvalidString()
        {
            "abc".TryParseBase64(out _).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method StringExtensions_TryParseBase64_Encoding_InvalidString.
        /// </summary>
        [Fact]
        public void StringExtensions_TryParseBase64_Encoding_InvalidString()
        {
            "abc".TryParseBase64(Encoding.ASCII, out _).Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToBase64_UTF8.
        /// </summary>
        [Fact]
        public void StringExtensions_ToBase64_UTF8()
        {
            "This is a string".ToBase64().Should().Be("VGhpcyBpcyBhIHN0cmluZw==");
        }

        /// <summary>
        /// Defines the test method StringExtensions_ToPropertyValueDictionary.
        /// </summary>
        /// <param name="input">Number string to test.</param>
        /// <param name="output">Encoding type as string.</param>
        /// <param name="chDelim">Delimiter.</param>
        /// <param name="count">Count.</param>
        /// <param name="upperCaseProperty">Uppercase property.</param>
        /// <param name="upperCaseValue">Uppercase value.</param>
        [Theory]
        [InlineData("", "", ' ', 0, false, false)]
        [InlineData("property value", "property value", ' ', 1, false, false)]
        [InlineData("property value", "PROPERTY value", ' ', 1, true, false)]
        [InlineData("property value", "PROPERTY VALUE", ' ', 1, true, true)]
        [InlineData("property1 value1 property2 value2", "property1 value1 property2 value2", ' ', 2, false, false)]
        [InlineData("property1 value1 property2 value2", "PROPERTY1 value1 PROPERTY2 value2", ' ', 2, true, false)]
        [InlineData("property1 value1 property2 value2", "PROPERTY1 VALUE1 PROPERTY2 VALUE2", ' ', 2, true, true)]
        [InlineData("property1 value1 property2 value2 property3", "PROPERTY1 VALUE1 PROPERTY2 VALUE2", ' ', 2, true, true)]
        [InlineData("property1", "", ' ', 0, true, true)]
        [InlineData("property1 ", "PROPERTY1 ", ' ', 1, true, true)]
        public void StringExtensions_ToPropertyValueDictionary(string input, string output, char chDelim, int count, bool upperCaseProperty, bool upperCaseValue)
        {
            IDictionary<string, string> dict = input.ToPropertyValueDictionary(chDelim, upperCaseProperty, upperCaseValue);

            dict.Count.Should().Be(count);
            var sb = new StringBuilder();

            foreach (KeyValuePair<string, string> item in dict)
            {
                sb.Append($"{item.Key}{chDelim}{item.Value}{chDelim}");
            }

            if (sb.Length > 0)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            sb.ToString().Should().Be(output);
        }

        /// <summary>
        /// Defines the test method StringExtensions_CenterTitleString.
        /// </summary>
        [Fact]
        public void StringExtensions_CenterTitleString()
        {
            this.TestOutputHelper.WriteLine("Title".CenterTitle('*', '*', 4));
        }

        /// <summary>
        /// Defines the test method StringExtensions_CenterTitleString_Empty.
        /// </summary>
        [Fact]
        public void StringExtensions_CenterTitleString_Empty()
        {
            string.Empty.CenterTitle('*', '*', 4).Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method StringExtensions_CenterTitleString_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_CenterTitleString_Null()
        {
            ((string)null).CenterTitle('*', '*', 4).Should().BeEmpty();
        }

        /// <summary>
        /// Tests the <see cref="NetworkVisor.Core.Extensions.StringExtensions.CenterTitles"/> method by providing various input parameters.
        /// </summary>
        /// <param name="titleChar">The character used for the title.</param>
        /// <param name="indentChar">The character used for indentation.</param>
        /// <param name="indent">The number of indentations.</param>
        [Theory]
        [InlineData('=', ' ', 2)]
        [InlineData('*', '*', 1)]
        public void StringExtensions_CenterTitleStrings(char titleChar, char indentChar, int indent)
        {
            DateTime dateTimeNow = DateTime.UtcNow;
            string[] strings = new string[]
            {
                $"{this.TestDisplayName}",
                $"{this.TestClassType.GetTraitOperatingSystem()} {this.TestClassType.GetTraitTestType()} ({this.TestOperatingSystem.FrameworkInfo.BuiltFrameworkType.ToString()} {this.TestOperatingSystem.FrameworkInfo.BuiltFrameworkVersion?.ToString()})",
                $"S: {this.TestOutputHelper.DateTimeStart:o}",
                $"E: {dateTimeNow:o}",
                $"D: {dateTimeNow - this.TestOutputHelper.DateTimeStart:c}",
            };

            this.TestOutputHelper.WriteLine(strings.CenterTitles(titleChar, indentChar, indent));
        }

        /// <summary>
        /// Defines the test method StringExtensions_CenterTitleStrings_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_CenterTitleStrings_EmptyString()
        {
            string[] strings = [string.Empty];
            this.TestOutputHelper.WriteLine(strings.CenterTitles('*', '*', 1));
        }

        /// <summary>
        /// Defines the test method StringExtensions_CenterTitleStrings_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_CenterTitleStrings_OneChar()
        {
            string[] strings = ["1"];
            this.TestOutputHelper.WriteLine(strings.CenterTitles('*', '*', 1));
        }

        /// <summary>
        /// Defines the test method StringExtensions_CenterTitleStrings_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_CenterTitleStrings_TwoChar()
        {
            string[] strings = ["12"];
            this.TestOutputHelper.WriteLine(strings.CenterTitles('*', '*', 1));
        }

        /// <summary>
        /// Defines the test method StringExtensions_CenterTitleStrings_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_CenterTitleStrings_ZeroIdent()
        {
            string[] strings = ["12"];
            this.TestOutputHelper.WriteLine(strings.CenterTitles('*', '*', 0));
        }

        /// <summary>
        /// Defines the test method StringExtensions_CenterTitleStrings_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_CenterTitleStrings_Empty()
        {
            Array.Empty<string>().CenterTitles('*', '*', 4).Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test method StringExtensions_CenterTitleStrings_Null.
        /// </summary>
        [Fact]
        public void StringExtensions_CenterTitleStrings_Null()
        {
            ((string[])null).CenterTitles('*', '*', 4).Should().BeEmpty();
        }

        [Theory]
        [InlineData("10.1.10.10/8", "10.1.10.10", "255.0.0.0")]
        [InlineData("10.1.10.10/16", "10.1.10.10", "255.255.0.0")]
        [InlineData("10.1.10.10/24", "10.1.10.10", "255.255.255.0")]
        [InlineData("10.1.10.10/30", "10.1.10.10", "255.255.255.252")]
        [InlineData("10.1.10.10/32", "10.1.10.10", "255.255.255.255")]
        [InlineData("10.1.10.10", "10.1.10.10", "255.255.255.255")]
        [InlineData("10.1.10.", null, "255.255.255.255")]
        public void StringExtensions_ToIPAddressSubnet(string testString, string? expectedIPAddressString, string expectedSubnetMaskString)
        {
            CoreIPAddressSubnet? ipAddressSubnet = testString.ToIPAddressSubnet();

            if (expectedIPAddressString is null)
            {
                ipAddressSubnet.Should().BeNull();
            }
            else
            {
                ipAddressSubnet.Should().NotBeNull();
                this.TestOutputHelper.WriteLine(ipAddressSubnet.ToString());
                ipAddressSubnet.IPAddress.Should().Be(IPAddress.Parse(expectedIPAddressString));
                ipAddressSubnet.SubnetMask.Should().Be(IPAddress.Parse(expectedSubnetMaskString));
            }
        }
    }
}
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
