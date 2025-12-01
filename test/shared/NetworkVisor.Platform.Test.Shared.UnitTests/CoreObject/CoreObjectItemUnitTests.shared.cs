// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreObjectItemUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.CoreObject
{
    /// <summary>
    /// Class CoreObjectItemUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreObjectItemUnitTests))]

    public class CoreObjectItemUnitTests : CoreTestCaseBase
    {
        private const string TestProp = "Test";
        private const string TestStringValue1 = "Value1";
        private const string TestStringValue2 = "Value2";
        private const string IPAddressProp = "IPAddress";
        private const string PhysicalAddressProp = "PhysicalAddress";

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreObjectItemUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreObjectItemUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectItem_Ctor()
        {
            this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void CoreObjectItem_Initial()
        {
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, TestStringValue1, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.StringValue);

            objectItem.PropName.Should().Be(TestProp);
            objectItem.PropValue.Should().Be(TestStringValue1);
            objectItem.ObjectItemProvider.Should().Be(CoreObjectItemProvider.Test);
            objectItem.PropConfidenceScore.Should().Be(CorePropConfidenceScore.High);
            objectItem.LogPropertyType.Should().Be(CoreLogPropertyType.StringValue);
        }

        [Theory]
        [InlineData(null, CorePropConfidenceScore.None)]
        [InlineData("", CorePropConfidenceScore.LowMed)]
        [InlineData(" ", CorePropConfidenceScore.LowMed)]
        [InlineData(TestStringValue1, CorePropConfidenceScore.High)]
        public void CoreObjectItem_CalculatePropConfidenceScore_String(string? testValue, CorePropConfidenceScore propConfidenceScoreExpected)
        {
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.StringValue);

            objectItem.PropName.Should().Be(TestProp);
            objectItem.PropValue.Should().Be(testValue);
            objectItem.ObjectItemProvider.Should().Be(CoreObjectItemProvider.Test);
            objectItem.PropConfidenceScore.Should().Be(CoreObjectItem.CalculatePropConfidenceScore(CorePropID.TestProp, CoreObjectItemProvider.Test, true, propConfidenceScoreExpected));
            objectItem.LogPropertyType.Should().Be(CoreLogPropertyType.StringValue);
        }

        [Theory]
        [InlineData(null, CorePropConfidenceScore.None)]
        [InlineData(CorePropConfidenceScore.None, CorePropConfidenceScore.LowMed)]
        [InlineData(CorePropConfidenceScore.Low, CorePropConfidenceScore.MedHigh)]
        public void CoreObjectItem_CalculatePropConfidenceScore_Enum(Enum? testValue, CorePropConfidenceScore propConfidenceScoreExpected)
        {
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.StringValue);

            objectItem.PropName.Should().Be(TestProp);
            objectItem.PropValue.Should().Be(testValue);
            objectItem.ObjectItemProvider.Should().Be(CoreObjectItemProvider.Test);
            objectItem.PropConfidenceScore.Should().Be(CoreObjectItem.CalculatePropConfidenceScore(CorePropID.TestProp, CoreObjectItemProvider.Test, true, propConfidenceScoreExpected));
            objectItem.LogPropertyType.Should().Be(CoreLogPropertyType.StringValue);
        }

        [Theory]
        [InlineData(null, CoreIPAddressScore.None)]
        [InlineData(CoreIPAddressExtensions.StringBroadcast, CoreIPAddressScore.None)]
        [InlineData(CoreIPAddressExtensions.StringAny, CoreIPAddressScore.None)]
        [InlineData(CoreIPAddressExtensions.StringLoopback, CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.IPv4)]
        [InlineData("169.254.0.2", CoreIPAddressScore.PrivateNetwork | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.IPv4)]
        [InlineData("10.1.10.2", CoreIPAddressScore.PrivateNetwork | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.IPv4)]
        [InlineData("fd52:1ece:1f8:e4e:9981:8926:7375:9d29", CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.IPv6)]
        [InlineData("fe80::b97d:3662:18bf:fcc3%22", CoreIPAddressScore.LinkLocal | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.IPv6)]
        public void CoreObjectItem_CalculatePropConfidenceScore_IPAddress(string? testValue, CoreIPAddressScore ipAddressScore)
        {
            IPAddress? iPAddress = testValue is null ? null : IPAddress.Parse(testValue);

            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, iPAddress, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, iPAddress, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.IPAddress);

            objectItem.PropName.Should().Be(TestProp);
            objectItem.PropValue.Should().Be(iPAddress);
            objectItem.ObjectItemProvider.Should().Be(CoreObjectItemProvider.Test);
            objectItem.PropConfidenceScore.Should().Be(CoreObjectItem.CalculatePropConfidenceScore(CorePropID.TestProp, CoreObjectItemProvider.Test, true, (CorePropConfidenceScore)ipAddressScore));
            objectItem.LogPropertyType.Should().Be(CoreLogPropertyType.IPAddress);
        }

        [Theory]
        [InlineData(null, CoreIPAddressScore.None)]
        [InlineData(CoreIPAddressExtensions.StringBroadcast, CoreIPAddressScore.None)]
        [InlineData(CoreIPAddressExtensions.StringAny, CoreIPAddressScore.None)]
        [InlineData(CoreIPAddressExtensions.StringLoopback, CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.IPv4)]
        [InlineData("169.254.0.2", CoreIPAddressScore.PrivateNetwork | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.IPv4)]
        [InlineData("10.1.10.2", CoreIPAddressScore.PrivateNetwork | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.IPv4)]
        [InlineData("fd52:1ece:1f8:e4e:9981:8926:7375:9d29", CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.IPv6)]
        [InlineData("fe80::b97d:3662:18bf:fcc3%22", CoreIPAddressScore.LinkLocal | CoreIPAddressScore.NotPrivateAuto | CoreIPAddressScore.NotBroadcast | CoreIPAddressScore.NotLoopback | CoreIPAddressScore.NotMulticast | CoreIPAddressScore.IPv6)]
        public void CoreObjectItem_CalculatePropConfidenceScore_IPAddressSubnet(string? testValue, CoreIPAddressScore ipAddressScore)
        {
            IPAddress? iPAddress = testValue is null ? null : IPAddress.Parse(testValue);
            CoreIPAddressSubnet? ipAddressSubnet = testValue is null ? null : iPAddress.ToIPAddressSubnet(IPAddress.None);

            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, ipAddressSubnet, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, ipAddressSubnet, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.IPAddressSubnet);

            objectItem.PropName.Should().Be(TestProp);
            objectItem.PropValue.Should().Be(ipAddressSubnet);
            objectItem.ObjectItemProvider.Should().Be(CoreObjectItemProvider.Test);
            objectItem.PropConfidenceScore.Should().Be(CoreObjectItem.CalculatePropConfidenceScore(CorePropID.TestProp, CoreObjectItemProvider.Test, true, (CorePropConfidenceScore)ipAddressScore));
            objectItem.LogPropertyType.Should().Be(CoreLogPropertyType.IPAddressSubnet);
        }

        [Theory]
        [InlineData(null, CorePropConfidenceScore.None)]
        [InlineData("", CorePropConfidenceScore.LowMed)]
        [InlineData("Apple", CorePropConfidenceScore.Medium)]
        [InlineData("01", CorePropConfidenceScore.MedHigh)]

        public void CoreObjectItem_CalculatePropConfidenceScore_PhysicalAddress(string? testValue, CorePropConfidenceScore propConfidenceScoreExpected)
        {
            PhysicalAddress? physicalAddress = testValue is null ? null : string.IsNullOrEmpty(testValue) ? PhysicalAddress.None : testValue.Equals("Apple") ? PhysicalAddressExtensions.RestrictedPhysicalAddress : PhysicalAddressExtensions.NormalizedParse(testValue);

            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, physicalAddress, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, physicalAddress, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.PhysicalAddress);

            objectItem.PropName.Should().Be(TestProp);
            objectItem.PropValue.Should().Be(physicalAddress);
            objectItem.ObjectItemProvider.Should().Be(CoreObjectItemProvider.Test);
            objectItem.PropConfidenceScore.Should().Be(CoreObjectItem.CalculatePropConfidenceScore(CorePropID.TestProp, CoreObjectItemProvider.Test, true, propConfidenceScoreExpected));
            objectItem.LogPropertyType.Should().Be(CoreLogPropertyType.PhysicalAddress);
        }

        [Fact]
        public async Task CoreObjectItem_SerializeAsync_String()
        {
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, TestStringValue1, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.StringValue);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);
            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_DeserializeAsync_String()
        {
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, TestStringValue1, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.StringValue);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);

            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            stream.Seek(0, SeekOrigin.Begin);
            ICoreObjectItem? objectItemNew = await JsonSerializer.DeserializeAsync<ICoreObjectItem>(stream, settings);
            objectItemNew.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(objectItemNew.ToStringWithParentsPropNameMultiLine());
            this.TestOutputHelper.WriteLine();
            objectItemNew.Should().Be(objectItem);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_SerializeAsync_PhysicalAddress()
        {
            var physicalAddress = PhysicalAddress.Parse("1E-CE-51-60-5C-BE");
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, physicalAddress, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, physicalAddress, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.PhysicalAddress);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);
            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_DeserializeAsync_PhysicalAddress()
        {
            var physicalAddress = PhysicalAddress.Parse("1E-CE-51-60-5C-BE");
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, physicalAddress, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, physicalAddress, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.PhysicalAddress);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);

            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            stream.Seek(0, SeekOrigin.Begin);
            ICoreObjectItem? objectItemNew = await JsonSerializer.DeserializeAsync<ICoreObjectItem>(stream, settings);
            objectItemNew.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(objectItemNew.ToStringWithParentsPropNameMultiLine());
            this.TestOutputHelper.WriteLine();
            objectItemNew.Should().Be(objectItem);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_SerializeAsync_Bool()
        {
            var testValue = true;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Bool);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);
            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_DeserializeAsync_Bool()
        {
            var testValue = true;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Bool);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);

            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            stream.Seek(0, SeekOrigin.Begin);
            ICoreObjectItem? objectItemNew = await JsonSerializer.DeserializeAsync<ICoreObjectItem>(stream, settings);
            objectItemNew.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(objectItemNew.ToStringWithParentsPropNameMultiLine());
            this.TestOutputHelper.WriteLine();
            objectItemNew.Should().Be(objectItem);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_SerializeAsync_Number_UInt64()
        {
            var testValue = ulong.MaxValue;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);
            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_DeserializeAsync_Number_UInt64()
        {
            var testValue = ulong.MaxValue;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);

            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            stream.Seek(0, SeekOrigin.Begin);
            ICoreObjectItem? objectItemNew = await JsonSerializer.DeserializeAsync<ICoreObjectItem>(stream, settings);
            objectItemNew.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(objectItemNew.ToStringWithParentsPropNameMultiLine());
            this.TestOutputHelper.WriteLine();
            objectItemNew.Should().Be(objectItem);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_SerializeAsync_Number_Int64()
        {
            long testValue = -1;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);
            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_DeserializeAsync_Number_Int64()
        {
            long testValue = -1;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);

            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            stream.Seek(0, SeekOrigin.Begin);
            ICoreObjectItem? objectItemNew = await JsonSerializer.DeserializeAsync<ICoreObjectItem>(stream, settings);
            objectItemNew.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(objectItemNew.ToStringWithParentsPropNameMultiLine());
            this.TestOutputHelper.WriteLine();
            objectItemNew.Should().Be(objectItem);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_SerializeAsync_Number_UInt32()
        {
            var testValue = uint.MaxValue;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);
            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_DeserializeAsync_Number_UInt32()
        {
            var testValue = uint.MaxValue;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);

            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            stream.Seek(0, SeekOrigin.Begin);
            ICoreObjectItem? objectItemNew = await JsonSerializer.DeserializeAsync<ICoreObjectItem>(stream, settings);
            objectItemNew.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(objectItemNew.ToStringWithParentsPropNameMultiLine());
            this.TestOutputHelper.WriteLine();
            objectItemNew.Should().Be(objectItem);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_SerializeAsync_Number_Int32()
        {
            int testValue = -1;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);
            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_DeserializeAsync_Number_Int32()
        {
            int testValue = -1;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);

            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            stream.Seek(0, SeekOrigin.Begin);
            ICoreObjectItem? objectItemNew = await JsonSerializer.DeserializeAsync<ICoreObjectItem>(stream, settings);
            objectItemNew.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(objectItemNew.ToStringWithParentsPropNameMultiLine());
            this.TestOutputHelper.WriteLine();
            objectItemNew.Should().Be(objectItem);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_SerializeAsync_Number_UInt16()
        {
            var testValue = ushort.MaxValue;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);
            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_DeserializeAsync_Number_UInt16()
        {
            var testValue = ushort.MaxValue;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);

            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            stream.Seek(0, SeekOrigin.Begin);
            ICoreObjectItem? objectItemNew = await JsonSerializer.DeserializeAsync<ICoreObjectItem>(stream, settings);
            objectItemNew.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(objectItemNew.ToStringWithParentsPropNameMultiLine());
            this.TestOutputHelper.WriteLine();
            objectItemNew.Should().Be(objectItem);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_SerializeAsync_Number_Int16()
        {
            short testValue = -1;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);
            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        [Fact]
        public async Task CoreObjectItem_DeserializeAsync_Number_Int16()
        {
            short testValue = -1;
            ICoreObjectItem objectItem = new CoreObjectItem(TestProp, testValue, CoreObjectItemProvider.Test, CoreObjectItem.CalculateConfidenceScore(CorePropID.TestProp, testValue, CoreObjectItemProvider.Test, (ICoreObjectItem?)null, true), CoreLogPropertyType.Number);

            using Stream stream = new MemoryStream();
            JsonSerializerOptions settings = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider);

            await JsonSerializer.SerializeAsync(stream, objectItem, settings);

            stream.Seek(0, SeekOrigin.Begin);
            ICoreObjectItem? objectItemNew = await JsonSerializer.DeserializeAsync<ICoreObjectItem>(stream, settings);
            objectItemNew.Should().NotBeNull();
            this.TestOutputHelper.WriteLine(objectItemNew.ToStringWithParentsPropNameMultiLine());
            this.TestOutputHelper.WriteLine();
            objectItemNew.Should().Be(objectItem);

            byte[] buffer = new byte[stream.Length];
            stream.Seek(0, SeekOrigin.Begin);
            long readBytes = await stream.ReadAsync(buffer, 0, buffer.Length);
            readBytes.Should().Be(stream.Length);

            this.TestOutputBufferToPrettyJson(buffer);
        }

        private void TestOutputBufferToPrettyJson(byte[] buffer)
        {
            using JsonDocument jsonDocument = JsonDocument.Parse(Encoding.UTF8.GetString(buffer));

            // Convert the JsonDocument back to a pretty-printed JSON string
            string prettifiedJson = JsonSerializer.Serialize(jsonDocument.RootElement, new
                    JsonSerializerOptions
            {
                WriteIndented = true,
            });

            this.TestOutputHelper.WriteLine($"Stream Json:\n{prettifiedJson}");
        }
    }
}
