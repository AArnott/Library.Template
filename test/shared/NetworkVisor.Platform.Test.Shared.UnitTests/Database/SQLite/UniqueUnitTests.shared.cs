// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="UniqueUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteUniqueUnitTests.
    /// </summary>
    [PlatformTrait(typeof(UniqueUnitTests))]

    public class UniqueUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UniqueUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public UniqueUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CreateUniqueIndexes()
        {
            using var db = new TestDb<UniqueUnitTests>(this.TestFileSystem);
            db.CreateTable<TheOne>();
            List<IndexInfo> indexes = db.Query<IndexInfo>("PRAGMA INDEX_LIST (\"TheOne\")");
            indexes.Count.Should().Be(4, "# of indexes");
            CheckIndex(db, indexes, "UX_Uno", true, "Uno");
            CheckIndex(db, indexes, "UX_Dos", true, "Dos", "Tres");
            CheckIndex(db, indexes, "UX_Uno_bool", true, "Cuatro");
            CheckIndex(db, indexes, "UX_Dos_bool", true, "Cinco", "Seis");
        }

        private static void CheckIndex(TestDb<UniqueUnitTests> db, List<IndexInfo> indexes, string iname, bool unique, params string[] columns)
        {
            if (columns is null)
            {
                throw new Exception("Don't!");
            }

            IndexInfo? idx = indexes.SingleOrDefault(i => i.Name == iname);
            idx.Should().NotBeNull($"Index {iname} not found");
            unique.Should().Be(idx!.Unique, $"Index {iname} unique expected {unique} but got {idx.Unique}");

            List<IndexColumns> idx_columns = db.Query<IndexColumns>($"PRAGMA INDEX_INFO (\"{iname}\")");
            idx_columns.Count.Should().Be(columns.Length, $"# of columns: expected {columns.Length}, got {idx_columns.Count}");

            foreach (string col in columns)
            {
                idx_columns.SingleOrDefault(c => c.Name == col).Should().NotBeNull($"Column {col} not in index {idx.Name}");
            }
        }

        public class TheOne
        {
            [PrimaryKey, AutoIncrement]
            public int ID { get; set; }

            [Unique(Name = "UX_Uno")]
            public int Uno { get; set; }

            [Unique(Name = "UX_Dos")]
            public int Dos { get; set; }

            [Unique(Name = "UX_Dos")]
            public int Tres { get; set; }

            [Indexed(Name = "UX_Uno_bool", Unique = true)]
            public int Cuatro { get; set; }

            [Indexed(Name = "UX_Dos_bool", Unique = true)]
            public int Cinco { get; set; }

            [Indexed(Name = "UX_Dos_bool", Unique = true)]
            public int Seis { get; set; }
        }

        public class IndexColumns
        {
            public int Seqno { get; set; }

            public int Cid { get; set; }

            public string? Name { get; set; }
        }

        public class IndexInfo
        {
            public int Seq { get; set; }

            public string? Name { get; set; }

            public bool Unique { get; set; }
        }
    }
}
