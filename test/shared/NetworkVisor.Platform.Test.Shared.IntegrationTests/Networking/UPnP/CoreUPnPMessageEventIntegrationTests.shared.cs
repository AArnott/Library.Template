// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreUPnPMessageEventIntegrationTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Base;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Database;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Entities.Hosts;
using NetworkVisor.Core.Entities.Networks.Addresses;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Messaging.Inbox;
using NetworkVisor.Core.Messaging.Queries.Entities;
using NetworkVisor.Core.Messaging.Queries.Entities.Base;
using NetworkVisor.Core.Messaging.Queries.Entities.NetworkAddress;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Ping;
using NetworkVisor.Core.Networking.Services.Discovery.Commands;
using NetworkVisor.Core.Networking.Services.Ping.Commands;
using NetworkVisor.Core.Networking.Services.UPnP.Events;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base;
using Paramore.Brighter;
using Xunit;
using IDispatcher = Paramore.Brighter.ServiceActivator.IDispatcher;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.UPnP
{
    /// <summary>
    /// Class CoreUPnPMessageEventIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreUPnPMessageEventIntegrationTests))]

    public class CoreUPnPMessageEventIntegrationTests : CoreCommandTestCaseBase
    {
        private const string TestUPnPMessageDevice = @"NOTIFY * HTTP/1.1
HOST: 239.255.255.250:1900
CACHE-CONTROL: max-age=8
LOCATION: http://10.1.10.154:50664/rootDesc.xml
SERVER: Netgear_Switch Debian/buster/sid UPnP/1.1 GC108/2.2.2
NT: upnp:rootdevice
USN: uuid:fb8e67ba-7dba-11e7-be55-8c3bad6fc17c::upnp:rootdevice
NTS: ssdp:alive
OPT: ""http://schemas.upnp.org/upnp/1/0/""; ns=01
01-NLS: 1740946921
BOOTID.UPNP.ORG: 1740946921
CONFIGID.UPNP.ORG: 1337
";

        private const string TestUPnPMessageSearch = @"M-SEARCH * HTTP/1.1
HOST: 239.255.255.250:1900
MAN: ""ssdp:discover""
MX: 1
ST: urn:schemas-upnp-org:device:ZonePlayer:1
USER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)
";

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreUPnPMessageEventIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreUPnPMessageEventIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this.EntityDatabase = this.TestCaseServiceProvider.GetRequiredService<ICoreEntityDatabase>();
        }

        protected ICoreEntityDatabase EntityDatabase { get; }

        [Fact]
        public void UPnPMessageCommandIntegration_CommandDispatchService_Ctor()
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
        public void UPnPMessageCommandIntegration_CommandDispatchService_IsRunning()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void UPnPMessageCommandIntegration_CommandDispatchService_Dispatcher()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.Dispatcher.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IDispatcher>();
        }

        [Fact]
        public async Task UPnPMessageCommandIntegration_Search_PublishAsync()
        {
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            CoreUPnPMessageEvent messageEvent = this.CreateUPnPMessageEvent(TestUPnPMessageSearch);

            await this.TestCommandProcessor!.PublishAsync(messageEvent);
        }

        private CoreUPnPMessageEvent CreateUPnPMessageEvent(string? messageBody = null, CoreIPEndPoint? remoteEndPoint = null)
        {
            return new CoreUPnPMessageEvent
            {
                MessageBody = messageBody ?? TestUPnPMessageDevice,
                RemoteEndPoint = remoteEndPoint ?? new CoreIPEndPoint(this.TestNetworkingSystem.PreferredLocalNetworkAddress?.IPAddress ?? IPAddress.Any, 1900),
            };
        }
    }
}
