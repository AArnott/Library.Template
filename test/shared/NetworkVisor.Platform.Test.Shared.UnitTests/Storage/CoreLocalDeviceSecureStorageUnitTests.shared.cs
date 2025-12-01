// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreLocalDeviceSecureStorageUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Storage;
using NetworkVisor.Core.Storage.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Org.BouncyCastle.Crypto;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Storage
{
    /// <summary>
    /// Class CoreLocalDeviceSecureStorageUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLocalDeviceSecureStorageUnitTests))]

    public class CoreLocalDeviceSecureStorageUnitTests : CoreTestCaseBase
    {
        private static readonly byte[] DefaultKey = [132, 19, 13, 129, 1, 135, 31, 237, 242, 183, 80, 235, 21, 84, 21, 210, 140, 231, 99, 144, 75, 186, 3, 76, 61, 80, 251, 238, 237, 23, 120, 167];
        private static readonly byte[] BadKey = [13, 19, 13, 129, 1, 135, 31, 237, 242, 183, 80, 235, 21, 84, 21, 210, 140, 231, 99, 144, 75, 186, 3, 76, 61, 80, 251, 238, 237, 23, 120, 167];
        private static readonly string DefaultValue = "Value";
        private static readonly string DefaultValue2 = "Value2";
        private static readonly byte[] DefaultNonSecretPayload = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLocalDeviceSecureStorageUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLocalDeviceSecureStorageUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.LocalDeviceSecureStorage = TestCoreLocalDeviceSecureStorage.TestCreateSecureStorage(this.TestNetworkingSystem, $"storage.test.{Guid.NewGuid().ToStringNoDashes()}", CoreTaskCacheStateFlags.NotInitialized);
        }

        internal TestCoreLocalDeviceSecureStorage? LocalDeviceSecureStorage { get; }

        [Fact]
        public async Task LocalDeviceSecureStorage_GetAsync_Empty()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();

            (await this.LocalDeviceSecureStorage!.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty)).Should().BeNull();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Property()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Empty()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.LocalDeviceSecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, string.Empty)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);
            propertyValue.Should().BeEmpty();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Null()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.LocalDeviceSecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, null!)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);
            propertyValue.Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Property_Twice()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.LocalDeviceSecureStorage.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue2)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue2);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Property_MissingKey()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty.ToVersionedStoragePropName(2));
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().BeNull();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Property_InvalidKey()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty | CoreStoragePropName.ComponentSystem, DefaultValue)).Should().BeOfType<ArgumentException>();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_GetAsync_Empty()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();

            (await this.LocalDeviceSecureStorage!.TestGetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty)).Should().BeNull();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetAsync_Property()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.TestGetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetAsync_Empty()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.TestGetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, string.Empty)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            propertyValue = await this.LocalDeviceSecureStorage.TestGetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            propertyValue.Should().BeEmpty();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetAsync_Null()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.TestGetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, null!)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            propertyValue = await this.LocalDeviceSecureStorage.TestGetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);

            propertyValue.Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetAsync_NonSecretPayload()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultNonSecretPayload)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey, DefaultNonSecretPayload.Length);

            propertyValue.Should().NotBeNull();
            propertyValue!.Value.Exception.Should().BeNull();
            propertyValue!.Value.PropValue.Should().Be(DefaultValue);
            propertyValue!.Value.NonSecretPayload.Should().Equal(DefaultNonSecretPayload);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetAsync_Null_NonSecretPayload()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultNonSecretPayload)).Should().BeNull();

            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey, DefaultNonSecretPayload.Length);

            propertyValue.Should().NotBeNull();
            propertyValue!.Value.Exception.Should().BeNull();
            propertyValue!.Value.PropValue.Should().Be(DefaultValue);
            propertyValue!.Value.NonSecretPayload.Should().Equal(DefaultNonSecretPayload);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, null!, DefaultNonSecretPayload)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey, DefaultNonSecretPayload.Length);

            propertyValue.Should().NotBeNull();
            propertyValue!.Value.PropValue.Should().BeEmpty();
            propertyValue!.Value.NonSecretPayload.Should().Equal(DefaultNonSecretPayload);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetAsync_Property_NonSecretPayload()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultNonSecretPayload)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? propertyValue = await this.LocalDeviceSecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey, DefaultNonSecretPayload.Length);

            propertyValue.Should().NotBeNull();
            propertyValue!.Value.Exception.Should().BeNull();
            propertyValue!.Value.PropValue.Should().Be(DefaultValue);
            propertyValue!.Value.NonSecretPayload.Should().Equal(DefaultNonSecretPayload);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetAsync_Property_Twice()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.TestGetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.LocalDeviceSecureStorage.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue2)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            propertyValue = await this.LocalDeviceSecureStorage.TestGetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue2);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetAsync_Property_MissingKey()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.LocalDeviceSecureStorage.TestGetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty.ToVersionedStoragePropName(2));
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValue.Should().BeNull();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetAsync_Property_InvalidKey()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.TestSetPropAsyncWithEncryption(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty | CoreStoragePropName.ComponentSystem, DefaultValue)).Should().BeOfType<ArgumentException>();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetPropWithChecksumAsync()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.SetPropWithChecksumAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            (string? PropValue, Exception? Exception)? propertyValueWithChecksum = await this.LocalDeviceSecureStorage.GetPropWithChecksumAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValueWithChecksum.Should().NotBeNull();
            propertyValueWithChecksum!.Value.Exception.Should().BeNull();
            propertyValueWithChecksum!.Value.PropValue.Should().Be(DefaultValue);
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_SetPropWithChecksumAsync_BadKey()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();

            (await this.LocalDeviceSecureStorage!.SetPropWithChecksumAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            (string? PropValue, Exception? Exception)? propertyValueWithChecksum = await this.LocalDeviceSecureStorage.GetPropWithChecksumAsync(CoreStoragePropName.TestAppLocalUserTestProperty, BadKey);
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            propertyValueWithChecksum.Should().NotBeNull();
            propertyValueWithChecksum!.Value.Exception.Should().BeOfType<InvalidCipherTextException>();
            propertyValueWithChecksum!.Value.PropValue.Should().BeNull();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_StoragePropNames_Initial_OutputAsync()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();
            this.TestOutputHelper.WriteLine("All Storage Properties".CenterTitle());

            foreach (CoreStoragePropName propName in CoreStoragePropNameExtensions.StoragePropName2Version.Keys)
            {
                string? propertyString = propName.ToPropertyString();
                this.TestOutputHelper.WriteLine($"[{propName}, {propertyString}] = {await this.LocalDeviceSecureStorage!.GetPropAsync(propName) ?? StringExtensions.NullString}");
            }
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_StoragePropNames_ValidateProperties()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();
            this.TestOutputHelper.WriteLine("All Storage Properties".CenterTitle());

            foreach (CoreStoragePropName propName in CoreStoragePropNameExtensions.StoragePropName2Version.Keys)
            {
                string? propertyString = propName.ToPropertyString();
                this.TestOutputHelper.WriteLine($"[{propName}, {propertyString}] = {await this.LocalDeviceSecureStorage!.GetPropAsync(propName) ?? StringExtensions.NullString}");

                propertyString.Should().NotBeNullOrEmpty();

                if (propName.HasFlag(CoreStoragePropName.Encrypted))
                {
                    propertyString.Should().Contain("e.");
                }

                if (propName.HasFlag(CoreStoragePropName.Test))
                {
                    propertyString.Should().Contain("t.");
                }

                if (propName.HasFlag(CoreStoragePropName.Local))
                {
                    propertyString.Should().Contain("l.");
                }

                if (propName.HasFlag(CoreStoragePropName.Device))
                {
                    propertyString.Should().Contain("d.");
                }

                if (propName.HasFlag(CoreStoragePropName.User))
                {
                    propertyString.Should().Contain("u.");
                }
            }
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_StoragePropNames_Initial_WithValues_OutputAsync()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();
            this.TestOutputHelper.WriteLine("Storage Properties With Values".CenterTitle());
            int found = 0;

            foreach (CoreStoragePropName propName in CoreStoragePropNameExtensions.StoragePropName2Version.Keys)
            {
                string? propValue = await this.LocalDeviceSecureStorage!.GetPropAsync(propName);

                if (!string.IsNullOrEmpty(propValue))
                {
                    this.TestOutputHelper.WriteLine($"[{propName}, {propName.ToPropertyString()}] = {propValue}");
                    found++;
                }
            }

            found.Should().Be(1, "Did not find at least system revision values");
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_StoragePropNames_WithValues_OutputAsync()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();
            this.TestOutputHelper.WriteLine("Storage Properties With Values".CenterTitle());
            int found = 0;

            this.AddStoragePropValue(CoreStoragePropName.TestAppLocalUserTestProperty).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(2);

            this.AddStoragePropValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty).Should().BeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(3);

            foreach (CoreStoragePropName propName in CoreStoragePropNameExtensions.StoragePropName2Version.Keys)
            {
                string? propValue = await this.LocalDeviceSecureStorage!.TestGetPropAsyncWithEncryption(propName);

                if (!string.IsNullOrEmpty(propValue))
                {
                    this.TestOutputHelper.WriteLine($"[{propName}, {propName.ToPropertyString()}] = {propValue}");
                    found++;
                }
            }

            found.Should().BeGreaterThanOrEqualTo(3, "Did not find added properties");
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_ProductionStoragePropNames_Initial()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();
            this.TestOutputHelper.WriteLine("Production Storage Properties With Values".CenterTitle());
            int found = 0;

            foreach (CoreStoragePropName propName in CoreStoragePropNameExtensions.ProductionStoragePropNames)
            {
                string? propValue = await this.LocalDeviceSecureStorage!.GetPropAsync(propName);

                if (!string.IsNullOrEmpty(propValue))
                {
                    this.TestOutputHelper.WriteLine($"[{propName}, {propName.ToPropertyString()}] = {propValue}");
                    found++;
                }
            }

            found.Should().Be(0, "There should be no initial production values.");
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_TestStoragePropNames_Initial()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();
            this.TestOutputHelper.WriteLine("Test Storage Properties With Values".CenterTitle());
            int found = 0;

            foreach (CoreStoragePropName propName in CoreStoragePropNameExtensions.TestStoragePropNames)
            {
                string? propValue = await this.LocalDeviceSecureStorage!.GetPropAsync(propName);

                if (!string.IsNullOrEmpty(propValue))
                {
                    this.TestOutputHelper.WriteLine($"[{propName}, {propName.ToPropertyString()}] = {propValue}");
                    propName.Should().BeOneOf(CoreStoragePropName.TestAppLocalUserSystemRevision, CoreStoragePropName.TestAppLocalUserSystemRevision);
                    propValue.Should().Be("1");
                    found++;
                }
            }

            found.Should().Be(1, "Did not find system revision value");
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_PlainTextStoragePropNames_Initial()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();
            this.TestOutputHelper.WriteLine("PlainText Storage Properties With Values".CenterTitle());
            int found = 0;

            foreach (CoreStoragePropName propName in CoreStoragePropNameExtensions.PlainTextStoragePropNames)
            {
                string? propValue = await this.LocalDeviceSecureStorage!.GetPropAsync(propName);

                if (!string.IsNullOrEmpty(propValue))
                {
                    this.TestOutputHelper.WriteLine($"[{propName}, {propName.ToPropertyString()}] = {propValue}");
                    propName.Should().BeOneOf(CoreStoragePropName.TestAppLocalUserSystemRevision, CoreStoragePropName.TestAppLocalUserSystemRevision);
                    propName.HasFlag(CoreStoragePropName.Encrypted).Should().BeFalse();
                    propValue.Should().Be("1");
                    found++;
                }
            }

            found.Should().Be(1, "Did not find system revision value");
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_EncryptedStoragePropNames_Initial()
        {
            this.LocalDeviceSecureStorage.Should().NotBeNull();
            this.LocalDeviceSecureStorage!.Revision.Should().Be(1);
            this.LocalDeviceSecureStorage.IsInitialized.Should().BeTrue();
            this.TestOutputHelper.WriteLine("PlainText Storage Properties With Values".CenterTitle());
            int found = 0;

            foreach (CoreStoragePropName propName in CoreStoragePropNameExtensions.EncryptedStoragePropNames)
            {
                string? propValue = await this.LocalDeviceSecureStorage!.GetPropAsync(propName);

                if (!string.IsNullOrEmpty(propValue))
                {
                    this.TestOutputHelper.WriteLine($"[{propName}, {propName.ToPropertyString()}] = {propValue}");
                    propName.Should().BeOneOf(CoreStoragePropName.TestAppLocalUserSystemRevision, CoreStoragePropName.TestAppLocalUserSystemRevision);
                    propName.HasFlag(CoreStoragePropName.Encrypted).Should().BeFalse();
                    propValue.Should().Be("1");
                    found++;
                }
            }

            found.Should().Be(0, "There should be no initial encrypted values.");
        }

        protected Exception? AddStoragePropValue(CoreStoragePropName propName)
        {
            return this.LocalDeviceSecureStorage is null
                ? throw new ArgumentNullException(nameof(this.LocalDeviceSecureStorage))
                : propName.HasFlag(CoreStoragePropName.Encrypted)
                ? this.LocalDeviceSecureStorage.TestSetPropAsyncWithEncryption(propName, DefaultValue).GetAwaiter().GetResult()
                : this.LocalDeviceSecureStorage.SetPropAsync(propName, DefaultValue).GetAwaiter().GetResult();
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (disposing)
                    {
                        this.LocalDeviceSecureStorage?.Dispose();
                    }
                }
                finally
                {
                    this._disposed = true;
                }
            }

            base.Dispose(disposing);
        }

        internal class TestCoreLocalDeviceSecureStorage : CoreLocalDeviceSecureStorage
        {
            /// <inheritdoc />
            protected TestCoreLocalDeviceSecureStorage(ICoreNetworkingSystem networkingSystem, string storageName)
                : base(networkingSystem, storageName)
            {
            }

            /// <summary>
            /// Creates a Secure Storage instance.
            /// </summary>
            /// <param name="networkingSystem">Networking system interface.</param>
            /// <param name="storageName">Name of storage.</param>
            /// <param name="taskCacheStateFlags">Initial state of storage.</param>
            /// <returns>ICoreSecureStorage or null on failure.</returns>
            public static TestCoreLocalDeviceSecureStorage? TestCreateSecureStorage(ICoreNetworkingSystem networkingSystem, string storageName, CoreTaskCacheStateFlags taskCacheStateFlags)
            {
                var secureStorage = new TestCoreLocalDeviceSecureStorage(networkingSystem, storageName);

                return secureStorage.Initialize(taskCacheStateFlags) ? secureStorage : null;
            }

            /// <summary>
            /// Gets the test version of GetPlatformRevisionPropName.
            /// </summary>
            /// <returns>CoreStoragePropName.</returns>
            public CoreStoragePropName TestGetPlatformRevisionPropName() => this.GetPlatformRevisionPropName();

            /// <summary>
            /// Gets and decrypts the value for a given property.  Uses default key for decryption.
            /// </summary>
            /// <param name="storagePropName">The storage property name to retrieve the value for.</param>
            /// <returns>A tuple the decrypted string or <see langword="null"/> if a value was not found.</returns>
            public Task<string?> TestGetPropAsyncWithEncryption(CoreStoragePropName storagePropName)
            {
                return this.GetPropAsync(storagePropName, this.GetEncryptionKey);
            }

            /// <summary>
            /// Test Sets and encrypts a value for a given property. Uses key for encryption.
            /// </summary>
            /// <param name="storagePropName">The storage property name to set the value for.</param>
            /// <param name="value">Value to set.</param>
            /// <param name="nonSecretPayload">Optional non-secret payload.</param>
            /// <param name="incrementRevision">Optionally, increment revision.</param>
            /// <param name="saveToStorage">Optionally save to storage.</param>
            /// <returns>A <see cref="Task"/> object with the current status of the asynchronous operation.</returns>
            public Task<Exception?> TestSetPropAsyncWithEncryption(CoreStoragePropName storagePropName, string value, byte[]? nonSecretPayload = null, bool incrementRevision = true, bool saveToStorage = true)
            {
                return this.SetPropAsync(storagePropName, value, this.GetEncryptionKey, nonSecretPayload, incrementRevision, saveToStorage);
            }

            public IEnumerable<KeyValuePair<string, string?>> TestGetPlatformStoredStrings() => this.GetPlatformStoredStrings();

            /// <inheritdoc/>
            protected override CoreStoragePropName GetPlatformRevisionPropName() => CoreStoragePropName.TestAppLocalUserSystemRevision.ToVersionedStoragePropName();

            protected byte[]? GetEncryptionKey(CoreStoragePropName propName)
            {
                return propName.HasFlag(CoreStoragePropName.Encrypted) ? DefaultKey : null;
            }
        }
    }
}
