// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="ByteArrayUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.ComponentModel;
using FluentAssertions;
using NetworkVisor.Core.Database.Providers.SQLite.Attributes;
using NetworkVisor.Core.Database.Providers.SQLite.Connections;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite.Database;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Database.SQLite
{
    /// <summary>
    /// Class ByteArrayUnitTests.
    /// </summary>
    [PlatformTrait(typeof(ByteArrayUnitTests))]

    public class ByteArrayUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ByteArrayUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public ByteArrayUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        [Description("Create objects with various byte arrays and check they can be stored and retrieved correctly")]
        public void ByteArrays()
        {
            // Byte Arrays for comparison
            ByteArrayClass[] byteArrays = new ByteArrayClass[]
            {
                new() { Bytes = new byte[] { 1, 2, 3, 4, 250, 252, 253, 254, 255 } }, // Range check
                new() { Bytes = new byte[] { 0 } }, // null bytes need to be handled correctly
                new() { Bytes = new byte[] { 0, 0 } },
                new() { Bytes = new byte[] { 0, 1, 0 } },
                new() { Bytes = new byte[] { 1, 0, 1 } },
                new() { Bytes = new byte[] { } }, // Empty byte array should stay empty (and not become null)
                new() { Bytes = null }, // Null should be supported
            };

            using var database = new TestDb<ByteArrayUnitTests>(this.TestFileSystem);
            database.CreateTable<ByteArrayClass>();

            // Insert all of the ByteArrayClass
            foreach (ByteArrayClass b in byteArrays)
            {
                database.Insert(b);
            }

            // Get them back out
            ByteArrayClass[] fetchedByteArrays = database.Table<ByteArrayClass>().OrderBy(x => x.ID).ToArray();

            fetchedByteArrays.Length.Should().Be(byteArrays.Length);

            // Check they are the same
            for (int i = 0; i < byteArrays.Length; i++)
            {
                byteArrays[i].AssertEquals(fetchedByteArrays[i]);
            }
        }

        [Fact]
        [Description("Uses a byte array to find a record")]
        public void ByteArrayWhere()
        {
            // Byte Arrays for comparisson
            ByteArrayClass[] byteArrays = new ByteArrayClass[]
            {
                new() { Bytes = new byte[] { 1, 2, 3, 4, 250, 252, 253, 254, 255 } }, // Range check
                new() { Bytes = new byte[] { 0 } }, // null bytes need to be handled correctly
                new() { Bytes = new byte[] { 0, 0 } },
                new() { Bytes = new byte[] { 0, 1, 0 } },
                new() { Bytes = new byte[] { 1, 0, 1 } },
                new() { Bytes = new byte[] { } }, // Empty byte array should stay empty (and not become null)
                new() { Bytes = null }, // Null should be supported
            };

            using var database = new TestDb<ByteArrayUnitTests>(this.TestFileSystem);

            database.CreateTable<ByteArrayClass>();

            byte[] criterion = new byte[] { 1, 0, 1 };

            // Insert all of the ByteArrayClass
            int id = 0;
            foreach (ByteArrayClass b in byteArrays)
            {
                database.Insert(b);
                if (b.Bytes is not null && criterion.SequenceEqual<byte>(b.Bytes))
                {
                    id = b.ID;
                }
            }

            id.Should().NotBe(0, "An ID wasn't set");

            // Get it back out
            ByteArrayClass fetchedByteArray = database.Table<ByteArrayClass>().Where(x => x.Bytes == criterion).First();
            fetchedByteArray.Should().NotBeNull();

            // Check they are the same
            id.Should().Be(fetchedByteArray.ID);
        }

        [Fact]
        [Description("Uses a null byte array to find a record")]
        public void ByteArrayWhereNull()
        {
            // Byte Arrays for comparison
            ByteArrayClass[] byteArrays = new ByteArrayClass[]
            {
                new() { Bytes = new byte[] { 1, 2, 3, 4, 250, 252, 253, 254, 255 } }, // Range check
                new() { Bytes = new byte[] { 0 } }, // null bytes need to be handled correctly
                new() { Bytes = new byte[] { 0, 0 } },
                new() { Bytes = new byte[] { 0, 1, 0 } },
                new() { Bytes = new byte[] { 1, 0, 1 } },
                new() { Bytes = new byte[] { } }, // Empty byte array should stay empty (and not become null)
                new() { Bytes = null }, // Null should be supported
            };

            using var database = new TestDb<ByteArrayUnitTests>(this.TestFileSystem);
            database.CreateTable<ByteArrayClass>();

            byte[]? criterion = null;

            // Insert all of the ByteArrayClass
            int id = 0;
            foreach (ByteArrayClass b in byteArrays)
            {
                database.Insert(b);
                if (b.Bytes is null)
                {
                    id = b.ID;
                }
            }

            id.Should().NotBe(0, "An ID wasn't set");

            // Get it back out
            ByteArrayClass fetchedByteArray = database.Table<ByteArrayClass>().Where(x => x.Bytes == criterion).First();

            fetchedByteArray.Should().NotBeNull();

            // Check they are the same
            id.Should().Be(fetchedByteArray.ID);
        }

        [Fact]
        [Description("Create A large byte array and check it can be stored and retrieved correctly")]
        public void LargeByteArray()
        {
            const int byteArraySize = 1024 * 1024;
            byte[] bytes = new byte[byteArraySize];
            for (int i = 0; i < byteArraySize; i++)
            {
                bytes[i] = (byte)(i % 256);
            }

            ByteArrayClass byteArray = new ByteArrayClass() { Bytes = bytes };

            using var database = new TestDb<ByteArrayUnitTests>(this.TestFileSystem);
            database.CreateTable<ByteArrayClass>();

            // Insert the ByteArrayClass
            database.Insert(byteArray);

            // Get it back out
            ByteArrayClass[] fetchedByteArrays = database.Table<ByteArrayClass>().ToArray();

            fetchedByteArrays.Length.Should().Be(1);

            // Check they are the same
            byteArray.AssertEquals(fetchedByteArrays[0]);
        }

        public class ByteArrayClass
        {
            [PrimaryKey, AutoIncrement]
            public int ID { get; set; }

            public byte[]? Bytes { get; set; }

            public void AssertEquals(ByteArrayClass other)
            {
                other.ID.Should().Be(this.ID);
                if (other.Bytes is null || this.Bytes is null)
                {
                    other.Bytes.Should().BeNull();
                    this.Bytes.Should().BeNull();
                }
                else
                {
                    other.Bytes.Length.Should().Be(this.Bytes.Length);
                    for (var i = 0; i < this.Bytes.Length; i++)
                    {
                        other.Bytes[i].Should().Be(this.Bytes[i]);
                    }
                }
            }
        }
    }
}
