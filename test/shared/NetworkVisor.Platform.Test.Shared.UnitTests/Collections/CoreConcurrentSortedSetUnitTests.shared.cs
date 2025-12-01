// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreConcurrentSortedSetUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Concurrent Sorted List Unit Tests.</summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Collections;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Collections
{
    /// <summary>
    /// Class CoreConcurrentSortedSetUnitTests. Concurrent Sorted Set Unit Tests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreConcurrentSortedSetUnitTests))]
    public class CoreConcurrentSortedSetUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreConcurrentSortedSetUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreConcurrentSortedSetUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void ConcurrentSortedSet_Add_ShouldAddItemToSet()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>();
            var item = 5;

            // Act
            set.Add(item);

            // Assert
            set.Contains(item).Should().BeTrue();
        }

        [Fact]
        public void ConcurrentSortedSet_Add_ShouldNotAddDuplicateItem()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>();
            var item = 5;

            // Act
            set.Add(item);
            set.Add(item);

            // Assert
            set.Count.Should().Be(1);
        }

        [Fact]
        public void ConcurrentSortedSet_Remove_ShouldRemoveItemFromSet()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>();
            var item = 5;
            set.Add(item);

            // Act
            var result = set.Remove(item);

            // Assert
            result.Should().BeTrue();
            set.Contains(item).Should().BeFalse();
        }

        [Fact]
        public void ConcurrentSortedSet_Remove_ShouldReturnFalseIfItemNotInSet()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>();
            var item = 5;

            // Act
            var result = set.Remove(item);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ConcurrentSortedSet_Contains_ShouldReturnTrueIfItemExists()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>();
            var item = 5;
            set.Add(item);

            // Act
            var result = set.Contains(item);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ConcurrentSortedSet_Contains_ShouldReturnFalseIfItemDoesNotExist()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>();
            var item = 5;

            // Act
            var result = set.Contains(item);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ConcurrentSortedSet_Clear_ShouldRemoveAllItems()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
                2,
            };

            // Act
            set.Clear();

            // Assert
            set.Count.Should().Be(0);
        }

        [Fact]
        public void ConcurrentSortedSet_CopyTo_ShouldCopyElementsToArray()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
                2,
            };
            var array = new int[2];

            // Act
            set.CopyTo(array, 0);

            // Assert
            array.Should().Contain(new[] { 1, 2 });
        }

        [Fact]
        public void ConcurrentSortedSet_UnionWith_ShouldCombineSets()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
            };
            var other = new List<int> { 2, 3 };

            // Act
            set.UnionWith(other);

            // Assert
            set.Should().Contain(new[] { 1, 2, 3 });
        }

        [Fact]
        public void ConcurrentSortedSet_IntersectWith_ShouldRetainCommonElements()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
                2,
            };
            var other = new List<int> { 2, 3 };

            // Act
            set.IntersectWith(other);

            // Assert
            set.Should().ContainSingle().Which.Should().Be(2);
        }

        [Fact]
        public void ConcurrentSortedSet_ExceptWith_ShouldRemoveElementsInOtherSet()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
                2,
            };
            var other = new List<int> { 2, 3 };

            // Act
            set.ExceptWith(other);

            // Assert
            set.Should().ContainSingle().Which.Should().Be(1);
        }

        [Fact]
        public void ConcurrentSortedSet_SymmetricExceptWith_ShouldRetainUniqueElements()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
                2,
            };
            var other = new List<int> { 2, 3 };

            // Act
            set.SymmetricExceptWith(other);

            // Assert
            set.Should().Contain(new[] { 1, 3 });
        }

        [Fact]
        public void ConcurrentSortedSet_IsSubsetOf_ShouldReturnTrueForSubset()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
            };
            var other = new List<int> { 1, 2 };

            // Act
            var result = set.IsSubsetOf(other);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ConcurrentSortedSet_IsSupersetOf_ShouldReturnTrueForSuperset()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
                2,
            };
            var other = new List<int> { 1 };

            // Act
            var result = set.IsSupersetOf(other);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ConcurrentSortedSet_Overlaps_ShouldReturnTrueIfSetsOverlap()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
            };
            var other = new List<int> { 1, 2 };

            // Act
            var result = set.Overlaps(other);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ConcurrentSortedSet_SetEquals_ShouldReturnTrueForEqualSets()
        {
            // Arrange
            var set = new CoreConcurrentSortedSet<int>
            {
                1,
            };
            var other = new List<int> { 1 };

            // Act
            var result = set.SetEquals(other);

            // Assert
            result.Should().BeTrue();
        }
    }
}
