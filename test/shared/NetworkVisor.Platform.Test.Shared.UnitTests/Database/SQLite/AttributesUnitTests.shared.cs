// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="AttributesUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Commands;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Database.Providers.SQLite.Types;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Helpers;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class AttributesUnitTests.
    /// </summary>
    [PlatformTrait(typeof(AttributesUnitTests))]

    public class AttributesUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AttributesUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public AttributesUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void TestCtors()
        {
            new Action(() => _ = new CollationAttribute("NOCASE")).Should().NotThrow();
            new Action(() => _ = new ColumnAttribute("Bar")).Should().NotThrow();
            new Action(() => _ = new IgnoreAttribute()).Should().NotThrow();
            new Action(() => _ = new IndexedAttribute()).Should().NotThrow();
            new Action(() => _ = new JsonTextAttribute()).Should().NotThrow();
            new Action(() => _ = new JsonBlobAttribute()).Should().NotThrow();
            new Action(() => _ = new NotNullAttribute()).Should().NotThrow();
            new Action(() => _ = new NetworkVisor.Core.Database.Providers.SQLite.Attributes.PreserveAttribute()).Should().NotThrow();
            new Action(() => _ = new PrimaryKeyAttribute()).Should().NotThrow();
            new Action(() => _ = new StoreAsTextAttribute()).Should().NotThrow();
            new Action(() => _ = new TableAttribute("Foo")).Should().NotThrow();
            new Action(() => _ = new UniqueAttribute()).Should().NotThrow();
        }
    }
}
