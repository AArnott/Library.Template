// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreSerializableObjectUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreObject;
using NetworkVisor.Core.Device;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Startup;
using NetworkVisor.Core.Storage.Property;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Serialization
{
    /// <summary>
    /// Class CoreSerializableObjectUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreSerializableObjectUnitTests))]

    public class CoreSerializableObjectUnitTests : CoreTestCaseBase
    {
        private static readonly string FrameworkInfoType = @"""$type"": ""NetworkVisor.Core.Networking.Devices.CoreNetworkGatewayAddressInfo NetworkVisor.Platform.";
        private static readonly int DefaultIntValue = 1;
        private static readonly string DefaultStringValue = "TestStringValue";

        private static readonly IPAddress DefaultIPv4Address = CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1;
        private static readonly IPAddress DefaultIPv6Address = CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1;

        private static readonly CoreIPEndPoint DefaultIPEndPointV4 = new(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreMulticastDnsConstants.MulticastDnsServerPort);
        private static readonly CoreIPEndPoint DefaultIPEndPointV6 = new(CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1, CoreMulticastDnsConstants.MulticastDnsServerPort);

        private static readonly CoreIPAddressSubnet DefaultIPAddressSubnetV4 = CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC);
        private static readonly CoreIPAddressSubnet DefaultIPAddressSubnetV6 = CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC);

        private static readonly byte[] StorageDocumentBytesProperties = [123, 34, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 77, 97, 106, 111, 114, 34, 58, 49, 44, 34, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 77, 105, 110, 111, 114, 34, 58, 49, 44, 34, 82, 101, 118, 105, 115, 105, 111, 110, 34, 58, 49, 44, 34, 76, 97, 115, 116, 85, 112, 100, 97, 116, 101, 100, 85, 116, 99, 34, 58, 34, 50, 48, 50, 52, 45, 48, 55, 45, 50, 56, 84, 49, 53, 58, 48, 52, 58, 48, 48, 46, 52, 48, 50, 52, 50, 57, 53, 43, 48, 48, 58, 48, 48, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 105, 101, 115, 34, 58, 123, 34, 80, 114, 111, 112, 101, 114, 116, 121, 51, 34, 58, 34, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 121, 50, 34, 58, 34, 86, 97, 108, 117, 101, 50, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 121, 49, 34, 58, 34, 86, 97, 108, 117, 101, 49, 34, 125, 125,];

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSerializableObjectUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSerializableObjectUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            AssertionConfiguration.Current.Formatting.MaxLines = 2000;
            AssertionConfiguration.Current.Formatting.MaxDepth = 200;
        }

        [Fact]
        public void SerializableObject_GetJsonString_Compact()
        {
            var serializableObject = new TestCoreSerializableObject(CoreStartupServices.ServiceProvider);
            _ = serializableObject.Should().NotBeNull();
            serializableObject.IntProperty = 1;
            string? jsonString = serializableObject.GetJsonString<TestCoreSerializableObject>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);
            _ = jsonString.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine(jsonString);
        }

        [Fact]
        public void SerializableObject_GetJsonString_Formatted()
        {
            var serializableObject = new TestCoreSerializableObject(CoreStartupServices.ServiceProvider);
            _ = serializableObject.Should().NotBeNull();
            serializableObject.IntProperty = 1;
            string? jsonString = serializableObject.GetJsonString<TestCoreSerializableObject>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);
            _ = jsonString.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine(jsonString);
        }

        [Fact]
        public async Task SerializableObject_DeserializeJsonStringAsync()
        {
            var serializableObject = new TestCoreSerializableObject(CoreStartupServices.ServiceProvider);
            _ = serializableObject.Should().NotBeNull();
            serializableObject.IntProperty = 1;
            string? jsonString = serializableObject.GetJsonString<TestCoreSerializableObject>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);
            _ = jsonString.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Formatted Before:\n{jsonString}");

            (TestCoreSerializableObject? SerializedObject, Exception? Exception) result = CoreSerializableObject.CreateFromJson<TestCoreSerializableObject>(jsonString, CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider, this.TestCaseLogger);

            _ = result.Should().NotBeNull();
            _ = result.Exception.Should().BeNull();
            _ = result.SerializedObject.Should().NotBeNull();
            _ = result.SerializedObject.Should().Be(serializableObject);

            (string? JsonString, Exception? Exception) resultFormatted = await result.SerializedObject!.GetJsonStringAsync<TestCoreSerializableObject>(CoreSerializationFormatFlags.JsonFormatted);
            _ = resultFormatted.Should().NotBeNull();
            _ = resultFormatted.Exception.Should().BeNull();

            this.TestOutputHelper.WriteLine($"\nFormatted After:\n{resultFormatted.JsonString}");
        }

        [Fact]
        public void SerializableObject_DeserializeJsonString_WrongPlatform()
        {
            var serializableObject = new TestCoreSerializableObject(CoreStartupServices.ServiceProvider);
            _ = serializableObject.Should().NotBeNull();
            serializableObject.IntProperty = 1;
            string? jsonString = serializableObject.GetJsonString<TestCoreSerializableObject>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);
            _ = jsonString.Should().NotBeNullOrEmpty();

            int indexStart = jsonString!.IndexOf(FrameworkInfoType, StringComparison.Ordinal);
            _ = indexStart.Should().NotBe(-1, "FrameworkInfoType not found in jsonString");
            int indexEnd = jsonString.IndexOf("\"", indexStart + FrameworkInfoType.Length, StringComparison.Ordinal);

            string? jsonStringWrongPlatform = $"{jsonString.Substring(0, indexStart + FrameworkInfoType.Length)}Unknown{jsonString.Substring(indexEnd)}";

            this.TestOutputHelper.WriteLine($"WrongPlatform:\n{jsonStringWrongPlatform}\n");

            (TestCoreSerializableObject? SerializedObject, Exception? Exception) result = CoreSerializableObject.CreateFromJson<TestCoreSerializableObject>(jsonStringWrongPlatform, CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider, this.TestCaseLogger);

            _ = result.Should().NotBeNull();
            _ = result.Exception.Should().BeNull();
            _ = result.SerializedObject.Should().NotBeNull();
            _ = result.SerializedObject.Should().Be(serializableObject);

            this.TestOutputHelper.WriteLine($"\nFormatted After:\n{result.SerializedObject.ToStringFormattedJson<TestCoreSerializableObject>(CoreStartupServices.ServiceProvider)!}");
        }

        [Theory]
        [InlineData(LogLevel.Trace)]
        [InlineData(LogLevel.Debug)]
        [InlineData(LogLevel.Information)]
        [InlineData(LogLevel.Warning)]
        [InlineData(LogLevel.Error)]
        [InlineData(LogLevel.Critical)]
        [InlineData(LogLevel.None)]
        public void SerializableObject_DeserializeJsonString_LogLevels(LogLevel logLevel)
        {
            JsonSerializerOptions jsonOptions = CoreDefaultJsonSerializerOptions.GetJsonSerializerOptionsWithLogLevels(logLevel, CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);

            var serializableObject = new TestCoreSerializableObject(CoreStartupServices.ServiceProvider) { LogLevel = LogLevel.Debug, };
            _ = serializableObject.Should().NotBeNull();
            serializableObject.IntProperty = 1;
            string? jsonString = serializableObject.GetJsonString<TestCoreSerializableObject>(jsonOptions);

            _ = jsonString.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"LogLevel Suppressed = {logLevel > LogLevel.Debug}:\n{jsonString}");

            if (logLevel <= LogLevel.Debug)
            {
                _ = jsonString.Should().Contain("\"LogLevel\": 1,");
            }

            (TestCoreSerializableObject? SerializedObject, Exception? Exception) result = CoreSerializableObject.CreateFromJson<TestCoreSerializableObject>(jsonString, jsonOptions, this.TestCaseLogger);

            _ = result.Should().NotBeNull();
            _ = result.Exception.Should().BeNull();
            _ = result.SerializedObject.Should().NotBeNull();
            _ = result.SerializedObject.Should().Be(serializableObject);

            this.TestOutputHelper.WriteLine($"\nFormatted with LogLevel={logLevel}:\n{result.SerializedObject.ToStringFormattedJsonWithLogLevel<TestCoreSerializableObject>(logLevel, CoreStartupServices.ServiceProvider)!}");
            _ = result.SerializedObject!.LogLevel.Should().Be(logLevel <= LogLevel.Debug ? LogLevel.Debug : LogLevel.None);
        }

        public class TestCoreSerializableObject : CoreSerializableObject, IEqualityComparer<TestCoreSerializableObject>, IEquatable<TestCoreSerializableObject>
        {
            private const string TestProp = "Test";
            private const string TestStringValue1 = "Value1";
            private const string TestStringValue2 = "Value2";
            private const string IPAddressProp = "IPAddress";
            private const string PhysicalAddressProp = "PhysicalAddress";
            private const string EnumDescriptionProp = "EnumDescription";
            private const string BooleanProp = "Boolean";
            private const string UInt16Prop = "UInt16";
            private const string UInt32Prop = "UInt32";
            private const string UInt64Prop = "UInt64";
            private const string Int16Prop = "Int16";
            private const string Int32Prop = "Int32";
            private const string Int64Prop = "Int64";
            private const string GuidProp = "Guid";

            public TestCoreSerializableObject(IServiceProvider serviceProvider, ICoreTestCaseLogger? testCaseLogger)
                : base(testCaseLogger)
            {
                this.ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
                this.TestObject = new ObjectTest(testCaseLogger);
                this.ObjectBag = new CoreObjectBag(testCaseLogger);
                _ = this.ObjectBag.AddItems(
                [
                    new CoreObjectItem(TestProp, TestStringValue1, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(TestProp, TestStringValue2, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.StringValue),
                    new CoreObjectItem(IPAddressProp, CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.IPAddress),
                    new CoreObjectItem(PhysicalAddressProp, PhysicalAddressExtensions.RestrictedPhysicalAddress, CoreObjectItemProvider.Test, CorePropConfidenceScore.MedHigh, CoreLogPropertyType.PhysicalAddress),
                    new CoreObjectItem(EnumDescriptionProp, CoreDeviceType.LocalNetworkDevice, CoreObjectItemProvider.Test, CorePropConfidenceScore.LowMed, CoreLogPropertyType.EnumDescription),
                    new CoreObjectItem(BooleanProp, true, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.Bool),
                    new CoreObjectItem(UInt16Prop, ushort.MaxValue, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.Number),
                    new CoreObjectItem(UInt32Prop, uint.MaxValue, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.Number),
                    new CoreObjectItem(UInt64Prop, ulong.MaxValue, CoreObjectItemProvider.Test, CorePropConfidenceScore.High, CoreLogPropertyType.Number),
                    new CoreObjectItem(Int16Prop, short.MinValue, CoreObjectItemProvider.Test, CorePropConfidenceScore.Low, CoreLogPropertyType.Number),
                    new CoreObjectItem(Int32Prop, int.MinValue, CoreObjectItemProvider.Test, CorePropConfidenceScore.Medium, CoreLogPropertyType.Number),
                    new CoreObjectItem(Int64Prop, long.MinValue, CoreObjectItemProvider.Test, CorePropConfidenceScore.High, CoreLogPropertyType.Number),
                    new CoreObjectItem(GuidProp, this.TestObject.ObjectId, CoreObjectItemProvider.Test, CorePropConfidenceScore.High, CoreLogPropertyType.GuidValue),
                ]);

                _ = this.TestObject.AddItems(this.ObjectBag.FindAllItems());

                this.NetworkGatewayAddressInfo = new CoreNetworkGatewayAddressInfo(
                    this.ServiceProvider,
                    DefaultIPAddressSubnetV4,
                    PhysicalAddressExtensions.RestrictedPhysicalAddress);

                using Stream streamLoaded = new MemoryStream(StorageDocumentBytesProperties);
                (ICorePropertyStorageDocument? PropertyStorageDocument, Exception? Exception) result = CorePropertyStorageDocument.CreateDocumentFromStreamAsync(streamLoaded, 0, testCaseLogger).ConfigureAwait(false).GetAwaiter().GetResult();

                this.PropertyStorageDocument = result.PropertyStorageDocument;
            }

            public TestCoreSerializableObject(IServiceProvider serviceProvider)
                : this(serviceProvider, ActiveTestCaseLogger)
            {
            }

            public TestCoreSerializableObject()
                : this(CoreStartupServices.ServiceProvider)
            {
            }

            public int IntProperty { get; set; } = DefaultIntValue;

            public string StringProperty { get; set; } = DefaultStringValue;

            public IPAddress IPv4Address { get; set; } = DefaultIPv4Address;

            public IPAddress IPv6Address { get; set; } = DefaultIPv6Address;

            public CoreIPEndPoint IPv4EndPoint { get; set; } = DefaultIPEndPointV4;

            public CoreIPEndPoint IPv6EndPoint { get; set; } = DefaultIPEndPointV6;

            [JsonLoggingLevel(LogLevel = LogLevel.Debug)]
            public LogLevel LogLevel { get; set; } = LogLevel.None;

            public IDictionary<string, string> DictString2String { get; set; } = new Dictionary<string, string>() { { "StringProperty", DefaultStringValue } };

            public IDictionary<string, IPAddress> DictString2IPAddress { get; set; } = new Dictionary<string, IPAddress>()
            {
                { "IPv4Address", DefaultIPv4Address },
                { "IPv6Address", DefaultIPv6Address },
            };

            public IDictionary<string, CoreIPEndPoint> DictString2IPEndPoint { get; set; } = new Dictionary<string, CoreIPEndPoint>()
            {
                { "IPv4EndPoint", DefaultIPEndPointV4 },
                { "IPv6EndPoint", DefaultIPEndPointV6 },
            };

            public IDictionary<string, CoreIPAddressSubnet> DictString2IPAddressSubnet { get; set; } = new Dictionary<string, CoreIPAddressSubnet>()
            {
                { "IPv4IPAddressSubnet", DefaultIPAddressSubnetV4 },
                { "IPv6IPAddressSubnet", DefaultIPAddressSubnetV6 },
            };

            public ICoreNetworkGatewayAddressInfo NetworkGatewayAddressInfo { get; set; }

            public ICoreObject TestObject { get; set; }

            public CorePropID PropID { get; set; } = CorePropID.TestProp;

            public ICorePropertyStorageDocument? PropertyStorageDocument { get; set; }

            public ICoreObjectBag ObjectBag { get; set; }

            protected IServiceProvider ServiceProvider { get; }

            public override bool Equals(object? obj) => this.Equals(obj as TestCoreSerializableObject);

            public bool Equals(TestCoreSerializableObject? other) => this.Equals(this, other);

            public bool Equals(TestCoreSerializableObject? x, TestCoreSerializableObject? y)
            {
                return ReferenceEquals(x, y)
                    || (x is not null
                    && y is not null
                    && x.GetType() == y.GetType()
                    && x.IntProperty == y.IntProperty
                    && x.StringProperty == y.StringProperty
                    && x.IPv4Address.Equals(y.IPv4Address)
                    && x.IPv6Address.Equals(y.IPv6Address)
                    && x.IPv4EndPoint.Equals(y.IPv4EndPoint)
                    && x.IPv6EndPoint.Equals(y.IPv6EndPoint)
                    && x.DictString2String.SequenceEqual(y.DictString2String)
                    && x.DictString2IPAddress.SequenceEqual(y.DictString2IPAddress)
                    && x.DictString2IPEndPoint.SequenceEqual(y.DictString2IPEndPoint)
                    && x.DictString2IPAddressSubnet.SequenceEqual(y.DictString2IPAddressSubnet)
                    && x.NetworkGatewayAddressInfo.Equals(y.NetworkGatewayAddressInfo)
                    && x.TestObject.Equals(y.TestObject)
                    && x.PropID == y.PropID
                    && x.PropertyStorageDocument is not null
                    && y.PropertyStorageDocument is not null
                    && x.PropertyStorageDocument.Equals(y.PropertyStorageDocument)
                    && x.ObjectBag.Equals(y.ObjectBag));
            }

            public int GetHashCode(TestCoreSerializableObject? obj)
            {
                var hashCode = default(HashCode);

                if (obj is not null)
                {
                    hashCode.Add(obj.IntProperty);
                    hashCode.Add(obj.StringProperty);
                    hashCode.Add(obj.IPv4Address);
                    hashCode.Add(obj.IPv6Address);
                    hashCode.Add(obj.IPv4EndPoint);
                    hashCode.Add(obj.IPv6EndPoint);
                    hashCode.Add((int)obj.LogLevel);
                    hashCode.Add(obj.DictString2String);
                    hashCode.Add(obj.DictString2IPAddress);
                    hashCode.Add(obj.DictString2IPEndPoint);
                    hashCode.Add(obj.DictString2IPAddressSubnet);
                    hashCode.Add(obj.NetworkGatewayAddressInfo);
                    hashCode.Add(obj.TestObject);
                    hashCode.Add((int)obj.PropID);
                    hashCode.Add(obj.PropertyStorageDocument);
                    hashCode.Add(obj.ObjectBag);
                }

                return hashCode.ToHashCode();
            }

            public override int GetHashCode() => this.GetHashCode(this);
        }

        /// <summary>
        /// Class ObjectTest.
        /// </summary>
        public class ObjectTest : CoreObjectBase
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ObjectTest"/> class.
            /// </summary>
            /// <param name="logger">The logger.</param>
            public ObjectTest(ICoreLogger? logger)
            : base(logger)
            {
            }

            public ObjectTest()
                : this(null)
            {
            }

            protected override void SetObjectId(Guid newObjectID, bool updateObjectVersion = true)
            {
                base.SetObjectId(newObjectID, false);
            }

            protected override void SetCreatedTimestamp(DateTimeOffset newCreatedTimestamp, bool updateObjectVersion = true)
            {
                base.SetCreatedTimestamp(newCreatedTimestamp, false);
            }

            protected override void SetModifiedTimestamp(DateTimeOffset newModifiedTimestamp, bool updateObjectVersion = true)
            {
                base.SetModifiedTimestamp(newModifiedTimestamp, false);
            }

            protected override void SetObjectVersion(ulong newObjectVersion, bool updateObjectVersion = true)
            {
                base.SetObjectVersion(newObjectVersion, false);
            }
        }
    }
}
