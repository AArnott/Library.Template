// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreLocalDeviceSecureStorageIntegrationTests.shared.cs" company="Network Visor">
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

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Storage
{
    /// <summary>
    /// Class CoreLocalDeviceSecureStorageIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreLocalDeviceSecureStorageIntegrationTests))]

    public class CoreLocalDeviceSecureStorageIntegrationTests : CoreTestCaseBase
    {
        private static readonly byte[] DefaultKey = [132, 19, 13, 129, 1, 135, 31, 237, 242, 183, 80, 235, 21, 84, 21, 210, 140, 231, 99, 144, 75, 186, 3, 76, 61, 80, 251, 238, 237, 23, 120, 167];
        private static readonly byte[] BadKey = [13, 19, 13, 129, 1, 135, 31, 237, 242, 183, 80, 235, 21, 84, 21, 210, 140, 231, 99, 144, 75, 186, 3, 76, 61, 80, 251, 238, 237, 23, 120, 167];
        private static readonly string DefaultValue = "Value";
        private static readonly string DefaultValue2 = "Value2";
        private static readonly byte[] DefaultNonSecretPayload = [0, 1, 2, 3, 4, 5, 6, 7, 8, 9];
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreLocalDeviceSecureStorageIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreLocalDeviceSecureStorageIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.SecureStorage = TestCoreSecureStorage.TestCreateSecureStorage(this.TestNetworkingSystem, $"storage.test.{Guid.NewGuid().ToStringNoDashes()}", CoreTaskCacheStateFlags.NotInitialized);
        }

        internal TestCoreSecureStorage? SecureStorage { get; }

        [Fact]
        public async Task LocalDeviceSecureStorage_GetAsync_Empty()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey)).Should().BeNull();

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Property()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_RemoveAsync()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.SecureStorage.RemovePropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty)).Should().BeTrue();
            this.SecureStorage.Revision.Should().Be(3);

            propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(3);
            propertyValue.Should().BeNull();

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_RemoveAsync_MissingKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.RemovePropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty)).Should().BeFalse();
            this.SecureStorage.Revision.Should().Be(1);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(1);
            propertyValue.Should().BeNull();

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_RemoveAllAsync()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.SecureStorage.RemoveAllAsync()).Should().BeTrue();
            this.SecureStorage.Revision.Should().Be(3);

            propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(3);
            propertyValue.Should().BeNull();

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_RemoveAllAsync_ResetRevision()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.SecureStorage.RemoveAllAsync(false, true)).Should().BeTrue();
            this.SecureStorage.Revision.Should().Be(1);

            propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(1);
            propertyValue.Should().BeNull();

            this.OutputSecureStorage();
        }

        [Fact]
        public void LocalDeviceSecureStorage_Revision_Initial()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            this.OutputSecureStorage();
        }

        [Fact]
        public void LocalDeviceSecureStorage_Revision_Increment()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            this.SecureStorage.IncrementRevision().Should().Be(2);
            this.SecureStorage.IsInitialized.Should().BeTrue();
            this.SecureStorage.Revision.Should().Be(2);
            this.SecureStorage.GetProp(this.SecureStorage.TestGetPlatformRevisionPropName()).Should().Be("2");

            this.OutputSecureStorage();
        }

        [Fact]
        public void LocalDeviceSecureStorage_Revision_IncrementBy5()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            this.SecureStorage.IncrementRevision(5).Should().Be(6);
            this.SecureStorage.IsInitialized.Should().BeTrue();
            this.SecureStorage.Revision.Should().Be(6);
            this.SecureStorage.GetProp(this.SecureStorage.TestGetPlatformRevisionPropName()).Should().Be("6");

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Property_NonSecretPayload()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey, DefaultNonSecretPayload)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey, DefaultNonSecretPayload.Length);
            this.SecureStorage.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue!.Value.Exception.Should().BeNull();
            propertyValue!.Value.PropValue.Should().Be(DefaultValue);
            propertyValue.Value.NonSecretPayload.Should().NotBeNull();
            propertyValue.Value.NonSecretPayload.Should().Equal(DefaultNonSecretPayload);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Property_Twice()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue2, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(3);

            propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(3);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue2);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Property_MissingKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty.ToVersionedStoragePropName(2), DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);

            propertyValue.Should().BeNull();

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_SetAsync_Property_InvalidKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage!.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty | CoreStoragePropName.ComponentSystem, DefaultValue, DefaultKey)).Should().BeOfType<ArgumentException>();
            this.SecureStorage!.Revision.Should().Be(1);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_DecryptProperty_PropertyValue()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? encryptedPropertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue);
            encryptedPropertyValue.Should().NotBeNullOrEmpty();

            string? plainTextPropertyValue = this.SecureStorage.DecryptPropertyValue(encryptedPropertyValue, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValue);
            plainTextPropertyValue.Should().NotBeNullOrEmpty();
            plainTextPropertyValue.Should().Be(DefaultValue);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_DecryptProperty_PropertyValue_NullKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? encryptedPropertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue);
            encryptedPropertyValue.Should().NotBeNullOrEmpty();

            string? plainTextPropertyValue = this.SecureStorage.DecryptPropertyValue(encryptedPropertyValue, null);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValue);
            plainTextPropertyValue.Should().NotBeNullOrEmpty();
            plainTextPropertyValue.Should().Be(encryptedPropertyValue);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_DecryptProperty_PropertyValue_BadKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? encryptedPropertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue);
            encryptedPropertyValue.Should().NotBeNullOrEmpty();

            string? plainTextPropertyValue = this.SecureStorage.DecryptPropertyValue(encryptedPropertyValue, BadKey);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValue);
            plainTextPropertyValue.Should().BeNull();

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_DecryptProperty_KeyFunc()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? encryptedPropertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue);
            encryptedPropertyValue.Should().NotBeNullOrEmpty();

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? result = this.SecureStorage.DecryptPropertyValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue, (_) => DefaultKey);

            this.SecureStorage.Revision.Should().Be(2);
            result.Should().NotBeNull();
            result?.Exception.Should().BeNull();
            result?.NonSecretPayload.Should().BeEmpty();
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, result?.PropValue);
            result?.PropValue.Should().NotBeNullOrEmpty();
            result?.PropValue.Should().Be(DefaultValue);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_DecryptProperty_KeyFunc_NullKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? encryptedPropertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue);
            encryptedPropertyValue.Should().NotBeNullOrEmpty();

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? result = this.SecureStorage.DecryptPropertyValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue, (_) => null);

            this.SecureStorage.Revision.Should().Be(2);
            result.Should().NotBeNull();
            result?.Exception.Should().BeNull();
            result?.NonSecretPayload.Should().BeNull();
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, result?.PropValue);
            result?.PropValue.Should().NotBeNullOrEmpty();
            result?.PropValue.Should().Be(encryptedPropertyValue);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_DecryptProperty_KeyFunc_BadKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? encryptedPropertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue);
            encryptedPropertyValue.Should().NotBeNullOrEmpty();

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? result = this.SecureStorage.DecryptPropertyValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue, (_) => BadKey);

            this.SecureStorage.Revision.Should().Be(2);
            result.Should().NotBeNull();
            result?.Exception.Should().BeOfType<InvalidCipherTextException>();
            result?.NonSecretPayload.Should().BeNull();
            result?.PropValue.Should().BeNull();

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_DecryptProperty_NonSecretPayload()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey, DefaultNonSecretPayload)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? encryptedPropertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue);
            encryptedPropertyValue.Should().NotBeNullOrEmpty();

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? result = this.SecureStorage.DecryptPropertyValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue, (_) => DefaultKey, DefaultNonSecretPayload.Length);

            this.SecureStorage.Revision.Should().Be(2);
            result.Should().NotBeNull();
            result?.Exception.Should().BeNull();
            result?.NonSecretPayload.Should().BeEquivalentTo(DefaultNonSecretPayload);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, result?.PropValue);
            result?.PropValue.Should().NotBeNullOrEmpty();
            result?.PropValue.Should().Be(DefaultValue);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_DecryptProperty_NonSecretPayload_NullKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey, DefaultNonSecretPayload)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? encryptedPropertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue);
            encryptedPropertyValue.Should().NotBeNullOrEmpty();

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? result = this.SecureStorage.DecryptPropertyValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue, (_) => null, DefaultNonSecretPayload.Length);

            this.SecureStorage.Revision.Should().Be(2);
            result.Should().NotBeNull();
            result?.Exception.Should().BeNull();
            result?.NonSecretPayload.Should().BeNull();
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, result?.PropValue);
            result?.PropValue.Should().NotBeNullOrEmpty();
            result?.PropValue.Should().Be(encryptedPropertyValue);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_DecryptProperty_NonSecretPayload_BadKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey, DefaultNonSecretPayload)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? encryptedPropertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue);
            encryptedPropertyValue.Should().NotBeNullOrEmpty();

            (string? PropValue, byte[]? NonSecretPayload, Exception? Exception)? result = this.SecureStorage.DecryptPropertyValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedPropertyValue, (_) => BadKey, DefaultNonSecretPayload.Length);

            this.SecureStorage.Revision.Should().Be(2);
            result.Should().NotBeNull();
            result?.Exception.Should().BeOfType<InvalidCipherTextException>();
            result?.NonSecretPayload.Should().BeNull();
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, result?.PropValue);
            result?.PropValue.Should().BeNullOrEmpty();

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_ReKeyPropAsync()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? plainTextPropertyValueOld = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValueOld);
            plainTextPropertyValueOld.Should().NotBeNullOrEmpty();
            plainTextPropertyValueOld.Should().Be(DefaultValue);

            string? encryptedTextPropertyValueOld = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedTextPropertyValueOld);
            encryptedTextPropertyValueOld.Should().NotBeNullOrEmpty();

            Exception? result = await this.SecureStorage.ReKeyPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, (_) => DefaultKey, (_) => BadKey);
            this.SecureStorage.Revision.Should().Be(3);
            result.Should().BeNull();

            string? plainTextPropertyValueNew = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, BadKey);
            this.SecureStorage.Revision.Should().Be(3);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValueNew);
            plainTextPropertyValueNew.Should().Be(plainTextPropertyValueOld);
            plainTextPropertyValueNew.Should().Be(DefaultValue);

            string? encryptedTextPropertyValueNew = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(3);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedTextPropertyValueNew);
            encryptedTextPropertyValueNew.Should().NotBe(encryptedTextPropertyValueOld);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_ReKeyPropAsync_SameKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? plainTextPropertyValueOld = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValueOld);
            plainTextPropertyValueOld.Should().NotBeNullOrEmpty();
            plainTextPropertyValueOld.Should().Be(DefaultValue);

            string? encryptedTextPropertyValueOld = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedTextPropertyValueOld);
            encryptedTextPropertyValueOld.Should().NotBeNullOrEmpty();

            Exception? result = await this.SecureStorage.ReKeyPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, (_) => DefaultKey, (_) => DefaultKey);
            this.SecureStorage.Revision.Should().Be(3);
            result.Should().BeNull();

            string? plainTextPropertyValueNew = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(3);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValueNew);
            plainTextPropertyValueNew.Should().Be(plainTextPropertyValueOld);
            plainTextPropertyValueNew.Should().Be(DefaultValue);

            string? encryptedTextPropertyValueNew = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(3);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedTextPropertyValueNew);
            encryptedTextPropertyValueNew.Should().NotBe(encryptedTextPropertyValueOld);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_ReKeyPropAsync_WrongKey()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            string? plainTextPropertyValueOld = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValueOld);
            plainTextPropertyValueOld.Should().NotBeNullOrEmpty();
            plainTextPropertyValueOld.Should().Be(DefaultValue);

            string? encryptedTextPropertyValueOld = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, encryptedTextPropertyValueOld);
            encryptedTextPropertyValueOld.Should().NotBeNullOrEmpty();

            Exception? result = await this.SecureStorage.ReKeyPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, (_) => DefaultKey, (_) => BadKey);
            this.SecureStorage.Revision.Should().Be(3);
            result.Should().BeNull();

            (await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey)).Should().BeNull();
            (await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, BadKey)).Should().Be(DefaultValue);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_ReKeyAllPropsAsync()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(2);

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty | CoreStoragePropName.Version2, DefaultValue2, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(3);

            string? plainTextPropertyValueOld = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(3);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValueOld);
            plainTextPropertyValueOld.Should().NotBeNullOrEmpty();
            plainTextPropertyValueOld.Should().Be(DefaultValue);

            Dictionary<CoreStoragePropName, Exception?> results = await this.SecureStorage.ReKeyAllPropsAsync((spn) => spn.ToPropNameWithoutVersion() == CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, (_) => DefaultKey, (_) => BadKey);
            this.SecureStorage.Revision.Should().Be(5);
            results.Should().NotBeNull();

            results.Count.Should().Be(2);
            KeyValuePair<CoreStoragePropName, Exception?> result = results.FirstOrDefault(kvp => kvp.Key == (CoreStoragePropName.TestEncryptedAppLocalUserTestProperty | CoreStoragePropName.Version1));
            result.Should().NotBeNull();
            result.Key.Should().Be(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty | CoreStoragePropName.Version1);
            result.Value.Should().BeNull();

            string? plainTextPropertyValueNew = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty | CoreStoragePropName.Version2, BadKey);
            this.SecureStorage.Revision.Should().Be(5);
            this.OutputPropNameAndValue(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, plainTextPropertyValueNew);
            plainTextPropertyValueNew.Should().NotBeNullOrEmpty();
            plainTextPropertyValueNew.Should().Be(DefaultValue2);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Output_DatabaseFile_Exists()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            this.SecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);
            this.SecureStorage.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            this.TestOutputHelper.WriteLine($"StoragePath: {this.SecureStorage.StoragePath}");

            if (this.SecureStorage.UsesFileStorage)
            {
                this.TestFileSystem.FileExists(this.SecureStorage.StoragePath).Should().BeTrue();
            }

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Output_DatabaseFile_Contents()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            this.SecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);

            this.TestOutputHelper.WriteLine($"Property Value: {propertyValue}");
            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty.ToVersionedStoragePropName(2), DefaultValue2)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(3);

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty.ToVersionedStoragePropName(3), DefaultValue)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(4);

            this.TestOutputHelper.WriteLine($"StoragePath: {this.SecureStorage.StoragePath}");

            if (this.SecureStorage.UsesFileStorage)
            {
                this.TestFileSystem.FileExists(this.SecureStorage.StoragePath).Should().BeTrue();

                var storageContents = await this.TestFileSystem.ReadFileContentsAsync(this.SecureStorage.StoragePath);
                this.TestOutputHelper.WriteLine();
                this.TestOutputHelper.WriteLine($"Secure Database:{Environment.NewLine}{storageContents}");
            }

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Output_DatabaseFile_ReloadFromStorage()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage!.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            this.SecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);

            this.TestOutputHelper.WriteLine($"Property Value: {propertyValue}");
            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty.ToVersionedStoragePropName(2), DefaultValue2)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(3);

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty.ToVersionedStoragePropName(3), DefaultValue)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(4);

            this.TestOutputHelper.WriteLine($"StoragePath: {this.SecureStorage.StoragePath}");
            this.TestFileSystem.FileExists(this.SecureStorage.StoragePath).Should().Be(this.SecureStorage.UsesFileStorage);

            (await this.SecureStorage.TestLoadFromStorageAsync(true)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(4);

            string? propertyValueReload = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty);

            this.TestOutputHelper.WriteLine($"Reloaded property Value: {propertyValueReload}");
            propertyValueReload.Should().NotBeNull();
            propertyValueReload.Should().Be(DefaultValue);
            this.SecureStorage.Revision.Should().Be(4);
            propertyValueReload.Should().Be(propertyValue);

            if (this.SecureStorage.UsesFileStorage)
            {
                var storageContents = await this.TestFileSystem.ReadFileContentsAsync(this.SecureStorage.StoragePath);
                this.TestOutputHelper.WriteLine();
                this.TestOutputHelper.WriteLine($"Secure Database:{Environment.NewLine}{storageContents}");
            }

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_Output_DatabaseFile_Exists()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage!.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);
            this.SecureStorage.Revision.Should().Be(2);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            this.TestOutputHelper.WriteLine($"StoragePath: {this.SecureStorage.StoragePath}");
            this.TestFileSystem.FileExists(this.SecureStorage.StoragePath).Should().Be(this.SecureStorage.UsesFileStorage);

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_Encrypted_Output_DatabaseFile_Contents()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage!.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage!.Revision.Should().Be(2);

            string? propertyValue = await this.SecureStorage.GetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty, DefaultKey);

            propertyValue.Should().NotBeNull();
            propertyValue.Should().Be(DefaultValue);

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty.ToVersionedStoragePropName(2), DefaultValue2, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(3);

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty.ToVersionedStoragePropName(3), DefaultValue, DefaultKey)).Should().BeNull();
            this.SecureStorage.Revision.Should().Be(4);

            this.TestOutputHelper.WriteLine($"StoragePath: {this.SecureStorage.StoragePath}");
            this.TestFileSystem.FileExists(this.SecureStorage.StoragePath).Should().Be(this.SecureStorage.UsesFileStorage);

            if (this.SecureStorage.UsesFileStorage)
            {
                var storageContents = await this.TestFileSystem.ReadFileContentsAsync(this.SecureStorage.StoragePath);
                this.TestOutputHelper.WriteLine();
                this.TestOutputHelper.WriteLine($"Secure Database:{Environment.NewLine}{storageContents}");
            }

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_StoredPropertyKeys()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty | CoreStoragePropName.Version2, DefaultValue, DefaultKey)).Should().BeNull();

            ICollection<string> keys = this.SecureStorage.StoredPropertyKeys;

            foreach (var key in keys.OrderBy(k => k))
            {
                this.TestOutputHelper.WriteLine($"[{key}]");
            }
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_GetPlatformStoredStrings()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty | CoreStoragePropName.Version2, DefaultValue, DefaultKey)).Should().BeNull();

            this.OutputSecureStorage();
        }

        [Fact]
        public async Task LocalDeviceSecureStorage_PropertyToStoragePropNames()
        {
            this.SecureStorage.Should().NotBeNull();
            this.SecureStorage!.Revision.Should().Be(1);
            this.SecureStorage.IsInitialized.Should().BeTrue();

            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestAppLocalUserTestProperty, DefaultValue)).Should().BeNull();
            (await this.SecureStorage.SetPropAsync(CoreStoragePropName.TestEncryptedAppLocalUserTestProperty | CoreStoragePropName.Version2, DefaultValue, DefaultKey)).Should().BeNull();

            foreach (KeyValuePair<string, CoreStoragePropName> kvp in this.SecureStorage.PropertyToStoragePropNames)
            {
                this.TestOutputHelper.WriteLine($"{kvp.Key}, [{kvp.Value}]");
                kvp.Key.ToStoragePropName().Should().Be(kvp.Value);
                kvp.Value.ToPropertyString().Should().Be(kvp.Key);
            }

            this.OutputSecureStorage();
        }

        protected void OutputPropNameAndValue(CoreStoragePropName storagePropName, string? propValue)
        {
            string? propertyString = storagePropName.ToPropertyString();

            this.TestOutputHelper.WriteLine($"[{storagePropName}, {propertyString}] = {propValue}");
        }

        protected void OutputPropNameAndValue(string? propName, string? propValue)
        {
            this.TestOutputHelper.WriteLine($"[{propName}] = {propValue}");
        }

        protected void OutputSecureStorage()
        {
            this.SecureStorage.Should().NotBeNull();

            this.TestOutputHelper.WriteLine();
            this.TestOutputHelper.WriteLine("Contents of Secure Storage".CenterTitle());
            IEnumerable<KeyValuePair<string, string?>> storedValues = this.SecureStorage!.TestGetPlatformStoredStrings();

            foreach (KeyValuePair<string, string?> kvp in storedValues.OrderBy(kvp => kvp.Key))
            {
                this.OutputPropNameAndValue(kvp.Key, kvp.Value);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this._disposed)
            {
                try
                {
                    if (disposing)
                    {
                        this.SecureStorage?.Dispose();
                    }
                }
                finally
                {
                    this._disposed = true;
                }
            }

            base.Dispose(disposing);
        }

        internal class TestCoreSecureStorage : CoreLocalDeviceSecureStorage
        {
            /// <inheritdoc />
            protected TestCoreSecureStorage(ICoreNetworkingSystem networkingSystem, string storageName)
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
            public static TestCoreSecureStorage? TestCreateSecureStorage(ICoreNetworkingSystem networkingSystem, string storageName, CoreTaskCacheStateFlags taskCacheStateFlags)
            {
                var secureStorage = new TestCoreSecureStorage(networkingSystem, storageName);

                return secureStorage.Initialize(taskCacheStateFlags) ? secureStorage : null;
            }

            /// <summary>
            /// Gets the test version of GetPlatformRevisionPropName.
            /// </summary>
            /// <returns>CoreStoragePropName.</returns>
            public CoreStoragePropName TestGetPlatformRevisionPropName() => this.GetPlatformRevisionPropName();

            public Task<Exception?> TestLoadFromStorageAsync(bool forceReload = false)
            {
                return this.LoadFromStorageAsync(forceReload);
            }

            /// <summary>
            /// Gets and decrypts the value for a given property name.
            /// </summary>
            /// <param name="propName">Property name to retrieve the value for.</param>
            /// <returns>The decrypted string value or <see langword="null"/> if a value was not found.</returns>
            public string? GetProp(CoreStoragePropName propName)
            {
                return this.GetPropAsync(propName).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            /// <summary>
            /// Sets and encrypts a value for a given property. Uses key for encryption.
            /// </summary>
            /// <param name="propName">Storage property name to set the value for.</param>
            /// <param name="value">Value to set.</param>
            /// <param name="incrementRevision">Optionally, increment revision.</param>
            /// <param name="saveToStorage">Optionally save to storage.</param>
            /// <returns>True if successful.</returns>
            public Exception? SetProp(CoreStoragePropName propName, string value, bool incrementRevision = true, bool saveToStorage = true)
            {
                return this.SetPropAsync(propName, value, incrementRevision, saveToStorage).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            public IEnumerable<KeyValuePair<string, string?>> TestGetPlatformStoredStrings() => this.GetPlatformStoredStrings();

            /// <inheritdoc/>
            protected override CoreStoragePropName GetPlatformRevisionPropName() => CoreStoragePropName.TestAppLocalUserSystemRevision.ToVersionedStoragePropName();
        }
    }
}
