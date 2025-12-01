// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreNetworkPingIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Ping;
using NetworkVisor.Core.Networking.Services.Ping;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Extensions;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Ping
{
    /// <summary>
    /// Class CoreNetworkPingIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkPingIntegrationTests))]

    public class CoreNetworkPingIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkPingIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkPingIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method NetworkPing_AndroidEmulator_IPAddress.
        /// </summary>
        [Fact]
        public async Task NetworkPing_AndroidEmulator_IPAddress()
        {
            // If Running under Android Emulator ping the first DNS address: 10.0.2.3
            if (this.TestOperatingSystem.IsAndroid && this.TestOperatingSystem.DeviceHostType == CoreDeviceHostType.Virtual)
            {
                ICoreNetworkPing networkPing = this.CreateNetworkPing(this.TestCaseLogger);
                CorePingResult pingResult = await networkPing.PingAsync("10.0.2.3");

                this.OutputPingResult(pingResult);

                pingResult.Status.Should().Be(IPStatus.Success);
            }
        }

#if NV_PLAT_NETCORE
        [Fact]
        public async Task NetworkPing_CorePingResult_Loopback_Address()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Ping))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Ping} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            // Use Dotnet ping implementation
            var pingClient = new System.Net.NetworkInformation.Ping();
            PingReply dotNetPingReply = await pingClient.SendPingAsync(IPAddress.Loopback);

            CorePingResult pingResult = new CorePingResult(dotNetPingReply);

            pingResult.Should().NotBeNull();

            this.OutputPingResult(pingResult);

            pingResult.Status.Should().Be(IPStatus.Success);

            pingResult.Address.Should().Be(IPAddress.Loopback);
        }
#endif

        [Fact]
        public async Task NetworkPing_Loopback()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Ping))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Ping} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.SendToLoopback))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.SendToLoopback} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            ICoreNetworkPing networkPing = this.CreateNetworkPing(this.TestCaseLogger);

            CorePingResult pingResult = await networkPing.PingAsync(IPAddress.Loopback);

            pingResult.Should().NotBeNull();

            this.OutputPingResult(pingResult);

            pingResult.Status.Should().Be(IPStatus.Success);

            pingResult.Address.Should().Be(IPAddress.Loopback);
        }

        /// <summary>
        /// Defines the test method NetworkPing_PublicServerIPAddress.
        /// </summary>
        [Fact]
        public async Task NetworkPing_PublicServerIPAddress()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Ping))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Ping} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            // Does not work on Android emulator
            if (this.TestOperatingSystem.IsAndroid && this.TestOperatingSystem.DeviceHostType == CoreDeviceHostType.Virtual)
            {
                return;
            }

            ICoreNetworkPing networkPing = this.CreateNetworkPing(this.TestCaseLogger);
            IPAddress publicServerIPAddress = CoreIPAddressExtensions.GetRandomPublicServerAddress();
            CorePingResult pingResult = await networkPing.PingAsync(publicServerIPAddress);

            pingResult.Should().NotBeNull();

            this.OutputPingResult(pingResult);

            if (CoreAppConstants.IsRunningInCI && pingResult.Status == IPStatus.TimedOut)
            {
                // Cloud clients sometimes cannot ping out
                pingResult.Status.Should().Be(IPStatus.TimedOut);
            }
            else
            {
                pingResult.Status.Should().Be(IPStatus.Success);
                pingResult.Address.Should().Be(publicServerIPAddress);
            }
        }

        /// <summary>
        /// Defines the test method NetworkPing_GoogleDNS_Hostname.
        /// </summary>
        [Fact]
        public async Task NetworkPing_GoogleDNS_Hostname()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Ping))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Ping} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            // Does not work on Android emulator
            if (this.TestOperatingSystem.IsAndroid && this.TestOperatingSystem.DeviceHostType == CoreDeviceHostType.Virtual)
            {
                return;
            }

            ICoreNetworkPing networkPing = this.CreateNetworkPing(this.TestCaseLogger);

            // Wait for random delay up to 10 seconds to prevent denial of service
            this.TestDelay(DateTime.Now.Millisecond % 10000, this.TestCaseLogger).Should().BeTrue();

            // Update to 12 seconds as timeout may occur due to concurrency
            CorePingResult pingResult = await networkPing.PingAsync("dns.google", 15000);

            this.OutputPingResult(pingResult);

            if (CoreAppConstants.IsRunningInCI && pingResult.Status == IPStatus.TimedOut)
            {
                // Cloud clients sometimes cannot ping out
                pingResult.Status.Should().Be(IPStatus.TimedOut);
            }
            else
            {
                pingResult.Status.Should().Be(IPStatus.Success);

                // Make sure we are matching a known Google DNS server
                pingResult.Address.ToString().Should().NotBeNullOrWhiteSpace();
                pingResult.Address.ToString().Should().Match(ip => ip.Equals("8.8.8.8") || ip.Equals("8.8.4.4") || ip.Equals("2001:4860:4860::8844") || ip.Equals("2001:4860:4860::8888"));
            }
        }

        /// <summary>
        /// Defines the test method NetworkPing_Unreachable.
        /// </summary>
        [Fact]
        public async Task NetworkPing_Unreachable()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Ping))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.Ping} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            ICoreNetworkPing networkPing = this.CreateNetworkPing(this.TestCaseLogger);
            CorePingResult pingResult = await networkPing.PingAsync("70.89.122.39");

            this.OutputPingResult(pingResult);

            pingResult.IsStatusExpired.Should().BeTrue();
        }

        /// <summary>
        /// Outputs the ping result.
        /// </summary>
        /// <param name="pingResult">The ping result.</param>
        [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
        private void OutputPingResult(CorePingResult pingResult)
        {
            this.TestOutputHelper.WriteLine($"Ping Status: {pingResult.Status}");
            this.TestOutputHelper.WriteLine($"Ping Error Message: {pingResult.ErrorMessage}");
            this.TestOutputHelper.WriteLine($"Ping Host Address: {pingResult.Address}");
            this.TestOutputHelper.WriteLine($"Ping RoundtripTime (ms): {pingResult.RoundtripTime}");
        }

        private ICoreNetworkPing CreateNetworkPing(ICoreTestCaseLogger testCaseLogger)
        {
            return new CoreNetworkPing(this.TestNetworkingSystem, this.TestCaseLogger);
        }
    }
}
