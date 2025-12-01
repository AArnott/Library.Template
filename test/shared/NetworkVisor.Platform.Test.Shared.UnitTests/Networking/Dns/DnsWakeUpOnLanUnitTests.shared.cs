// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="DnsWakeUpOnLanUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Networking.Services.MulticastDns.Types;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Dns
{
    /// <summary>
    /// Class DnsWakeUpOnLanUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DnsWakeUpOnLanUnitTests))]

    public class DnsWakeUpOnLanUnitTests : CoreTestCaseBase
    {
        private static readonly byte[] OptionBytesNoPassword = [0x00, 0x00, 0x8B, 0xDC, 0xAB, 0x5D, 0xAD, 0xEA, 0xF3, 0xD2, 0x88, 0x6B, 0x28, 0x71];
        private static readonly byte[] OptionBytesPassword = [0x00, 0x00, 0x8B, 0xDC, 0xAB, 0x5D, 0xAD, 0xEA, 0xF3, 0xD2, 0x88, 0x6B, 0x28, 0x71, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36];
        private static readonly byte[] OptionBytesPassword2 = [0x00, 0x00, 0x80, 0xDC, 0xAB, 0x5D, 0xAD, 0xEA, 0xF3, 0xD2, 0x88, 0x6B, 0x28, 0x71, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36];
        private static readonly byte[] OptionBytesPassword3 = [0x00, 0x00, 0x8B, 0xDC, 0xAB, 0x5D, 0xAD, 0xEA, 0xF0, 0xD2, 0x88, 0x6B, 0x28, 0x71, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36];
        private static readonly byte[] OptionBytesShortPassword = [0x00, 0x00, 0x8B, 0xDC, 0xAB, 0x5D, 0xAD, 0xEA, 0xF3, 0xD2, 0x88, 0x6B, 0x28, 0x71, 0x31, 0x32, 0x33, 0x34];
        private static readonly byte[] OptionBytesNoWakeup = [0x00, 0x00, 0x8B, 0xDC, 0xAB, 0x5D, 0xAD, 0xEA];
        private static readonly byte[] Password = [0x31, 0x32, 0x33, 0x34, 0x35, 0x36];
        private static readonly byte[] ShortPassword = [0x31, 0x32, 0x33, 0x34];
        private static readonly PhysicalAddress PrimaryPhysicalAddress = PhysicalAddress.Parse("8BDCAB5DADEA");
        private static readonly PhysicalAddress PrimaryPhysicalAddress2 = PhysicalAddress.Parse("80DCAB5DADEA");
        private static readonly PhysicalAddress WakeupPhysicalAddress = PhysicalAddress.Parse("F3D2886B2871");
        private static readonly PhysicalAddress WakeupPhysicalAddress3 = PhysicalAddress.Parse("F0D2886B2871");
        private static readonly CoreIPEndPoint RemoteIPEndPoint = new(IPAddress.Loopback, CoreMulticastDnsConstants.MulticastDnsServerPort);
        private static readonly ushort SenderUdpPayloadSize = 1440;

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsWakeUpOnLanUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DnsWakeUpOnLanUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void DnsWakeUpOnLan_NoPassword()
        {
            var dnsWakeUpOnLan = new DnsWakeUpOnLan(OptionBytesNoPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan.Should().NotBeNull();
            dnsWakeUpOnLan.Version.Should().Be(0);
            dnsWakeUpOnLan.Sequence.Should().Be(0);
            dnsWakeUpOnLan.SenderUdpPayloadSize.Should().Be(SenderUdpPayloadSize);
            dnsWakeUpOnLan.PrimaryPhysicalAddress.Should().Be(PrimaryPhysicalAddress);
            dnsWakeUpOnLan.WakeupPhysicalAddress.Should().Be(WakeupPhysicalAddress);
            dnsWakeUpOnLan.Password.Should().BeNull();
            dnsWakeUpOnLan.RemoteIPEndPoint.Should().Be(RemoteIPEndPoint);
        }

        [Fact]
        public void DnsWakeUpOnLan_Null()
        {
            Func<DnsWakeUpOnLan> fx = () => new DnsWakeUpOnLan(null!, SenderUdpPayloadSize, RemoteIPEndPoint);
            fx.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void DnsWakeUpOnLan_Password()
        {
            var dnsWakeUpOnLan = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan.Should().NotBeNull();
            dnsWakeUpOnLan.Version.Should().Be(0);
            dnsWakeUpOnLan.Sequence.Should().Be(0);
            dnsWakeUpOnLan.SenderUdpPayloadSize.Should().Be(SenderUdpPayloadSize);
            dnsWakeUpOnLan.PrimaryPhysicalAddress.Should().Be(PrimaryPhysicalAddress);
            dnsWakeUpOnLan.WakeupPhysicalAddress.Should().Be(WakeupPhysicalAddress);
            dnsWakeUpOnLan.Password.Should().BeEquivalentTo(Password);
            dnsWakeUpOnLan.RemoteIPEndPoint.Should().Be(RemoteIPEndPoint);
        }

        [Fact]
        public void DnsWakeUpOnLan_ShortPassword()
        {
            var dnsWakeUpOnLan = new DnsWakeUpOnLan(OptionBytesShortPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan.Should().NotBeNull();
            dnsWakeUpOnLan.Version.Should().Be(0);
            dnsWakeUpOnLan.Sequence.Should().Be(0);
            dnsWakeUpOnLan.SenderUdpPayloadSize.Should().Be(SenderUdpPayloadSize);
            dnsWakeUpOnLan.PrimaryPhysicalAddress.Should().Be(PrimaryPhysicalAddress);
            dnsWakeUpOnLan.WakeupPhysicalAddress.Should().Be(WakeupPhysicalAddress);
            dnsWakeUpOnLan.Password.Should().BeEquivalentTo(ShortPassword);
            dnsWakeUpOnLan.RemoteIPEndPoint.Should().Be(RemoteIPEndPoint);
        }

        [Fact]
        public void DnsWakeUpOnLan_NoWakeup()
        {
            var dnsWakeUpOnLan = new DnsWakeUpOnLan(OptionBytesNoWakeup, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan.Should().NotBeNull();
            dnsWakeUpOnLan.Version.Should().Be(0);
            dnsWakeUpOnLan.Sequence.Should().Be(0);
            dnsWakeUpOnLan.SenderUdpPayloadSize.Should().Be(SenderUdpPayloadSize);
            dnsWakeUpOnLan.PrimaryPhysicalAddress.Should().Be(PrimaryPhysicalAddress);
            dnsWakeUpOnLan.WakeupPhysicalAddress.Should().Be(PrimaryPhysicalAddress);
            dnsWakeUpOnLan.Password.Should().BeNull();
            dnsWakeUpOnLan.RemoteIPEndPoint.Should().Be(RemoteIPEndPoint);
        }

        [Fact]
        public void DnsWakeUpOnLan_ToStringWithParentsPropNameMultiLine()
        {
            var dnsWakeUpOnLan = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan.Should().NotBeNull();
            dnsWakeUpOnLan.Version.Should().Be(0);
            dnsWakeUpOnLan.Sequence.Should().Be(0);
            dnsWakeUpOnLan.SenderUdpPayloadSize.Should().Be(SenderUdpPayloadSize);
            dnsWakeUpOnLan.PrimaryPhysicalAddress.Should().Be(PrimaryPhysicalAddress);
            dnsWakeUpOnLan.WakeupPhysicalAddress.Should().Be(WakeupPhysicalAddress);
            dnsWakeUpOnLan.Password.Should().BeEquivalentTo(Password);
            dnsWakeUpOnLan.RemoteIPEndPoint.Should().Be(RemoteIPEndPoint);
            this.TestOutputHelper.WriteLine(dnsWakeUpOnLan.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void DnsWakeUpOnLan_RemoteEndPoint_Null()
        {
            var dnsWakeUpOnLan = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, null);
            dnsWakeUpOnLan.Should().NotBeNull();
            dnsWakeUpOnLan.Version.Should().Be(0);
            dnsWakeUpOnLan.Sequence.Should().Be(0);
            dnsWakeUpOnLan.SenderUdpPayloadSize.Should().Be(SenderUdpPayloadSize);
            dnsWakeUpOnLan.PrimaryPhysicalAddress.Should().Be(PrimaryPhysicalAddress);
            dnsWakeUpOnLan.WakeupPhysicalAddress.Should().Be(WakeupPhysicalAddress);
            dnsWakeUpOnLan.Password.Should().BeEquivalentTo(Password);
            dnsWakeUpOnLan.RemoteIPEndPoint.Should().BeNull();
            this.TestOutputHelper.WriteLine(dnsWakeUpOnLan.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void DnsWakeUpOnLan_Equals()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            var dnsWakeUpOnLan2 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();
            dnsWakeUpOnLan2.Should().NotBeNull();

            dnsWakeUpOnLan1.Equals(dnsWakeUpOnLan2).Should().BeTrue();
            dnsWakeUpOnLan2.Equals(dnsWakeUpOnLan1).Should().BeTrue();
        }

        [Fact]
        public void DnsWakeUpOnLan_Equals_SameAs()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();

            dnsWakeUpOnLan1.Equals(dnsWakeUpOnLan1).Should().BeTrue();
            dnsWakeUpOnLan1.Equals(dnsWakeUpOnLan1).Should().BeTrue();
            dnsWakeUpOnLan1.Should().BeSameAs(dnsWakeUpOnLan1);
        }

        [Fact]
        public void DnsWakeUpOnLan_Equals_Null()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();

            dnsWakeUpOnLan1.Equals(null).Should().BeFalse();
        }

        [Fact]
        public void DnsWakeUpOnLan_CompareTo_Equals()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            var dnsWakeUpOnLan2 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();
            dnsWakeUpOnLan2.Should().NotBeNull();

            dnsWakeUpOnLan1.CompareTo(dnsWakeUpOnLan2).Should().Be(0);
            dnsWakeUpOnLan2.CompareTo(dnsWakeUpOnLan1).Should().Be(0);
        }

        [Fact]
        public void DnsWakeUpOnLan_CompareTo_Null()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();

            dnsWakeUpOnLan1.CompareTo(null).Should().BeGreaterThan(0);
        }

        [Fact]
        public void DnsWakeUpOnLan_CompareTo_SameAs()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();

            dnsWakeUpOnLan1.CompareTo(dnsWakeUpOnLan1).Should().Be(0);
            dnsWakeUpOnLan1.CompareTo(dnsWakeUpOnLan1).Should().Be(0);
            dnsWakeUpOnLan1.Should().BeSameAs(dnsWakeUpOnLan1);
        }

        [Fact]
        public void DnsWakeUpOnLan_CompareTo_NotEqual_PrimaryPhysicalAddress()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            var dnsWakeUpOnLan2 = new DnsWakeUpOnLan(OptionBytesPassword2, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();
            dnsWakeUpOnLan2.Should().NotBeNull();

            dnsWakeUpOnLan1.PrimaryPhysicalAddress.Should().Be(PrimaryPhysicalAddress);
            dnsWakeUpOnLan2.PrimaryPhysicalAddress.Should().Be(PrimaryPhysicalAddress2);

            dnsWakeUpOnLan1.CompareTo(dnsWakeUpOnLan2).Should().BeGreaterThan(0);
            dnsWakeUpOnLan2.CompareTo(dnsWakeUpOnLan1).Should().BeLessThan(0);
        }

        [Fact]
        public void DnsWakeUpOnLan_CompareTo_NotEqual_SenderPayloadSize()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            var dnsWakeUpOnLan2 = new DnsWakeUpOnLan(OptionBytesPassword, 440, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();
            dnsWakeUpOnLan2.Should().NotBeNull();

            dnsWakeUpOnLan1.SenderUdpPayloadSize.Should().Be(SenderUdpPayloadSize);
            dnsWakeUpOnLan2.SenderUdpPayloadSize.Should().Be(440);

            dnsWakeUpOnLan1.CompareTo(dnsWakeUpOnLan2).Should().BeGreaterThan(0);
            dnsWakeUpOnLan2.CompareTo(dnsWakeUpOnLan1).Should().BeLessThan(0);
        }

        [Fact]
        public void DnsWakeUpOnLan_CompareTo_NotEqual_WakeupPhysicalAddress()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            var dnsWakeUpOnLan2 = new DnsWakeUpOnLan(OptionBytesPassword3, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();
            dnsWakeUpOnLan2.Should().NotBeNull();

            dnsWakeUpOnLan1.WakeupPhysicalAddress.Should().Be(WakeupPhysicalAddress);
            dnsWakeUpOnLan2.WakeupPhysicalAddress.Should().Be(WakeupPhysicalAddress3);

            dnsWakeUpOnLan1.CompareTo(dnsWakeUpOnLan2).Should().BeGreaterThan(0);
            dnsWakeUpOnLan2.CompareTo(dnsWakeUpOnLan1).Should().BeLessThan(0);
        }

        [Fact]
        public void DnsWakeUpOnLan_CompareTo_NotEqual_Password()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            var dnsWakeUpOnLan2 = new DnsWakeUpOnLan(OptionBytesShortPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            dnsWakeUpOnLan1.Should().NotBeNull();
            dnsWakeUpOnLan2.Should().NotBeNull();

            dnsWakeUpOnLan1.Password.Should().BeEquivalentTo(Password);
            dnsWakeUpOnLan2.Password.Should().BeEquivalentTo(ShortPassword);

            // Password uses hashcode so only equal is a valid comparison
            dnsWakeUpOnLan1.CompareTo(dnsWakeUpOnLan2).Should().NotBe(0);
            dnsWakeUpOnLan2.CompareTo(dnsWakeUpOnLan1).Should().NotBe(0);
        }

        [Fact]
        public void DnsWakeUpOnLan_CompareTo_NotEqual_RemoteEndPoint()
        {
            var dnsWakeUpOnLan1 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, RemoteIPEndPoint);
            var dnsWakeUpOnLan2 = new DnsWakeUpOnLan(OptionBytesPassword, SenderUdpPayloadSize, null);
            dnsWakeUpOnLan1.Should().NotBeNull();
            dnsWakeUpOnLan2.Should().NotBeNull();

            dnsWakeUpOnLan1.RemoteIPEndPoint.Should().Be(RemoteIPEndPoint);
            dnsWakeUpOnLan2.RemoteIPEndPoint.Should().BeNull();

            dnsWakeUpOnLan1.CompareTo(dnsWakeUpOnLan2).Should().BeGreaterThan(0);
            dnsWakeUpOnLan2.CompareTo(dnsWakeUpOnLan1).Should().BeLessThan(0);
        }
    }
}
