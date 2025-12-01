// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// // ***********************************************************************
// <copyright file="CoreFirewallUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Core Firewall integration tests.</summary>
using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Firewall.Addresses;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Networking.Firewall
{
    /// <summary>
    /// Class CoreFirewallIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreFirewallUnitTests))]

    public class CoreFirewallUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreFirewallUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreFirewallUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NetworkFirewall_FirewallAddressParse()
        {
            var addresses = new ICoreFirewallAddress[]
            {
                new CoreFirewallAddressDHCPService(), new CoreFirewallAddressDNSService(),
                new CoreFirewallAddressDefaultGateway(), new CoreFirewallAddressWINSService(),
                new CoreFirewallAddressLocalSubnet(),
                new CoreFirewallIPRange(IPAddress.Parse("192.168.0.0"), IPAddress.Parse("192.168.1.0")),
                new CoreFirewallNetworkAddress(IPAddress.Parse("10.10.0.0"), IPAddress.Parse("255.255.255.0")),
                new CoreFirewallAddressSingleIP(IPAddress.Parse("192.168.1.1")), CoreFirewallAddressSingleIP.Loopback,
            };

            string addressesInString = CoreFirewallAddressHelper.AddressesToString(addresses);

            // Check if all addresses resulted in an entry
            addressesInString.Count(c => c == ',').Should().Be(addresses.Length - 1);

            ICoreFirewallAddress[] addressesParsed = CoreFirewallAddressHelper.StringToAddresses(addressesInString);

            // Check if parsing result in the same output
            addresses.SequenceEqual(addressesParsed).Should().BeTrue();

            addressesInString = CoreFirewallAddressHelper.AddressesToString(addresses.Concat(new ICoreFirewallAddress[] { CoreFirewallAddressSingleIP.Any }).ToArray());

            // Check if adding `SingleIP.Any` results in ignoring all other addresses
            addressesInString.Should().Be("*");
        }

        [Fact]
        public void NetworkFirewall_IPv4MaxMin()
        {
            var ip1 = IPAddress.Parse("192.168.1.1");
            var ip2 = IPAddress.Parse("190.168.1.1");
            IPAddress max = CoreFirewallAddressHelper.Max(ip1, ip2);
            IPAddress min = CoreFirewallAddressHelper.Min(ip1, ip2);

            ip1.Should().Be(max);
            ip2.Should().Be(min);
        }

        [Fact]
        public void NetworkFirewall_IPv6MaxMin()
        {
            var ip1 = IPAddress.Parse("2607:fea8:4260:31a::9");
            var ip2 = IPAddress.Parse("2607:fea8:4260:315::9");
            IPAddress max = CoreFirewallAddressHelper.Max(ip1, ip2);
            IPAddress min = CoreFirewallAddressHelper.Min(ip1, ip2);

            ip1.Should().Be(max);
            ip2.Should().Be(min);
        }

        [Fact]
        public void NetworkFirewall_FirewallIPRange_InvalidParses()
        {
            // Can't parse empty strings
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse(string.Empty);
            });

            // Can't parse combined ip address families
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("127.0.0.1-ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("::1-255.255.255.255");
            });

            // Can't parse invalid ip addresses
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("-1.0.0.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("-1::");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("256.0.0.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("10000::");
            });

            // Can't parse ip ranges with `any` addresses inside
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("0.0.0.0-192.168.1.1");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("::-2001::");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("192.168.1.1-0.0.0.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("2001::-::");
            });

            // Can't parse network addresses
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("127.0.0.1/28");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("::1/112");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("192.168.1.1/255.255.255.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallIPRange.Parse("2001:1::/ffff:ffff:ffff:ffff:ffff:ffff:ffff:0");
            });
        }

        [Fact]
        public void NetworkFirewall_FirewallIPRange_ValidParses()
        {
            string[] addresses = new[]
            {
                "*",
                CoreIPAddressExtensions.StringAny,
                CoreIPAddressExtensions.StringLoopback,
                "192.168.1.0-192.168.1.0",
                "192.168.2.0-192.168.2.255",
                "192.168.3.30-192.168.4.100",
                "::",
                "::1",
                "2001:1::-2001:1::",
                "2001:2::-2001:2::ffff",
                "2001:3::1212-2001:4::e1e1",
            };

            CoreFirewallIPRange[] expected = new[]
            {
                new CoreFirewallIPRange(IPAddress.Any),
                new CoreFirewallIPRange(IPAddress.Any),
                new CoreFirewallIPRange(IPAddress.Loopback),
                new CoreFirewallIPRange(IPAddress.Parse("192.168.1.0")),
                new CoreFirewallIPRange(IPAddress.Parse("192.168.2.0"), IPAddress.Parse("192.168.2.255")),
                new CoreFirewallIPRange(IPAddress.Parse("192.168.3.30"), IPAddress.Parse("192.168.4.100")),
                new CoreFirewallIPRange(IPAddress.IPv6Any),
                new CoreFirewallIPRange(IPAddress.IPv6Loopback),
                new CoreFirewallIPRange(IPAddress.Parse("2001:1::")),
                new CoreFirewallIPRange(
                    IPAddress.Parse("2001:2::"),
                    IPAddress.Parse("2001:2::ffff")),
                new CoreFirewallIPRange(
                    IPAddress.Parse("2001:3::1212"),
                    IPAddress.Parse("2001:4::e1e1")),
            };

            CoreFirewallIPRange[] actual = addresses.Select(CoreFirewallIPRange.Parse).ToArray();

            expected.SequenceEqual(actual).Should().BeTrue();

            string addressesInString = string.Join(",", actual.Select(address => address!.ToString()).ToArray());

            addressesInString.Should().Be("*,*,127.0.0.1,192.168.1.0,192.168.2.0-192.168.2.255,192.168.3.30-192.168.4.100," +
                "*,::1,2001:1::,2001:2::-2001:2::ffff,2001:3::1212-2001:4::e1e1");
        }

        [Fact]
        public void NetworkFirewall_FirewallNetworkAddress_InvalidParses()
        {
            // Can't parse empty strings
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse(string.Empty);
            });

            // Can't parse combined ip address families
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("127.0.0.1/ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("::1/255.255.255.255");
            });

            // Can't parse invalid ip addresses
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("-1.0.0.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("-1::");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("256.0.0.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("10000::");
            });

            // Can't parse multi ip subnet with `any` addresses
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("0.0.0.0/24");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("::/112");
            });

            // Can't parse ip addresses with zero subnet mask (which means `any`)
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("127.0.0.1/0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("::1/0");
            });

            // Can't parse ip addresses with higher than possible subnet mask
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("127.0.0.1/33");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("::1/129");
            });

            // Can't parse ip ranges
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("192.168.1.1-192.168.2.1");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallNetworkAddress.Parse("2001:1::-2001:2::");
            });
        }

        [Fact]
        public void NetworkFirewall_FirewallNetworkAddress_ValidParses()
        {
            string[] addresses = new[]
            {
                "*",
                CoreIPAddressExtensions.StringAny,
                CoreIPAddressExtensions.StringLoopback,
                "192.168.1.0/255.255.255.255",
                "192.168.2.0/24",
                "192.168.3.0/255.255.0.0",
                "::",
                "::1",
                "2001:1::/ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff",
                "2001:2::/112",
                "2001:3::/ffff:ffff:ffff:ffff:ffff:ffff::",
            };

            CoreFirewallNetworkAddress[] expected = new[]
            {
                new CoreFirewallNetworkAddress(IPAddress.Any),
                new CoreFirewallNetworkAddress(IPAddress.Any),
                new CoreFirewallNetworkAddress(IPAddress.Loopback),
                new CoreFirewallNetworkAddress(IPAddress.Parse("192.168.1.0")),
                new CoreFirewallNetworkAddress(IPAddress.Parse("192.168.2.0"), IPAddress.Parse("255.255.255.0")),
                new CoreFirewallNetworkAddress(IPAddress.Parse("192.168.3.0"), IPAddress.Parse("255.255.0.0")),
                new CoreFirewallNetworkAddress(IPAddress.IPv6Any),
                new CoreFirewallNetworkAddress(IPAddress.IPv6Loopback),
                new CoreFirewallNetworkAddress(IPAddress.Parse("2001:1::")),
                new CoreFirewallNetworkAddress(
                    IPAddress.Parse("2001:2::"),
                    IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff:ffff:0")),
                new CoreFirewallNetworkAddress(
                    IPAddress.Parse("2001:3::"),
                    IPAddress.Parse("ffff:ffff:ffff:ffff:ffff:ffff::")),
            };

            CoreFirewallNetworkAddress[] actual = addresses.Select(CoreFirewallNetworkAddress.Parse).ToArray();

            expected.SequenceEqual(actual).Should().BeTrue();

            string addressesInString = string.Join(",", actual.Select(address => address?.ToString()).ToArray());

            addressesInString.Should().Be(
                "*,*,127.0.0.1,192.168.1.0,192.168.2.0/255.255.255.0,192.168.3.0/255.255.0.0," +
                "*,::1,2001:1::,2001:2::/ffff:ffff:ffff:ffff:ffff:ffff:ffff:0,2001:3::/ffff:ffff:ffff:ffff:ffff:ffff::");
        }

        [Fact]
        public void NetworkFirewall_FirewallNetworkAddress_CheckRanges()
        {
            var expectedRanges = new Dictionary<string, Tuple<string, string>>()
            {
                {
                    "1.1.1.1/32", new Tuple<string, string>("1.1.1.1", "1.1.1.1")
                },
                {
                    "1.1.1.1/31", new Tuple<string, string>("1.1.1.0", "1.1.1.1")
                },
                {
                    "1.1.1.1/30", new Tuple<string, string>("1.1.1.0", "1.1.1.3")
                },
                {
                    "1.1.1.1/29", new Tuple<string, string>("1.1.1.0", "1.1.1.7")
                },
                {
                    "1.1.1.1/28", new Tuple<string, string>("1.1.1.0", "1.1.1.15")
                },
                {
                    "1.1.1.1/27", new Tuple<string, string>("1.1.1.0", "1.1.1.31")
                },
                {
                    "1.1.1.1/26", new Tuple<string, string>("1.1.1.0", "1.1.1.63")
                },
                {
                    "1.1.1.1/25", new Tuple<string, string>("1.1.1.0", "1.1.1.127")
                },
                {
                    "1.1.1.1/24", new Tuple<string, string>("1.1.1.0", "1.1.1.255")
                },
                {
                    "1.1.1.1/23", new Tuple<string, string>("1.1.0.0", "1.1.1.255")
                },
                {
                    "1.1.1.1/9", new Tuple<string, string>("1.0.0.0", "1.127.255.255")
                },
                {
                    "1.1.1.1/8", new Tuple<string, string>("1.0.0.0", "1.255.255.255")
                },
                {
                    "1.1.1.1/7", new Tuple<string, string>(CoreIPAddressExtensions.StringAny, "1.255.255.255")
                },
                {
                    "1.1.1.1/6", new Tuple<string, string>(CoreIPAddressExtensions.StringAny, "3.255.255.255")
                },
                {
                    "1.1.1.1/5", new Tuple<string, string>(CoreIPAddressExtensions.StringAny, "7.255.255.255")
                },
                {
                    "1.1.1.1/4", new Tuple<string, string>(CoreIPAddressExtensions.StringAny, "15.255.255.255")
                },
                {
                    "1.1.1.1/3", new Tuple<string, string>(CoreIPAddressExtensions.StringAny, "31.255.255.255")
                },
                {
                    "1.1.1.1/2", new Tuple<string, string>(CoreIPAddressExtensions.StringAny, "63.255.255.255")
                },
                {
                    "1.1.1.1/1", new Tuple<string, string>(CoreIPAddressExtensions.StringAny, "127.255.255.255")
                },
                {
                    "127.127.127.127/32", new Tuple<string, string>("127.127.127.127", "127.127.127.127")
                },
                {
                    "127.127.127.127/31", new Tuple<string, string>("127.127.127.126", "127.127.127.127")
                },
                {
                    "127.127.127.127/30", new Tuple<string, string>("127.127.127.124", "127.127.127.127")
                },
                {
                    "127.127.127.127/29", new Tuple<string, string>("127.127.127.120", "127.127.127.127")
                },
                {
                    "127.127.127.127/25", new Tuple<string, string>("127.127.127.0", "127.127.127.127")
                },
                {
                    "127.127.127.127/24", new Tuple<string, string>("127.127.127.0", "127.127.127.255")
                },
                {
                    "127.127.127.127/23", new Tuple<string, string>("127.127.126.0", "127.127.127.255")
                },
                {
                    "127.127.127.127/3", new Tuple<string, string>("96.0.0.0", "127.255.255.255")
                },
                {
                    "127.127.127.127/2", new Tuple<string, string>("64.0.0.0", "127.255.255.255")
                },
                {
                    "127.127.127.127/1", new Tuple<string, string>(CoreIPAddressExtensions.StringAny, "127.255.255.255")
                },
                {
                    "255.255.255.255/32", new Tuple<string, string>(CoreIPAddressExtensions.StringBroadcast, CoreIPAddressExtensions.StringBroadcast)
                },
                {
                    "255.255.255.255/31", new Tuple<string, string>("255.255.255.254", CoreIPAddressExtensions.StringBroadcast)
                },
                {
                    "255.255.255.255/30", new Tuple<string, string>("255.255.255.252", CoreIPAddressExtensions.StringBroadcast)
                },
                {
                    "255.255.255.255/29", new Tuple<string, string>("255.255.255.248", CoreIPAddressExtensions.StringBroadcast)
                },
                {
                    "255.255.255.255/25", new Tuple<string, string>("255.255.255.128", CoreIPAddressExtensions.StringBroadcast)
                },
                {
                    "255.255.255.255/24", new Tuple<string, string>("255.255.255.0", CoreIPAddressExtensions.StringBroadcast)
                },
                {
                    "255.255.255.255/23", new Tuple<string, string>("255.255.254.0", CoreIPAddressExtensions.StringBroadcast)
                },
                {
                    "255.255.255.255/3", new Tuple<string, string>("224.0.0.0", CoreIPAddressExtensions.StringBroadcast)
                },
                {
                    "255.255.255.255/2", new Tuple<string, string>("192.0.0.0", CoreIPAddressExtensions.StringBroadcast)
                },
                {
                    "255.255.255.255/1", new Tuple<string, string>("128.0.0.0", CoreIPAddressExtensions.StringBroadcast)
                },
            };

            foreach (KeyValuePair<string, Tuple<string, string>> expectedRange in expectedRanges)
            {
                var address = CoreFirewallNetworkAddress.Parse(expectedRange.Key);
                address.Should().NotBeNull();
                IPAddress.Parse(expectedRange.Value.Item1).Should().Be(address!.StartAddress, "Start of {0}", expectedRange.Key);
                IPAddress.Parse(expectedRange.Value.Item2).Should().Be(address.EndAddress, "End of {0}", expectedRange.Key);
            }
        }

        [Fact]
        public void NetworkFirewall_FirewallAddressSingleIP_InvalidParses()
        {
            // Can't parse empty strings
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse(string.Empty);
            });

            // Can't parse invalid ip addresses
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("-1.0.0.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("-1::");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("256.0.0.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("10000::");
            });

            // Can't parse ip ranges containing more than one ip address
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("192.168.1.1-192.168.2.1");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("2001:1::-2001:2::");
            });

            // Can't parse network addresses containing more than one ip address
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("127.0.0.1/28");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("::1/112");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("192.168.1.1/255.255.255.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSingleIP.Parse("2001:1::/ffff:ffff:ffff:ffff:ffff:ffff:ffff:0");
            });
        }

        [Fact]
        public void NetworkFirewall_FirewallAddressSingleIP_ValidParses()
        {
            string[] addresses = new[]
            {
                "*",
                CoreIPAddressExtensions.StringAny,
                CoreIPAddressExtensions.StringLoopback,
                "192.168.1.0-192.168.1.0",
                "192.168.2.0/255.255.255.255",
                "::",
                "::1",
                "2001:1::-2001:1::",
                "2001:2::/ffff:ffff:ffff:ffff:ffff:ffff:ffff:ffff",
            };

            CoreFirewallAddressSingleIP[] expected = new[]
            {
                new CoreFirewallAddressSingleIP(IPAddress.Any),
                new CoreFirewallAddressSingleIP(IPAddress.Any),
                new CoreFirewallAddressSingleIP(IPAddress.Loopback),
                new CoreFirewallAddressSingleIP(IPAddress.Parse("192.168.1.0")),
                new CoreFirewallAddressSingleIP(IPAddress.Parse("192.168.2.0")),
                new CoreFirewallAddressSingleIP(IPAddress.IPv6Any),
                new CoreFirewallAddressSingleIP(IPAddress.IPv6Loopback),
                new CoreFirewallAddressSingleIP(IPAddress.Parse("2001:1::")),
                new CoreFirewallAddressSingleIP(IPAddress.Parse("2001:2::")),
            };

            CoreFirewallAddressSingleIP[] actual = addresses.Select(CoreFirewallAddressSingleIP.Parse).ToArray();

            expected.SequenceEqual(actual).Should().BeTrue();

            string addressesInString = string.Join(",", actual.Select(address => address?.ToString()).ToArray());

            addressesInString.Should().Be("*,*,127.0.0.1,192.168.1.0,192.168.2.0,*,::1,2001:1::,2001:2::");
        }

        [Fact]
        public void NetworkFirewall_FirewallAddressSpecial_DefaultGatewayValidParse()
        {
            // ReSharper disable once StringLiteralTypo
            var str = "Defaultgateway";
            var address = CoreFirewallAddressSpecial.Parse(str);

            address.Should().Be(new CoreFirewallAddressDefaultGateway());
            address.Should().NotBeNull();
            str.Should().Be(address!.ToString());
        }

        [Fact]
        public void NetworkFirewall_FirewallAddressSpecial_DHCPServiceValidParse()
        {
            var str = "DHCP";
            var address = CoreFirewallAddressSpecial.Parse(str);

            address.Should().NotBeNull();
            address.Should().Be(new CoreFirewallAddressDHCPService());
            address.Should().NotBeNull();
            str.Should().Be(address!.ToString());
        }

        [Fact]
        public void NetworkFirewall_FirewallAddressSpecial_DNSServiceValidParse()
        {
            var str = "DNS";
            var address = CoreFirewallAddressSpecial.Parse(str);

            address.Should().NotBeNull();
            address.Should().Be(new CoreFirewallAddressDNSService());
            address.Should().NotBeNull();
            str.Should().Be(address!.ToString());
        }

        [Fact]
        public void NetworkFirewall_FirewallAddressSpecial_LocalSubnetValidParse()
        {
            var str = "LocalSubnet";
            var address = CoreFirewallAddressSpecial.Parse(str);

            address.Should().NotBeNull();
            address.Should().Be(new CoreFirewallAddressLocalSubnet());
            address.Should().NotBeNull();
            str.Should().Be(address!.ToString());
        }

        [Fact]
        public void NetworkFirewall_FirewallAddressSpecial_WINSServiceValidParse()
        {
            var str = "WINS";
            var address = CoreFirewallAddressSpecial.Parse(str);

            address.Should().NotBeNull();
            address.Should().Be(new CoreFirewallAddressWINSService());
            address.Should().NotBeNull();
            str.Should().Be(address!.ToString());
        }

        [Fact]
        public void NetworkFirewall_FirewallAddressSpecial_InvalidParses()
        {
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSpecial.Parse(string.Empty);
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSpecial.Parse("SOME_UNKNOWN_STRING");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSpecial.Parse("*");
            });

            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSpecial.Parse("192.168.1.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSpecial.Parse("2001:1::");
            });

            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSpecial.Parse("192.168.2.0/24");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSpecial.Parse("2001:1::/112");
            });

            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSpecial.Parse("192.168.3.0-192.168.4.0");
            });
            Assert.Throws<FormatException>(() =>
            {
                CoreFirewallAddressSpecial.Parse("2001:2::-2001:3::");
            });
        }
    }
}
