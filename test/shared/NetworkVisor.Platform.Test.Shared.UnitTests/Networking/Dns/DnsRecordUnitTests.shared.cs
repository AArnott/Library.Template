// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="DnsRecordUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Collections.Immutable;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Networking.Services.MulticastDns.Records;
using NetworkVisor.Core.Networking.Services.MulticastDns.Types;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Dns
{
    /// <summary>
    /// Class DnsRecordUnitTests.
    /// </summary>
    [PlatformTrait(typeof(DnsRecordUnitTests))]

    public class DnsRecordUnitTests : CoreTestCaseBase
    {
        private const string DefaultServiceHostDomainName1 = "MyComputer.local";
        private const string DefaultServiceHostDomainName2 = "YourComputer.local";

        private const string DefaultAuthoritativeDomain = "MyDomain.local";

        private const string DefaultServiceName1 = "_myservice._tcp.local";
        private const string DefaultServiceName2 = "_yourservice._tcp.local";

        private const string DefaultServiceInstance1 = "instance1._myservice._tcp.local";
        private const string DefaultServiceInstance2 = "instance2._myservice._tcp.local";

        private const string DefaultRootDomain = ".";

        /// <summary>
        /// Initializes a new instance of the <see cref="DnsRecordUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public DnsRecordUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void DnsRecord_A_Equals()
        {
            var recordA = new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
            recordA.Address.Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
            recordA.HostName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordA);
            recordA.HostName.Should().Be(DefaultServiceHostDomainName1);

            this.TestDelay(2000, this.TestCaseLogger).Should().BeTrue();
            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1));

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_A_NotEquals()
        {
            var recordA1 = new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
            recordA1.Address.Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);
            recordA1.HostName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordA1);
            recordA1.HostName.Should().Be(DefaultServiceHostDomainName1);

            this.TestDelay(2000, this.TestCaseLogger).Should().BeTrue();
            var recordA2 = new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address2);
            recordA2.Address.Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address2);
            recordA2.HostName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordA2);
            recordA2.HostName.Should().Be(DefaultServiceHostDomainName1);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_A_OwnerName_NotEquals()
        {
            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1));
            CreateAdditionalDnsResponseRecord(new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1), DefaultServiceHostDomainName2).Equals(additionalDnsResponseRecord1).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_A_DnsRecordClass_NotEquals()
        {
            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1));
            CreateAdditionalDnsResponseRecord(new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1), DefaultServiceHostDomainName1, DnsRecordClass.CS).Equals(additionalDnsResponseRecord1).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_A_TimeToLive_Equals()
        {
            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1));
            CreateAdditionalDnsResponseRecord(new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1), DefaultServiceHostDomainName1, DnsRecordClass.IN, 4000).Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_A_TimeStamp_Equals()
        {
            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1));
            CreateAdditionalDnsResponseRecord(new DnsRecordA(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1), DefaultServiceHostDomainName1, DnsRecordClass.IN, 4500, DateTimeOffset.UtcNow).Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_AAAA_Equals()
        {
            var recordAAAA = new DnsRecordAAAA(CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1);
            recordAAAA.Address.Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1);
            recordAAAA.HostName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordAAAA);
            recordAAAA.HostName.Should().Be(DefaultServiceHostDomainName1);

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(new DnsRecordAAAA(CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1));

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_AAAA_NotEquals()
        {
            var recordAAAA = new DnsRecordAAAA(CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1);
            recordAAAA.Address.Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1);
            recordAAAA.HostName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordAAAA);
            recordAAAA.HostName.Should().Be(DefaultServiceHostDomainName1);

            var recordAAAA2 = new DnsRecordAAAA(CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address2);
            recordAAAA2.Address.Should().Be(CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address2);
            recordAAAA2.HostName.Should().BeNull();
            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordAAAA2);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_NS_Equals()
        {
            var recordNS1 = new DnsRecordNS(DefaultServiceHostDomainName1);
            recordNS1.NameServerHostName.Should().Be(DefaultServiceHostDomainName1);
            recordNS1.AuthoritativeDomain.Should().BeNull();

            AuthorityDnsResponseRecord authorityDnsResponseRecord1 = CreateAuthorityDnsResponseRecord(recordNS1, DefaultAuthoritativeDomain);
            recordNS1.AuthoritativeDomain.Should().Be(DefaultAuthoritativeDomain);

            var recordNS2 = new DnsRecordNS(DefaultServiceHostDomainName1);
            recordNS2.NameServerHostName.Should().Be(DefaultServiceHostDomainName1);
            recordNS2.AuthoritativeDomain.Should().BeNull();

            AuthorityDnsResponseRecord authorityDnsResponseRecord2 = CreateAuthorityDnsResponseRecord(recordNS2, DefaultAuthoritativeDomain);

            this.TestOutputHelper.WriteLine(authorityDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{authorityDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            authorityDnsResponseRecord2.Equals(authorityDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_NS_NotEquals()
        {
            var recordNS1 = new DnsRecordNS(DefaultServiceHostDomainName1);
            recordNS1.NameServerHostName.Should().Be(DefaultServiceHostDomainName1);
            recordNS1.AuthoritativeDomain.Should().BeNull();

            AuthorityDnsResponseRecord authorityDnsResponseRecord1 = CreateAuthorityDnsResponseRecord(recordNS1, DefaultAuthoritativeDomain);
            recordNS1.AuthoritativeDomain.Should().Be(DefaultAuthoritativeDomain);

            var recordNS2 = new DnsRecordNS(DefaultServiceHostDomainName2);
            recordNS2.NameServerHostName.Should().Be(DefaultServiceHostDomainName2);
            recordNS2.AuthoritativeDomain.Should().BeNull();

            AuthorityDnsResponseRecord authorityDnsResponseRecord2 = CreateAuthorityDnsResponseRecord(recordNS2, DefaultAuthoritativeDomain);

            this.TestOutputHelper.WriteLine(authorityDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{authorityDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            authorityDnsResponseRecord2.Equals(authorityDnsResponseRecord1).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_NSEC_Equals()
        {
            var recordNSEC1 = new DnsRecordNSEC(DefaultServiceInstance1, [DnsRecordType.TXT, DnsRecordType.SRV]);
            recordNSEC1.NextDomainName.Should().Be(DefaultServiceInstance1);
            recordNSEC1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordNSEC1, DefaultServiceInstance1);
            recordNSEC1.OwnerDomainName.Should().Be(DefaultServiceInstance1);

            var recordNSEC2 = new DnsRecordNSEC(DefaultServiceInstance1, [DnsRecordType.SRV, DnsRecordType.TXT]);
            recordNSEC2.NextDomainName.Should().Be(DefaultServiceInstance1);
            recordNSEC2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordNSEC2, DefaultServiceInstance1);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_NSEC_NotEquals()
        {
            var recordNSEC1 = new DnsRecordNSEC(DefaultServiceInstance1, [DnsRecordType.TXT, DnsRecordType.SRV]);
            recordNSEC1.NextDomainName.Should().Be(DefaultServiceInstance1);
            recordNSEC1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordNSEC1, DefaultServiceInstance1);
            recordNSEC1.OwnerDomainName.Should().Be(DefaultServiceInstance1);

            var recordNSEC2 = new DnsRecordNSEC(DefaultServiceInstance2, [DnsRecordType.TXT, DnsRecordType.SRV]);
            recordNSEC2.NextDomainName.Should().Be(DefaultServiceInstance2);
            recordNSEC2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordNSEC2, DefaultServiceInstance1);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeFalse();

            AdditionalDnsResponseRecord additionalDnsResponseRecord3 = CreateAdditionalDnsResponseRecord(new DnsRecordNSEC(DefaultServiceInstance1, [DnsRecordType.A, DnsRecordType.AAAA]), DefaultServiceInstance1);
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord3.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord3.Equals(additionalDnsResponseRecord1).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_Opt_Equals()
        {
            var optionItems = new HashSet<DnsRecordOpt.DnsOptionItem>()
            {
                new(DnsOptionCode.EdnsOwner, [0, 0, 138, 220, 171, 77, 173, 233, 244, 212, 136, 107, 40, 114]),
            }.ToImmutableHashSet();
            var recordOpt1 = new DnsRecordOpt(optionItems);

            recordOpt1.OptionItems.SequenceEqual(optionItems).Should().BeTrue();
            recordOpt1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordOpt1, DefaultRootDomain);
            recordOpt1.OwnerDomainName.Should().Be(DefaultRootDomain);

            var recordOpt2 = new DnsRecordOpt(optionItems);
            recordOpt1.OptionItems.SequenceEqual(optionItems).Should().BeTrue();
            recordOpt2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordOpt2, DefaultRootDomain);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_Opt_NotEquals()
        {
            // Wake up on Lan
            var optionItems1 = new HashSet<DnsRecordOpt.DnsOptionItem>()
            {
                new(DnsOptionCode.EdnsOwner, [0, 0, 138, 220, 171, 77, 173, 233, 244, 212, 136, 107, 40, 114]),
            }.ToImmutableHashSet();
            var recordOpt1 = new DnsRecordOpt(optionItems1);

            recordOpt1.OptionItems.SequenceEqual(optionItems1).Should().BeTrue();
            recordOpt1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordOpt1, DefaultRootDomain);
            recordOpt1.OwnerDomainName.Should().Be(DefaultRootDomain);

            // Wake up on Lan. Changed the physical address to 112.
            var optionItems2 = new HashSet<DnsRecordOpt.DnsOptionItem>()
            {
                new(DnsOptionCode.EdnsOwner, [0, 0, 138, 220, 171, 77, 173, 233, 244, 212, 136, 107, 40, 112]),
            }.ToImmutableHashSet();

            var recordOpt2 = new DnsRecordOpt(optionItems2);
            recordOpt2.OptionItems.SequenceEqual(optionItems2).Should().BeTrue();
            recordOpt2.OptionItems.SequenceEqual(optionItems1).Should().BeFalse();
            recordOpt2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordOpt2, DefaultRootDomain);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_Opt_WakeUpOnLan()
        {
            DnsRecordOpt.DnsOptionItem optionItem = new(DnsOptionCode.EdnsOwner, [0, 0, 138, 220, 171, 77, 173, 233, 244, 212, 136, 107, 40, 114]);

            var dnsWakeUpOnLan = new DnsWakeUpOnLan(optionItem.OptionData, DnsRecordOpt.DefaultSenderUdpPayloadSize, null);

            this.TestOutputHelper.WriteLine(dnsWakeUpOnLan.ToStringWithPropNameMultiLine());

            dnsWakeUpOnLan.PrimaryPhysicalAddress.Should().Be(PhysicalAddress.Parse("8A-DC-AB-4D-AD-E9"));
            dnsWakeUpOnLan.WakeupPhysicalAddress.Should().Be(PhysicalAddress.Parse("F4-D4-88-6B-28-72"));
            dnsWakeUpOnLan.Password.Should().BeNull();
            dnsWakeUpOnLan.SenderUdpPayloadSize.Should().Be(DnsRecordOpt.DefaultSenderUdpPayloadSize);
            dnsWakeUpOnLan.Sequence.Should().Be(0);
            dnsWakeUpOnLan.Version.Should().Be(0);
        }

        [Fact]
        public void DnsRecord_Ptr_Equals()
        {
            var recordPtr1 = new DnsRecordPtr(DefaultServiceName1);
            recordPtr1.PtrDomainName.Should().Be(DefaultServiceName1);
            recordPtr1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordPtr1, CoreDnsConstants.DnsBrowseQueryLocalDomain);
            recordPtr1.OwnerDomainName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);

            var recordPtr2 = new DnsRecordPtr(DefaultServiceName1);
            recordPtr2.PtrDomainName.Should().Be(DefaultServiceName1);
            recordPtr2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordPtr2, CoreDnsConstants.DnsBrowseQueryLocalDomain);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_Ptr_NotEquals()
        {
            var recordPtr1 = new DnsRecordPtr(DefaultServiceName1);
            recordPtr1.PtrDomainName.Should().Be(DefaultServiceName1);
            recordPtr1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordPtr1, CoreDnsConstants.DnsBrowseQueryLocalDomain);
            recordPtr1.OwnerDomainName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);

            var recordPtr2 = new DnsRecordPtr(DefaultServiceName2);
            recordPtr2.PtrDomainName.Should().Be(DefaultServiceName2);
            recordPtr2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordPtr2, CoreDnsConstants.DnsBrowseQueryLocalDomain);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_Srv_Equals()
        {
            var recordSrv1 = new DnsRecordSrv(1, 2, 3, DefaultServiceHostDomainName1);
            recordSrv1.Priority.Should().Be(1);
            recordSrv1.Weight.Should().Be(2);
            recordSrv1.Port.Should().Be(3);
            recordSrv1.TargetDomain.Should().Be(DefaultServiceHostDomainName1);
            recordSrv1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordSrv1, DefaultServiceInstance1);
            recordSrv1.OwnerDomainName.Should().Be(DefaultServiceInstance1);

            var recordSrv2 = new DnsRecordSrv(1, 2, 3, DefaultServiceHostDomainName1);
            recordSrv2.TargetDomain.Should().Be(DefaultServiceHostDomainName1);
            recordSrv2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordSrv2, DefaultServiceInstance1);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_Srv_NotEquals()
        {
            var recordSrv1 = new DnsRecordSrv(1, 2, 3, DefaultServiceHostDomainName1);
            recordSrv1.Priority.Should().Be(1);
            recordSrv1.Weight.Should().Be(2);
            recordSrv1.Port.Should().Be(3);
            recordSrv1.TargetDomain.Should().Be(DefaultServiceHostDomainName1);
            recordSrv1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordSrv1, DefaultServiceInstance1);
            recordSrv1.OwnerDomainName.Should().Be(DefaultServiceInstance1);

            var recordSrv2 = new DnsRecordSrv(1, 2, 3, DefaultServiceHostDomainName2);
            recordSrv2.TargetDomain.Should().Be(DefaultServiceHostDomainName2);
            recordSrv2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordSrv2, DefaultServiceInstance1);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_Txt_Equals()
        {
            ImmutableSortedSet<string> txtRecords = "mf=Elgato,dt=53,id=3C:6A:9D:12:FE:4A,md=Elgato Key Light 20GAK9901,pv=1.0".Split(',').ToImmutableSortedSet(StringComparer.InvariantCultureIgnoreCase);

            var recordTxt1 = new DnsRecordTxt(txtRecords);

            recordTxt1.TxtRecords.SequenceEqual(txtRecords).Should().BeTrue();
            recordTxt1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordTxt1, DefaultServiceInstance1);
            recordTxt1.OwnerDomainName.Should().Be(DefaultServiceInstance1);

            var recordTxt2 = new DnsRecordTxt(txtRecords);
            recordTxt2.TxtRecords.SequenceEqual(txtRecords).Should().BeTrue();
            recordTxt2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordTxt2, DefaultServiceInstance1);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_Txt_NotEquals()
        {
            ImmutableSortedSet<string> txtRecords1 = "mf=Elgato,dt=53,id=3C:6A:9D:12:FE:4A,md=Elgato Key Light 20GAK9901,pv=1.0".Split(',').ToImmutableSortedSet(StringComparer.InvariantCultureIgnoreCase);

            var recordTxt1 = new DnsRecordTxt(txtRecords1);

            recordTxt1.TxtRecords.SequenceEqual(txtRecords1).Should().BeTrue();
            recordTxt1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordTxt1, DefaultServiceInstance1);
            recordTxt1.OwnerDomainName.Should().Be(DefaultServiceInstance1);

            var recordTxt2 = new DnsRecordTxt(txtRecords1);
            recordTxt2.TxtRecords.SequenceEqual(txtRecords1).Should().BeTrue();
            recordTxt2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordTxt2, DefaultServiceInstance2);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeFalse();

            ImmutableSortedSet<string> txtRecords2 = "mf=Elgato,dt=53,md=Elgato Key Light 20GAK9901,pv=1.0".Split(',').ToImmutableSortedSet(StringComparer.InvariantCultureIgnoreCase);
            AdditionalDnsResponseRecord additionalDnsResponseRecord3 = CreateAdditionalDnsResponseRecord(new DnsRecordTxt(txtRecords2), DefaultServiceInstance1);
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord3.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord1.Equals(additionalDnsResponseRecord3).Should().BeFalse();
        }

        [Fact]
        public void DnsRecord_Unknown_Equals()
        {
            byte[] unknownData1 = [1, 2, 3];

            var recordUnknown1 = new DnsRecordUnknown(unknownData1);

            recordUnknown1.RecordData.SequenceEqual(unknownData1).Should().BeTrue();
            recordUnknown1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordUnknown1, DefaultServiceInstance1);
            recordUnknown1.OwnerDomainName.Should().Be(DefaultServiceInstance1);

            var recordUnknown2 = new DnsRecordUnknown(unknownData1);
            recordUnknown2.RecordData.SequenceEqual(unknownData1).Should().BeTrue();
            recordUnknown2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordUnknown2, DefaultServiceInstance1);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeTrue();
        }

        [Fact]
        public void DnsRecord_Unknown_NotEquals()
        {
            byte[] unknownData1 = [1, 2, 3];

            var recordUnknown1 = new DnsRecordUnknown(unknownData1);

            recordUnknown1.RecordData.SequenceEqual(unknownData1).Should().BeTrue();
            recordUnknown1.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord1 = CreateAdditionalDnsResponseRecord(recordUnknown1, DefaultServiceInstance1);
            recordUnknown1.OwnerDomainName.Should().Be(DefaultServiceInstance1);

            byte[] unknownData2 = [3, 2, 1];

            var recordUnknown2 = new DnsRecordUnknown(unknownData2);
            recordUnknown2.RecordData.SequenceEqual(unknownData2).Should().BeTrue();
            recordUnknown2.OwnerDomainName.Should().BeNull();

            AdditionalDnsResponseRecord additionalDnsResponseRecord2 = CreateAdditionalDnsResponseRecord(recordUnknown2, DefaultServiceInstance1);

            this.TestOutputHelper.WriteLine(additionalDnsResponseRecord1.ToStringWithPropNameMultiLine());
            this.TestOutputHelper.WriteLine($"\n{additionalDnsResponseRecord2.ToStringWithPropNameMultiLine()}");
            additionalDnsResponseRecord2.Equals(additionalDnsResponseRecord1).Should().BeFalse();
        }

        private static AdditionalDnsResponseRecord CreateAdditionalDnsResponseRecord(DnsRecordBase dnsRecordBase, string? ownerName = null, DnsRecordClass dnsRecordClass = DnsRecordClass.IN, uint timeToLive = 4500, DateTimeOffset? dateTimeOffset = null)
        {
            ownerName ??= DefaultServiceHostDomainName1;
            dateTimeOffset ??= DateTimeOffset.UtcNow;
            return new AdditionalDnsResponseRecord(dnsRecordBase, ownerName, dnsRecordClass, timeToLive, dateTimeOffset.Value);
        }

        private static AuthorityDnsResponseRecord CreateAuthorityDnsResponseRecord(DnsRecordBase dnsRecordBase, string? ownerName = null, DnsRecordClass dnsRecordClass = DnsRecordClass.IN, uint timeToLive = 4500, DateTimeOffset? dateTimeOffset = null)
        {
            ownerName ??= DefaultServiceHostDomainName1;
            dateTimeOffset ??= DateTimeOffset.UtcNow;
            return new AuthorityDnsResponseRecord(dnsRecordBase, ownerName, dnsRecordClass, timeToLive, dateTimeOffset.Value);
        }
    }
}
