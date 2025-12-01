// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="EnumCacheUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Caches;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteEnumCacheUnitTests.
    /// </summary>
    [PlatformTrait(typeof(EnumCacheUnitTests))]

    public class EnumCacheUnitTests : CoreTestCaseBase
    {
        private const int Count = 100;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumCacheUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public EnumCacheUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [StoreAsText]
        public enum TestEnumStoreAsText
        {
            Value1,

            Value2,

            Value3,
        }

        public enum TestEnumStoreAsInt
        {
            Value1,

            Value2,

            Value3,
        }

        public enum TestByteEnumStoreAsInt : byte
        {
            Value1,

            Value2,

            Value3,
        }

        public enum TestEnumWithRepeats
        {
            Value1 = 1,

            Value2 = 2,

            Value2Again = 2,

            Value3 = 3,
        }

        [StoreAsText]
        public enum TestEnumWithRepeatsAsText
        {
            Value1 = 1,

            Value2 = 2,

            Value2Again = 2,

            Value3 = 3,
        }

        [Fact]
        public void ShouldReturnTrueForEnumStoreAsText()
        {
            EnumCacheInfo info = EnumCache.GetInfo<TestEnumStoreAsText>();

            info.IsEnum.Should().BeTrue();
            info.StoreAsText.Should().BeTrue();
            info.EnumValues.Should().NotBeNull();

            var values = Enum.GetValues(typeof(TestEnumStoreAsText)).Cast<object>().ToList();

            for (int i = 0; i < values.Count; i++)
            {
                info.EnumValues[i].Should().Be(values[i].ToString());
            }
        }

        [Fact]
        public void ShouldReturnTrueForEnumStoreAsInt()
        {
            EnumCacheInfo info = EnumCache.GetInfo<TestEnumStoreAsInt>();

            info.IsEnum.Should().BeTrue();
            info.StoreAsText.Should().BeFalse();
            info.EnumValues.Should().BeEmpty();
        }

        [Fact]
        public void ShouldReturnTrueForByteEnumStoreAsInt()
        {
            EnumCacheInfo info = EnumCache.GetInfo<TestByteEnumStoreAsInt>();

            info.IsEnum.Should().BeTrue();
            info.StoreAsText.Should().BeFalse();
        }

        [Fact]
        public void ShouldReturnFalseForClass()
        {
            EnumCacheInfo info = EnumCache.GetInfo<TestClassThusNotEnum>();

            info.IsEnum.Should().BeFalse();
            info.StoreAsText.Should().BeFalse();
            info.EnumValues.Should().BeEmpty();
        }

        [Fact]
        public void Issue598_EnumsWithRepeatedValues()
        {
            EnumCacheInfo info = EnumCache.GetInfo<TestEnumWithRepeats>();

            info.IsEnum.Should().BeTrue();
            info.StoreAsText.Should().BeFalse();
            info.EnumValues.Should().BeEmpty();
        }

        [Fact]
        public void Issue598_EnumsWithRepeatedValuesAsText()
        {
            EnumCacheInfo info = EnumCache.GetInfo<TestEnumWithRepeatsAsText>();

            info.IsEnum.Should().BeTrue();
            info.StoreAsText.Should().BeTrue();
            info.EnumValues.Should().NotBeNull();
        }

        public class TestClassThusNotEnum
        {
        }
    }
}
