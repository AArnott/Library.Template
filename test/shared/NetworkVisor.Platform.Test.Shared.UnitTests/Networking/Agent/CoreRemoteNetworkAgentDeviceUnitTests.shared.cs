// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreRemoteNetworkAgentDeviceUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Encryption;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Services.MulticastDns.Events;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking.Agent
{
    /// <summary>
    /// Class CoreRemoteNetworkAgentDeviceUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreRemoteNetworkAgentDeviceUnitTests))]

    public class CoreRemoteNetworkAgentDeviceUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreRemoteNetworkAgentDeviceUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreRemoteNetworkAgentDeviceUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method RemoteNetworkAgentDevice_ParseServiceInstanceNameForDeviceID.
        /// </summary>
        /// <param name="serviceInstanceName">Service Instance name.</param>
        /// <param name="expectedDeviceID">Expected DeviceID.</param>
        [Theory]
        [InlineData(
            "NetworkVisor [183cfc362cd71756ac64469261997324]._sack._tcp.networkvisor.local.",
            "183cfc362cd71756ac64469261997324")]
        [InlineData(
            "NetworkVisor [a94675fa-8669-40b2-b308-db7f1bfbd3f6]._sack._tcp.networkvisor.local.",
            "a94675fa866940b2b308db7f1bfbd3f6")]
        [InlineData("NetworkVisor [183cfc362cd71756ac64469261997324._sack._tcp.networkvisor.local.", null)]
        [InlineData("NetworkVisor 183cfc362cd71756ac64469261997324]._sack._tcp.networkvisor.local.", null)]
        public void RemoteNetworkAgentDevice_ParseServiceInstanceNameForDeviceID(
            string? serviceInstanceName,
            string? expectedDeviceID)
        {
            Guid? result = CoreNetworkAgentConstants.ParseServiceInstanceNameForDeviceID(serviceInstanceName);
            this.TestOutputHelper.WriteLine($"DeviceID : {result}");

            if (expectedDeviceID is null)
            {
                result.Should().BeNull();
            }
            else
            {
                result.Should().NotBeNull();
                result!.Value.ToStringNoDashes().Should().Be(expectedDeviceID);
            }
        }

        [Fact]
        public void RemoteNetworkAgentDevice_TxtRecord_Encrypt()
        {
        }

        /// <summary>
        /// Protects an array of bytes.
        /// </summary>
        /// <param name="data">Array of bytes to protect.</param>
        /// <param name="key"></param>
        /// <param name="nonSecretPayload"></param>
        /// <returns>Encrypted array of bytes.</returns>
        protected virtual (byte[]? EncryptedBytes, Exception? Exception)? Encrypt(byte[] data, byte[] key, byte[]? nonSecretPayload = null)
        {
            return CoreEncryptionBouncyCastle.SimpleEncrypt(data, key, nonSecretPayload);
        }

        /// <summary>
        /// Unprotects an array of bytes.
        /// </summary>
        /// <param name="encryptedMessage"></param>
        /// <param name="key"></param>
        /// <param name="nonSecretPayloadLength"></param>
        /// <returns>Unencrypted array of bytes.</returns>
        protected virtual (byte[]? PlainBytes, Exception? Exception)? Decrypt(byte[] encryptedMessage, byte[] key, int nonSecretPayloadLength = 0)
        {
            return CoreEncryptionBouncyCastle.SimpleDecrypt(encryptedMessage, key, nonSecretPayloadLength);
        }
    }
}
