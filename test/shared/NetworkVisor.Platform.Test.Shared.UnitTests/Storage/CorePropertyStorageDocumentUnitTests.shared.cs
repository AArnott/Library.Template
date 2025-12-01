// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CorePropertyStorageDocumentUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Text;
using FluentAssertions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Storage.Property;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Storage
{
    /// <summary>
    /// Class CorePropertyStorageDocumentUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CorePropertyStorageDocumentUnitTests))]

    public class CorePropertyStorageDocumentUnitTests : CoreTestCaseBase
    {
        private static readonly byte[] StorageDocumentBytesEmpty = [123, 34, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 77, 97, 106, 111, 114, 34, 58, 49, 44, 34, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 77, 105, 110, 111, 114, 34, 58, 49, 44, 34, 82, 101, 118, 105, 115, 105, 111, 110, 34, 58, 49, 44, 34, 76, 97, 115, 116, 85, 112, 100, 97, 116, 101, 100, 85, 116, 99, 34, 58, 34, 50, 48, 50, 52, 45, 48, 55, 45, 50, 56, 84, 49, 52, 58, 52, 57, 58, 51, 54, 46, 53, 56, 54, 52, 52, 54, 49, 43, 48, 48, 58, 48, 48, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 105, 101, 115, 34, 58, 123, 125, 125];
        private static readonly byte[] StorageDocumentBytesProperties = [123, 34, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 77, 97, 106, 111, 114, 34, 58, 49, 44, 34, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 77, 105, 110, 111, 114, 34, 58, 49, 44, 34, 82, 101, 118, 105, 115, 105, 111, 110, 34, 58, 49, 44, 34, 76, 97, 115, 116, 85, 112, 100, 97, 116, 101, 100, 85, 116, 99, 34, 58, 34, 50, 48, 50, 52, 45, 48, 55, 45, 50, 56, 84, 49, 53, 58, 48, 52, 58, 48, 48, 46, 52, 48, 50, 52, 50, 57, 53, 43, 48, 48, 58, 48, 48, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 105, 101, 115, 34, 58, 123, 34, 80, 114, 111, 112, 101, 114, 116, 121, 51, 34, 58, 34, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 121, 50, 34, 58, 34, 86, 97, 108, 117, 101, 50, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 121, 49, 34, 58, 34, 86, 97, 108, 117, 101, 49, 34, 125, 125,];

        /// <summary>
        /// Initializes a new instance of the <see cref="CorePropertyStorageDocumentUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CorePropertyStorageDocumentUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void PropertyStorageDocument_Empty()
        {
            var propertyStorageDocument = new CorePropertyStorageDocument();

            this.TestOutputHelper.WriteLine(propertyStorageDocument.ToStringWithParentsPropNameMultiLine());

            propertyStorageDocument.Properties.Count.Should().Be(0);
            propertyStorageDocument.LastUpdatedUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
            propertyStorageDocument.Revision.Should().Be(1);
            propertyStorageDocument.SchemaVersionMajor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMajor);
            propertyStorageDocument.SchemaVersionMinor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMinor);
            propertyStorageDocument.OwnerName.Should().BeEmpty();
            propertyStorageDocument.EncryptionKeyName.Should().BeEmpty();
        }

        [Fact]
        public async Task PropertyStorageDocument_SaveStorageToStreamAsync_Empty()
        {
            var propertyStorageDocument = new CorePropertyStorageDocument();

            using Stream streamLoaded = new MemoryStream();
            Exception? result = await propertyStorageDocument.SaveStorageToStreamAsync(streamLoaded);
            result.Should().BeNull();
            streamLoaded.Length.Should().BeGreaterThan(0);

            byte[] buffer = new byte[streamLoaded.Length];
            streamLoaded.Seek(0, SeekOrigin.Begin);
            long readBytes = await streamLoaded.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(streamLoaded.Length);

            this.TestOutputHelper.WriteLine(Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public async Task PropertyStorageDocument_CreateDocumentFromStreamAsync_Empty()
        {
            using Stream streamLoaded = new MemoryStream(StorageDocumentBytesEmpty);
            (ICorePropertyStorageDocument? PropertyStorageDocument, Exception? Exception) result = await CorePropertyStorageDocument.CreateDocumentFromStreamAsync(streamLoaded, 0, this.TestCaseLogger);

            result.Should().NotBeNull();
            result.Exception.Should().BeNull();
            result.PropertyStorageDocument.Should().NotBeNull();

            ICorePropertyStorageDocument? propertyStorageDocument = result.PropertyStorageDocument;

            this.TestOutputHelper.WriteLine(propertyStorageDocument.ToStringWithParentsPropNameMultiLine());

            propertyStorageDocument!.Properties.Count.Should().Be(0);
            propertyStorageDocument.LastUpdatedUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
            propertyStorageDocument.Revision.Should().Be(1);
            propertyStorageDocument.SchemaVersionMajor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMajor);
            propertyStorageDocument.SchemaVersionMinor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMinor);
            propertyStorageDocument.OwnerName.Should().BeEmpty();
            propertyStorageDocument.EncryptionKeyName.Should().BeEmpty();
        }

        [Fact]
        public async Task PropertyStorageDocument_SaveStorageToStreamAsync_Properties()
        {
            var propertyStorageDocument = new CorePropertyStorageDocument();

            propertyStorageDocument.Properties.Add("Property1", "Value1");
            propertyStorageDocument.Properties.Add("Property2", "Value2");
            propertyStorageDocument.Properties.Add("Property3", string.Empty);

            using Stream streamLoaded = new MemoryStream();
            Exception? result = await propertyStorageDocument.SaveStorageToStreamAsync(streamLoaded);
            result.Should().BeNull();
            streamLoaded.Length.Should().BeGreaterThan(0);

            byte[] buffer = new byte[streamLoaded.Length];
            streamLoaded.Seek(0, SeekOrigin.Begin);
            long readBytes = await streamLoaded.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(streamLoaded.Length);

            this.TestOutputHelper.WriteLine(Encoding.UTF8.GetString(buffer));
        }

        [Fact]
        public async Task PropertyStorageDocument_CreateDocumentFromStreamAsync_Properties()
        {
            using Stream streamLoaded = new MemoryStream(StorageDocumentBytesProperties);
            (ICorePropertyStorageDocument? PropertyStorageDocument, Exception? Exception) result = await CorePropertyStorageDocument.CreateDocumentFromStreamAsync(streamLoaded, 0, this.TestCaseLogger);

            result.Should().NotBeNull();
            result.Exception.Should().BeNull();
            result.PropertyStorageDocument.Should().NotBeNull();

            ICorePropertyStorageDocument? propertyStorageDocument = result.PropertyStorageDocument;

            this.TestOutputHelper.WriteLine(propertyStorageDocument.ToStringWithParentsPropNameMultiLine());

            propertyStorageDocument!.Properties.Count.Should().Be(3);
            propertyStorageDocument.LastUpdatedUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
            propertyStorageDocument.Revision.Should().Be(1);
            propertyStorageDocument.SchemaVersionMajor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMajor);
            propertyStorageDocument.SchemaVersionMinor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMinor);
        }

        [Fact]
        public async Task PropertyStorageDocument_UpdateFromDocumentAsync_Empty_ClearExisting_Different()
        {
            var propertyStorageDocument = new CorePropertyStorageDocument();
            using Stream streamLoaded = new MemoryStream(StorageDocumentBytesProperties);
            (ICorePropertyStorageDocument? PropertyStorageDocument, Exception? Exception) result = await CorePropertyStorageDocument.CreateDocumentFromStreamAsync(streamLoaded, 0, this.TestCaseLogger);

            result.Should().NotBeNull();
            result.Exception.Should().BeNull();
            result.PropertyStorageDocument.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Original:\n{propertyStorageDocument.ToStringWithParentsPropNameMultiLine()}\n");

            Exception? updateResult = await propertyStorageDocument.UpdateFromDocumentAsync(result.PropertyStorageDocument!, CoreSerializableMergeOptions.ClearExisting);

            updateResult.Should().BeNull();

            this.TestOutputHelper.WriteLine($"Updated:\n{propertyStorageDocument.ToStringWithParentsPropNameMultiLine()}");

            propertyStorageDocument!.Properties.Count.Should().Be(3);
            propertyStorageDocument.LastUpdatedUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
            propertyStorageDocument.Revision.Should().Be(2);
            propertyStorageDocument.SchemaVersionMajor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMajor);
            propertyStorageDocument.SchemaVersionMinor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMinor);
            propertyStorageDocument.LastUpdatedUtc.Should().BeOnOrAfter(result.PropertyStorageDocument!.LastUpdatedUtc);
        }

        [Fact]
        public async Task PropertyStorageDocument_UpdateFromDocumentAsync_KeepExisting_Same()
        {
            using Stream streamLoaded = new MemoryStream(StorageDocumentBytesProperties);
            (ICorePropertyStorageDocument? PropertyStorageDocument, Exception? Exception) result = await CorePropertyStorageDocument.CreateDocumentFromStreamAsync(streamLoaded, 0, this.TestCaseLogger);
            (ICorePropertyStorageDocument? PropertyStorageDocument, Exception? Exception) result2 = await CorePropertyStorageDocument.CreateDocumentFromStreamAsync(streamLoaded, 0, this.TestCaseLogger);

            result.Should().NotBeNull();
            result.Exception.Should().BeNull();
            result.PropertyStorageDocument.Should().NotBeNull();

            DateTimeOffset lastUpdatedUtc = result.PropertyStorageDocument!.LastUpdatedUtc;

            result2.Should().NotBeNull();
            result2.Exception.Should().BeNull();
            result2.PropertyStorageDocument.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Original:\n{result.PropertyStorageDocument.ToStringWithParentsPropNameMultiLine()}\n");

            Exception? updateResult = await result.PropertyStorageDocument!.UpdateFromDocumentAsync(result2.PropertyStorageDocument!, CoreSerializableMergeOptions.KeepExisting);

            updateResult.Should().BeNull();

            this.TestOutputHelper.WriteLine($"Updated:\n{result.PropertyStorageDocument.ToStringWithParentsPropNameMultiLine()}");

            result.PropertyStorageDocument!.Properties.Count.Should().Be(3);
            result.PropertyStorageDocument.LastUpdatedUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
            result.PropertyStorageDocument.Revision.Should().Be(1);
            result.PropertyStorageDocument.SchemaVersionMajor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMajor);
            result.PropertyStorageDocument.SchemaVersionMinor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMinor);
            result.PropertyStorageDocument.LastUpdatedUtc.Should().Be(lastUpdatedUtc);
        }

        [Fact]
        public async Task PropertyStorageDocument_UpdateFromDocumentAsync_KeepExisting_Different()
        {
            var propertyStorageDocument = new CorePropertyStorageDocument();
            using Stream streamLoaded = new MemoryStream(StorageDocumentBytesProperties);
            (ICorePropertyStorageDocument? PropertyStorageDocument, Exception? Exception) result = await CorePropertyStorageDocument.CreateDocumentFromStreamAsync(streamLoaded, 0, this.TestCaseLogger);

            result.Should().NotBeNull();
            result.Exception.Should().BeNull();
            result.PropertyStorageDocument.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Original:\n{propertyStorageDocument.ToStringWithParentsPropNameMultiLine()}\n");

            Exception? updateResult = await propertyStorageDocument.UpdateFromDocumentAsync(result.PropertyStorageDocument!, CoreSerializableMergeOptions.KeepExisting);

            updateResult.Should().BeNull();

            this.TestOutputHelper.WriteLine($"Updated:\n{propertyStorageDocument.ToStringWithParentsPropNameMultiLine()}");

            propertyStorageDocument!.Properties.Count.Should().Be(3);
            propertyStorageDocument.LastUpdatedUtc.Should().BeOnOrBefore(DateTimeOffset.UtcNow);
            propertyStorageDocument.Revision.Should().Be(2);
            propertyStorageDocument.SchemaVersionMajor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMajor);
            propertyStorageDocument.SchemaVersionMinor.Should().Be(CorePropertyStorageDocument.CurrentSchemaMinor);
            propertyStorageDocument.LastUpdatedUtc.Should().BeOnOrAfter(result.PropertyStorageDocument!.LastUpdatedUtc);
        }
    }
}
