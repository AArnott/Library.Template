// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreEncryptionRSAIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using NetworkVisor.Core.Encryption.Asymmetry.Certificates;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Org.BouncyCastle.Asn1.X509;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Encryption
{
    /// <summary>
    /// Class CoreEncryptionRSAIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreEncryptionRSAIntegrationTests))]

    public class CoreEncryptionRSAIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreEncryptionRSAIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreEncryptionRSAIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreEncryptionRSA_IssueCertificate()
        {
            string randomFileName = Guid.NewGuid().ToStringNoDashes();
            string certPath = Path.Combine(this.TestFileSystem.LocalUserAppCertsFolderPath, $"{randomFileName}.crt");
            string pfxPath = Path.Combine(this.TestFileSystem.LocalUserAppCertsFolderPath, $"{randomFileName}.pfx");

            var cn = $"CN={this.TestNetworkingSystem.DeviceHostNameQualified}";
            var issuer = new X509Name(cn);
            var subject = new X509Name(cn);

            DateTime dateTimeStart = DateTime.UtcNow;

            var keyUsage = new KeyUsage(KeyUsage.KeyEncipherment | KeyUsage.DataEncipherment);

            // Add the "Extended Key Usage" attribute, specifying "server authentication".
            KeyPurposeID[] usages = new[] { KeyPurposeID.id_kp_serverAuth };

            CoreCertificateGenerator.GenerateCACertificateFilesX509V3(
                    this.TestNetworkingSystem,
                    "RSA",
                    2048,
                    "TestPassword",
                    "SHA512WITHRSA",
                    dateTimeStart,
                    dateTimeStart.AddYears(1),
                    issuer,
                    subject,
                    certPath,
                    pfxPath,
                    "Network Visor Test Cert",
                    keyUsage,
                    usages)
                .Should().BeTrue();

            this.TestFileSystem.FileExists(certPath).Should().BeTrue();
            this.TestFileSystem.FileExists(pfxPath).Should().BeTrue();
        }
    }
}
