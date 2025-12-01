// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreEnumerableExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CoreEnumerableExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreEnumerableExtensionsUnitTests))]

    public class CoreEnumerableExtensionsUnitTests : CoreTestCaseBase
    {
        private readonly List<int> randomInts = [1, 2, 3, 4, 5, 6];
        private readonly List<string> randomStrings = ["a", "b", "c", "d", "e", "f"];
        private readonly List<char> randomChars = ['a', 'b', 'c', 'd', 'e', 'f'];

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEnumerableExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreEnumerableExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreEnumerableExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_MaxStringLength_Empty.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_MaxStringLength_Empty()
        {
            int maxLength = new List<string>().MaxStringLength();

            maxLength.Should().Be(0);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_MaxStringLength_IntList.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_MaxStringLength_IntList()
        {
            var intList = new List<int>
            {
                12345,
                123456,
                123,
                15,
            };

            int maxLength = intList.MaxStringLength();

            maxLength.Should().Be("123456".Length);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_MaxStringLength_Null.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_MaxStringLength_Null()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            int maxLength = ((List<string>)null).MaxStringLength();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.

            maxLength.Should().Be(0);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_MaxStringLength_StringList.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_MaxStringLength_StringList()
        {
            var stringList = new List<string>
            {
                "12345",
                "123456",
                "123",
                "15",
            };

            int maxLength = stringList.MaxStringLength();

            maxLength.Should().Be("123456".Length);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_MaxStringLength_TestObject.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_MaxStringLength_TestObject()
        {
            var testObjectList = new List<TestObject>
            {
                new(),
            };

            int maxLength = testObjectList.MaxStringLength();

            maxLength.Should().Be(typeof(TestObject).ToString().Length);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Random_Ints.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Random_Ints()
        {
            this.randomInts.Random().Should().Match(i => i >= 1 && i <= 6);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Random_Strings.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Random_Strings()
        {
            var random = new Random();
            this.randomStrings.Random(random, out bool foundItem).Should().Match<string>(s => s[0] >= 'a' && s[0] <= 'f');
            foundItem.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Random_Chars.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Random_Chars()
        {
            var random = new Random();
            this.randomChars.Random(random).Should().Match<char>(c => c.CompareTo('a') >= 0 && c.CompareTo('f') <= 0);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Random_TestObjects.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Random_TestObjects()
        {
            this.randomChars.Random(out bool foundItem).Should().NotBeNull();
            foundItem.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Random_Null.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Random_Null()
        {
            ((IEnumerable<int>)null!).Random().Should().Be(default);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Random_Found_Null.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Random_Found_Null()
        {
            ((IEnumerable<int>)null!).Random(out _).Should().Be(default);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Each.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Each()
        {
            var stringList = new List<string>
            {
                "12345",
                "123456",
                "123",
                "15",
            };

            stringList.Each(item => item.Should().StartWith("1"));
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Each_Output.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Each_Output()
        {
            var stringList = new List<string>
            {
                "12345",
                "123456",
                "123",
                "15",
            };

            stringList.Each(this.TestOutputHelper.WriteLine);
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Each_Null.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public void EnumerableExtensions_Each_Null()
        {
            ((IEnumerable<string>)null!).Each(item => item.Should().StartWith("1"));
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Each_Null.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Dequeue_Null()
        {
            Func<string> fx = () => ((SortedSet<string>)null!).Dequeue();
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("set");
        }

        /// <summary>
        /// Defines the test method EnumerableExtensions_Dequeue.
        /// </summary>
        [Fact]
        public void EnumerableExtensions_Dequeue()
        {
            var sortedSet = new SortedSet<string>()
            {
                "12345",
                "123",
                "15",
                "123456",
            };

            sortedSet.Dequeue().Should().Be("123");
            sortedSet.Dequeue().Should().Be("12345");
            sortedSet.Dequeue().Should().Be("123456");
            sortedSet.Dequeue().Should().Be("15");

            sortedSet.Count.Should().Be(0);
            sortedSet.Any().Should().BeFalse();

            Func<string> fx = () => sortedSet.Dequeue();
            fx.Should().Throw<InvalidOperationException>().And.Message.Should().BeOneOf("Sequence contains no elements", "NoElements");
        }

        /// <summary>
        /// Class TestObject.
        /// </summary>
        private class TestObject
        {
        }
    }
}
