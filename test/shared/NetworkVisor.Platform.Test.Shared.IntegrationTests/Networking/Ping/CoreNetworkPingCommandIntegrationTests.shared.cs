// Assembly         : NetworkVisor.Platform.Test.Shared.Messaging.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreNetworkPingCommandIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Database.Providers.SQLite.Tables;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Database;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Entities.Networks.Addresses;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Messaging.Inbox;
using NetworkVisor.Core.Messaging.Queries.Entities;
using NetworkVisor.Core.Messaging.Queries.Entities.Base;
using NetworkVisor.Core.Messaging.Queries.Entities.NetworkAddress;
using NetworkVisor.Core.Messaging.Tables;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Ping;
using NetworkVisor.Core.Networking.Services.Ping.Commands;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base;
using Paramore.Brighter;
using Xunit;
using IDispatcher = Paramore.Brighter.ServiceActivator.IDispatcher;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.Ping
{
    /// <summary>
    /// Class CoreNetworkPingCommandIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkPingCommandIntegrationTests))]

    public class CoreNetworkPingCommandIntegrationTests : CoreCommandTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkPingCommandIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkPingCommandIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NetworkPingCommand_CommandDispatchService_Ctor()
        {
            _ = this.TestCommandProcessor.ProducersConfiguration.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IAmProducersConfiguration>();

            _ = this.TestCommandProcessor.ServiceActivatorConsumerOptions.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IAmConsumerOptions>();

            _ = this.TestCommandProcessor.InboxConfiguration.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<InboxConfiguration>();

            _ = this.TestCommandProcessor.SqliteInbox.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<ICoreSqliteInbox>();
        }

        [Fact]
        public void NetworkPingCommand_CommandDispatchService_IsRunning()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void NetworkPingCommand_CommandDispatchService_Dispatcher()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.Dispatcher.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IDispatcher>();
        }

        [Fact]
        public async Task NetworkPingCommand_PublicServerIPv4AddressAsync()
        {
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

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

            if (CoreAppConstants.IsRunningOnGitHub)
            {
                this.TestOutputHelper.WriteLine("GitHub runner doesn't support ICMP.");
                return;
            }

            IPAddress publicServerIPAddress = CoreIPAddressExtensions.GetRandomPublicServerAddress();
            var pingCommand = new CorePingCommand(publicServerIPAddress);

            await this.TestCommandProcessor!.SendAsync(pingCommand);
            await this.ValidatePingCommandResultsAsync(pingCommand, publicServerIPAddress, null);
        }

        [Fact]
        public async Task NetworkPingCommand_PublicServerIPv6AddressAsync()
        {
            if (this.TestNetworkingSystem.PreferredLocalNetworkAddress!.PreferredNetworkAddressInfo.IPAddress.AddressFamily != AddressFamily.InterNetworkV6)
            {
                this.TestOutputHelper.WriteLine("IPv6 is not supported on this device.");
                return;
            }

            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

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

            IPAddress publicServerIPAddress = CoreIPAddressExtensions.GetRandomPublicServerAddress(AddressFamily.InterNetworkV6);

            var pingCommand = new CorePingCommand(publicServerIPAddress);

            await this.TestCommandProcessor!.SendAsync(pingCommand);
            await this.ValidatePingCommandResultsAsync(pingCommand, publicServerIPAddress, null);
        }

        [Fact]
        public async Task NetworkPingCommand_PublicServerHostAsync()
        {
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

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

            if (CoreAppConstants.IsRunningOnGitHub)
            {
                this.TestOutputHelper.WriteLine("GitHub runner doesn't support ICMP.");
                return;
            }

            string publicServerHostName = CoreIPAddressExtensions.StringGooglePublicDnsServer;
            var pingCommand = new CorePingCommand(publicServerHostName);

            await this.TestCommandProcessor!.SendAsync(pingCommand);
            await this.ValidatePingCommandResultsAsync(pingCommand, null, CoreIPAddressExtensions.StringGooglePublicDnsServer);
        }

        private async Task ValidatePingCommandResultsAsync(CorePingCommand pingCommand, IPAddress? ipAddress, string? hostNameOrIPAddress)
        {
            _ = pingCommand.Should().NotBeNull();
            _ = pingCommand.MessageBody.Should().NotBeNull().And.Subject.Should().BeAssignableTo<CorePingCommandBody>();
            _ = pingCommand.MessageBody!.PingResult.Should().NotBeNull().And.Subject.Should().BeAssignableTo<CorePingResult>();

            _ = pingCommand.MessageBody.PingResult!.Status.Should().Be(IPStatus.Success);
            _ = pingCommand.MessageBody.PingResult!.RoundtripTime.Should().BeGreaterThan(0);

            if (ipAddress is not null)
            {
                _ = pingCommand.MessageBody.PingResult!.Address.Should().Be(ipAddress);
            }

            if (!string.IsNullOrEmpty(hostNameOrIPAddress))
            {
                if (IPAddress.TryParse(hostNameOrIPAddress, out ipAddress))
                {
                    _ = pingCommand.MessageBody.PingResult!.Address.Should().Be(ipAddress);
                }
                else
                {
                    IPHostEntry? dnsEntries = await this.TestNetworkingSystem.GetDnsHostEntryAsync(hostNameOrIPAddress);
                    _ = dnsEntries.Should().NotBeNull();
                    _ = dnsEntries.AddressList.Contains(pingCommand.MessageBody.PingResult!.Address).Should().BeTrue();
                }
            }

            _ = pingCommand.MessageBody.NetworkAddressEntityID.Should().NotBeEmpty();

            CoreNetworkAddressEntity? networkAddressEntity = await this.TestQueryProcessor.ExecuteAsync(new CoreEntityByIDQuery<CoreNetworkAddressEntity>(pingCommand.MessageBody.NetworkAddressEntityID.Value, CoreEntityConstants.DefaultSnapshotID));
            networkAddressEntity.Should().NotBeNull().And.Subject.Should().BeAssignableTo<CoreNetworkAddressEntity>();

            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider);
            string json = JsonSerializer.Serialize(networkAddressEntity, typeof(CoreNetworkAddressEntity), options);

            string title = $"Ping Results for {(string.IsNullOrEmpty(hostNameOrIPAddress) ? ipAddress : hostNameOrIPAddress)}";

            this.TestOutputHelper.WriteLine($"{title.CenterTitle()}\n{json}");
            _ = networkAddressEntity.Should().NotBeNull();

            CoreNetworkAddressEntity? lookupNetworkAddressEntity = await this.TestQueryProcessor.ExecuteAsync(new CoreEntityByIPAddressQuery<CoreNetworkAddressEntity>(networkAddressEntity.NetworkAddress, CoreEntityConstants.DefaultSnapshotID));
            lookupNetworkAddressEntity.Should().BeEquivalentTo(networkAddressEntity);
        }
    }
}
