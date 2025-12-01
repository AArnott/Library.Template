// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreSerializableObjectIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Startup;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using NetworkVisor.Platform.Test.TestObjects;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Serialization
{
    /// <summary>
    /// Class CoreSerializableObjectIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreSerializableObjectIntegrationTests))]

    public partial class CoreSerializableObjectIntegrationTests : CoreTestCaseBase
    {
        private static readonly string FileSystemType = @"""$type"": ""NetworkVisor.Core.CoreSystem.CoreFileSystem NetworkVisor.Platform.";

        private static readonly IPAddress DefaultIPv4Address = CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1;
        private static readonly IPAddress DefaultIPv6Address = CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1;

        private static readonly CoreIPEndPoint DefaultIPEndPointV4 = new(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1, CoreMulticastDnsConstants.MulticastDnsServerPort);
        private static readonly CoreIPEndPoint DefaultIPEndPointV6 = new(CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1, CoreMulticastDnsConstants.MulticastDnsServerPort);

        private static readonly CoreIPAddressSubnet DefaultIPAddressSubnetV4 = CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC);
        private static readonly CoreIPAddressSubnet DefaultIPAddressSubnetV6 = CoreIPAddressExtensions.GooglePublicDnsServerIPv6Address1.ToIPAddressSubnet(CoreIPAddressExtensions.SubnetClassC);

        private static readonly byte[] StorageDocumentBytesProperties = [123, 34, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 77, 97, 106, 111, 114, 34, 58, 49, 44, 34, 83, 99, 104, 101, 109, 97, 86, 101, 114, 115, 105, 111, 110, 77, 105, 110, 111, 114, 34, 58, 49, 44, 34, 82, 101, 118, 105, 115, 105, 111, 110, 34, 58, 49, 44, 34, 76, 97, 115, 116, 85, 112, 100, 97, 116, 101, 100, 85, 116, 99, 34, 58, 34, 50, 48, 50, 52, 45, 48, 55, 45, 50, 56, 84, 49, 53, 58, 48, 52, 58, 48, 48, 46, 52, 48, 50, 52, 50, 57, 53, 43, 48, 48, 58, 48, 48, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 105, 101, 115, 34, 58, 123, 34, 80, 114, 111, 112, 101, 114, 116, 121, 51, 34, 58, 34, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 121, 50, 34, 58, 34, 86, 97, 108, 117, 101, 50, 34, 44, 34, 80, 114, 111, 112, 101, 114, 116, 121, 49, 34, 58, 34, 86, 97, 108, 117, 101, 49, 34, 125, 125,];

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSerializableObjectIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSerializableObjectIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            AssertionConfiguration.Current.Formatting.MaxLines = 2000;
        }

        [Fact]
        public void SerializableObject_GetJsonString_Compact()
        {
            using var serializableObject = new CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>(CoreStartupServices.ServiceProvider);
            _ = serializableObject.Should().NotBeNull();
            string? jsonString = serializableObject.GetJsonString<CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);
            _ = jsonString.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine(jsonString);
        }

        [Fact]
        public void SerializableObject_GetJsonString_Formatted()
        {
            using var serializableObject = new CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>(CoreStartupServices.ServiceProvider);
            _ = serializableObject.Should().NotBeNull();
            string? jsonString = serializableObject.GetJsonString<CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);
            _ = jsonString.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine(jsonString);
        }

        [Fact]
        public async Task SerializableObject_DeserializeJsonStringAsync()
        {
            using var serializableObject = new CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>(CoreStartupServices.ServiceProvider);
            _ = serializableObject.Should().NotBeNull();
            string? jsonString = serializableObject.GetJsonString<CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);
            _ = jsonString.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Formatted Before:\n{jsonString}");

            (CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>? SerializedObject, Exception? Exception) result = CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>.TestCreateFromJson<CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>>(jsonString, CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider, this.TestCaseLogger);

            _ = result.Should().NotBeNull();
            _ = result.Exception.Should().BeNull();
            _ = result.SerializedObject.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Serialized:\n{result.SerializedObject!.ToJsonString<CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider)}");

            // Update AppSessionID from original object
            result.SerializedObject!.FileSystem.AppSettings.AppSessionID = serializableObject.FileSystem.AppSettings.AppSessionID;

            // FileSystem setter comparison tests
            _ = result.SerializedObject!.FileSystem.AppSettings.AppFolderName.Should().Be(serializableObject.FileSystem.AppSettings.AppFolderName);

            // Test Mobile overrides
            _ = result.SerializedObject.FileSystem.LocalUserAppDataFolderPath.Should().Be(serializableObject.FileSystem.LocalUserAppDataFolderPath);

            // Test Android overrides
            _ = result.SerializedObject.FileSystem.AssemblyFolderPath.Should().Be(serializableObject.FileSystem.AssemblyFolderPath);
            _ = result.SerializedObject.FileSystem.ExecutingAssemblyPath.Should().Be(serializableObject.FileSystem.ExecutingAssemblyPath);

            // Synchronize version information before comparison.
            result.SerializedObject.SynchronizeVersionInfo(serializableObject);

            _ = result.SerializedObject.Should().Be(serializableObject);

            (string? JsonString, Exception? Exception) resultFormatted = await result.SerializedObject.GetJsonStringAsync<CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);
            _ = resultFormatted.Should().NotBeNull();
            _ = resultFormatted.Exception.Should().BeNull();

            this.TestOutputHelper.WriteLine($"\nFormatted After:\n{resultFormatted.JsonString}");
            result.SerializedObject?.Dispose();
        }

        [Fact]
        public void SerializableObject_DeserializeJsonString_WrongPlatform()
        {
            using var serializableObject = new CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>(CoreStartupServices.ServiceProvider);
            _ = serializableObject.Should().NotBeNull();
            string? jsonString = serializableObject.GetJsonString<CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>>(CoreSerializationFormatFlags.JsonFormatted, CoreStartupServices.ServiceProvider);
            _ = jsonString.Should().NotBeNullOrEmpty();

            int indexStart = jsonString!.IndexOf(FileSystemType, StringComparison.Ordinal);
            int indexEnd = jsonString.IndexOf("\"", indexStart + FileSystemType.Length, StringComparison.Ordinal);

            string? jsonStringWrongPlatform = $"{jsonString.Substring(0, indexStart + FileSystemType.Length)}Unknown{jsonString.Substring(indexEnd)}";

            this.TestOutputHelper.WriteLine($"WrongPlatform:\n{jsonStringWrongPlatform}\n");

            (CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>? SerializedObject, Exception? Exception) result = CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>.TestCreateFromJson<CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>>(jsonStringWrongPlatform, CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider, this.TestCaseLogger);

            _ = result.Should().NotBeNull();
            _ = result.Exception.Should().BeNull();
            _ = result.SerializedObject.Should().NotBeNull();

            // Update AppSessionID from original object
            result.SerializedObject!.FileSystem.AppSettings.AppSessionID = serializableObject.FileSystem.AppSettings.AppSessionID;

            // Synchronize version information before comparison.
            result.SerializedObject.SynchronizeVersionInfo(serializableObject);

            _ = result.SerializedObject.Should().Be(serializableObject);

            this.TestOutputHelper.WriteLine($"\nFormatted After:\n{result.SerializedObject.ToStringFormattedJson<CoreTestSerializableObject<CoreSerializableObjectIntegrationTests>>()!}");
            result.SerializedObject?.Dispose();
        }
    }
}
