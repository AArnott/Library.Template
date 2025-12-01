// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreLogPropertyUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Logging.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Logging.Types
{
    /// <summary>
    /// Class CoreLogPropertyUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreLogPropertyUnitTests))]

    public class CoreLogPropertyUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLogPropertyUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLogPropertyUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_String.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_String()
        {
            var coreLogProperty = new CoreLogProperty("property", "value", CoreLogPropertyType.StringValue);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().Be("value").And.Subject.Should().BeOfType<string>();
            coreLogProperty.ToString().Should().Be("value");
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_String_Null.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_String_Null()
        {
            var coreLogProperty = new CoreLogProperty("property", null, CoreLogPropertyType.StringValue);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeNull();
            coreLogProperty.ToString().Should().Be(StringExtensions.NullString);
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary_Methods.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary_Methods()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
            };

            loggableDictionary.Count.Should().Be(2);
            loggableDictionary.IsReadOnly.Should().BeFalse();

            loggableDictionary.Keys.Count.Should().Be(2);
            loggableDictionary.Values.Count.Should().Be(2);

            loggableDictionary["item1"].Key.Should().Be("prop1");
            loggableDictionary["item1"].Value.Should().Be("value1");

            loggableDictionary.First().Key.Should().Be("item1");
            loggableDictionary.First().Value.Key.Should().Be("prop1");
            loggableDictionary.First().Value.Value.Should().Be("value1");

            loggableDictionary["item2"].Key.Should().Be("prop2");
            loggableDictionary["item2"].Value.Should().Be("value2");

            loggableDictionary.Last().Key.Should().Be("item2");
            loggableDictionary.Last().Value.Key.Should().Be("prop2");
            loggableDictionary.Last().Value.Value.Should().Be("value2");
            loggableDictionary.ContainsKey("item2").Should().BeTrue();

            loggableDictionary.Remove("item1").Should().BeTrue();
            loggableDictionary.Count.Should().Be(1);
            loggableDictionary.Remove("item1").Should().BeFalse();
            loggableDictionary.ContainsKey("item1").Should().BeFalse();
            loggableDictionary.TryGetValue("item1", out ICoreLogProperty _).Should().BeFalse();

            loggableDictionary.Add(new KeyValuePair<string, ICoreLogProperty>("item3", new CoreLogProperty("prop3", "value3", CoreLogPropertyType.StringValue)));
            loggableDictionary.Last().Key.Should().Be("item3");
            loggableDictionary.Last().Value.Key.Should().Be("prop3");
            loggableDictionary.Last().Value.Value.Should().Be("value3");
            loggableDictionary.ContainsKey("item3").Should().BeTrue();

            loggableDictionary["item4"] = new CoreLogProperty("prop4", "value4", CoreLogPropertyType.StringValue);
            loggableDictionary.Last().Key.Should().Be("item4");
            loggableDictionary.Last().Value.Key.Should().Be("prop4");
            loggableDictionary.Last().Value.Value.Should().Be("value4");
            loggableDictionary.ContainsKey("item4").Should().BeTrue();

            loggableDictionary.TryGetValue("item4", out ICoreLogProperty logPropertyItem4).Should().BeTrue();
            logPropertyItem4.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogProperty>();
            logPropertyItem4.Key.Should().Be("prop4");
            logPropertyItem4.Value.Should().Be("value4");
            loggableDictionary.Contains(new KeyValuePair<string, ICoreLogProperty>("item4", logPropertyItem4)).Should().BeTrue();
            loggableDictionary.Remove(new KeyValuePair<string, ICoreLogProperty>("item4", logPropertyItem4)).Should().BeTrue();
            loggableDictionary.Contains(new KeyValuePair<string, ICoreLogProperty>("item4", logPropertyItem4)).Should().BeFalse();

            loggableDictionary.Clear();
            loggableDictionary.Count.Should().Be(0);

            ((IEnumerable)loggableDictionary).GetEnumerator().Should().BeAssignableTo<IEnumerator<KeyValuePair<string, ICoreLogProperty>>>();
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary_CopyTo.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary_CopyTo()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
            };

            var loggableArray = new KeyValuePair<string, ICoreLogProperty>[2];

            loggableDictionary.CopyTo(loggableArray, 0);

            loggableArray[0].Key.Should().Be("item1");
            loggableArray[0].Value.Key.Should().Be("prop1");
            loggableArray[0].Value.Value.Should().Be("value1");

            loggableArray[1].Key.Should().Be("item2");
            loggableArray[1].Value.Key.Should().Be("prop2");
            loggableArray[1].Value.Value.Should().Be("value2");
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary_CopyToEnd.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary_CopyToEnd()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
            };

            var loggableArray = new KeyValuePair<string, ICoreLogProperty>[3];

            loggableDictionary.CopyTo(loggableArray, 1);

            loggableArray[0].Key.Should().BeNull();
            loggableArray[0].Value.Should().BeNull();

            loggableArray[1].Key.Should().Be("item1");
            loggableArray[1].Value.Key.Should().Be("prop1");
            loggableArray[1].Value.Value.Should().Be("value1");

            loggableArray[2].Key.Should().Be("item2");
            loggableArray[2].Value.Key.Should().Be("prop2");
            loggableArray[2].Value.Value.Should().Be("value2");
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary_CopyBeyond.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary_CopyBeyond()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
            };

            var loggableArray = new KeyValuePair<string, ICoreLogProperty>[5];

            loggableDictionary.CopyTo(loggableArray, 2);

            loggableArray[0].Key.Should().BeNull();
            loggableArray[0].Value.Should().BeNull();

            loggableArray[1].Key.Should().BeNull();
            loggableArray[1].Value.Should().BeNull();

            loggableArray[2].Key.Should().Be("item1");
            loggableArray[2].Value.Key.Should().Be("prop1");
            loggableArray[2].Value.Value.Should().Be("value1");

            loggableArray[3].Key.Should().Be("item2");
            loggableArray[3].Value.Key.Should().Be("prop2");
            loggableArray[3].Value.Value.Should().Be("value2");

            loggableArray[4].Key.Should().BeNull();
            loggableArray[4].Value.Should().BeNull();
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary_CopyTo_Null.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary_CopyTo_Null()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
            };

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Action act = () => loggableDictionary.CopyTo(null, 0);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("array");
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary_CopyTo_NotLongEnough.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary_CopyTo_NotLongEnough()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
                { "item3", new CoreLogProperty("prop3", "value3", CoreLogPropertyType.StringValue) },
            };

            var loggableArray = new KeyValuePair<string, ICoreLogProperty>[5];

            Action act = () => loggableDictionary.CopyTo(loggableArray, 3);
            act.Should().Throw<ArgumentException>().And.Message.Should().Contain("Destination array is not long enough");
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary_CopyTo_Overflow.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary_CopyTo_Overflow()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
                { "item3", new CoreLogProperty("prop3", "value3", CoreLogPropertyType.StringValue) },
            };

            var loggableArray = new KeyValuePair<string, ICoreLogProperty>[2];

            Action act = () => loggableDictionary.CopyTo(loggableArray, 0);
            act.Should().Throw<ArgumentException>().And.Message.Should().Contain("Destination array is not long enough");
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary_CopyTo_TooShort.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary_CopyTo_TooShort()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
                { "item3", new CoreLogProperty("prop3", "value3", CoreLogPropertyType.StringValue) },
            };

            var loggableArray = new KeyValuePair<string, ICoreLogProperty>[5];

            Action act = () => loggableDictionary.CopyTo(loggableArray, 3);
            act.Should().Throw<ArgumentException>().And.Message.Should().Contain("Destination array is not long enough");
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary_CopyTo_InvalidIndex.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary_CopyTo_InvalidIndex()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
                { "item3", new CoreLogProperty("prop3", "value3", CoreLogPropertyType.StringValue) },
            };

            var loggableArray = new KeyValuePair<string, ICoreLogProperty>[5];

            Action act = () => loggableDictionary.CopyTo(loggableArray, -1);
            act.Should().Throw<ArgumentException>().And.Message.Should().Contain("The starting index is less than zero or greater than array length");
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_LoggableDictionary.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_LoggableDictionary()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
            };

            var coreLogProperty = new CoreLogProperty("property", loggableDictionary, CoreLogPropertyType.Dictionary);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeSameAs(loggableDictionary).And.Subject.Should().BeAssignableTo<ICoreLogPropertyDictionary>();
            coreLogProperty.ToString().Should().Be("{item1={prop1=value1},item2={prop2=value2}}");
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_IEnumerable_Null.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_IEnumerable_Null()
        {
            var coreLogProperty = new CoreLogProperty("property", null, CoreLogPropertyType.StringValue);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeNull();
            coreLogProperty.ToString().Should().Be(StringExtensions.NullString);
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_Integer.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_Integer()
        {
            var coreLogProperty = new CoreLogProperty("property", 5, CoreLogPropertyType.Number);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().Be(5).And.Subject.Should().BeOfType<int>();
            coreLogProperty.ToString().Should().Be("5");
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_IntegerWithSeparator.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_IntegerWithSeparator()
        {
            var coreLogProperty = new CoreLogProperty("property", 5000000, CoreLogPropertyType.NumberWithSeparator);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().Be(5000000).And.Subject.Should().BeOfType<int>();
            coreLogProperty.ToString().Should().Be("5,000,000");
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_IntegerArray.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_IntegerArray()
        {
            int[] intArray = { 1, 2, 3 };
            var coreLogProperty = new CoreLogProperty("property", intArray, CoreLogPropertyType.Array);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeOfType<int[]>();
            coreLogProperty.Value.Should().BeEquivalentTo(new[] { 1, 2, 3 });
            coreLogProperty.ToString().Should().Be("[1,2,3]");
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_IntegerArray_Empty.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_IntegerArray_Empty()
        {
            int[] intArray = [];
            var coreLogProperty = new CoreLogProperty("property", intArray, CoreLogPropertyType.Array);
            coreLogProperty.Should().NotBeNull();
            coreLogProperty.Key.Should().Be("property");
            coreLogProperty.Value.Should().BeOfType<int[]>();
            coreLogProperty.Value.Should().BeEquivalentTo(Array.Empty<int>());
            coreLogProperty.ToString().Should().Be("[]");
            this.TestOutputHelper.WriteLine(coreLogProperty.ToString());
        }

        /// <summary>
        /// Defines the test method CoreLogPropertyUnit_IntegerArray_Empty.
        /// </summary>
        [Fact]
        public void CoreLogPropertyUnit_TryGetValue_Null()
        {
            var loggableDictionary = new CoreLogPropertyDictionary
            {
                { "item1", new CoreLogProperty("prop1", "value1", CoreLogPropertyType.StringValue) },
                { "item2", new CoreLogProperty("prop2", "value2", CoreLogPropertyType.StringValue) },
            };

            Action act = () => loggableDictionary.TryGetValue(null!, out _);
            act.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("key");
        }
    }
}
