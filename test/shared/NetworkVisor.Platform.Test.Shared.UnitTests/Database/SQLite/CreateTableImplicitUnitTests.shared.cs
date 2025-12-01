// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CreateTableImplicitUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Diagnostics;
using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Exceptions;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Helpers;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class CreateTableImplicitUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CreateTableImplicitUnitTests))]

    public class CreateTableImplicitUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CreateTableImplicitUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CreateTableImplicitUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void WithoutImplicitMapping()
        {
            using var db = new TestDb<CreateTableImplicitUnitTests>(this.TestFileSystem);

            db.CreateTable<NoAttributesNoOptions>();

            TableMapping? mapping = db.GetMapping<NoAttributesNoOptions>();

            mapping?.PK.Should().BeNull("Should not be a key");

            TableColumn tableColumn = mapping!.Columns[2];
            tableColumn.Name.Should().Be("IndexedId");
            tableColumn.Indices.Any().Should().BeFalse();
        }

        [Fact]
        public void ImplicitPK()
        {
            using var db = new TestDb<CreateTableImplicitUnitTests>(this.TestFileSystem);

            db.CreateTable<NoAttributes>(CreateFlags.ImplicitPK);

            TableMapping? mapping = db.GetMapping<NoAttributes>();

            mapping?.PK.Should().NotBeNull();
            mapping!.PK!.Name.Should().Be("Id");
            mapping.PK.IsPK.Should().BeTrue();
            mapping.PK.IsAutoInc.Should().BeFalse();

            this.CheckPK(db);
        }

        [Fact]
        public void ImplicitAutoInc()
        {
            using var db = new TestDb<CreateTableImplicitUnitTests>(this.TestFileSystem);

            db.CreateTable<PkAttribute>(CreateFlags.AutoIncPK);

            TableMapping? mapping = db.GetMapping<PkAttribute>();

            mapping?.PK.Should().NotBeNull();
            mapping!.PK!.Name.Should().Be("Id");
            mapping.PK.IsPK.Should().BeTrue();
            mapping.PK.IsAutoInc.Should().BeTrue();
        }

        [Fact]
        public void ImplicitIndex()
        {
            using var db = new TestDb<CreateTableImplicitUnitTests>(this.TestFileSystem);

            db.CreateTable<NoAttributes>(CreateFlags.ImplicitIndex);

            TableMapping? mapping = db.GetMapping<NoAttributes>();
            TableColumn tableColumn = mapping!.Columns[2];
            tableColumn.Name.Should().Be("IndexedId");
            tableColumn.Indices.Any().Should().BeTrue();
        }

        [Fact]
        public void ImplicitPKAutoInc()
        {
            using var db = new TestDb<CreateTableImplicitUnitTests>(this.TestFileSystem);

            db.CreateTable(typeof(NoAttributes), CreateFlags.ImplicitPK | CreateFlags.AutoIncPK);

            TableMapping? mapping = db.GetMapping<NoAttributes>();

            mapping?.PK.Should().NotBeNull();
            mapping!.PK!.Name.Should().Be("Id");
            mapping.PK.IsPK.Should().BeTrue();
            mapping.PK.IsAutoInc.Should().BeTrue();
        }

        [Fact]
        public void ImplicitAutoIncAsPassedInTypes()
        {
            using var db = new TestDb<CreateTableImplicitUnitTests>(this.TestFileSystem);

            db.CreateTable(typeof(PkAttribute), CreateFlags.AutoIncPK);

            TableMapping? mapping = db.GetMapping<PkAttribute>();

            mapping?.PK.Should().NotBeNull();
            mapping!.PK!.Name.Should().Be("Id");
            mapping.PK.IsPK.Should().BeTrue();
            mapping.PK.IsAutoInc.Should().BeTrue();
        }

        [Fact]
        public void ImplicitPkAsPassedInTypes()
        {
            using var db = new TestDb<CreateTableImplicitUnitTests>(this.TestFileSystem);

            db.CreateTable(typeof(NoAttributes), CreateFlags.ImplicitPK);

            TableMapping? mapping = db.GetMapping<NoAttributes>();

            mapping?.PK.Should().NotBeNull();
            mapping!.PK!.Name.Should().Be("Id");
            mapping.PK.IsPK.Should().BeTrue();
            mapping.PK.IsAutoInc.Should().BeFalse();
        }

        [Fact]
        public void ImplicitPKAutoIncAsPassedInTypes()
        {
            using var db = new TestDb<CreateTableImplicitUnitTests>(this.TestFileSystem);

            db.CreateTable(typeof(NoAttributes), CreateFlags.ImplicitPK | CreateFlags.AutoIncPK);

            TableMapping? mapping = db.GetMapping<NoAttributes>();

            mapping?.PK!.Should().NotBeNull();
            mapping!.PK!.Name.Should().Be("Id");
            mapping.PK.IsPK.Should().BeTrue();
            mapping.PK.IsAutoInc.Should().BeTrue();
        }

        private void CheckPK(TestDb<CreateTableImplicitUnitTests> db)
        {
            for (int i = 1; i <= 10; i++)
            {
                var na = new NoAttributes { Id = i, AColumn = i.ToString(), IndexedId = 0 };
                db.Insert(na);
            }

            NoAttributes? item = db.Get<NoAttributes>(2);
            item.Should().NotBeNull();
            item!.Id.Should().Be(2);
        }

        public class NoAttributes
        {
            public int Id { get; set; }

            public string? AColumn { get; set; }

            public int IndexedId { get; set; }
        }

        public class NoAttributesNoOptions
        {
            public int Id { get; set; }

            public string? AColumn { get; set; }

            public int IndexedId { get; set; }
        }

        public class PkAttribute
        {
            [PrimaryKey]
            public int Id { get; set; }

            public string? AColumn { get; set; }

            public int IndexedId { get; set; }
        }
    }
}
