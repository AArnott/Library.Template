// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreListExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
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
    /// Class CoreListExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreListExtensionsUnitTests))]

    public class CoreListExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreListExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreListExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreListExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method ListExtensions_AddItemCollection_IList_Null.
        /// </summary>
        [Fact]
        public void ListExtensions_AddItemCollection_IList_Null()
        {
            Func<IList<int>> fx = () => ListExtensions.AddItem(null, 1);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("list");
        }

        /// <summary>
        /// Defines the test method ListExtensions_AddItemCollection_ICollection_Null.
        /// </summary>
        [Fact]
        public void ListExtensions_AddItemCollection_ICollection_Null()
        {
            Func<ICollection<int>> fx = () => ListExtensions.AddItem((ICollection<int>?)null, 1);
            fx.Should().Throw<ArgumentNullException>().And.ParamName.Should().Be("list");
        }

        /// <summary>
        /// Defines the test method ListExtensions_AddItemCollection.
        /// </summary>
        [Fact]
        public void ListExtensions_AddItemCollection()
        {
            var listOfIntegers = new List<int>
            {
                1, 2, 4, 5, 6, 8, 9, 10,
            };

            int oldCount = listOfIntegers.Count;
            ICollection<int> listOfIntegersReturned = ((ICollection<int>)listOfIntegers).AddItem(11);

            listOfIntegersReturned.Should().BeSameAs(listOfIntegers);
            listOfIntegersReturned.Count.Should().Be(oldCount + 1);
            listOfIntegersReturned.Last().Should().Be(11);
        }

        /// <summary>
        /// Defines the test method ListExtensions_AddItemList.
        /// </summary>
        [Fact]
        public void ListExtensions_AddItemList()
        {
            var listOfIntegers = new List<int>
            {
                1, 2, 4, 5, 6, 8, 9, 10,
            };

            int oldCount = listOfIntegers.Count;
            IList<int> listOfIntegersReturned = listOfIntegers.AddItem(11);

            listOfIntegersReturned.Should().BeSameAs(listOfIntegers);
            listOfIntegersReturned.Count.Should().Be(oldCount + 1);
            listOfIntegersReturned.Last().Should().Be(11);
        }

        /// <summary>
        /// Defines the test method ListExtensions_CloneList.
        /// </summary>
        [Fact]
        public void ListExtensions_CloneList()
        {
            var objectTestList = new List<ObjectTest>
            {
                new(),
                new(),
                new(),
                new(),
                new(),
            };

            IList<ObjectTest> objectTestListClone = objectTestList.CloneList();
            objectTestListClone.Should().BeEquivalentTo(objectTestList);
            objectTestList.First().ObjectField.Should().NotBeSameAs(objectTestListClone.First().ObjectField);
        }

        /// <summary>
        /// Defines the test method ListExtensions_ShuffleList.
        /// </summary>
        [Fact]
        public void ListExtensions_ShuffleList()
        {
            var listOfIntegers = new List<int>
            {
                1, 2, 4, 5, 6, 8, 9, 10, 11, 12, 13, 15, 16, 18, 19, 20,
            };

            var listRandom = listOfIntegers.ToList();
            listRandom.Shuffle();

            listRandom.Should().NotBeInAscendingOrder();
        }

        /// <summary>
        /// Defines the test method ListExtensions_ShuffleList_Null.
        /// </summary>
        [Fact]
        public void ListExtensions_ShuffleList_Null()
        {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
            ((List<int>)null).Shuffle();
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }

        /// <summary>
        /// Class ObjectField.
        /// </summary>
        protected class ObjectField
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>The unique identifier.</value>
            public Guid ObjectGuid { get; set; } = Guid.NewGuid();
        }

        /// <summary>
        /// Class ObjectTest.
        /// Implements the <see cref="ICloneable" />.
        /// </summary>
        /// <seealso cref="ICloneable" />
        protected class ObjectTest : ICloneable
        {
            /// <summary>
            /// Gets or sets the unique identifier.
            /// </summary>
            /// <value>The unique identifier.</value>
            public Guid ObjectGuid { get; set; } = Guid.NewGuid();

            /// <summary>
            /// Gets or sets the object field.
            /// </summary>
            /// <value>The object field.</value>
            public ObjectField ObjectField { get; set; } = new ObjectField();

            /// <inheritdoc />
            public object Clone()
            {
                return new ObjectTest
                {
                    ObjectGuid = this.ObjectGuid,
                    ObjectField = new ObjectField
                    {
                        ObjectGuid = this.ObjectField.ObjectGuid,
                    },
                };
            }
        }
    }
}
