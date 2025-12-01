// ***********************************************************************
// Assembly         : NetworkVisor.Core.Test
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// ***********************************************************************
// <copyright file="CoreTestEntityConstants.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using System.Net.NetworkInformation;
using NetworkVisor.Core.Entities;
using NetworkVisor.Core.Entities.Constants;
using NetworkVisor.Core.Entities.Extensions;
using NetworkVisor.Core.Entities.Hosts;
using NetworkVisor.Core.Entities.Networks.Addresses;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.NetworkInterface;

namespace NetworkVisor.Platform.Test.TestEntities
{
    public static class CoreTestEntityConstants
    {
        public static (CoreHostEntityType, string, Guid, CoreHostEntityScore)[] TestHostEntities =
        [
            (CoreHostEntityType.GenericHost, string.Empty, CoreHostEntityConstants.NoneHostEntityId, CoreHostEntityScore.Default),
            (CoreHostEntityType.UnknownHost, CoreEntityConstants.UnknownHostName, CoreHostEntityConstants.NoneHostEntityId, CoreHostEntityScore.Default),
            (CoreHostEntityType.GenericHost, "localhost", CoreHostEntityConstants.LocalHostEntityId, CoreHostEntityScore.Default),
            (CoreHostEntityType.GenericHost, "LocalHost", CoreHostEntityConstants.LocalHostEntityId, CoreHostEntityScore.Default),
            (CoreHostEntityType.DnsHost, CoreIPAddressExtensions.StringGooglePublicDnsServer, CoreHostEntityConstants.GooglePublicDnsServerEntityId, CoreHostEntityScore.Default),
            (CoreHostEntityType.MDnsHost, "_airplay._tcp.local.", Guid.Parse("24945a61-0307-6917-d3d1-22e97331bfae"), CoreHostEntityScore.Default),
            (CoreHostEntityType.PingHost, CoreIPAddressExtensions.StringGooglePublicDnsServer, CoreHostEntityConstants.GooglePublicDnsServerEntityId, CoreHostEntityScore.Default),
        ];

        public static (IPAddress, int, CoreNetworkAddressEntityType, CoreIPAddressScore)[] TestNetworkAddresses =
        [
            (IPAddress.Parse(CoreIPAddressExtensions.StringBroadcast), 32, CoreNetworkAddressEntityType.IPv4NetworkAddressWithPrefix, CoreIPAddressScore.None),
            (IPAddress.Parse(CoreIPAddressExtensions.StringLoopback), 32, CoreNetworkAddressEntityType.IPv4NetworkAddressWithPrefix, CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.IPv4),
            (IPAddress.IPv6Loopback, 128, CoreNetworkAddressEntityType.IPv6NetworkAddress, CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.IPv6),
            (CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreNetworkAddressConstants.UnknownNetworkAddressPrefixLength, CoreNetworkAddressEntityType.IPv4NetworkAddress, CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.IPv4),
            (CoreIPAddressExtensions.SubnetClassA, CoreIPAddressExtensions.Private192IPAddressPrefixLength, CoreNetworkAddressEntityType.IPv4NetworkAddressWithPrefix, CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.IPv4),
            (CoreIPAddressExtensions.SubnetClassB, CoreIPAddressExtensions.Private172IPAddressPrefixLength, CoreNetworkAddressEntityType.IPv4NetworkAddressWithPrefix, CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.IPv4),
            (CoreIPAddressExtensions.SubnetClassC, CoreIPAddressExtensions.Private10IPAddressPrefixLength, CoreNetworkAddressEntityType.IPv4NetworkAddressWithPrefix, CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.IPv4),
        ];
    }
}
