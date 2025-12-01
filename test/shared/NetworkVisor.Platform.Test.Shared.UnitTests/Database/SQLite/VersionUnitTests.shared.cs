// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="VersionUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Interop;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteVersionUnitTests.
    /// </summary>
    [PlatformTrait(typeof(VersionUnitTests))]

    public class VersionUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VersionUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public VersionUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void SQLiteVersion_Version_Select_Float()
        {
            using var dbTest = new TestDb<VersionUnitTests>(this.TestFileSystem);
            float pragmaVersion = dbTest.ExecuteScalar<float>("SELECT sqlite_version();");
            this.TestOutputHelper.WriteLine($"Select Version Int: {pragmaVersion}");
        }

        [Fact]
        public void SQLiteVersion_Version_Select_String()
        {
            using var dbTest = new TestDb<VersionUnitTests>(this.TestFileSystem);
            string? pragmaVersion = dbTest.ExecuteScalar<string?>("SELECT sqlite_version();");
            pragmaVersion.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Select Version: {pragmaVersion}");
            pragmaVersion.Should().Be(SQLite3.LibVersionString());
        }

        [Fact]
        public void SQLiteVersion_Version_LibVersionString()
        {
            this.TestOutputHelper.WriteLine($"Lib Version String: {SQLite3.LibVersionString()}");
        }

        [Fact]
        public void SQLiteVersion_Version_LibVersion()
        {
            this.TestOutputHelper.WriteLine($"Lib Version: {SQLite3.LibVersion()}");
        }

        [Fact]
        public void SQLiteVersion_Version_SQLCipherVersionString()
        {
            this.TestOutputHelper.WriteLine($"SQLCipher Version String: {SQLite3.SQLCipherVersionString(this.TestFileSystem)}");
        }

        [Fact]
        public void SQLiteVersion_Version_SQLCipherVersion()
        {
            this.TestOutputHelper.WriteLine($"SQLCipher Version: {SQLite3.SQLCipherVersion(this.TestFileSystem)}");
        }
    }
}
