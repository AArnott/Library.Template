// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-20-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-20-2020
// // ***********************************************************************
// <copyright file="MulticastDnsUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Collections.Immutable;
using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Networking.Services.MulticastDns.Records;
using NetworkVisor.Core.Networking.Services.MulticastDns.Request;
using NetworkVisor.Core.Networking.Services.MulticastDns.Response;
using NetworkVisor.Core.Networking.Services.MulticastDns.Types;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.MulticastDns
{
    /// <summary>
    /// Class MulticastDnsUnitTests.
    /// </summary>
    [PlatformTrait(typeof(MulticastDnsUnitTests))]

    public class MulticastDnsUnitTests : CoreTestCaseBase
    {
        private static readonly byte[] BufferQuery =
            [0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x09, 0x5f, 0x73, 0x65, 0x72, 0x76, 0x69, 0x63, 0x65, 0x73, 0x07, 0x5f, 0x64, 0x6e, 0x73, 0x2d, 0x73, 0x64, 0x04, 0x5f, 0x75, 0x64, 0x70, 0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x00, 0x00, 0x0c, 0x00, 0x01,];

        private static readonly byte[] BufferTwoPtrAnswers =
            [0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00, 0x09, 0x5f, 0x73, 0x65, 0x72, 0x76, 0x69, 0x63, 0x65, 0x73, 0x07, 0x5f, 0x64, 0x6e, 0x73, 0x2d, 0x73, 0x64, 0x04, 0x5f, 0x75, 0x64, 0x70, 0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x00, 0x00, 0x0c, 0x00, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x0c, 0x04, 0x5f, 0x68, 0x61, 0x70, 0x04, 0x5f, 0x74, 0x63, 0x70, 0xc0, 0x23, 0xc0, 0x0c, 0x00, 0x0c, 0x00, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x0b, 0x08, 0x5f, 0x61, 0x69, 0x72, 0x70, 0x6c, 0x61, 0x79, 0xc0, 0x39,];

        private static readonly byte[] BufferFivePtrAnswers =
            [0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x05, 0x00, 0x00, 0x00, 0x00, 0x09, 0x5f, 0x73, 0x65, 0x72, 0x76, 0x69, 0x63, 0x65, 0x73, 0x07, 0x5f, 0x64, 0x6e, 0x73, 0x2d, 0x73, 0x64, 0x04, 0x5f, 0x75, 0x64, 0x70, 0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x00, 0x00, 0x0c, 0x00, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x18, 0x10, 0x5f, 0x73, 0x70, 0x6f, 0x74, 0x69, 0x66, 0x79, 0x2d, 0x63, 0x6f, 0x6e, 0x6e, 0x65, 0x63, 0x74, 0x04, 0x5f, 0x74, 0x63, 0x70, 0xc0, 0x23, 0xc0, 0x0c, 0x00, 0x0c, 0x00, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x0e, 0x0b, 0x5f, 0x68, 0x65, 0x6f, 0x73, 0x2d, 0x61, 0x75, 0x64, 0x69, 0x6f, 0xc0, 0x45, 0xc0, 0x0c, 0x00, 0x0c, 0x00, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x08, 0x05, 0x5f, 0x68, 0x74, 0x74, 0x70, 0xc0, 0x45, 0xc0, 0x0c, 0x00, 0x0c, 0x00, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x0b, 0x08, 0x5f, 0x61, 0x69, 0x72, 0x70, 0x6c, 0x61, 0x79, 0xc0, 0x45, 0xc0, 0x0c, 0x00, 0x0c, 0x00, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x08, 0x05, 0x5f, 0x72, 0x61, 0x6f, 0x70, 0xc0, 0x45,];

        private static readonly byte[] BufferAdditionals =
            [0x00, 0x00, 0x84, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00, 0x00, 0x00, 0x06, 0x04, 0x5f, 0x65, 0x6c, 0x67, 0x04, 0x5f, 0x74, 0x63, 0x70, 0x05, 0x6c, 0x6f, 0x63, 0x61, 0x6c, 0x00, 0x00, 0x0c, 0x00, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x18, 0x15, 0x45, 0x6c, 0x67, 0x61, 0x74, 0x6f, 0x20, 0x4b, 0x65, 0x79, 0x20, 0x4c, 0x69, 0x67, 0x68, 0x74, 0x20, 0x36, 0x35, 0x44, 0x33, 0xc0, 0x0c, 0xc0, 0x27, 0x00, 0x21, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x1e, 0x00, 0x00, 0x00, 0x00, 0x23, 0xa3, 0x15, 0x65, 0x6c, 0x67, 0x61, 0x74, 0x6f, 0x2d, 0x6b, 0x65, 0x79, 0x2d, 0x6c, 0x69, 0x67, 0x68, 0x74, 0x2d, 0x36, 0x35, 0x64, 0x33, 0xc0, 0x16, 0xc0, 0x27, 0x00, 0x10, 0x80, 0x01, 0x00, 0x00, 0x11, 0x94, 0x00, 0x4a, 0x09, 0x6d, 0x66, 0x3d, 0x45, 0x6c, 0x67, 0x61, 0x74, 0x6f, 0x05, 0x64, 0x74, 0x3d, 0x35, 0x33, 0x14, 0x69, 0x64, 0x3d, 0x33, 0x43, 0x3a, 0x36, 0x41, 0x3a, 0x39, 0x44, 0x3a, 0x31, 0x32, 0x3a, 0x46, 0x45, 0x3a, 0x34, 0x41, 0x1d, 0x6d, 0x64, 0x3d, 0x45, 0x6c, 0x67, 0x61, 0x74, 0x6f, 0x20, 0x4b, 0x65, 0x79, 0x20, 0x4c, 0x69, 0x67, 0x68, 0x74, 0x20, 0x32, 0x30, 0x47, 0x41, 0x4b, 0x39, 0x39, 0x30, 0x31, 0x06, 0x70, 0x76, 0x3d, 0x31, 0x2e, 0x30, 0xc0, 0x51, 0x00, 0x01, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x04, 0x0a, 0x01, 0x0a, 0xae, 0xc0, 0x51, 0x00, 0x1c, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x10, 0xfe, 0x80, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x3e, 0x6a, 0x9d, 0xff, 0xfe, 0x12, 0xfe, 0x4a, 0xc0, 0x27, 0x00, 0x2f, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x09, 0xc0, 0x27, 0x00, 0x05, 0x00, 0x00, 0x80, 0x00, 0x40, 0xc0, 0x51, 0x00, 0x2f, 0x80, 0x01, 0x00, 0x00, 0x00, 0x78, 0x00, 0x08, 0xc0, 0x51, 0x00, 0x04, 0x40, 0x00, 0x00, 0x08,];

        private static readonly string ElgatoService = "_elg._tcp.local.";
        private static readonly string ElagatoHostDomain = "elgato-key-light-65d3.local.";
        private static readonly string ElgatoServiceInstance = "Elgato Key Light 65D3._elg._tcp.local.";

        /// <summary>
        /// Initializes a new instance of the <see cref="MulticastDnsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public MulticastDnsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void MulticastDns_Response_Query_Read()
        {
            DnsResponse response = new DnsResponse(this.TestCaseServiceProvider, BufferQuery, null, this.TestCaseLogger);

            response.Should().NotBeNull();
            response.IsQueryResponse.Should().BeFalse();
            response.Answers.Count.Should().Be(0);
            response.Authorities.Count.Should().Be(0);
            response.Additionals.Count.Should().Be(0);
            response.Header.DnsOperationCode.Should().Be(DnsOperationCode.Query);

            response.Questions.Count.Should().Be(1);
            response.Questions[0].QuestionName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);
            response.Questions[0].DnsRequestQuestionType.Should().Be(DnsRequestQuestionType.PTR);
            response.Questions[0].DnsRequestQueryClass.Should().Be(DnsRequestQueryClass.IN);

            this.TestOutputHelper.WriteLine(response.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void MulticastDns_Response_Query_Write()
        {
            DnsResponse response = new DnsResponse(this.TestCaseServiceProvider, 0, 0, null, this.TestCaseLogger);
            response.AddQuestion(new DnsRequestQuestion(CoreDnsConstants.DnsBrowseQueryLocalDomain, DnsRequestQuestionType.PTR, DnsRequestQueryClass.IN));
            DnsRecordWriter dnsRecordWriter = new DnsRecordWriter();

            response.Write(dnsRecordWriter);
            dnsRecordWriter.GetData().Should().BeEquivalentTo(BufferQuery);

            this.TestOutputHelper.WriteLine(response.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void MulticastDns_Response_TwoPtrAnswers_Read()
        {
            DnsResponse response = new DnsResponse(this.TestCaseServiceProvider, BufferTwoPtrAnswers, null, this.TestCaseLogger);

            response.Should().NotBeNull();
            response.IsQueryResponse.Should().BeTrue();
            response.Answers.Count.Should().Be(2);
            response.Authorities.Count.Should().Be(0);
            response.Additionals.Count.Should().Be(0);
            response.Header.DnsOperationCode.Should().Be(DnsOperationCode.Query);
            response.Header.DnsResponseCode.Should().Be(DnsResponseCode.NoError);
            response.Header.IsAuthoritativeAnswer.Should().BeTrue();

            response.Questions.Count.Should().Be(0);
            response.ResponseRecords.Count.Should().Be(2);
            response.Answers[0].OwnerDomainName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);
            response.Answers[0].DnsRecordClass.Should().Be(DnsRecordClass.IN);
            response.Answers[0].DnsRecordType.Should().Be(DnsRecordType.PTR);
            response.ResponseRecords[0].DnsRecordBase.Should().BeAssignableTo<DnsRecordPtr>();
            DnsRecordPtr responsePtrRecord = response.ResponseRecords[0].DnsRecordBase.As<DnsRecordPtr>();
            responsePtrRecord.Should().NotBeNull();
            responsePtrRecord.PtrDomainName.Should().Be("_hap._tcp.local.");

            response.Answers[1].OwnerDomainName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);
            response.Answers[1].DnsRecordClass.Should().Be(DnsRecordClass.IN);
            response.Answers[1].DnsRecordType.Should().Be(DnsRecordType.PTR);
            response.ResponseRecords[1].DnsRecordBase.Should().BeAssignableTo<DnsRecordPtr>();
            responsePtrRecord = response.ResponseRecords[1].DnsRecordBase.As<DnsRecordPtr>();
            responsePtrRecord.Should().NotBeNull();
            responsePtrRecord.PtrDomainName.Should().Be("_airplay._tcp.local.");

            this.TestOutputHelper.WriteLine(response.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void MulticastDns_Response_TwoPtrAnswers_Write()
        {
            DnsResponse dnsResponseWrite = new DnsResponse(this.TestCaseServiceProvider, 0, 0, null, this.TestCaseLogger);

            dnsResponseWrite.Header.IsQueryResponse = true;
            dnsResponseWrite.Header.DnsOperationCode = DnsOperationCode.Query;
            dnsResponseWrite.Header.IsAuthoritativeAnswer = true;

            dnsResponseWrite.AddAnswer(new AnswerDnsResponseRecord(new DnsRecordPtr("_hap._tcp.local."), CoreDnsConstants.DnsBrowseQueryLocalDomain, DnsRecordClass.IN, 4500, DateTimeOffset.UtcNow), false);
            dnsResponseWrite.AddAnswer(new AnswerDnsResponseRecord(new DnsRecordPtr("_airplay._tcp.local."), CoreDnsConstants.DnsBrowseQueryLocalDomain, DnsRecordClass.IN, 4500, DateTimeOffset.UtcNow), false);
            dnsResponseWrite.ResetDnsResponse();

            DnsRecordWriter dnsRecordWriter = new DnsRecordWriter();

            dnsResponseWrite.Write(dnsRecordWriter);
            var dnsResponseWriteBuffer = dnsRecordWriter.GetData();

            DnsResponse dnsResponseRead = new DnsResponse(this.TestCaseServiceProvider, dnsResponseWriteBuffer, null, this.TestCaseLogger);

            dnsRecordWriter = new DnsRecordWriter();
            dnsResponseRead.Write(dnsRecordWriter);
            var dnsResponseReadBuffer = dnsRecordWriter.GetData();

            // Fix TTLs to be the same
            dnsResponseReadBuffer[49] = dnsResponseWriteBuffer[49];
            dnsResponseReadBuffer[73] = dnsResponseWriteBuffer[73];

            dnsResponseReadBuffer.Should().BeEquivalentTo(dnsResponseWriteBuffer);

            this.TestOutputHelper.WriteLine(dnsResponseWrite.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void MulticastDns_Response_FivePtrAnswers_Read()
        {
            DnsResponse response = new DnsResponse(this.TestCaseServiceProvider, BufferFivePtrAnswers, null, this.TestCaseLogger);

            response.Should().NotBeNull();

            response.IsQueryResponse.Should().BeTrue();
            response.Answers.Count.Should().Be(5);
            response.Authorities.Count.Should().Be(0);
            response.Additionals.Count.Should().Be(0);
            response.Header.DnsOperationCode.Should().Be(DnsOperationCode.Query);
            response.Header.DnsResponseCode.Should().Be(DnsResponseCode.NoError);
            response.Header.IsAuthoritativeAnswer.Should().BeTrue();

            response.Questions.Count.Should().Be(0);
            response.ResponseRecords.Count.Should().Be(5);
            response.Answers[0].OwnerDomainName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);
            response.Answers[0].DnsRecordClass.Should().Be(DnsRecordClass.IN);
            response.Answers[0].DnsRecordType.Should().Be(DnsRecordType.PTR);
            response.ResponseRecords[0].DnsRecordBase.Should().BeAssignableTo<DnsRecordPtr>();
            DnsRecordPtr responsePtrRecord = response.ResponseRecords[0].DnsRecordBase.As<DnsRecordPtr>();
            responsePtrRecord.Should().NotBeNull();
            responsePtrRecord.PtrDomainName.Should().Be("_spotify-connect._tcp.local.");

            response.Answers[1].OwnerDomainName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);
            response.Answers[1].DnsRecordClass.Should().Be(DnsRecordClass.IN);
            response.Answers[1].DnsRecordType.Should().Be(DnsRecordType.PTR);
            response.ResponseRecords[1].DnsRecordBase.Should().BeAssignableTo<DnsRecordPtr>();
            responsePtrRecord = response.ResponseRecords[1].DnsRecordBase.As<DnsRecordPtr>();
            responsePtrRecord.Should().NotBeNull();
            responsePtrRecord.PtrDomainName.Should().Be("_heos-audio._tcp.local.");

            response.Answers[2].OwnerDomainName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);
            response.Answers[2].DnsRecordClass.Should().Be(DnsRecordClass.IN);
            response.Answers[2].DnsRecordType.Should().Be(DnsRecordType.PTR);
            response.ResponseRecords[2].DnsRecordBase.Should().BeAssignableTo<DnsRecordPtr>();
            responsePtrRecord = response.ResponseRecords[2].DnsRecordBase.As<DnsRecordPtr>();
            responsePtrRecord.Should().NotBeNull();
            responsePtrRecord.PtrDomainName.Should().Be("_http._tcp.local.");

            response.Answers[3].OwnerDomainName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);
            response.Answers[3].DnsRecordClass.Should().Be(DnsRecordClass.IN);
            response.Answers[3].DnsRecordType.Should().Be(DnsRecordType.PTR);
            response.ResponseRecords[3].DnsRecordBase.Should().BeAssignableTo<DnsRecordPtr>();
            responsePtrRecord = response.ResponseRecords[3].DnsRecordBase.As<DnsRecordPtr>();
            responsePtrRecord.Should().NotBeNull();
            responsePtrRecord.PtrDomainName.Should().Be("_airplay._tcp.local.");

            response.Answers[4].OwnerDomainName.Should().Be(CoreDnsConstants.DnsBrowseQueryLocalDomain);
            response.Answers[4].DnsRecordClass.Should().Be(DnsRecordClass.IN);
            response.Answers[4].DnsRecordType.Should().Be(DnsRecordType.PTR);
            response.ResponseRecords[4].DnsRecordBase.Should().BeAssignableTo<DnsRecordPtr>();
            responsePtrRecord = response.ResponseRecords[4].DnsRecordBase.As<DnsRecordPtr>();
            responsePtrRecord.Should().NotBeNull();
            responsePtrRecord.PtrDomainName.Should().Be("_raop._tcp.local.");

            this.TestOutputHelper.WriteLine(response.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void MulticastDns_Response_Additionals_Read()
        {
            DnsResponse response = new DnsResponse(this.TestCaseServiceProvider, BufferAdditionals, null, this.TestCaseLogger);

            response.Should().NotBeNull();

            response.IsQueryResponse.Should().BeTrue();
            response.Answers.Count.Should().Be(1);
            response.Authorities.Count.Should().Be(0);
            response.Additionals.Count.Should().Be(6);
            response.Header.DnsOperationCode.Should().Be(DnsOperationCode.Query);
            response.Header.DnsResponseCode.Should().Be(DnsResponseCode.NoError);
            response.Header.IsAuthoritativeAnswer.Should().BeTrue();

            response.Questions.Count.Should().Be(0);
            response.ResponseRecords.Count.Should().Be(7);
            response.Answers[0].OwnerDomainName.Should().Be(ElgatoService);
            response.Answers[0].DnsRecordClass.Should().Be(DnsRecordClass.IN);
            response.Answers[0].DnsRecordType.Should().Be(DnsRecordType.PTR);
            response.ResponseRecords[0].DnsRecordBase.Should().BeAssignableTo<DnsRecordPtr>();
            DnsRecordPtr responsePtrRecord = response.ResponseRecords[0].DnsRecordBase.As<DnsRecordPtr>();
            responsePtrRecord.Should().NotBeNull();
            responsePtrRecord.PtrDomainName.Should().Be(ElgatoServiceInstance);

            response.ResponseRecords[1].OwnerDomainName.Should().Be(ElgatoServiceInstance);
            response.ResponseRecords[1].DnsRecordClass.Should().Be(DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH);
            response.ResponseRecords[1].DnsRecordType.Should().Be(DnsRecordType.SRV);
            response.ResponseRecords[1].DnsRecordBase.Should().BeAssignableTo<DnsRecordSrv>();
            DnsRecordSrv responseSrvRecord = response.ResponseRecords[1].DnsRecordBase.As<DnsRecordSrv>();
            responseSrvRecord.Should().NotBeNull();
            responseSrvRecord.TargetDomain.Should().Be(ElagatoHostDomain);
            responseSrvRecord.Port.Should().Be(9123);

            response.ResponseRecords[2].OwnerDomainName.Should().Be(ElgatoServiceInstance);
            response.ResponseRecords[2].DnsRecordClass.Should().Be(DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH);
            response.ResponseRecords[2].DnsRecordType.Should().Be(DnsRecordType.TXT);
            response.ResponseRecords[2].DnsRecordBase.Should().BeAssignableTo<DnsRecordTxt>();
            DnsRecordTxt responseTxtRecord = response.ResponseRecords[2].DnsRecordBase.As<DnsRecordTxt>();
            responseTxtRecord.Should().NotBeNull();
            responseTxtRecord.TxtRecords.Should().HaveCount(5);

            response.ResponseRecords[3].OwnerDomainName.Should().Be(ElagatoHostDomain);
            response.ResponseRecords[3].DnsRecordClass.Should().Be(DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH);
            response.ResponseRecords[3].DnsRecordType.Should().Be(DnsRecordType.A);
            response.ResponseRecords[3].DnsRecordBase.Should().BeAssignableTo<DnsRecordA>();
            DnsRecordA responseARecord = response.ResponseRecords[3].DnsRecordBase.As<DnsRecordA>();
            responseARecord.Should().NotBeNull();
            responseARecord.Address.ToString().Should().Be("10.1.10.174");

            response.ResponseRecords[4].OwnerDomainName.Should().Be(ElagatoHostDomain);
            response.ResponseRecords[4].DnsRecordClass.Should().Be(DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH);
            response.ResponseRecords[4].DnsRecordType.Should().Be(DnsRecordType.AAAA);
            response.ResponseRecords[4].DnsRecordBase.Should().BeAssignableTo<DnsRecordAAAA>();
            DnsRecordAAAA responseAaaaRecord = response.ResponseRecords[4].DnsRecordBase.As<DnsRecordAAAA>();
            responseAaaaRecord.Should().NotBeNull();
            responseAaaaRecord.Address.ToString().Should().Be("fe80::3e6a:9dff:fe12:fe4a");

            response.ResponseRecords[5].OwnerDomainName.Should().Be(ElgatoServiceInstance);
            response.ResponseRecords[5].DnsRecordClass.Should().Be(DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH);
            response.ResponseRecords[5].DnsRecordType.Should().Be(DnsRecordType.NSEC);
            response.ResponseRecords[5].DnsRecordBase.Should().BeAssignableTo<DnsRecordNSEC>();
            DnsRecordNSEC responseNSecRecord = response.ResponseRecords[5].DnsRecordBase.As<DnsRecordNSEC>();
            responseNSecRecord.Should().NotBeNull();

            response.ResponseRecords[6].OwnerDomainName.Should().Be(ElagatoHostDomain);
            response.ResponseRecords[6].DnsRecordClass.Should().Be(DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH);
            response.ResponseRecords[6].DnsRecordType.Should().Be(DnsRecordType.NSEC);
            response.ResponseRecords[6].DnsRecordBase.Should().BeAssignableTo<DnsRecordNSEC>();
            responseNSecRecord = response.ResponseRecords[6].DnsRecordBase.As<DnsRecordNSEC>();
            responseNSecRecord.Should().NotBeNull();

            this.TestOutputHelper.WriteLine(response.ToStringWithParentsPropNameMultiLine());
        }

        [Fact]
        public void MulticastDns_Response_Additionals_ReadWrite()
        {
            DnsResponse dnsResponseReader = new DnsResponse(this.TestCaseServiceProvider, BufferAdditionals, null, this.TestCaseLogger);

            DnsRecordWriter dnsRecordWriter = new DnsRecordWriter();
            dnsResponseReader.Write(dnsRecordWriter);
            var dnsWriterData = dnsRecordWriter.GetData();

            DnsResponse dnsResponseWriter = new DnsResponse(this.TestCaseServiceProvider, dnsWriterData, null, this.TestCaseLogger);

            dnsResponseWriter.Equals(dnsResponseReader).Should().BeTrue();
        }

        [Fact]
        public void MulticastDns_Response_Additionals_ReadWriteRead()
        {
            DnsResponse dnsResponseTest = new DnsResponse(this.TestCaseServiceProvider, BufferAdditionals, null, this.TestCaseLogger);

            DnsRecordWriter dnsRecordWriter = new DnsRecordWriter();
            dnsResponseTest.Write(dnsRecordWriter);

            DnsResponse dnsResponseWrite = new DnsResponse(this.TestCaseServiceProvider, dnsRecordWriter.GetData(), null, this.TestCaseLogger);
            dnsResponseWrite.Equals(dnsResponseTest).Should().BeTrue();

            // dnsRecordWriter.GetData().Should().BeEquivalentTo(BufferAdditionals);
            DnsResponse dnsResponseNew = new DnsResponse(this.TestCaseServiceProvider, 0, 0, null, this.TestCaseLogger);

            dnsResponseNew.Header.IsQueryResponse = true;
            dnsResponseNew.Header.DnsOperationCode = DnsOperationCode.Query;
            dnsResponseNew.Header.IsAuthoritativeAnswer = true;

            dnsResponseNew.AddAnswer(new AnswerDnsResponseRecord(new DnsRecordPtr(ElgatoServiceInstance), ElgatoService, DnsRecordClass.IN, 4500, DateTimeOffset.UtcNow), false);
            dnsResponseNew.AddAdditional(new AdditionalDnsResponseRecord(new DnsRecordSrv(0, 0, 9123, ElagatoHostDomain), ElgatoServiceInstance, DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH, 4500, DateTimeOffset.UtcNow), false);
            dnsResponseNew.AddAdditional(new AdditionalDnsResponseRecord(new DnsRecordTxt(new HashSet<string>(["mf=Elgato", "dt=53", "id=3C:6A:9D:12:FE:4A", "md=Elgato Key Light 20GAK9901", "pv=1.0"]).ToImmutableHashSet()), ElgatoServiceInstance, DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH, 4500, DateTimeOffset.UtcNow), false);
            dnsResponseNew.AddAdditional(new AdditionalDnsResponseRecord(new DnsRecordA(IPAddress.Parse("10.1.10.174")), ElagatoHostDomain, DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH, 120, DateTimeOffset.UtcNow), false);
            dnsResponseNew.AddAdditional(new AdditionalDnsResponseRecord(new DnsRecordAAAA(IPAddress.Parse("fe80::3e6a:9dff:fe12:fe4a")), ElagatoHostDomain, DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH, 120, DateTimeOffset.UtcNow), false);
            dnsResponseNew.AddAdditional(new AdditionalDnsResponseRecord(new DnsRecordNSEC(ElgatoServiceInstance, [DnsRecordType.TXT, DnsRecordType.SRV]), ElgatoServiceInstance, DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH, 120, DateTimeOffset.UtcNow), false);
            dnsResponseNew.AddAdditional(new AdditionalDnsResponseRecord(new DnsRecordNSEC(ElagatoHostDomain, [DnsRecordType.A, DnsRecordType.AAAA]), ElagatoHostDomain, DnsRecordClass.IN | DnsRecordClass.CACHEFLUSH, 120, DateTimeOffset.UtcNow), false);

            dnsResponseNew.ResetDnsResponse();

            this.TestOutputHelper.WriteLine(dnsResponseNew.ToStringWithParentsPropNameMultiLine());

            dnsRecordWriter = new DnsRecordWriter();

            dnsResponseNew.Write(dnsRecordWriter);

            DnsResponse dnsResponseRead = new DnsResponse(this.TestCaseServiceProvider, dnsRecordWriter.GetData(), null, this.TestCaseLogger);
            dnsResponseRead.Equals(dnsResponseTest).Should().BeTrue();
        }
    }
}
