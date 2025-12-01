// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="BooleanUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Diagnostics;
using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Commands;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class BooleanUnitTests.
    /// </summary>
    [PlatformTrait(typeof(BooleanUnitTests))]

    public class BooleanUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BooleanUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public BooleanUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void TestBoolean()
        {
            using var db = new DbAcs(this.TestFileSystem);

            db.BuildTable();

            for (int i = 0; i < 10; i++)
            {
                db.Insert(new VO() { Flag = i % 3 == 0, Text = $"VO{i}" });
            }

            // count vo which flag is true
            db.CountWithFlag(true).Should().Be(4);
            db.CountWithFlag(false).Should().Be(6);

            this.TestOutputHelper.WriteLine("VO with true flag:");
            foreach (VO vo in db.Query<VO>("SELECT * FROM VO Where Flag = ?", true))
            {
                this.TestOutputHelper.WriteLine(vo.ToString());
            }

            this.TestOutputHelper.WriteLine("VO with false flag:");
            foreach (VO vo in db.Query<VO>("SELECT * FROM VO Where Flag = ?", false))
            {
                this.TestOutputHelper.WriteLine(vo.ToString());
            }
        }

        public class VO
        {
            [AutoIncrement, PrimaryKey]
            public int ID { get; set; }

            public bool Flag { get; set; }

            public string? Text { get; set; }

            public override string ToString()
            {
                return $"VO:: ID:{this.ID} Flag:{this.Flag} Text:{this.Text}";
            }
        }

        public class DbAcs : TestDbBase<BooleanUnitTests>
        {
            public DbAcs(ICoreFileSystem fileSystem)
                : base(fileSystem)
            {
            }

            public void BuildTable()
            {
                this.CreateTable<VO>();
            }

            public int CountWithFlag(bool flag)
            {
                CoreSQLiteCommand cmd = this.CreateCommand("SELECT COUNT(*) FROM VO Where Flag = ?", flag);
                return cmd.ExecuteScalar<int>();
            }
        }
    }
}
