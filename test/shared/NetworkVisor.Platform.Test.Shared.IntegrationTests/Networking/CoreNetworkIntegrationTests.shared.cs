// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// ***********************************************************************
// <copyright file="CoreNetworkIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Extensions;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking
{
    /// <summary>
    /// Class CoreNetworkIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkIntegrationTests))]

    public class CoreNetworkIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
        }

        /// <summary>
        /// Defines the test method NetworkEnumerator_Ctor.
        /// </summary>
        [Fact]
        public void NetworkEnumerator_Ctor()
        {
            var networkEnumerator = new CoreNetworkCollection(IPAddress.Parse("192.168.1.0").ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC));
            var count = 0;

            foreach (IPAddress ipAddress in networkEnumerator)
            {
                this.TestOutputHelper.WriteLine($"IPAddress({count}):\t{ipAddress}");
                count++;
            }

            count.Should().Be(256);
        }

        /// <summary>
        /// Defines the test method NetworkIntegration_GetHostName.
        /// </summary>
        [Fact]
        public async Task NetworkIntegration_GetHostIPAddressAsync()
        {
            var hostName = this.TestNetworkingSystem.DeviceHostName;
            hostName.Should().NotBeNullOrWhiteSpace();
            this.TestOutputHelper.WriteLine($"Hostname {hostName}");

            // Use empty string if result is null.  Returns local IP address.
            IPHostEntry? ipHostEntry = await this.TestNetworkingSystem.GetDnsHostEntryAsync(hostName, CancellationToken.None);

            if (ipHostEntry is null)
            {
                CoreAppConstants.IsRunningInCI.Should().BeTrue();
                this.TestOperatingSystem.BuildHostType.Should().Be(CoreBuildHostType.MacOS);
            }
            else
            {
                foreach (IPAddress? ipAddress in ipHostEntry.AddressList)
                {
                    this.TestOutputHelper.WriteLine($"IPAddress {ipAddress}");
                }
            }
        }

        /// <summary>
        /// Defines the test method NetworkIntegration_GetLocalHostIPAddressAsync.
        /// </summary>
        [Fact]
        public async Task NetworkIntegration_GetLocalHostIPAddressAsync()
        {
            // Use empty string if result is null.  Returns local IP address.
            IPHostEntry? ipHostEntry = await this.TestNetworkingSystem.GetDnsHostEntryAsync(string.Empty);

            if (ipHostEntry is null)
            {
                CoreAppConstants.IsRunningInCI.Should().BeTrue();
                this.TestOperatingSystem.BuildHostType.Should().Be(CoreBuildHostType.MacOS);
            }
            else
            {
                foreach (IPAddress? ipAddress in ipHostEntry.AddressList)
                {
                    this.TestOutputHelper.WriteLine($"IPAddress {ipAddress}");
                }
            }
        }
    }
}
