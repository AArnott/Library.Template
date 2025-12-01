// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreUPnPDiscoveredSearchEntityUnitTests.shared.cs" company="Network Visor">
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
    [PlatformTrait(typeof(CoreUPnPDiscoveredSearchEntityUnitTests))]
    public class CoreUPnPDiscoveredSearchEntityUnitTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreUPnPDiscoveredSearchEntityUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreUPnPDiscoveredSearchEntityUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: ssdp:all\r\n\r\n", true)]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: upnp:rootdevice\r\n\r\n", true)]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 3\r\nST: uuid:device-uuid\r\n\r\n", true)]
        public void UPnPSearchMessageUnit_ParseFromString_ValidMSearchRequests(string request, bool isValid)
        {
            var discoveredSearchEntity = CoreUPnPDiscoveredSearchEntity.ParseSearchFromString(request, null, this.TestCaseLogger);

            if (!isValid)
            {
                _ = discoveredSearchEntity.Should().BeNull();
                return;
            }

            _ = discoveredSearchEntity.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"{request}\n{this.UPnPDiscoveredSearchEntityToJson(discoveredSearchEntity)}");
            _ = discoveredSearchEntity!.UPnPSearchMessage.Should().NotBeNull();
            _ = discoveredSearchEntity.UPnPMessage.Header.MessageType.Should().Be(CoreUPnPMessageType.Discover);
            _ = discoveredSearchEntity.UPnPMessage.Host.Host.Should().Be(CoreUPnPConstants.DefaultHostProperty);
            _ = discoveredSearchEntity.UPnPMessage.Host.Address.Should().Be(CoreIPAddressExtensions.UPnPMulticastIPAddress);
            _ = discoveredSearchEntity.UPnPMessage.Host.Port.Should().Be(CoreUPnPConstants.UPnPListenerPort);
        }

        [Theory]
        [InlineData("M-SEARCH * HTTP/1.1\r\nMAN: \"ssdp:discover\"\r\nMX: 2\r\nST: ssdp:all\r\n\r\n", true)] // Missing HOST
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMX: 2\r\nST: ssdp:all\r\n\r\n", true)] // Missing MAN
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: ssdp:discover\r\nMX: 2\r\nST: ssdp:all\r\n\r\n", true)] // MAN not quoted
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nST: ssdp:all\r\n\r\n", true)] // Missing MX
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: two\r\nST: ssdp:all\r\n\r\n", true)] // MX not numeric
        public void UPnPSearchMessageUnit_ParseFromString_InvalidMSearchRequests(string request, bool isValid)
        {
            var discoveredSearchEntity = CoreUPnPDiscoveredSearchEntity.ParseSearchFromString(request, null, this.TestCaseLogger);

            if (!isValid)
            {
                _ = discoveredSearchEntity.Should().BeNull();
                return;
            }

            _ = discoveredSearchEntity.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"{request}\n{this.UPnPDiscoveredSearchEntityToJson(discoveredSearchEntity)}");
            _ = discoveredSearchEntity!.UPnPSearchMessage.Should().NotBeNull();
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
