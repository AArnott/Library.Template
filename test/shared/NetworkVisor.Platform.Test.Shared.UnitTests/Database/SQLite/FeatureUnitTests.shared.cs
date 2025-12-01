// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="FeatureUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.ComponentModel;
using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Interop;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteFeatureUnitTests.
    /// </summary>
    [PlatformTrait(typeof(FeatureUnitTests))]

    public class FeatureUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FeatureUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public FeatureUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData(CoreSQLiteFeature.Json, "3.37.0", false)]
        [InlineData(CoreSQLiteFeature.Json, "3.38.0", true)]
        [InlineData(CoreSQLiteFeature.Json, "3.46.0", true)]

        [InlineData(CoreSQLiteFeature.JsonB, "3.37.0", false)]
        [InlineData(CoreSQLiteFeature.JsonB, "3.45.0", true)]
        [InlineData(CoreSQLiteFeature.JsonB, "3.46.0", true)]

        [InlineData(CoreSQLiteFeature.Json5, "3.37.0", false)]
        [InlineData(CoreSQLiteFeature.Json5, "3.42.0", true)]
        [InlineData(CoreSQLiteFeature.Json5, "3.46.0", true)]

        [InlineData(CoreSQLiteFeature.JsonErrorPosition, "3.37.0", false)]
        [InlineData(CoreSQLiteFeature.JsonErrorPosition, "3.42.0", true)]
        [InlineData(CoreSQLiteFeature.JsonErrorPosition, "3.46.0", true)]

        [InlineData(CoreSQLiteFeature.JsonPretty, "3.37.0", false)]
        [InlineData(CoreSQLiteFeature.JsonPretty, "3.46.0", true)]
        [InlineData(CoreSQLiteFeature.JsonPretty, "3.46.1", true)]

        [InlineData(CoreSQLiteFeature.FTS5, "2.37.0", false)]
        [InlineData(CoreSQLiteFeature.FTS5, "3.9.0", true)]
        [InlineData(CoreSQLiteFeature.FTS5, "3.46.0", true)]

        [InlineData(CoreSQLiteFeature.ComputedColumn, "2.37.0", false)]
        [InlineData(CoreSQLiteFeature.ComputedColumn, "3.31.0", true)]
        [InlineData(CoreSQLiteFeature.ComputedColumn, "3.46.0", true)]

        public void SQLiteFeature_Versions(CoreSQLiteFeature sqLiteFeature, string versionString, bool expectedResult)
        {
            SQLite3.IsFeatureSupported(this.TestFileSystem, sqLiteFeature, Version.Parse(versionString)).Should().Be(expectedResult);
        }

        [Fact]
        public void SQLiteFeature_Feature_SQLCipher()
        {
            SQLite3.IsFeatureSupported(this.TestFileSystem, CoreSQLiteFeature.SQLCipher).Should().Be(SQLite3.SQLCipherVersion(this.TestFileSystem) is not null);
        }

        [Fact]
        public void SQLiteFeature_Feature_LoadExtensions()
        {
            SQLite3.IsFeatureSupported(this.TestFileSystem, CoreSQLiteFeature.LoadExtensions).Should().BeTrue();
        }
    }
}
