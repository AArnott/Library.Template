// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreConcurrentSortedListUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Concurrent Sorted List Unit Tests.</summary>
// ***********************************************************************

using System.Reflection;
using System.Runtime.Versioning;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using NetworkVisor.Core.Collections;
using NetworkVisor.Core.Configuration;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Core.Utilities;
using NetworkVisor.Platform.Test.Extensions;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestDevices;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Collections
{
    /// <summary>
    /// Class CoreConcurrentSortedListUnitTests. Concurrent Sorted List Unit Tests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreConcurrentSortedListUnitTests))]
    public class CoreConcurrentSortedListUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreConcurrentSortedListUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreConcurrentSortedListUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void ConcurrentSortedList_Add_ShouldAddKeyValuePair()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();

            // Act
            list.Add(1, "Value1");

            // Assert
            list.Count.Should().Be(1);
            list[0].Should().Be(new KeyValuePair<int, string>(1, "Value1"));
        }

        [Fact]
        public void ConcurrentSortedList_Add_ShouldThrowArgumentNullException_WhenKeyIsNull()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<string, string>();

            // Act
            Action act = () => list.Add(null!, "Value");

            // Assert
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConcurrentSortedList_Remove_ShouldRemoveKeyValuePair()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();
            list.Add(1, "Value1");

            // Act
            var result = list.Remove(1);

            // Assert
            result.Should().BeTrue();
            list.Count.Should().Be(0);
        }

        [Fact]
        public void ConcurrentSortedList_Remove_ShouldReturnFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();

            // Act
            var result = list.Remove(1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ConcurrentSortedList_TryGetValue_ShouldReturnTrueAndValue_WhenKeyExists()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();
            list.Add(1, "Value1");

            // Act
            var result = list.TryGetValue(1, out var value);

            // Assert
            result.Should().BeTrue();
            value.Should().Be("Value1");
        }

        [Fact]
        public void ConcurrentSortedList_TryGetValue_ShouldReturnFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();

            // Act
            var result = list.TryGetValue(1, out var value);

            // Assert
            result.Should().BeFalse();
            value.Should().BeNull();
        }

        [Fact]
        public void ConcurrentSortedList_Modify_ShouldUpdateValue_WhenKeyExists()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();
            list.Add(1, "Value1");

            // Act
            list.Modify(1, "UpdatedValue");

            // Assert
            list[0].Value.Should().Be("UpdatedValue");
        }

        [Fact]
        public void ConcurrentSortedList_Modify_ShouldNotThrow_WhenKeyDoesNotExist()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();

            // Act
            Action act = () => list.Modify(1, "UpdatedValue");

            // Assert
            act.Should().NotThrow();
        }

        [Fact]
        public void ConcurrentSortedList_Clear_ShouldRemoveAllItems()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();
            list.Add(1, "Value1");
            list.Add(2, "Value2");

            // Act
            list.Clear();

            // Assert
            list.Count.Should().Be(0);
        }

        [Fact]
        public void ConcurrentSortedList_Contains_ShouldReturnTrue_WhenKeyExists()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();
            list.Add(1, "Value1");

            // Act
            var result = list.Contains(1);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void ConcurrentSortedList_Contains_ShouldReturnFalse_WhenKeyDoesNotExist()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();

            // Act
            var result = list.Contains(1);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ConcurrentSortedList_IndexOfKey_ShouldReturnCorrectIndex_WhenKeyExists()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();
            list.Add(1, "Value1");

            // Act
            var index = list.IndexOfKey(1);

            // Assert
            index.Should().Be(0);
        }

        [Fact]
        public void ConcurrentSortedList_IndexOfKey_ShouldReturnMinusOne_WhenKeyDoesNotExist()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();

            // Act
            var index = list.IndexOfKey(1);

            // Assert
            index.Should().Be(-1);
        }

        [Fact]
        public void ConcurrentSortedList_IndexOfValue_ShouldReturnCorrectIndex_WhenValueExists()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();
            list.Add(1, "Value1");

            // Act
            var index = list.IndexOfValue("Value1");

            // Assert
            index.Should().Be(0);
        }

        [Fact]
        public void ConcurrentSortedList_IndexOfValue_ShouldReturnMinusOne_WhenValueDoesNotExist()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();

            // Act
            var index = list.IndexOfValue("Value1");

            // Assert
            index.Should().Be(-1);
        }

        [Fact]
        public void ConcurrentSortedList_CopyTo_ShouldCopyItemsToArray()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();
            list.Add(1, "Value1");
            var array = new KeyValuePair<int, string>[1];

            // Act
            list.CopyTo(array, 0);

            // Assert
            array[0].Should().Be(new KeyValuePair<int, string>(1, "Value1"));
        }

        [Fact]
        public void ConcurrentSortedList_GetEnumerator_ShouldEnumerateAllItems()
        {
            // Arrange
            var list = new CoreConcurrentSortedList<int, string>();
            list.Add(1, "Value1");

            // Act
            IEnumerator<KeyValuePair<int, string>> enumerator = list.GetEnumerator();

            // Assert
            enumerator.MoveNext().Should().BeTrue();
            enumerator.Current.Should().Be(new KeyValuePair<int, string>(1, "Value1"));
        }
    }
}
