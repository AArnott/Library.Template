// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// ***********************************************************************
// <copyright file="CoreUPnPMessageParserUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// ***********************************************************************
// <summary>
// </summary>

using System.Net;
using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Entities.Devices.UPnP;
using NetworkVisor.Core.Entities.Devices.UPnP.Base;
using NetworkVisor.Core.Entities.Devices.UPnP.Device;
using NetworkVisor.Core.Entities.Devices.UPnP.Notify;
using NetworkVisor.Core.Entities.Devices.UPnP.Search;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Services.UPnP.Constants;
using NetworkVisor.Core.Networking.Services.UPnP.Message;
using NetworkVisor.Core.Networking.Services.UPnP.Message.Base;
using NetworkVisor.Core.Networking.Services.UPnP.Message.Search;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Messaging.Shared.UnitTests.Entities.Devices
{
    /// <summary>
    /// Class CoreUPnPMessageParserUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreUPnPMessageParserUnitTests))]
    public class CoreUPnPMessageParserUnitTests : CoreEntityTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreUPnPMessageParserUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">
        /// The test class fixture that provides shared context and dependencies for the test class.
        /// </param>
        public CoreUPnPMessageParserUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Theory]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-upnp-org:service:ContentDirectory:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:microsoft-com:service:X_MS_MediaReceiverRegistrar:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-upnp-org:device:MediaServer:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: upnp:rootdevice\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-upnp-org:device:ZonePlayer:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: uuid:12345678-1234-1234-1234-123456789abc\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-sonos-com:device:ZonePlayer:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        public void UPnPMessageParserUnit_ParseUPnPEntityBase_Notify(string content)
        {
            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted);

            var upnpEntityBase = CoreUPnPEntityBase.ParseUPnPEntityFromString(content, null, this.TestCaseLogger);
            upnpEntityBase.Should().NotBeNull("The parsed UPnP device response should not be null.");
            upnpEntityBase.EntityID.Should().NotBeEmpty();

            string json = JsonSerializer.Serialize(upnpEntityBase, typeof(CoreUPnPEntityBase), options);

            this.TestOutputHelper.WriteLine($"{upnpEntityBase.UPnPMessage.MessageTarget.Target.CenterTitle()}\n{content}\n{json}\n");
        }

        [Theory]
        [InlineData("urn:schemas-upnp-org:device:MediaServer:1", CoreUPnPMessageTargetType.UPnPDevice)]
        [InlineData("urn:schemas-upnp-org:service:ContentDirectory:1", CoreUPnPMessageTargetType.UPnPService)]
        [InlineData("urn:schemas-sonos-com:device:ZonePlayer:1", CoreUPnPMessageTargetType.VendorDevice)]
        [InlineData("urn:microsoft-com:service:X_MS_MediaReceiverRegistrar:1", CoreUPnPMessageTargetType.VendorService)]
        [InlineData("uuid:12345678-1234-1234-1234-123456789abc", CoreUPnPMessageTargetType.DiscoverUuid)]
        [InlineData(CoreUPnPConstants.SearchUPnPMessageDiscoverAll, CoreUPnPMessageTargetType.DiscoverAllDevices)]
        [InlineData(CoreUPnPConstants.SearchUPnPMessageDiscoverRootDevice, CoreUPnPMessageTargetType.DiscoverRootDevices)]
        [InlineData("urn:schemas-upnp-org:device:ZonePlayer", CoreUPnPMessageTargetType.Unknown)]
        [InlineData("urn:schemas-upnp-org:device:ZonePlayer:abc", CoreUPnPMessageTargetType.Unknown)]
        [InlineData("invalid:format:string", CoreUPnPMessageTargetType.Unknown)]
        public void UPnPMessageParserUnit_Parse_StField_ReturnsExpectedFormat(string input, CoreUPnPMessageTargetType expectedFormat)
        {
            CoreUPnPMessageTarget? result = CoreUPnPMessageTarget.ParseMessageTargetFromString(input, CoreUPnPMessageTargetSource.Unknown, this.TestCaseLogger);

            if (expectedFormat != CoreUPnPMessageTargetType.Unknown)
            {
                result.Should().NotBeNull("The parsed UPnP search target should not be null.");

                JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted);
                string json = JsonSerializer.Serialize(result, typeof(CoreUPnPMessageTarget), options);

                this.TestOutputHelper.WriteLine($"{input.CenterTitle()}\n{json}");

                result.TargetType.Should().Be(expectedFormat);
            }
            else
            {
                result.Should().BeNull("The parsed UPnP search target should be null for unknown formats.");
            }
        }

        [Theory]
        [InlineData("urn:schemas-upnp-org:device:ZonePlayer:1", true, "schemas-upnp-org", CoreUPnPMessageTargetType.UPnPDevice, "ZonePlayer", 1)]
        [InlineData("urn:schemas-upnp-org:service:ContentDirectory:2", true, "schemas-upnp-org", CoreUPnPMessageTargetType.UPnPService, "ContentDirectory", 2)]
        [InlineData("urn:schemas-upnp-org:device:MediaRenderer:3", true, "schemas-upnp-org", CoreUPnPMessageTargetType.UPnPDevice, "MediaRenderer", 3)]
        [InlineData("urn:schemas-upnp-org:service:RenderingControl:1", true, "schemas-upnp-org", CoreUPnPMessageTargetType.UPnPService, "RenderingControl", 1)]
        [InlineData("urn:custom-schema:device:SmartLight:5", true, "custom-schema", CoreUPnPMessageTargetType.VendorDevice, "SmartLight", 5)]

        [InlineData("urn:schemas-upnp-org:device:ZonePlayer", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        [InlineData("urn:schemas-upnp-org:device::1", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        [InlineData("urn:schemas-upnp-org:device:ZonePlayer:abc", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        [InlineData("schemas-upnp-org:device:ZonePlayer:1", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        [InlineData("urn:schemas-upnp-org:device:ZonePlayer:1:extra", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        [InlineData("urn::device:ZonePlayer:1", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        [InlineData("urn:schemas-upnp-org:device", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        [InlineData("urn:schemas-upnp-org:device:ZonePlayer:", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        [InlineData("urn:schemas-upnp-org:device:ZonePlayer:1.0", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        [InlineData("urn:schemas-upnp-org:device:ZonePlayer:-1", false, null, CoreUPnPMessageTargetType.Unknown, null, 0)]
        public void UPnPMessageParserUnit_Parse_StField_ValidatesCorrectly(string input, bool expectedValid, string? expectedSchema, CoreUPnPMessageTargetType searchTargetType, string? expectedName, int expectedVersion)
        {
            CoreUPnPMessageTarget? result = CoreUPnPMessageTarget.ParseMessageTargetFromString(input, CoreUPnPMessageTargetSource.Unknown, this.TestCaseLogger);

            if (expectedValid)
            {
                result.Should().NotBeNull("The parsed UPnP search target should not be null.");
                Assert.Equal(expectedSchema, result.Scheme);
                Assert.Equal(searchTargetType, result.TargetType);
                Assert.Equal(expectedName, result.Name);
                Assert.Equal(expectedVersion, result.Version);

                JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted);
                string json = JsonSerializer.Serialize(result, typeof(CoreUPnPMessageTarget), options);

                this.TestOutputHelper.WriteLine($"{input.CenterTitle()}\n{json}");
            }
            else
            {
                result.Should().BeNull("The parsed UPnP search target should be null.");
            }
        }

        [Theory]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-upnp-org:service:ContentDirectory:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:microsoft-com:service:X_MS_MediaReceiverRegistrar:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-upnp-org:device:MediaServer:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: upnp:rootdevice\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-upnp-org:device:ZonePlayer:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: uuid:12345678-1234-1234-1234-123456789abc\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        [InlineData("M-SEARCH * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nMAN: \"ssdp:discover\"\r\nMX: 1\r\nST: urn:schemas-sonos-com:device:ZonePlayer:1\r\nUSER-AGENT: Linux UPnP/1.0 Sonos/85.0-64200 (WDCR:Microsoft Windows NT 10.0.26100 64-bit)\r\n")]
        public void UPnPMessageParserUnit_ParseUPnPDiscoveredSearchEntity(string content)
        {
            CoreUPnPDiscoveredSearchEntity? uPnPSearchResponseEntity = CoreUPnPDiscoveredSearchEntity.ParseSearchFromString(content, null, this.TestCaseLogger);
            _ = uPnPSearchResponseEntity.Should().NotBeNull("The parsed UPnP search response should not be null.");

            this.TestOutputHelper.WriteLine($"{uPnPSearchResponseEntity.UPnPMessage.MessageTarget.Target.CenterTitle()}\n{content}\n{this.UPnPDiscoveredSearchEntityToJson(uPnPSearchResponseEntity)}\n");
        }

        [Theory]
        [InlineData("NOTIFY * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nCACHE-CONTROL: max-age=180\r\nLOCATION: http://10.1.10.200:60006/upnp/desc/aios_device/aios_device.xml\r\nVERSIONS.UPNP.HEOS.COM: 11,-323646733,-170053632,363364703,1598223468,363364703,1840750642,-824042063,105553199,-316033077,1711326982,-1264877755\r\nNETWORKID.UPNP.HEOS.COM: 7c5a1cab6680\r\nBOOTID.UPNP.ORG: 1652543702\r\nIPCACHE.URL.UPNP.HEOS.COM: /ajax/upnp/get_device_info\r\nNT: upnp:rootdevice\r\nNTS: ssdp:alive\r\nSERVER: LINUX UPnP/1.0 Denon-Heos/741831966851804794c04638e30087210440342a\r\nUSN: uuid:28d44699-04e5-155a-0080-0005cdc4c89e::upnp:rootdevice\r\n")]
        [InlineData("NOTIFY * HTTP/1.1\r\nLOCATION: http://10.1.10.32:49789/\r\nHOST: 239.255.255.250:1900\r\nSERVER: WINDOWS, UPnP/1.0, MicroStack/1.0.4931\r\nNTS: ssdp:alive\r\nUSN: uuid:3014c9c6-58a6-4e7f-afca-3a8b5e290351b8::upnp:rootdevice\r\nCACHE-CONTROL: max-age=60\r\nNT: upnp:rootdevice\r\n")]
        [InlineData("NOTIFY * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nCACHE-CONTROL: max-age=8\r\nLOCATION: http://10.1.10.154:50664/rootDesc.xml\r\nSERVER: Netgear_Switch Debian/buster/sid UPnP/1.1 GC108/2.2.2\r\nNT: upnp:rootdevice\r\nUSN: uuid:fb8e67ba-7dba-11e7-be55-8c3bad6fc17c::upnp:rootdevice\r\nNTS: ssdp:alive\r\nOPT: \"http://schemas.upnp.org/upnp/1/0/\"; ns=01\r\n01-NLS: 1740946921\r\nBOOTID.UPNP.ORG: 1740946921\r\nCONFIGID.UPNP.ORG: 1337\r\n")]
        [InlineData("NOTIFY * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nCACHE-CONTROL: max-age=8\r\nLOCATION: http://10.1.10.158:53483/rootDesc.xml\r\nSERVER: Netgear_Switch Debian/buster/sid UPnP/1.1 GC108/2.2.2\r\nNT: upnp:rootdevice\r\nUSN: uuid:fb8e67ba-7dba-11e7-be55-2880886e8634::upnp:rootdevice\r\nNTS: ssdp:alive\r\nOPT: \"http://schemas.upnp.org/upnp/1/0/\"; ns=01\r\n01-NLS: 1750134294\r\nBOOTID.UPNP.ORG: 1750134294\r\nCONFIGID.UPNP.ORG: 1337\r\n")]
        [InlineData("NOTIFY * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nCACHE-CONTROL: max-age=8\r\nLOCATION: http://10.1.10.159:44829/rootDesc.xml\r\nSERVER: Netgear_Switch Debian/buster/sid UPnP/1.1 GC108/2.2.2\r\nNT: upnp:rootdevice\r\nUSN: uuid:fb8e67ba-7dba-11e7-be55-2880886e85ac::upnp:rootdevice\r\nNTS: ssdp:alive\r\nOPT: \"http://schemas.upnp.org/upnp/1/0/\"; ns=01\r\n01-NLS: 1741895932\r\nBOOTID.UPNP.ORG: 1741895932\r\nCONFIGID.UPNP.ORG: 1337\r\n")]
        [InlineData("NOTIFY * HTTP/1.1\r\nHOST: 239.255.255.250:1900\r\nCACHE-CONTROL: max-age=8\r\nLOCATION: http://10.1.10.160:39732/rootDesc.xml\r\nSERVER: Netgear_Switch Debian/buster/sid UPnP/1.1 GC108/2.2.2\r\nNT: upnp:rootdevice\r\nUSN: uuid:fb8e67ba-7dba-11e7-be55-2880886e8628::upnp:rootdevice\r\nNTS: ssdp:alive\r\nOPT: \"http://schemas.upnp.org/upnp/1/0/\"; ns=01\r\n01-NLS: 1740692113\r\nBOOTID.UPNP.ORG: 1740692113\r\nCONFIGID.UPNP.ORG: 1337\r\n")]
        public void UPnPMessageParserUnit_ParseUPnPDiscoveredNotifyEntity(string content)
        {
            CoreUPnPDiscoveredNotifyEntity? notifyEntity = CoreUPnPDiscoveredNotifyEntity.ParseNotifyFromString(content, null, this.TestCaseLogger);
            _ = notifyEntity.Should().NotBeNull("The parsed UPnP notify response should not be null.");

            this.TestOutputHelper.WriteLine($"{notifyEntity.UPnPNotifyMessage.UniqueServiceName?.USN.CenterTitle()}\n{content}\n{this.UPnPDiscoveredNotifyEntityToJson(notifyEntity)}\n");
        }

        [Theory]
        [InlineData(@"M-SEARCH * HTTP/1.1
HOST: 239.255.255.250:1900
MAN: ""ssdp:discover""
MX: 2
ST: ssdp:all
USER-AGENT: Windows/10.0 UPnP/1.1 MyApp/2.0")]

        [InlineData(@"M-SEARCH * HTTP/1.1
HOST: 192.168.1.100:1900
MAN: ""ssdp:discover""
MX: 1
ST: urn:schemas-upnp-org:device:MediaRenderer:1
USER-AGENT: macOS/13.4 UPnP/1.1 AVClient/3.2")]

        [InlineData(@"M-SEARCH * HTTP/1.1
HOST: 10.0.0.5:1900
MAN: ""ssdp:discover""
MX: 0
ST: urn:schemas-upnp-org:device:InternetGatewayDevice:1
USER-AGENT: FreeBSD/13.2 UPnP/1.1 GatewayScanner/1.1")]
        public void UPnPMessageParserUnit_ParseSearchRequest_ValidInputs_ShouldExtractFields(string rawRequest)
        {
            CoreUPnPDiscoveredSearchEntity? result = CoreUPnPDiscoveredSearchEntity.ParseSearchFromString(rawRequest, null, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine($"Request:\n{rawRequest}");

            result.Should().NotBeNull("The parsed UPnP search response entity should not be null.");
            this.TestOutputHelper.WriteLine($"{result.UPnPMessage.MessageTarget?.Target.CenterTitle()}\n{this.UPnPDiscoveredSearchEntityToJson(result)}\n");

            result.UPnPMessage.Header.Should().NotBeNull("The message header should not be null.");
            result.UPnPMessage.Header?.HttpMethod.Should().Be(CoreUPnPConstants.SearchUPnPMessagePrefix, "The HTTP method should match the expected search prefix.");
            result.UPnPMessage.Host?.Address.Should().NotBeNull("The host address should not be null.");
            result.UPnPMessage.Host?.Port.Should().BeGreaterThan(0, "The host port should be greater than 0.");
            result.UPnPMessage.MessageTarget?.Target.Should().NotBeNullOrEmpty("The target should not be null or empty.");
            result.UPnPMessage.RemoteEndPoint.Should().BeNull("The remote endpoint should be null.");
            result.UPnPSearchMessage.MX.Should().NotBeNull("The MX value should not be null.");
        }

        [Theory]
        [InlineData(@"M-SEARCH * HTTP/1.1
MAN: ""ssdp:discover""
MX: 2
ST: ssdp:all")] // Missing HOST

        [InlineData(@"M-SEARCH * HTTP/1.1
HOST: 239.255.255.250
MAN: ""ssdp:discover""
MX: 2
ST: ssdp:all")] // Invalid HOST (missing port)

        [InlineData(@"M-SEARCH * HTTP/1.1
HOST: 239.255.255.250:1900
MX: 2
ST: ssdp:all")] // Missing MAN

        [InlineData(@"M-SEARCH * HTTP/1.1
HOST: 239.255.255.250:1900
MAN: ""ssdp:discover""
MX: notanumber
ST: ssdp:all")] // Invalid MX

        [InlineData(@"NOTIFY * HTTP/1.1
HOST: 239.255.255.250:1900
NTS: ssdp:alive
NT: upnp:rootdevice")] // Wrong method

        public void UPnPMessageParserUnit_ParseSearchRequest_MalformedInputs_ShouldHandleGracefully(string rawRequest)
        {
            CoreUPnPDiscoveredSearchEntity? result = CoreUPnPDiscoveredSearchEntity.ParseSearchFromString(rawRequest, null, this.TestCaseLogger);

            this.TestOutputHelper.WriteLine($"Request:\n{rawRequest}");

            if (rawRequest.StartsWith("NOTIFY"))
            {
                result.Should().BeNull("The parsed UPnP search response entity should be null for malformed NOTIFY requests.");
                return;
            }

            Assert.NotNull(result);
            this.TestOutputHelper.WriteLine($"{result.UPnPMessage.MessageTarget?.Target.CenterTitle()}\n{this.UPnPDiscoveredSearchEntityToJson(result)}\n");
            result.UPnPMessage.RemoteEndPoint.Should().BeNull("The remote endpoint should be null.");

            Assert.NotNull(result.UPnPMessage.Header?.HttpMethod);

            if (rawRequest.StartsWith("NOTIFY"))
            {
                Assert.Equal(CoreUPnPConstants.NotifyUPnPMessagePrefix, result.UPnPMessage.Header.HttpMethod); // Should still be M-SEARCH if present
            }
            else
            {
                Assert.Equal(CoreUPnPConstants.SearchUPnPMessagePrefix, result.UPnPMessage.Header.HttpMethod); // Should still be M-SEARCH if present
            }

            Assert.True(result.UPnPMessage.Host?.Address is null || result.UPnPMessage.Host.Port >= 0);
            Assert.True(result.UPnPSearchMessage.MX is null or >= 0);
        }

        [Theory]
        [InlineData("<root xmlns=\"urn:schemas-upnp-org:device-1-0\" xmlns:DMH=\"http://www.dmglobal.com\" xmlns:qq=\"http://www.tencent.com\" xmlns:avega_media_server=\"urn:schemas-avegasystems-com:media-server:metadata-1-0:DIDL-Lite\">\r\n  <specVersion>\r\n    <major>1</major>\r\n    <minor>0</minor>\r\n  </specVersion>\r\n  <device>\r\n    <deviceType>urn:schemas-denon-com:device:AiosDevice:1</deviceType>\r\n    <friendlyName>Great Room   </friendlyName>\r\n    <manufacturer>Denon</manufacturer>\r\n    <manufacturerURL>http://www.denon.com</manufacturerURL>\r\n    <modelName>Denon AVR-X6500H</modelName>\r\n    <modelNumber>Aios 4.025</modelNumber>\r\n    <serialNumber>BBY15181001682</serialNumber>\r\n    <UDN>uuid:28d44699-04e5-155a-0080-0005cdc4c89e</UDN>\r\n    <DMH:X_Audyssey>00000002</DMH:X_Audyssey>\r\n    <DMH:X_AudysseyPort>1256</DMH:X_AudysseyPort>\r\n    <DMH:X_WebAPIPort>8080</DMH:X_WebAPIPort>\r\n    <deviceList>\r\n      <device>\r\n        <deviceType>urn:schemas-upnp-org:device:MediaServer:1</deviceType>\r\n        <friendlyName>Great Room   </friendlyName>\r\n        <manufacturer>Denon</manufacturer>\r\n        <manufacturerURL>http://www.denon.com</manufacturerURL>\r\n        <modelDescription>Shares User defined folders and files to other Universal Plug and Play media devices.</modelDescription>\r\n        <modelName>Denon AVR-X6500H</modelName>\r\n        <modelNumber>Aios 4.025</modelNumber>\r\n        <serialNumber>BBY15181001682</serialNumber>\r\n        <UDN>uuid:d3da4c13-7712-0b48-ca35-2177520f1e31</UDN>\r\n        <avega_media_server:X_VirtualServersSupported>True</avega_media_server:X_VirtualServersSupported>\r\n        <serviceList>\r\n          <service>\r\n            <serviceType>urn:schemas-upnp-org:service:ContentDirectory:1</serviceType>\r\n            <serviceId>urn:upnp-org:serviceId:ContentDirectory</serviceId>\r\n            <SCPDURL>/upnp/scpd/ams_dvc/ContentDirectory.xml</SCPDURL>\r\n            <controlURL>/upnp/control/ams_dvc/ContentDirectory</controlURL>\r\n            <eventSubURL>/upnp/event/ams_dvc/ContentDirectory</eventSubURL>\r\n          </service>\r\n          <service>\r\n            <serviceType>urn:schemas-upnp-org:service:ConnectionManager:1</serviceType>\r\n            <serviceId>urn:upnp-org:serviceId:ConnectionManager</serviceId>\r\n            <SCPDURL>/upnp/scpd/ams_dvc/ConnectionManager.xml</SCPDURL>\r\n            <controlURL>/upnp/control/ams_dvc/ConnectionManager</controlURL>\r\n            <eventSubURL>/upnp/event/ams_dvc/ConnectionManager</eventSubURL>\r\n          </service>\r\n        </serviceList>\r\n      </device>\r\n      <device>\r\n        <deviceType>urn:schemas-upnp-org:device:MediaRenderer:1</deviceType>\r\n        <friendlyName>Great Room   </friendlyName>\r\n        <manufacturer>Denon</manufacturer>\r\n        <manufacturerURL>http://www.denon.com</manufacturerURL>\r\n        <modelName>Denon AVR-X6500H</modelName>\r\n        <modelNumber>Aios 4.025</modelNumber>\r\n        <UDN>uuid:ad785660-0eb2-1bd8-0080-0005cdc4c89e</UDN>\r\n        <X_Play_Queue_Capacity>1000</X_Play_Queue_Capacity>\r\n        <qq:X_QPlay_SoftwareCapability>QPlay:2</qq:X_QPlay_SoftwareCapability>\r\n        <serviceList>\r\n          <service>\r\n            <serviceType>urn:schemas-upnp-org:service:AVTransport:1</serviceType>\r\n            <serviceId>urn:upnp-org:serviceId:AVTransport</serviceId>\r\n            <SCPDURL>/upnp/scpd/renderer_dvc/AVTransport.xml</SCPDURL>\r\n            <controlURL>/upnp/control/renderer_dvc/AVTransport</controlURL>\r\n            <eventSubURL>/upnp/event/renderer_dvc/AVTransport</eventSubURL>\r\n          </service>\r\n          <service>\r\n            <serviceType>urn:schemas-upnp-org:service:ConnectionManager:1</serviceType>\r\n            <serviceId>urn:upnp-org:serviceId:ConnectionManager</serviceId>\r\n            <SCPDURL>/upnp/scpd/renderer_dvc/ConnectionManager.xml</SCPDURL>\r\n            <controlURL>/upnp/control/renderer_dvc/ConnectionManager</controlURL>\r\n            <eventSubURL>/upnp/event/renderer_dvc/ConnectionManager</eventSubURL>\r\n          </service>\r\n          <service>\r\n            <serviceType>urn:schemas-upnp-org:service:RenderingControl:1</serviceType>\r\n            <serviceId>urn:upnp-org:serviceId:RenderingControl</serviceId>\r\n            <SCPDURL>/upnp/scpd/renderer_dvc/RenderingControl.xml</SCPDURL>\r\n            <controlURL>/upnp/control/renderer_dvc/RenderingControl</controlURL>\r\n            <eventSubURL>/upnp/event/renderer_dvc/RenderingControl</eventSubURL>\r\n          </service>\r\n          <service>\r\n            <serviceType>urn:schemas-tencent-com:service:QPlay:1</serviceType>\r\n            <serviceId>urn:tencent-com:serviceId:QPlay</serviceId>\r\n            <SCPDURL>/upnp/scpd/renderer_dvc/QPlay.xml</SCPDURL>\r\n            <controlURL>/upnp/control/renderer_dvc/QPlay</controlURL>\r\n            <eventSubURL>/upnp/event/renderer_dvc/QPlay</eventSubURL>\r\n          </service>\r\n        </serviceList>\r\n      </device>\r\n      <device>\r\n        <deviceType>urn:schemas-denon-com:device:AiosServices:1</deviceType>\r\n        <friendlyName>AiosServices</friendlyName>\r\n        <manufacturer>Denon</manufacturer>\r\n        <manufacturerURL>http://www.denon.com</manufacturerURL>\r\n        <modelName>Denon AVR-X6500H</modelName>\r\n        <modelNumber>Aios 4.025</modelNumber>\r\n        <UDN>uuid:e6f24248-2c16-1baf-0080-0005cdc4c89e</UDN>\r\n        <serviceList>\r\n          <service>\r\n            <serviceType>urn:schemas-denon-com:service:ErrorHandler:1</serviceType>\r\n            <serviceId>urn:denon-com:serviceId:ErrorHandler</serviceId>\r\n            <SCPDURL>/upnp/scpd/AiosServicesDvc/ErrorHandler.xml</SCPDURL>\r\n            <controlURL>/upnp/control/AiosServicesDvc/ErrorHandler</controlURL>\r\n            <eventSubURL>/upnp/event/AiosServicesDvc/ErrorHandler</eventSubURL>\r\n          </service>\r\n          <service>\r\n            <serviceType>urn:schemas-denon-com:service:ZoneControl:2</serviceType>\r\n            <serviceId>urn:denon-com:serviceId:ZoneControl</serviceId>\r\n            <SCPDURL>/upnp/scpd/AiosServicesDvc/ZoneControl.xml</SCPDURL>\r\n            <controlURL>/upnp/control/AiosServicesDvc/ZoneControl</controlURL>\r\n            <eventSubURL>/upnp/event/AiosServicesDvc/ZoneControl</eventSubURL>\r\n          </service>\r\n          <service>\r\n            <serviceType>urn:schemas-denon-com:service:GroupControl:1</serviceType>\r\n            <serviceId>urn:denon-com:serviceId:GroupControl</serviceId>\r\n            <SCPDURL>/upnp/scpd/AiosServicesDvc/GroupControl.xml</SCPDURL>\r\n            <controlURL>/upnp/control/AiosServicesDvc/GroupControl</controlURL>\r\n            <eventSubURL>/upnp/event/AiosServicesDvc/GroupControl</eventSubURL>\r\n          </service>\r\n        </serviceList>\r\n      </device>\r\n      <device>\r\n        <deviceType>urn:schemas-denon-com:device:ACT-Denon:1</deviceType>\r\n        <friendlyName>Great Room   </friendlyName>\r\n        <manufacturer>Denon</manufacturer>\r\n        <manufacturerURL>http://www.denon.com</manufacturerURL>\r\n        <modelName>Denon AVR-X6500H</modelName>\r\n        <modelNumber>Aios 4.025</modelNumber>\r\n        <serialNumber>BBY15181001682</serialNumber>\r\n        <UDN>uuid:85104a30-802c-898d-94b9-94cfea8d2ad4</UDN>\r\n        <DeviceID>AIOS:0001</DeviceID>\r\n        <capability_version>4</capability_version>\r\n        <firmwareRevision>1747996302</firmwareRevision>\r\n        <firmware_date>Sun 2025-05-25 03:16:50</firmware_date>\r\n        <firmware_version>3.67.460</firmware_version>\r\n        <lanMac>00:05:CD:C4:C8:9C</lanMac>\r\n        <locale>en_NA</locale>\r\n        <moduleRevision>4</moduleRevision>\r\n        <moduleType>Aios 4.025</moduleType>\r\n        <productRevision>1</productRevision>\r\n        <releaseType>Production</releaseType>\r\n        <wlanMac>00:05:CD:C4:C8:9E</wlanMac>\r\n        <serviceList>\r\n          <service>\r\n            <serviceType>urn:schemas-denon-com:service:ACT:1</serviceType>\r\n            <serviceId>urn:denon-com:serviceId:ACT</serviceId>\r\n            <SCPDURL>/ACT/SCPD.xml</SCPDURL>\r\n            <controlURL>/ACT/control</controlURL>\r\n            <eventSubURL>/ACT/event</eventSubURL>\r\n          </service>\r\n        </serviceList>\r\n      </device>\r\n    </deviceList>\r\n  </device>\r\n</root>")]
        [InlineData("<root xmlns=\"urn:schemas-upnp-org:device-1-0\">\r\n  <specVersion>\r\n    <major>1</major>\r\n    <minor>0</minor>\r\n  </specVersion>\r\n  <device>\r\n    <deviceType>urn:schemas-upnp-org:device:AirBorne:1</deviceType>\r\n    <friendlyName>SBDEV</friendlyName>\r\n    <manufacturer>Nero AG</manufacturer>\r\n    <manufacturerURL>http://www.nero.com</manufacturerURL>\r\n    <modelDescription>Nero AirBurn</modelDescription>\r\n    <modelName>AirBurn</modelName>\r\n    <modelNumber>1.0</modelNumber>\r\n    <modelURL>http://www.nero.com</modelURL>\r\n    <serialNumber>0000001</serialNumber>\r\n    <UDN>uuid:3014c9c6-58a6-4e7f-afca-3a8b5e290351b8</UDN>\r\n    <iconList>\r\n      <icon>\r\n        <mimetype>image/png</mimetype>\r\n        <width>48</width>\r\n        <height>48</height>\r\n        <depth>32</depth>\r\n        <url>/icon.png</url>\r\n      </icon>\r\n      <icon>\r\n        <mimetype>image/jpg</mimetype>\r\n        <width>48</width>\r\n        <height>48</height>\r\n        <depth>32</depth>\r\n        <url>/icon.jpg</url>\r\n      </icon>\r\n      <icon>\r\n        <mimetype>image/png</mimetype>\r\n        <width>48</width>\r\n        <height>48</height>\r\n        <depth>32</depth>\r\n        <url>/icon2.png</url>\r\n      </icon>\r\n      <icon>\r\n        <mimetype>image/jpg</mimetype>\r\n        <width>48</width>\r\n        <height>48</height>\r\n        <depth>32</depth>\r\n        <url>/icon2.jpg</url>\r\n      </icon>\r\n    </iconList>\r\n    <serviceList>\r\n      <service>\r\n        <serviceType>urn:schemas-upnp-org:service:ABControl:1</serviceType>\r\n        <serviceId>urn:upnp-org:serviceId:ABControl</serviceId>\r\n        <SCPDURL>ABControl/scpd.xml</SCPDURL>\r\n        <controlURL>ABControl/control</controlURL>\r\n        <eventSubURL />\r\n      </service>\r\n    </serviceList>\r\n  </device>\r\n</root>")]
        [InlineData("<root xmlns=\"urn:schemas-upnp-org:device-1-0\" configId=\"1337\">\r\n  <specVersion>\r\n    <major>1</major>\r\n    <minor>1</minor>\r\n  </specVersion>\r\n  <device>\r\n    <deviceType>urn:schemas-upnp-org:device:InternetGatewayDevice:1</deviceType>\r\n    <friendlyName>Cameras-GC108PP-6FC17C</friendlyName>\r\n    <manufacturer>NETGEAR</manufacturer>\r\n    <manufacturerURL>http://www.netgear.com/</manufacturerURL>\r\n    <modelDescription>NETGEAR Switch</modelDescription>\r\n    <modelName>GC108PP</modelName>\r\n    <modelNumber>GC108PP</modelNumber>\r\n    <modelURL>http://www.netgear.com/business/products/switches/</modelURL>\r\n    <serialNumber>5V88147S00003</serialNumber>\r\n    <UDN>uuid:fb8e67ba-7dba-11e7-be55-8c3bad6fc17c</UDN>\r\n    <presentationURL>http://10.1.10.154/</presentationURL>\r\n    <firmwareVersion>1.0.8.9</firmwareVersion>\r\n  </device>\r\n</root>")]
        [InlineData("<root xmlns=\"urn:schemas-upnp-org:device-1-0\" configId=\"1337\">\r\n  <specVersion>\r\n    <major>1</major>\r\n    <minor>1</minor>\r\n  </specVersion>\r\n  <device>\r\n    <deviceType>urn:schemas-upnp-org:device:InternetGatewayDevice:1</deviceType>\r\n    <friendlyName>Theater-GC108PP-6E8634</friendlyName>\r\n    <manufacturer>NETGEAR</manufacturer>\r\n    <manufacturerURL>http://www.netgear.com/</manufacturerURL>\r\n    <modelDescription>NETGEAR Switch</modelDescription>\r\n    <modelName>GC108PP</modelName>\r\n    <modelNumber>GC108PP</modelNumber>\r\n    <modelURL>http://www.netgear.com/business/products/switches/</modelURL>\r\n    <serialNumber>5V849C7LA00C0</serialNumber>\r\n    <UDN>uuid:fb8e67ba-7dba-11e7-be55-2880886e8634</UDN>\r\n    <presentationURL>http://10.1.10.158/</presentationURL>\r\n    <firmwareVersion>1.0.8.9</firmwareVersion>\r\n  </device>\r\n</root>")]
        [InlineData("<root xmlns=\"urn:schemas-upnp-org:device-1-0\" configId=\"1337\">\r\n  <specVersion>\r\n    <major>1</major>\r\n    <minor>1</minor>\r\n  </specVersion>\r\n  <device>\r\n    <deviceType>urn:schemas-upnp-org:device:InternetGatewayDevice:1</deviceType>\r\n    <friendlyName>Gate-GC108PP-6E85AC</friendlyName>\r\n    <manufacturer>NETGEAR</manufacturer>\r\n    <manufacturerURL>http://www.netgear.com/</manufacturerURL>\r\n    <modelDescription>NETGEAR Switch</modelDescription>\r\n    <modelName>GC108PP</modelName>\r\n    <modelNumber>GC108PP</modelNumber>\r\n    <modelURL>http://www.netgear.com/business/products/switches/</modelURL>\r\n    <serialNumber>5V849C7VA009E</serialNumber>\r\n    <UDN>uuid:fb8e67ba-7dba-11e7-be55-2880886e85ac</UDN>\r\n    <presentationURL>http://10.1.10.159/</presentationURL>\r\n    <firmwareVersion>1.0.8.9</firmwareVersion>\r\n  </device>\r\n</root>")]
        [InlineData("<root xmlns=\"urn:schemas-upnp-org:device-1-0\" configId=\"1337\">\r\n  <specVersion>\r\n    <major>1</major>\r\n    <minor>1</minor>\r\n  </specVersion>\r\n  <device>\r\n    <deviceType>urn:schemas-upnp-org:device:InternetGatewayDevice:1</deviceType>\r\n    <friendlyName>Utility-GC108PP-6E8628</friendlyName>\r\n    <manufacturer>NETGEAR</manufacturer>\r\n    <manufacturerURL>http://www.netgear.com/</manufacturerURL>\r\n    <modelDescription>NETGEAR Switch</modelDescription>\r\n    <modelName>GC108PP</modelName>\r\n    <modelNumber>GC108PP</modelNumber>\r\n    <modelURL>http://www.netgear.com/business/products/switches/</modelURL>\r\n    <serialNumber>5V849C7YA00BD</serialNumber>\r\n    <UDN>uuid:fb8e67ba-7dba-11e7-be55-2880886e8628</UDN>\r\n    <presentationURL>http://10.1.10.160/</presentationURL>\r\n    <firmwareVersion>1.0.8.9</firmwareVersion>\r\n  </device>\r\n</root>")]
        public void UPnPMessageParserUnit_ParseUPnPRootDevice(string content)
        {
            var uPnPNetworkDeviceEntity = CoreUPnPNetworkDeviceEntity.ParseUPnPNetworkDeviceFromString(content, this.TestCaseLogger);
            _ = uPnPNetworkDeviceEntity.Should().NotBeNull("The parsed UPnP root device entity should not be null.");
            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted);
            string json = JsonSerializer.Serialize(uPnPNetworkDeviceEntity, typeof(CoreUPnPNetworkDeviceEntity), options);

            this.TestOutputHelper.WriteLine($"{uPnPNetworkDeviceEntity.Device?.DeviceType.CenterTitle()}\n{content}\n{json}\n");
        }

        [Theory]
        [InlineData("Windows/10.0 UPnP/1.0 Product/1.0", "Windows/10.0", "1.0", "Product/1.0")]
        [InlineData("Linux/4.15.0-20-generic UPnP/1.1 Product/2.0", "Linux/4.15.0-20-generic", "1.1", "Product/2.0")]
        [InlineData("Debian/buster UPnP/1.1 GC108/2.2.2", "Debian/buster", "1.1", "GC108/2.2.2")]
        [InlineData("Netgear_Switch Debian/buster/sid UPnP/1.1", "Debian/buster/sid", "1.1", "Netgear_Switch")]
        [InlineData("LINUX UPnP/1.0 Denon-Heos/741831966851804794c04638e30087210440342a", "LINUX", "1.0", "Denon-Heos/741831966851804794c04638e30087210440342a")]
        [InlineData("Linux UPnP/1.0 Sonos/85.0-65270", "Linux", "1.0", "Sonos/85.0-65270")]
        [InlineData("Netgear_Switch Debian/buster/sid UPnP/1.1 GC108", "Debian/buster/sid", "1.1", "Netgear_Switch GC108")]
        [InlineData("WINDOWS, UPnP/1.0, MicroStack/1.0.4931", "WINDOWS", "1.0", "MicroStack/1.0.4931")]
        public void ParseUPnPHeaderServer_ShouldParseValidInputs(string input, string expectedOS, string expectedVersion, string expectedProduct)
        {
            // Act
            var result = CoreUPnPMessageServer.ParseUPnPHeaderServer(input, this.TestCaseLogger);

            // Assert
            result.Should().NotBeNull();
            result!.OS.Should().Be(expectedOS);
            result.UPnPVersion.Should().Be(Version.Parse(expectedVersion));
            result.Product.Should().Be(expectedProduct);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        [InlineData("UPnP/1.0")]
        [InlineData("JustOneField")]
        [InlineData("OSOnly ProductOnly")]
        public void ParseUPnPHeaderServer_ShouldReturnNullForInvalidInputs(string? input)
        {
            // Act
            var result = CoreUPnPMessageServer.ParseUPnPHeaderServer(input!, this.TestCaseLogger);

            // Assert
            result.Should().BeNull();
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
