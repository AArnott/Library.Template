// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreNetworkingSystemBaseIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using FluentAssertions;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Firewall;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.MulticastDns.Resolver;
using NetworkVisor.Core.Networking.NetworkInterface;
using NetworkVisor.Core.Networking.Preferred;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Networking.Services.MulticastDns.Constants;
using NetworkVisor.Core.Networking.Services.Ping;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Networking.WiFi;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.NetworkingSystem;
using NetworkVisor.Platform.Networking.WiFiNetwork;
using NetworkVisor.Platform.Test.Extensions;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

#if NV_PLAT_MACOS || NV_PLAT_MACCATALYST
using NetworkVisor.Platform.Networking.SystemProfiler;
#endif

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking
{
    /// <summary>
    /// Class NetworkingSystemBaseIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreNetworkingSystemBaseIntegrationTests))]

    public class CoreNetworkingSystemBaseIntegrationTests : CoreTestCaseBase
    {
        public const int KerberosUdpPort = 88;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkingSystemBaseIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkingSystemBaseIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Gets the privileged UDP port to test.
        /// </summary>
        /// <param name="operatingSystem"></param>
        /// <returns>Privileged UDP port to test.</returns>
        public int[] PrivilegedUdpPortsToTest(ICoreOperatingSystem operatingSystem)
        {
            return new[]
            {
                KerberosUdpPort, CoreMulticastDnsConstants.MulticastDnsServerPort, DnsResolver.DefaultDnsPort,
            };
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_TestNetworkingSystem.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_TestNetworkingSystem()
        {
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);

            // These network services are support on all platforms.
            this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.SocketsNonPrivileged).Should().BeTrue();
            this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.Ping).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_Ctor.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_Ctor()
        {
            using var testCoreNetworkingSystem = new TestCoreNetworkingSystemBase(this.TestCaseServiceProvider, this.TestFileSystem, this.TestNetworkingSystem.ProcessRunner, this.TestNetworkingSystem.OperationRunner, this.TestCaseLogger);
            testCoreNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            testCoreNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            testCoreNetworkingSystem.ProcessRunner.Should().BeSameAs(this.TestNetworkingSystem.ProcessRunner);
            testCoreNetworkingSystem.OperationRunner.Should().BeSameAs(this.TestNetworkingSystem.OperationRunner);
            testCoreNetworkingSystem.TestOnSystemNetworkAddressChangedEvent();
            testCoreNetworkingSystem.TestOnSystemNetworkAvailabilityChangedEvent();
        }

        [Fact]
        public void NetworkingSystemBase_ValidateNetworkingSystem()
        {
            this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);

            // Networking Services is initialized so this should not be null.
            this.TestNetworkingSystem.NetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            this.TestNetworkingSystem.NetworkServices!.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_Ctor_Func.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_Ctor_Func()
        {
            using var testCoreNetworkingSystem = new TestCoreNetworkingSystemBase(this.TestCaseServiceProvider, this.TestFileSystem, this.TestNetworkingSystem.ProcessRunner, this.TestNetworkingSystem.OperationRunner, this.TestCaseLogger);
            testCoreNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            testCoreNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            testCoreNetworkingSystem.ProcessRunner.Should().BeSameAs(this.TestNetworkingSystem.ProcessRunner);
            testCoreNetworkingSystem.OperationRunner.Should().BeSameAs(this.TestNetworkingSystem.OperationRunner);
            testCoreNetworkingSystem.TestOnSystemNetworkAddressChangedEvent();
            testCoreNetworkingSystem.TestOnSystemNetworkAvailabilityChangedEvent();
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetDnsHostEntry_NetworkVisorComDnsAsync.
        /// </summary>
        [Fact]
        public async Task NetworkingSystemBase_GetDnsHostEntry_NetworkVisorComDnsAsync()
        {
            IPHostEntry? networkVisorIPHostEntry = await this.TestNetworkingSystem.GetDnsHostEntryAsync(CoreIPAddressExtensions.NetworkVisorComDns);
            networkVisorIPHostEntry.Should().NotBeNull().And.Subject.Should().BeOfType<IPHostEntry?>();

            this.TestOutputHelper.WriteLine(networkVisorIPHostEntry!.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine));

            networkVisorIPHostEntry!.AddressList.FirstOrDefault().Should().Be(CoreIPAddressExtensions.NetworkVisorComAddress);
            networkVisorIPHostEntry.HostName.ToLowerInvariant().Should().Be(CoreIPAddressExtensions.NetworkVisorComDns);
        }

        [Fact]
        public async Task NetworkingSystemBase_GetDnsHostEntry_Loopback_DnsAsync()
        {
            IPHostEntry? networkVisorIPHostEntry = await this.TestNetworkingSystem.GetDnsHostEntryAsync(CoreIPAddressExtensions.StringLoopback);

            networkVisorIPHostEntry.Should().NotBeNull().And.Subject.Should().BeOfType<IPHostEntry?>();

            if (networkVisorIPHostEntry is not null)
            {
                this.TestOutputHelper.WriteLine(networkVisorIPHostEntry!.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine));

                networkVisorIPHostEntry!.AddressList.Should().NotBeNull();

                this.TestOutputHelper.WriteLine($"OS Hostname: {this.TestNetworkingSystem.DeviceHostName}");

                this.TestOutputHelper.WriteLine($"IPAddresses from qualified hostname: {networkVisorIPHostEntry.HostName}");

                if (networkVisorIPHostEntry.HostName.Equals(this.TestNetworkingSystem.DeviceHostNameQualified, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.TestOutputHelper.WriteLine($"LocalHost resolving to qualified hostname: {networkVisorIPHostEntry.HostName}");
                }
                else
                {
                    foreach (IPAddress? ipAddress in networkVisorIPHostEntry.AddressList)
                    {
                        ipAddress.IsNullOrNone().Should().BeFalse();

                        this.TestOutputHelper.WriteLine($"   {ipAddress}");
                    }

                    networkVisorIPHostEntry!.AddressList!.Should().Contain(IPAddress.Loopback);
                }
            }
        }

        [Fact]
        public void NetworkingSystemBase_DeviceHostName()
        {
            string hostName = this.TestNetworkingSystem.DeviceHostName;

            this.TestOutputHelper.WriteLine($"Device Hostname: {hostName}");
            hostName.Should().NotBeNullOrWhiteSpace();
            hostName.Should().Be(System.Net.Dns.GetHostName());
        }

        [Fact]
        public void NetworkingSystemBase_DeviceHostNameQualified()
        {
            string? hostNameQualified = this.TestNetworkingSystem.DeviceHostNameQualified;

            this.TestOutputHelper.WriteLine($"Device Hostname Qualified: {hostNameQualified}");

            // hostNameQualified.Should().NotBeNullOrWhiteSpace();
            // hostNameQualified!.ToLower().Should().StartWith(Dns.GetHostName().ToLower());
        }

        [Fact]
        public void NetworkingSystemBase_HostNameQualified()
        {
            string? hostNameQualified = this.TestNetworkingSystem.HostNameQualified(this.TestNetworkingSystem.DeviceHostName);

            this.TestOutputHelper.WriteLine($"Hostname Qualified: {hostNameQualified}");

            // hostNameQualified.Should().NotBeNullOrWhiteSpace();
            // hostNameQualified!.ToLower().Should().StartWith(Dns.GetHostName().ToLower());
        }

        [Fact]
        public void NetworkingSystemBase_GetDnsHostEntry_Loopback_IP()
        {
            IPHostEntry? networkVisorIPHostEntry = this.TestNetworkingSystem.GetDnsHostEntry(IPAddress.Loopback);

            networkVisorIPHostEntry.Should().NotBeNull().And.Subject.Should().BeOfType<IPHostEntry?>();

            if (networkVisorIPHostEntry is not null)
            {
                this.TestOutputHelper.WriteLine(networkVisorIPHostEntry!.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine));

                networkVisorIPHostEntry!.AddressList.Should().NotBeNull();

                this.TestOutputHelper.WriteLine($"Device Hostname: {this.TestNetworkingSystem.DeviceHostName}");

                this.TestOutputHelper.WriteLine($"IPAddresses from qualified hostname: {networkVisorIPHostEntry.HostName}");

                if (networkVisorIPHostEntry.HostName.Equals(this.TestNetworkingSystem.DeviceHostNameQualified, StringComparison.InvariantCultureIgnoreCase))
                {
                    this.TestOutputHelper.WriteLine($"LocalHost resolving to qualified hostname: {networkVisorIPHostEntry.HostName}");
                }
                else
                {
                    foreach (IPAddress? ipAddress in networkVisorIPHostEntry.AddressList)
                    {
                        ipAddress.IsNullOrNone().Should().BeFalse();

                        this.TestOutputHelper.WriteLine($"   {ipAddress}");
                    }

                    networkVisorIPHostEntry!.AddressList!.Should().Contain(IPAddress.Loopback!);
                }
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetDnsHostEntry_PublicServerDNS.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetDnsHostEntry_PublicServerDNS()
        {
            IPHostEntry? networkVisorIPHostEntry = this.TestNetworkingSystem.GetDnsHostEntry(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1);

            // For some reason, this is failing in CI for both GitHub and Azure.
            if (!CoreAppConstants.IsRunningInCI)
            {
                networkVisorIPHostEntry.Should().NotBeNull().And.Subject.Should().BeOfType<IPHostEntry?>();

                if (networkVisorIPHostEntry is not null)
                {
                    this.TestOutputHelper.WriteLine(networkVisorIPHostEntry!.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine));

                    networkVisorIPHostEntry!.AddressList.Should().NotBeNull();
                    networkVisorIPHostEntry!.AddressList!.Should().Contain(CoreIPAddressExtensions.GooglePublicDnsServerIPv4Address1!);
                    networkVisorIPHostEntry.HostName.ToLowerInvariant().Should().Be(CoreIPAddressExtensions.StringGooglePublicDnsServer!);
                }
            }
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetDnsHostEntryAsync_String_Null.
        /// </summary>
        [Fact]
        public async Task NetworkingSystemBase_GetDnsHostEntryAsync_String_Null()
        {
            (await this.TestNetworkingSystem.GetDnsHostEntryAsync((string?)null)).Should().BeNull();
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetLocalHostEntryFromDns.
        /// </summary>
        [Fact]
        public async Task NetworkingSystemBase_GetLocalHostEntryFromDnsAsync()
        {
            IPAddress? preferredLocalIPAddress = this.TestNetworkServices.PreferredLocalNetworkAddress?.IPAddress;

            preferredLocalIPAddress.Should().NotBeNull();

            IPHostEntry? ipHostEntry = await this.TestNetworkingSystem.GetLocalHostEntryFromDnsAsync();

            if (ipHostEntry is null)
            {
                CoreAppConstants.IsRunningInCI.Should().BeTrue();
                this.TestOperatingSystem.BuildHostType.Should().Be(CoreBuildHostType.MacOS);
            }
            else
            {
                ipHostEntry.Should().NotBeNull();
                ipHostEntry!.AddressList.Should().NotBeNullOrEmpty();

                this.TestOutputHelper.WriteLine($"Preferred Local IP Address: {preferredLocalIPAddress}");
                this.TestOutputHelper.WriteLine($"Local IP Address:{Environment.NewLine}{ipHostEntry!.ToString(CoreLoggableFormatFlags.ToStringWithPropNameMultiLine)}");

                if (ipHostEntry.AddressList.Length > 1 || !ipHostEntry.AddressList[0]!.ToString()!.Equals(CoreIPAddressExtensions.StringLoopback))
                {
                    ipHostEntry.AddressList.Should().Contain(preferredLocalIPAddress!);
                }
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetDnsHostEntry_IPAddress_Null.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public void NetworkingSystemBase_GetDnsHostEntry_IPAddress_Null()
        {
            this.TestNetworkingSystem.GetDnsHostEntry((IPAddress?)null).Should().BeNull();
            this.TestNetworkingSystem.GetDnsHostEntry(IPAddress.None).Should().BeNull();
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetDnsHostEntry_NonRoutableAddress.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task NetworkingSystemBase_GetDnsHostEntry_NonRoutableHostNameAsync_Timeout()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(2000);

            // MacOS returns non-null with hostname as IPAddress
            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                (await this.TestNetworkingSystem.GetDnsHostEntryAsync(CoreIPAddressExtensions.StringNonRoutable, cts.Token)).Should().BeNull();
            }
            else
            {
                (await this.TestNetworkingSystem.GetDnsHostEntryAsync(CoreIPAddressExtensions.StringNonRoutable, cts.Token))?.HostName.Should().Be(CoreIPAddressExtensions.StringNonRoutable);
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetDnsHostEntry_NonRoutableAddress.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public void NetworkingSystemBase_GetDnsHostEntry_NonRoutableIPAddress()
        {
            // MacOS returns non-null with hostname as IPAddress
            if (this.TestOperatingSystem.IsWindowsPlatform)
            {
                this.TestNetworkingSystem.GetDnsHostEntry(CoreIPAddressExtensions.NonRoutableIPAddress).Should().BeNull();
            }
            else
            {
                this.TestNetworkingSystem.GetDnsHostEntry(CoreIPAddressExtensions.NonRoutableIPAddress)?.HostName.Should().Be(CoreIPAddressExtensions.StringNonRoutable);
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_PreferredLocalIPAddressFromConnection.
        /// </summary>
        [Fact]
        public async Task NetworkingSystemBase_PreferredLocalIPAddressFromConnection()
        {
            ICoreTaskResult<IPAddress?> taskResult = await this.TestNetworkingSystem.PreferredLocalIPAddressFromConnectionAsync();

            taskResult.Should().NotBeNull();
            taskResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();
            taskResult.Result.Should().NotBeNull();
            taskResult.Result.IsNullOrNone().Should().BeFalse();

            this.TestOutputHelper.WriteLine($"Preferred Local IP Address: {taskResult.Result}");
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_PreferredLocalIPAddressFromConnection.
        /// </summary>
        [Fact]
        public async Task NetworkingSystemBase_PreferredLocalIPAddressFromConnection_TimeSpan()
        {
            (await this.TestNetworkingSystem.PreferredLocalIPAddressFromConnectionAsync(this.TestNetworkingSystem.DefaultTimeout, CancellationToken.None)).Result.IsNullOrNone().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_PreferredLocalIPAddress.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_PreferredLocalIPAddress()
        {
            IPAddress? preferredLocalIPAddress = this.TestNetworkServices.PreferredLocalNetworkAddress?.IPAddress;

            this.TestOutputHelper.WriteLine($"Preferred Local IP Address: {preferredLocalIPAddress}");

            preferredLocalIPAddress.IsNullOrNone().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_PreferredLocalIPAddress_TimeSpan.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_PreferredLocalIPAddress_TimeSpan()
        {
            this.TestNetworkServices.PreferredLocalNetworkAddress?.IPAddress?.IsNullOrNone().Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_PreferredLocalAddressInfo.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_PreferredLocalAddressInfo()
        {
            ICoreNetworkAddressInfo? preferredLocalIPAddressInfo = this.TestNetworkServices.PreferredLocalNetworkAddress;

            preferredLocalIPAddressInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkAddressInfo>();
            this.TestOutputHelper.WriteLine($"PreferredLocalIPAddressInfo: {preferredLocalIPAddressInfo.ToStringWithParentsPropNameMultiLine()}");

            preferredLocalIPAddressInfo!.IPAddress.IsNullOrNone().Should().BeFalse();
            this.TestNetworkServices!.PreferredLocalNetworkAddress!.PreferredNetworkInterface.Should().NotBeNull();

            if (!this.TestNetworkServices!.PreferredLocalNetworkAddress!.PreferredNetworkInterface!.IsCellularConnection)
            {
                preferredLocalIPAddressInfo.SubnetMask.IsNullOrNone().Should().BeFalse();
            }

            preferredLocalIPAddressInfo.IPAddressSubnet.IsNullOrNone().Should().BeFalse();

            if (this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.LocalPhysicalAddress))
            {
                preferredLocalIPAddressInfo.PhysicalAddress.IsNullOrNone().Should().BeFalse();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.LocalPhysicalAddress} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_PreferredLocalAddressInfo_TimeSpan.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_PreferredLocalAddressInfo_TimeSpan()
        {
            ICoreNetworkAddressInfo? preferredLocalIPAddressInfo = this.TestNetworkServices.PreferredLocalNetworkAddress;

            preferredLocalIPAddressInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkAddressInfo>();
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetAllNetworkInterfaces.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetAllNetworkInterfaces()
        {
            var networkInterfacesList = this.TestNetworkingSystem.GetAllNetworkInterfaces().ToList();

            networkInterfacesList.Should().NotBeNull();

            networkInterfacesList.Count.Should().BeGreaterThanOrEqualTo(1);

            foreach (ICoreNetworkInterface networkInterface in networkInterfacesList)
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetAllNetworkInterfacesWithGateway.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetAllNetworkInterfacesWithGateway()
        {
            ISet<ICoreNetworkInterface> networkInterfacesWithGateway = this.TestNetworkingSystem.GetAllNetworkInterfacesWithGateway();

            networkInterfacesWithGateway.Should().NotBeNull();

            networkInterfacesWithGateway.Count.Should().BeGreaterThanOrEqualTo(1);

            foreach (ICoreNetworkInterface networkInterface in networkInterfacesWithGateway)
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetAllActiveMulticastNetworkInterfaces.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetAllActiveMulticastNetworkInterfaces()
        {
            ISet<ICoreNetworkInterface> multicastNetworkInterfaces = this.TestNetworkingSystem.GetAllActiveMulticastNetworkInterfaces();

            multicastNetworkInterfaces.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Multicast Interface Count: {multicastNetworkInterfaces.Count}");

            multicastNetworkInterfaces.Count.Should().BeGreaterThanOrEqualTo(1);

            foreach (ICoreNetworkInterface networkInterface in multicastNetworkInterfaces)
            {
                networkInterface.Should().NotBeNull();
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
                networkInterface.SupportsMulticast.Should().BeTrue();
                networkInterface.OperationalStatus.Should().Be(OperationalStatus.Up);
                networkInterface.NetworkInterfaceType.Should().NotBe(NetworkInterfaceType.Loopback);
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetAllActiveNetworkInterfacesWithGateway.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetAllActiveNetworkInterfacesWithGateway()
        {
            ISet<ICoreNetworkInterface> networkInterfacesWithGateway = this.TestNetworkingSystem.GetAllActiveNetworkInterfacesWithGateway();

            networkInterfacesWithGateway.Should().NotBeNull();

            networkInterfacesWithGateway.Count.Should().BeGreaterThanOrEqualTo(1);

            foreach (ICoreNetworkInterface networkInterface in networkInterfacesWithGateway)
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
                networkInterface.HasActiveGateway.Should().BeTrue();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetAllActiveNetworkInterfaces.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetAllActiveNetworkInterfaces()
        {
            var networkInterfaces = this.TestNetworkingSystem.GetAllActiveNetworkInterfaces().ToList();

            networkInterfaces.Should().NotBeNull();

            networkInterfaces.Count.Should().BeGreaterThanOrEqualTo(1);

            foreach (ICoreNetworkInterface networkInterface in networkInterfaces)
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
                networkInterface.OperationalStatus.Should().Be(OperationalStatus.Up);
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetAllActiveNetworkInterfacesWithDhcpServer.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetAllActiveNetworkInterfacesWithDhcpServer()
        {
            var networkInterfaces = this.TestNetworkingSystem.GetAllActiveNetworkInterfaces((ni) => ni.GetIsDhcpEnabled()).ToList();

            networkInterfaces.Should().NotBeNull();

            foreach (ICoreNetworkInterface networkInterface in networkInterfaces)
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
                networkInterface.PreferredNetworkAddress.Should().NotBeNull();
                networkInterface.GetIsDhcpEnabled().Should().BeTrue();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetAllActiveNetworkInterfacesWithDns.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetAllActiveNetworkInterfacesWithDns()
        {
            var networkInterfaces = this.TestNetworkingSystem.GetAllActiveNetworkInterfaces((ni) => ni.IsDnsEnabled).ToList();

            networkInterfaces.Should().NotBeNull();

            if (!this.TestOperatingSystem.IsServerBuildPlatform)
            {
                networkInterfaces.Count.Should().BeGreaterThanOrEqualTo(1);
            }

            foreach (ICoreNetworkInterface networkInterface in networkInterfaces)
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
                networkInterface.IsDnsEnabled.Should().BeTrue();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetAllActiveNetworkInterfacesWithValidSubnetMask.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetAllActiveNetworkInterfacesWithValidSubnetMask()
        {
            var networkInterfaces = this.TestNetworkingSystem.GetAllActiveNetworkInterfaces((ni) => !ni.PreferredSubnetMask.IsNullOrNone()).ToList();

            networkInterfaces.Should().NotBeNull();

            networkInterfaces.Count.Should().BeGreaterThanOrEqualTo(1);

            foreach (ICoreNetworkInterface networkInterface in networkInterfaces)
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
                networkInterface.OperationalStatus.Should().Be(OperationalStatus.Up);
                networkInterface.PreferredSubnetMask.IsNullOrNone().Should().BeFalse();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_FindPreferredNetworkAddressForNetworkDevice_UniqueID.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_FindPreferredNetworkAddressForNetworkDevice_UniqueID()
        {
            ICoreNetworkInterface? activeNetworkInterface = this.TestNetworkingSystem.GetAllActiveNetworkInterfacesWithGateway()?.FirstOrDefault();

            activeNetworkInterface.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkInterface>();
            activeNetworkInterface!.UniqueID.Should().NotBeNullOrEmpty();

            CoreIPAddressSubnet? gatewayAddressSubnet = activeNetworkInterface.GetPlatformPreferredGatewayIPAddressSubnet();

            gatewayAddressSubnet.IsNullOrNone().Should().BeFalse();

            ICorePreferredNetworkAddress? preferredNetworkAddress = this.TestNetworkingSystem.FindPreferredNetworkAddressForNetworkDevice(activeNetworkInterface.UniqueID!, gatewayAddressSubnet!.IPAddress);

            preferredNetworkAddress.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetworkAddress>();
            preferredNetworkAddress!.PreferredNetworkAddressInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkAddressInfo>();
            preferredNetworkAddress.PreferredNetworkInterface.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkInterface>();

            this.TestOutputHelper.WriteLine(preferredNetworkAddress.ToStringWithPropNameMultiLine());
            preferredNetworkAddress.PreferredNetworkInterface!.UniqueID.Should().Be(activeNetworkInterface.UniqueID);
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_OutputAllNetworkInterfaceScoredUnicastAddress.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_OutputAllNetworkInterfaceScoredUnicastAddress()
        {
            foreach (ICoreNetworkInterface networkInterface in this.TestNetworkingSystem.GetAllNetworkInterfaces())
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine();
                this.TestOutputHelper.WriteLine("Scored Unicast Addresses".CenterTitle());

                foreach (CoreUnicastIPAddressInfoScoreResult scoredUnicastAddress in networkInterface.GetScoredUnicastAddresses())
                {
                    this.TestOutputHelper.WriteLine($"    Total Score: {(int)scoredUnicastAddress.UnicastIPAddressInfoScore}");
                    this.TestOutputHelper.WriteLine($"    Scores: [{scoredUnicastAddress.UnicastIPAddressInfoScore}]");
                    this.TestOutputHelper.WriteLine($"    NetworkInterface: {scoredUnicastAddress.NetworkInterface?.DisplayName}");
                    this.TestOutputHelper.WriteLine($"    Address: {scoredUnicastAddress.UnicastAddress.Address}");
                    this.TestOutputHelper.WriteLine($"    IPv4Mask: {scoredUnicastAddress.UnicastAddress.IPv4Mask}");
                    this.TestOutputHelper.WriteLine($"    PrefixLength: {scoredUnicastAddress.UnicastAddress.PrefixLength}");

                    if (this.TestOperatingSystem.IsWindowsPlatform)
                    {
#pragma warning disable CA1416 // Validate platform compatibility
                        this.TestOutputHelper.WriteLine($"    IsTransient: {scoredUnicastAddress.UnicastAddress.IsTransient}");
                        this.TestOutputHelper.WriteLine($"    IsDnsEligible: {scoredUnicastAddress.UnicastAddress.IsDnsEligible}");
#pragma warning restore CA1416 // Validate platform compatibility
                    }

                    this.TestOutputHelper.WriteLine();
                    scoredUnicastAddress.NetworkInterface.Should().BeSameAs(networkInterface);
                }

                this.TestOutputHelper.WriteLine("*****************");
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_OutputAllNetworkInterfaceUnicastAddress.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_OutputAllNetworkInterfaceUnicastAddress()
        {
            foreach (ICoreNetworkInterface networkInterface in this.TestNetworkingSystem.GetAllNetworkInterfaces())
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine("Unicast Addresses".CenterTitle());

                foreach (UnicastIPAddressInformation unicastAddress in networkInterface.GetSystemUnicastAddresses())
                {
                    this.TestOutputHelper.WriteLine($"    Address: {unicastAddress.Address}");
                    this.TestOutputHelper.WriteLine($"    IPv4Mask: {unicastAddress.IPv4Mask}");
                    this.TestOutputHelper.WriteLine($"    PrefixLength: {unicastAddress.PrefixLength}");

                    if (this.TestOperatingSystem.IsWindowsPlatform)
                    {
#pragma warning disable CA1416 // Validate platform compatibility
                        this.TestOutputHelper.WriteLine($"    IsTransient: {unicastAddress.IsTransient}");
                        this.TestOutputHelper.WriteLine($"    IsDnsEligible: {unicastAddress.IsDnsEligible}");
#pragma warning restore CA1416 // Validate platform compatibility
                        this.TestOutputHelper.WriteLine();
                    }
                }

                this.TestOutputHelper.WriteLine("*****************");
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_OutputAllNetworkInterfaceMulticastAddresses.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_OutputAllNetworkInterfaceMulticastAddresses()
        {
            foreach (ICoreNetworkInterface networkInterface in this.TestNetworkingSystem.GetAllNetworkInterfaces())
            {
                this.TestOutputHelper.WriteLine(networkInterface.ToStringWithPropNameMultiLine());
                this.TestOutputHelper.WriteLine("Multicast Addresses".CenterTitle());

                foreach (MulticastIPAddressInformation multicastIPAddress in networkInterface.GetMulticastAddresses())
                {
                    this.TestOutputHelper.WriteLine($"    Address: {multicastIPAddress.Address}");
                    multicastIPAddress.Address.IsMulticastAddress().Should().BeTrue();

                    if (this.TestOperatingSystem.IsWindowsPlatform)
                    {
#pragma warning disable CA1416 // Validate platform compatibility
                        this.TestOutputHelper.WriteLine($"    IsTransient: {multicastIPAddress.IsTransient}");
                        this.TestOutputHelper.WriteLine($"    IsDnsEligible: {multicastIPAddress.IsDnsEligible}");
#pragma warning restore CA1416 // Validate platform compatibility
                    }

                    this.TestOutputHelper.WriteLine($"    IsMulticastDns: {multicastIPAddress.Address.Equals(multicastIPAddress.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork ? CoreIPAddressExtensions.MulticastDnsIPAddress : CoreIPAddressExtensions.MulticastDnsIPAddressV6)}");
                    this.TestOutputHelper.WriteLine();
                }

                this.TestOutputHelper.WriteLine("*****************");
                this.TestOutputHelper.WriteLine();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_FindPreferredNetworkAddressForNetworkDevice_IPAddress.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_FindPreferredNetworkAddressForNetworkDevice_IPAddress()
        {
            ICoreNetworkInterface? activeNetworkInterface = this.TestNetworkingSystem.GetAllActiveNetworkInterfacesWithGateway()?.FirstOrDefault();

            activeNetworkInterface.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkInterface>();

            ICorePreferredNetworkAddress? localNetworkAddress = activeNetworkInterface!.PreferredNetworkAddress;

            localNetworkAddress.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetworkAddress>();

            this.TestOutputHelper.WriteLine($"Local Address Info:\n{localNetworkAddress.ToStringWithParentsPropNameMultiLine()}\n");

            IPAddress? localIPAddress = localNetworkAddress?.IPAddress;

            localIPAddress.IsNullOrNone().Should().BeFalse();

            string? interfaceUniqueID = activeNetworkInterface?.UniqueID;
            interfaceUniqueID.Should().NotBeNullOrEmpty();

            this.TestOutputHelper.WriteLine($"Interface Unique ID: {interfaceUniqueID}");

            ICorePreferredNetworkAddress? preferredNetworkAddress = this.TestNetworkingSystem.FindPreferredNetworkAddressForNetworkDevice(interfaceUniqueID!, localIPAddress!);

            preferredNetworkAddress.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferredNetworkAddress>();

            preferredNetworkAddress!.PreferredNetworkAddressInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkAddressInfo>();
            preferredNetworkAddress.PreferredNetworkInterface.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkInterface>();

            preferredNetworkAddress.PreferredNetworkInterface!.UniqueID.Should().Be(interfaceUniqueID);

            preferredNetworkAddress.PreferredNetworkInterface.PreferredNetworkAddress.Should().NotBeNull();
            preferredNetworkAddress.PreferredNetworkInterface.PreferredNetworkAddress!.IPAddress.Should().Be(localIPAddress);
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_FindPreferredNetworkAddressForNetworkDevice_Null.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_FindPreferredNetworkAddressForNetworkDevice_Null()
        {
            Func<ICorePreferredNetworkAddress?> fx = () => this.TestNetworkingSystem.FindPreferredNetworkAddressForNetworkDevice((string)null!, IPAddress.None);

            fx.Should().Throw<ArgumentException>().And.ParamName.Should().Be("networkInterfaceUniqueID");

            fx = () => this.TestNetworkingSystem.FindPreferredNetworkAddressForNetworkDevice(string.Empty, IPAddress.None);

            fx.Should().Throw<ArgumentException>().And.ParamName.Should().Be("networkInterfaceUniqueID");
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetGlobalActiveTcpConnections_Output.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetGlobalActiveTcpConnections_Output()
        {
            TcpConnectionInformation[] activeTcpConnections = this.TestNetworkingSystem.GetGlobalActiveTcpConnections();

            if (this.TestNetworkingSystem.OperatingSystem.IsAndroid || this.TestNetworkingSystem.OperatingSystem.IsIOS)
            {
                this.TestOutputHelper.WriteLine("GetGlobalActiveTcpConnections is not available on IOS and Android");
                activeTcpConnections.Should().BeEmpty();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"GetGlobalActiveTcpConnections:\n\t{string.Join("\n\t", activeTcpConnections.Select(tcpConnection => $"[LocalEndPoint: {tcpConnection.LocalEndPoint}, RemoteEndPoint: {tcpConnection.RemoteEndPoint}, State: {tcpConnection.State}]"))}\n");

                activeTcpConnections.Should().NotBeEmpty();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetGlobalActiveTcpListeners_Output.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetGlobalActiveTcpListeners_Output()
        {
            CoreIPEndPoint[] activeTcpListeners = this.TestNetworkingSystem.GetGlobalActiveTcpListeners();

            if (this.TestNetworkingSystem.OperatingSystem.IsAndroid || this.TestNetworkingSystem.OperatingSystem.IsIOS)
            {
                this.TestOutputHelper.WriteLine("GetGlobalActiveTcpListeners is not available on IOS and Android");
                activeTcpListeners.Should().BeEmpty();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"GetGlobalActiveTcpListeners:\n\t{string.Join("\n\t", activeTcpListeners.Select(endpoint => endpoint.ToString()))}\n");

                activeTcpListeners.Should().NotBeEmpty();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetGlobalActiveUdpListeners_Output.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetGlobalActiveUdpListeners_Output()
        {
            CoreIPEndPoint[] activeUdpListeners = this.TestNetworkingSystem.GetGlobalActiveUdpListeners();

            if (this.TestNetworkingSystem.OperatingSystem.IsAndroid || this.TestNetworkingSystem.OperatingSystem.IsIOS)
            {
                this.TestOutputHelper.WriteLine("GetGlobalActiveUdpListeners is not available on IOS and Android");
                activeUdpListeners.Should().BeEmpty();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"GetGlobalActiveUdpListeners:\n\t{string.Join("\n\t", activeUdpListeners.Select(endpoint => endpoint.ToString()))}\n");

                activeUdpListeners.Should().NotBeEmpty();
            }
        }

        [Fact]
        public void NetworkingSystemBase_GetGlobalActiveTcpConnections_Loopback_Output()
        {
            TcpConnectionInformation[] activeTcpConnections = this.TestNetworkingSystem.GetGlobalActiveTcpConnections(item => item.LocalEndPoint.Address.Equals(IPAddress.Loopback));

            if (this.TestNetworkingSystem.OperatingSystem.IsAndroid || this.TestNetworkingSystem.OperatingSystem.IsIOS)
            {
                this.TestOutputHelper.WriteLine("GetGlobalActiveTcpConnections is not available on MacCatalyst, IOS and Android");
                activeTcpConnections.Should().BeEmpty();
            }
            else if (this.TestNetworkingSystem.OperatingSystem.IsMacCatalyst)
            {
                // MacCatalyst is flaky with this test so we're just going to output the results
                this.TestOutputHelper.WriteLine($"GetGlobalActiveTcpConnections for Loopback:\n\t{string.Join("\n\t", activeTcpConnections.Select(tcpConnection => $"[LocalEndPoint: {tcpConnection.LocalEndPoint}, RemoteEndPoint: {tcpConnection.RemoteEndPoint}, State: {tcpConnection.State}]"))}\n");

                if (activeTcpConnections.Any())
                {
                    activeTcpConnections.FirstOrDefault(item => !item.LocalEndPoint.Address.Equals(IPAddress.Loopback)).Should().BeNull();
                }
            }
            else
            {
                this.TestOutputHelper.WriteLine($"GetGlobalActiveTcpConnections for Loopback:\n\t{string.Join("\n\t", activeTcpConnections.Select(tcpConnection => $"[LocalEndPoint: {tcpConnection.LocalEndPoint}, RemoteEndPoint: {tcpConnection.RemoteEndPoint}, State: {tcpConnection.State}]"))}\n");

                activeTcpConnections.Should().NotBeEmpty();
                activeTcpConnections.FirstOrDefault(item => !item.LocalEndPoint.Address.Equals(IPAddress.Loopback)).Should().BeNull();
            }
        }

        [Fact]
        public void NetworkingSystemBase_GetGlobalActiveTcpListeners_Loopback_Output()
        {
            CoreIPEndPoint[] activeTcpListeners = this.TestNetworkingSystem.GetGlobalActiveTcpListeners(item => item.Address.Equals(IPAddress.Loopback));

            if (this.TestNetworkingSystem.OperatingSystem.IsAndroid || this.TestNetworkingSystem.OperatingSystem.IsIOS)
            {
                this.TestOutputHelper.WriteLine("GetGlobalActiveTcpListeners is not available on IOS and Android");
                activeTcpListeners.Should().BeEmpty();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"GetGlobalActiveTcpListeners for Loopback:\n\t{string.Join("\n\t", activeTcpListeners.Select(endpoint => endpoint.ToString()))}\n");

                activeTcpListeners.Should().NotBeEmpty();
                activeTcpListeners.FirstOrDefault(item => !item.Address.Equals(IPAddress.Loopback)).Should().BeNull();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetGlobalActiveUdpListeners_AnyHost_Output.
        /// </summary>
        [Fact]
        public void NetworkingSystemBase_GetGlobalActiveUdpListeners_AnyHost_Output()
        {
            CoreIPEndPoint[] activeUdpListeners = this.TestNetworkingSystem.GetGlobalActiveUdpListeners(item => item.Address.Equals(IPAddress.Any));

            if (this.TestNetworkingSystem.OperatingSystem.IsAndroid || this.TestNetworkingSystem.OperatingSystem.IsIOS)
            {
                this.TestOutputHelper.WriteLine("GetGlobalActiveUdpListeners is not available on IOS and Android");
                activeUdpListeners.Should().BeEmpty();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"GetGlobalActiveUdpListeners for AnyHost:\n\t{string.Join("\n\t", activeUdpListeners.Select(endpoint => endpoint.ToString()))}\n");

                activeUdpListeners.Should().NotBeEmpty();
                activeUdpListeners.FirstOrDefault(item => !item.Address.Equals(IPAddress.Any)).Should().BeNull();
            }
        }

        /// <summary>
        /// Defines the test method NetworkingSystemBase_GetGlobalActiveUdpListeners_PrivilegedPort_Output.
        /// </summary>
        [Fact(Skip = "Privileged ports are flaky in CI.")]
        public void NetworkingSystemBase_GetGlobalActiveUdpListeners_PrivilegedPort_Output()
        {
            int[] portsToTest = this.PrivilegedUdpPortsToTest(this.TestOperatingSystem);

            // Output ports we're testing
            this.TestOutputHelper.WriteLine("Testing privileged ports:", portsToTest);

            CoreIPEndPoint[] activeUdpListeners = this.TestNetworkingSystem.GetGlobalActiveUdpListeners(item => portsToTest.Contains(item.Port));

            if (this.TestNetworkingSystem.OperatingSystem.IsAndroid || this.TestNetworkingSystem.OperatingSystem.IsIOS)
            {
                this.TestOutputHelper.WriteLine("GetGlobalActiveUdpListeners is not available on IOS and Android");
                activeUdpListeners.Should().BeEmpty();
            }
            else if (activeUdpListeners.Any())
            {
                this.TestOutputHelper.WriteLine($"GetGlobalActiveUdpListeners for Privileged Port:\n\t{string.Join("\n\t", activeUdpListeners.Select(endpoint => endpoint.ToString()))}\n");
                activeUdpListeners.Should().NotBeEmpty();
                activeUdpListeners.FirstOrDefault(item => item.Port != 0).Should().NotBeNull();
            }
            else
            {
                this.TestOutputHelper.WriteLine($"GetGlobalActiveUdpListeners for All Ports:\n\t{string.Join("\n\t", this.TestNetworkingSystem.GetGlobalActiveUdpListeners().Select(endpoint => endpoint.ToString()))}\n");
                activeUdpListeners.Should().NotBeEmpty();
            }
        }

#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.

#if NV_PLAT_MACOS || NV_PLAT_MACCATALYST
        private class TestCoreNetworkingSystemBase : CoreNetworkingSystemBase, ICoreNetworkingSystemMacOS
#else
        private class TestCoreNetworkingSystemBase : CoreNetworkingSystemBase
#endif
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="TestCoreNetworkingSystemBase"/> class.
            /// </summary>
            /// <param name="serviceProvider">The service provider used for dependency injection.</param>
            /// <param name="fileSystem">The file system interface.</param>
            /// <param name="processRunner">The process runner interface.</param>
            /// <param name="operationRunner">The operation runner interface.</param>
            /// <param name="logger">The logger interface.</param>
            public TestCoreNetworkingSystemBase(IServiceProvider serviceProvider, ICoreFileSystem fileSystem, ICoreProcessRunner processRunner, ICoreOperationRunner operationRunner, ICoreLogger logger)
                : base(serviceProvider, fileSystem, processRunner, operationRunner, logger)
            {
            }

            public void TestOnSystemNetworkAddressChangedEvent()
            {
                this.OnSystemNetworkAddressChangedEvent(this, new EventArgs());
            }

#if NV_PLAT_MACOS || NV_PLAT_MACCATALYST
            /// <inheritdoc />
            public IList<SPNetworkDataType?> GetSystemProfileNetworkData(
                CoreTaskCacheLookupFlags taskCacheLookupFlags = CoreTaskCacheLookupFlags.CurrentCacheLookup,
                TimeSpan? timeout = null,
                CancellationToken ctx = default)
            {
                return [];
            }

            /// <inheritdoc />
            public Task<SPNetworkDataType?> GetSPNetworkDataType(
                string? networkInterfaceUniqueID,
                CoreTaskCacheLookupFlags taskCacheLookup = CoreTaskCacheLookupFlags.CurrentCacheLookup,
                TimeSpan? timeout = null,
                CancellationToken ctx = default)
            {
                return Task.FromResult<SPNetworkDataType?>(null);
            }

            public Task<ICoreTaskResult<CoreMacOSHardwareInfo?>> GetCoreMacOSHardwareInfoAsync(CoreTaskCacheLookupFlags taskCacheLookupFlags = CoreTaskCacheLookupFlags.CurrentCacheLookup, TimeSpan? timeout = null, CancellationToken ctx = default)
            {
                return Task.FromResult<ICoreTaskResult<CoreMacOSHardwareInfo?>>(new CoreTaskResult<CoreMacOSHardwareInfo?>());
            }
#endif

            public void TestOnSystemNetworkAvailabilityChangedEvent()
            {
                this.OnSystemNetworkAvailabilityChangedEvent(this, null!);
            }

            /// <inheritdoc />
            protected override Guid GetPlatformDeviceID() => CoreTestConstants.TestSystemDeviceID;

            /// <inheritdoc />.
            protected override Guid GetPlatformUserID() => CoreTestConstants.TestSystemUserID;

            /// <inheritdoc />
            protected override ICoreLocalNetworkServices PlatformCreateLocalNetworkServices(IServiceProvider serviceProvider, ICoreNetworkingSystem networkingSystem, ICoreLogger? logger = null)
            {
                return new CoreLocalNetworkServices(serviceProvider, networkingSystem, logger);
            }

            /// <inheritdoc />
            protected override ICoreWiFiNetworkManager PlatformCreateWiFiNetworkManager(IServiceProvider serviceProvider, ICoreLogger? logger = null)
            {
                return new CoreWiFiNetworkManager(serviceProvider, logger);
            }
        }
    }
}
