// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreVaultClientIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using Azure;
using Azure.Security.KeyVault.Secrets;
using FluentAssertions;
using NetworkVisor.Core.Cloud.Vault;
using NetworkVisor.Core.Configuration;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Cloud.Vault
{
    /// <summary>
    /// Class CoreVaultClientIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreVaultClientIntegrationTests))]

    public class CoreVaultClientIntegrationTests : CoreTestCaseBase
    {
        private const string? SkipReason = "Requires Default Azure Credential";

        private readonly string testKey = "TestKey";                                            // Key in vaultName to retrieve

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreVaultClientIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreVaultClientIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public static bool RunWithDefaultAzureCredential => CoreAppConstants.IsLocalDevEnvironment;

        public static string GetTestSecret(CoreHostEnvironment authHostEnvironment)
        {
            if (authHostEnvironment == CoreHostEnvironment.Default)
            {
                authHostEnvironment = CoreEnvironmentSettings.RuntimeHostEnvironment;
            }

            return $"TestSecret_{authHostEnvironment}";
        }

        [Fact]
        public void VaultClientIntegration_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            this.TestNetworkingSystem.Logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();
        }

        [Theory(SkipUnless = nameof(RunWithDefaultAzureCredential), Skip = SkipReason)]
        [MemberData(nameof(HostEnvironments))]
        public void VaultClientIntegration_KeyValue_DefaultAzureCredential(CoreHostEnvironment hostEnvironment)
        {
            var client = new CoreVaultClient(hostEnvironment);
            this.TestOutputHelper.WriteLine($"Retrieving your secret from {client.VaultUri}.");
            Response<KeyVaultSecret> secret = client.GetSecret(this.testKey);
            secret.Value.Value.Should().NotBeNull().And.Subject.Should().Be(GetTestSecret(hostEnvironment));
            this.TestOutputHelper.WriteLine($"Your secret is '{secret.Value.Value}'.");
        }

        [Theory(SkipUnless = nameof(RunWithDefaultAzureCredential), Skip = SkipReason)]
        [MemberData(nameof(HostEnvironments))]
        public async Task VaultClientIntegration_KeyValueAsync_DefaultAzureCredential(CoreHostEnvironment hostEnvironment)
        {
            var client = new CoreVaultClient(hostEnvironment);
            this.TestOutputHelper.WriteLine($"Retrieving your secret from {client.VaultUri}.");
            Response<KeyVaultSecret> secret = await client.GetSecretAsync(this.testKey);
            secret.Value.Value.Should().NotBeNull().And.Subject.Should().Be(GetTestSecret(hostEnvironment));
            this.TestOutputHelper.WriteLine($"Your secret is '{secret.Value.Value}'.");
        }
    }
}
