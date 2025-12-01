// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="InheritanceUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteInheritanceUnitTests.
    /// </summary>
    [PlatformTrait(typeof(InheritanceUnitTests))]

    public class InheritanceUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InheritanceUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public InheritanceUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void InheritanceWorks()
        {
            using var db = new TestDb<InheritanceUnitTests>(this.TestFileSystem);

            TableMapping? mapping = db.GetMapping<Derived>();

            mapping?.PK.Should().NotBeNull();
            mapping!.Columns.Length.Should().Be(3);
            mapping.PK!.Name.Should().Be("Id");
        }

        private class Base
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string? BaseProp { get; set; }
        }

        private class Derived : Base
        {
            public string? DerivedProp { get; set; }
        }
    }
}
