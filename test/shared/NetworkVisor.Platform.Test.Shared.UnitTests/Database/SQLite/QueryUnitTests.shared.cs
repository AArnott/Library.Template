// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="QueryUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CoreSQLiteQueryUnitTests.
    /// </summary>
    [PlatformTrait(typeof(QueryUnitTests))]

    public class QueryUnitTests : CoreTestCaseBase
    {
        private readonly TestDb<QueryUnitTests> _db;
        private readonly (int Value, double Walue)[] _records = new[]
        {
            (42, 0.5),
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public QueryUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._db = new TestDb<QueryUnitTests>(this.TestFileSystem, true);
            this._db.Execute("create table G(Value integer not null, Walue real not null)");

            for (int i = 0; i < this._records.Length; i++)
            {
                this._db.Execute("insert into G(Value, Walue) values (?, ?)", this._records[i].Value, this._records[i].Walue);
            }
        }

        [Fact]
        public void QueryGenericObject()
        {
            List<GenericObject> r = this._db.Query<GenericObject>("select * from G");

            r.Count.Should().Be(this._records.Length);
            r[0].Value.Should().Be(this._records[0].Value);
            r[0].Walue.Should().Be(this._records[0].Walue);
        }

        [Fact]
        public void QueryValueTuple()
        {
            List<(int Value, double Walue)> r = this._db.Query<(int Value, double Walue)>("select * from G");

            r.Count.Should().Be(this._records.Length);
            r[0].Value.Should().Be(this._records[0].Value);
            r[0].Walue.Should().Be(this._records[0].Walue);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._db?.Close();
            }

            base.Dispose(disposing);
        }

        private class GenericObject
        {
            public int Value { get; set; }

            public double Walue { get; set; }
        }
    }
}
