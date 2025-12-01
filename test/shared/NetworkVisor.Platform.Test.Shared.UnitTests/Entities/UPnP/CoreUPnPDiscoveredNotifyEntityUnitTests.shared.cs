// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreUPnPDiscoveredNotifyEntityUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.Entities.Devices.UPnP.Notify;
using NetworkVisor.Core.Entities.Devices.UPnP.Search;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Services.UPnP.Constants;
using NetworkVisor.Core.Networking.Services.UPnP.Message;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Networking.UPnP
{
    /// <summary>
    /// Class CoreUPnPDiscoveredSearchEntityUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreUPnPDiscoveredNotifyEntityUnitTests))]
    public class CoreUPnPDiscoveredNotifyEntityUnitTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreUPnPDiscoveredNotifyEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreUPnPDiscoveredNotifyEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData("NOTIFY * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nCACHE-CONTROL: max-age=1800\r\nLOCATION: http://10.1.10.160:39732/rootDesc.xml\r\nNT: urn:schemas-upnp-org:service:WANPPPConnection:1\r\nNTS: ssdp:alive\r\nSERVER: Debian/buster UPnP/1.1 GC108/2.2.2\r\nUSN: uuid:fb8e67ba-7dba-11e7-be55-2880886e862a::urn:schemas-upnp-org:service:WANPPPConnection:1\r\nBOOTID.UPNP.ORG: 1740692113\r\nCONFIGID.UPNP.ORG: 1337\r\n", CoreUPnPMessageType.NotifyAlive, true)]
        public void UPnPDiscoveredNotifyEntityUnit_ParseFromString_ValidNotifyRequests(string request, CoreUPnPMessageType messageType, bool isValid)
        {
            var discoveredNotifyEntity = CoreUPnPDiscoveredNotifyEntity.ParseNotifyFromString(request, null, this.TestCaseLogger);

            if (!isValid)
            {
                _ = discoveredNotifyEntity.Should().BeNull();
                return;
            }

            _ = discoveredNotifyEntity.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"{request}\n{this.UPnPDiscoveredNotifyEntityToJson(discoveredNotifyEntity)}");
            _ = discoveredNotifyEntity!.UPnPNotifyMessage.Should().NotBeNull();
            _ = discoveredNotifyEntity.UPnPMessage.Header.MessageType.Should().Be(messageType); // Fix: Use the 'messageType' parameter here
            _ = discoveredNotifyEntity.UPnPMessage.Host.Host.Should().Be(CoreUPnPConstants.DefaultHostProperty);
            _ = discoveredNotifyEntity.UPnPMessage.Host.Address.Should().Be(CoreIPAddressExtensions.UPnPMulticastIPAddress);
            _ = discoveredNotifyEntity.UPnPMessage.Host.Port.Should().Be(CoreUPnPConstants.UPnPListenerPort);
        }

        [Theory]
        [InlineData("NOTIFY * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nCACHE-CONTROL: max-age=1800\r\nLOCATION: http://10.1.10.160:39732/rootDesc.xml\r\nNT: urn:schemas-upnp-org:service:WANPPPConnection:1\r\nNTS: ssdp:alive\r\nSERVER: Debian/buster UPnP/1.1 GC108/2.2.2\r\nUSN: uuid:fb8e67ba-7dba-11e7-be55-2880886e862a::urn:schemas-upnp-org:service:WANPPPConnection:1\r\nBOOTID.UPNP.ORG: 1740692113\r\nCONFIGID.UPNP.ORG: 1337\r\n", CoreUPnPMessageType.NotifyAlive, true)]
        public void UPnPDiscoveredNotifyEntityUnit_ParseFromString_InvalidNotifyRequests(string request, CoreUPnPMessageType messageType, bool isValid)
        {
            var discoveredNotifyEntity = CoreUPnPDiscoveredNotifyEntity.ParseNotifyFromString(request, null, this.TestCaseLogger);

            if (!isValid)
            {
                _ = discoveredNotifyEntity.Should().BeNull();
                return;
            }

            _ = discoveredNotifyEntity.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"{request}\n{this.UPnPDiscoveredNotifyEntityToJson(discoveredNotifyEntity)}");
            _ = discoveredNotifyEntity!.UPnPNotifyMessage.Should().NotBeNull();
            _ = discoveredNotifyEntity.UPnPMessage.Header.MessageType.Should().Be(messageType);
        }

        private string UPnPDiscoveredSearchEntityToJson(CoreUPnPDiscoveredSearchEntity? searchEntity)
        {
            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted);
            return JsonSerializer.Serialize(searchEntity, typeof(CoreUPnPDiscoveredSearchEntity), options);
        }

        private string UPnPDiscoveredNotifyEntityToJson(CoreUPnPDiscoveredNotifyEntity? notifyEntity)
        {
            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted);
            return JsonSerializer.Serialize(notifyEntity, typeof(CoreUPnPDiscoveredNotifyEntity), options);
        }
    }
}
