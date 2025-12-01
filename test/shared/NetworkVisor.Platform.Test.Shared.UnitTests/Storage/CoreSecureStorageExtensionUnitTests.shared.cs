// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreSecureStorageExtensionUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Storage;
using NetworkVisor.Core.Storage.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Storage
{
    /// <summary>
    /// Class CoreSecureStorageExtensionsUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreSecureStorageExtensionsUnitTests))]

    public class CoreSecureStorageExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSecureStorageExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSecureStorageExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData("", false, CoreStoragePropName.Unknown)]
        [InlineData(null, false, CoreStoragePropName.Unknown)]
        [InlineData("a.l.u.nva.id", true, CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId)]
        [InlineData("a.l.u.nva.id.1", true, CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId | CoreStoragePropName.Version1)]
        [InlineData("a.l.u.nva.id.2", true, CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId | CoreStoragePropName.Version2)]
        [InlineData("a.l.u.nva.id.50", true, CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId | (CoreStoragePropName)50)]
        public void SecureStorageExtensions_TryParseToStoragePropName(string? propertyName, bool parseResult, CoreStoragePropName storagePropNameExcepted)
        {
            CoreStoragePropNameExtensions.TryParseToStoragePropName(propertyName!, out CoreStoragePropName storagePropName).Should().Be(parseResult);
            storagePropName.Should().Be(storagePropNameExcepted);
        }

        [Theory]
        [InlineData("", false, CoreStoragePropName.Unknown, 0)]
        [InlineData(null, false, CoreStoragePropName.Unknown, 0)]
        [InlineData("a.l.u.nva.id", true, CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId, 0)]
        [InlineData("a.l.u.nva.id.1", true, CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId, 1)]
        [InlineData("a.l.u.nva.id.2", true, CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId, 2)]
        [InlineData("a.l.u.nva.id.50", true, CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId, 50)]
        public void SecureStorageExtensions_TryParseToStoragePropNameWithVersion(string? propertyName, bool parseResult, CoreStoragePropName storagePropNameNoVersionExcepted, int versionExpected)
        {
            CoreStoragePropNameExtensions.TryParseToStoragePropNameWithVersion(propertyName!, out CoreStoragePropName storagePropName, out int storagePropVersion).Should().Be(parseResult);
            storagePropName.Should().Be(storagePropNameNoVersionExcepted);
            storagePropVersion.Should().Be(versionExpected);
        }

        [Theory]
        [InlineData(CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId, true, "a.l.u.nva.id.1", 1)]
        [InlineData(CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId | CoreStoragePropName.Version1, true, "a.l.u.nva.id.1", 1)]
        [InlineData(CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId | CoreStoragePropName.Version2, true, "a.l.u.nva.id.2", 2)]
        [InlineData(CoreStoragePropName.AppLocalUserNetworkVisorAgentDeviceId | (CoreStoragePropName)50, true, "a.l.u.nva.id.50", 50)]

        public void SecureStorageExtensions_ToPropertyString(CoreStoragePropName storagePropName, bool parseResult, string? propertyStringExpected, int versionExpected)
        {
            storagePropName.ToPropertyString().Should().Be(propertyStringExpected);
            versionExpected.Should().Be(versionExpected);
            parseResult.Should().BeTrue();
        }
    }
}
