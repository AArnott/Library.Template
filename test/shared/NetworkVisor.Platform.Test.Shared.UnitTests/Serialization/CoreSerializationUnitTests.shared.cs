// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreSerializationUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Serialization
{
    /// <summary>
    /// Class CoreSerializationUnitTests.
    /// </summary>
    [PlatformTrait(typeof(CoreSerializationUnitTests))]

    public class CoreSerializationUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSerializationUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSerializationUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /*
         * Json supported primitive types:
         *      Boolean
         *      Byte
         *      Base64
         *      DateTime
         *      DateTimeOffset
         *      Decimal
         *      Double
         *      Guid
         *      Int16
         *      Int32
         *      Int64
         *      String
         *      RawText
         *      SByte
         *      Single
         *      UInt16
         *      UInt32
         *      UInt64
         *
         * Json complex types supported:
         *      Array
         *      Object
         *      Dictionary<string, value> (non-string keys require a JsonConverter)
         *
         * User types requiring converters:
         *      Dictionary<key, value> (non-string keys require a JsonConverter)
         *      IPAddress
         *      CoreIPAddressSubnet
         *      Enum (JsonNumberEnumConverter, JsonConverterAttribute)
         *      PhysicalAddress
         *      WmiProperty
         *      Exception
         */

        [Theory]
        [InlineData("10.1.10.1", "\"10.1.10.1\"")]
        [InlineData(null, "null")]
        [InlineData("", "null")]
        [InlineData("fd52:1ece:1f8:e4e:8c:ffc9:b65f:1978", "\"fd52:1ece:1f8:e4e:8c:ffc9:b65f:1978\"")]
        [InlineData("fe80::286c:703a:9fb:325f%2", "\"fe80::286c:703a:9fb:325f%2\"")]
        [InlineData("fe80::95d8:2a40:6a35:42fa%33", "\"fe80::95d8:2a40:6a35:42fa%33\"")]
        [InlineData("fe80::ddb1:af2:511a:36dd%29", "\"fe80::ddb1:af2:511a:36dd%29\"")]
        [InlineData(CoreIPAddressExtensions.StringBroadcast, "\"255.255.255.255\"")]
        [InlineData(CoreIPAddressExtensions.StringAny, "\"0.0.0.0\"")]
        [InlineData(CoreIPAddressExtensions.StringLoopback, "\"127.0.0.1\"")]
        [InlineData("169.254.0.2", "\"169.254.0.2\"")]
        [InlineData("10.1.10.2", "\"10.1.10.2\"")]

        public void Serialization_IPAddressJsonConverter(string? ipAddressStringTest, string? jsonStringExpected)
        {
            IPAddress? ipAddressTest = string.IsNullOrEmpty(ipAddressStringTest) ? null : IPAddress.Parse(ipAddressStringTest);
            string? jsonString = JsonSerializer.Serialize(ipAddressTest, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider));

            jsonString.Should().Be(jsonStringExpected);
            JsonSerializer.Deserialize<IPAddress?>(jsonString, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider)).Should().Be(ipAddressTest);
        }

        [Theory]
        [InlineData("10.1.10.1", "{\"Address\":\"10.1.10.1\",\"Port\":0}")]
        [InlineData(null, "null")]
        [InlineData("", "null")]
        [InlineData("fd52:1ece:1f8:e4e:8c:ffc9:b65f:1978", "{\"Address\":\"fd52:1ece:1f8:e4e:8c:ffc9:b65f:1978\",\"Port\":0}")]
        [InlineData("fe80::286c:703a:9fb:325f%2", "{\"Address\":\"fe80::286c:703a:9fb:325f%2\",\"Port\":0}")]
        [InlineData("fe80::95d8:2a40:6a35:42fa%33", "{\"Address\":\"fe80::95d8:2a40:6a35:42fa%33\",\"Port\":0}")]
        [InlineData("fe80::ddb1:af2:511a:36dd%29", "{\"Address\":\"fe80::ddb1:af2:511a:36dd%29\",\"Port\":0}")]
        [InlineData(CoreIPAddressExtensions.StringBroadcast, "{\"Address\":\"255.255.255.255\",\"Port\":0}")]
        [InlineData(CoreIPAddressExtensions.StringAny, "{\"Address\":\"0.0.0.0\",\"Port\":0}")]
        [InlineData(CoreIPAddressExtensions.StringLoopback, "{\"Address\":\"127.0.0.1\",\"Port\":0}")]
        [InlineData("169.254.0.2", "{\"Address\":\"169.254.0.2\",\"Port\":0}")]
        [InlineData("10.1.10.2", "{\"Address\":\"10.1.10.2\",\"Port\":0}")]
        [InlineData("10.1.10.1:45", "{\"Address\":\"10.1.10.1\",\"Port\":45}")]
        [InlineData("[fd52:1ece:1f8:e4e:8c:ffc9:b65f:1978]:45", "{\"Address\":\"fd52:1ece:1f8:e4e:8c:ffc9:b65f:1978\",\"Port\":45}")]
        [InlineData("[fe80::286c:703a:9fb:325f%2]:2024", "{\"Address\":\"fe80::286c:703a:9fb:325f%2\",\"Port\":2024}")]
        [InlineData("[fe80::95d8:2a40:6a35:42fa%33]:80", "{\"Address\":\"fe80::95d8:2a40:6a35:42fa%33\",\"Port\":80}")]
        [InlineData("[fe80::ddb1:af2:511a:36dd%29]", "{\"Address\":\"fe80::ddb1:af2:511a:36dd%29\",\"Port\":0}")]
        [InlineData("255.255.255.255:123", "{\"Address\":\"255.255.255.255\",\"Port\":123}")]
        [InlineData("0.0.0.0:0", "{\"Address\":\"0.0.0.0\",\"Port\":0}")]
        [InlineData("127.0.0.1:345", "{\"Address\":\"127.0.0.1\",\"Port\":345}")]
        [InlineData("169.254.0.2:40", "{\"Address\":\"169.254.0.2\",\"Port\":40}")]
        [InlineData("10.1.10.2:50", "{\"Address\":\"10.1.10.2\",\"Port\":50}")]

        public void Serialization_CoreIPEndPointJsonConverter(string? ipEndPointStringTest, string? jsonStringExpected)
        {
            CoreIPEndPoint? ipEndPointTest = ipEndPointStringTest.ToCoreIPEndPoint();
            this.TestOutputHelper.WriteLine($"IPEndPoint: {ipEndPointTest}");
            string? jsonString = JsonSerializer.Serialize(ipEndPointTest, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider));

            jsonString.Should().Be(jsonStringExpected);
            JsonSerializer.Deserialize<CoreIPEndPoint?>(jsonString, CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonCompact, this.TestCaseServiceProvider)).Should().Be(ipEndPointTest);

            this.TestOutputHelper.WriteLine(jsonString);
        }
    }
}
