// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreNetworkServicesIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Net;
using FluentAssertions;
using NetworkVisor.Core.Cloud.Client;
using NetworkVisor.Core.Entities.Database;
using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Messaging.Services.CommandDispatch;
using NetworkVisor.Core.Messaging.Services.CommandProcessor;
using NetworkVisor.Core.Messaging.Services.QueryProcessor;
using NetworkVisor.Core.Networking.CoreIP;
using NetworkVisor.Core.Networking.Devices;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Networking.Services.Agent;
using NetworkVisor.Core.Networking.Services.Dhcp.BackgroundService;
using NetworkVisor.Core.Networking.Services.MulticastDns.Service;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Preferences;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Paramore.Brighter;
using Paramore.Darker;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking
{
    /// <summary>
    /// Class CoreNetworkServicesIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreNetworkServicesIntegrationTests))]

    public class CoreNetworkServicesIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNetworkServicesIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNetworkServicesIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void NetworkServices_Ctor()
        {
            // Validate this.TestNetworkingSystem.NetworkServices is resolved
            _ = this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            _ = this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            _ = this.TestNetworkingSystem.NetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            _ = this.TestNetworkingSystem.NetworkServices!.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);

            _ = this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServicesHost>();
            _ = this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            _ = this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreHostedService>();
            _ = this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkConnectivityClient>();
            _ = this.TestNetworkServices.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);

            _ = this.TestNetworkServices.CloudClient.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCloudClient>();

            _ = this.TestNetworkServices.CommandDispatchService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandDispatchService>();
            _ = this.TestNetworkServices.CommandDispatchService.Should().BeAssignableTo<ICoreCommandDispatchService>();

            _ = this.TestNetworkServices.CommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCommandProcessor>();
            _ = this.TestNetworkServices.CommandProcessor.Should().BeAssignableTo<IAmACommandProcessor>();

            _ = this.TestNetworkServices.QueryProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreQueryProcessor>();
            _ = this.TestNetworkServices.QueryProcessor.Should().BeAssignableTo<IQueryProcessor>();

            _ = this.TestNetworkServices.Preferences.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferences>();
            _ = this.TestNetworkServices.Preferences!.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);
        }

        [Fact]
        public void NetworkServices_Host_Ctor()
        {
            _ = this.TestNetworkServicesHost.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServicesHost>();
            _ = this.TestNetworkServicesHost.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            _ = this.TestNetworkServicesHost.Should().BeSameAs(this.TestNetworkServices);

            _ = this.TestNetworkServicesHost.DhcpBackgroundService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreDhcpBackgroundService>();
            _ = this.TestNetworkServicesHost.MulticastDnsBackgroundService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMulticastDnsBackgroundService>();
            _ = this.TestNetworkServicesHost.NetworkAgentBackgroundService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkAgentBackgroundService>();
            _ = this.TestNetworkServicesHost.MessagingDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMessagingDatabase>();
            _ = this.TestNetworkServicesHost.EntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreEntityDatabase>();
        }

        [Fact]
        public void NetworkServices_MessagingDatabase()
        {
            _ = this.TestNetworkServicesHost.MessagingDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMessagingDatabase>();
            _ = this.TestNetworkServicesHost.MessagingDatabase.DatabasePath.Should().NotBeNullOrEmpty();
            _ = this.TestFileSystem.FileExists(this.TestNetworkServicesHost.MessagingDatabase.DatabasePath).Should().BeTrue();
            this.TestOutputHelper.WriteLine($"Messaging database path: {this.TestNetworkServicesHost.MessagingDatabase.DatabasePath}");
        }

        [Fact]
        public void NetworkServices_EntityDatabase()
        {
            _ = this.TestNetworkServicesHost.EntityDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreEntityDatabase>();
            _ = this.TestNetworkServicesHost.EntityDatabase.DatabasePath.Should().NotBeNullOrEmpty();
            _ = this.TestFileSystem.FileExists(this.TestNetworkServicesHost.EntityDatabase.DatabasePath).Should().BeTrue();
            this.TestOutputHelper.WriteLine($"Entity database path: {this.TestNetworkServicesHost.EntityDatabase.DatabasePath}");
        }

        [Fact]
        public void NetworkServices_TestCase_Ctor()
        {
            // Validate this.TestNetworkingSystem.NetworkServices is resolved
            _ = this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            _ = this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            _ = this.TestNetworkingSystem.NetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            _ = this.TestNetworkingSystem.NetworkServices!.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);

            _ = this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServicesHost>();
            _ = this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            _ = this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreHostedService>();
            _ = this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkConnectivityClient>();
            _ = this.TestNetworkServices.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);

            _ = this.TestCloudClient.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCloudClient>();

            _ = this.TestCommandDispatchService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandDispatchService>();
            _ = this.TestCommandDispatchService.Should().BeAssignableTo<ICoreCommandDispatchService>();
            _ = this.TestCommandDispatchService.Should().BeSameAs(this.TestNetworkServices.CommandDispatchService);

            _ = this.CommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCommandProcessor>();
            _ = this.CommandProcessor.Should().BeAssignableTo<IAmACommandProcessor>();
            _ = this.CommandProcessor.Should().BeSameAs(this.TestNetworkServices.CommandProcessor);

            _ = this.TestQueryProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreQueryProcessor>();
            _ = this.TestQueryProcessor.Should().BeAssignableTo<IQueryProcessor>();
            _ = this.TestQueryProcessor.Should().BeSameAs(this.TestNetworkServices.QueryProcessor);

            _ = this.TestNetworkServices.Preferences.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICorePreferences>();
            _ = this.TestNetworkServices.Preferences!.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);
        }

        /// <summary>
        /// networking services gateway ip address information for local address as an asynchronous operation.
        /// </summary>
        [Fact]
        public async Task NetworkServices_NetworkingSystem_GetPublicIPAddressAsync()
        {
            _ = this.TestNetworkServices.CloudClient.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreCloudClient>();
            _ = this.TestNetworkServices.CloudClient.Should().BeSameAs(this.TestCloudClient);

            IPAddress? publicIPAddress = await this.TestNetworkServices.NetworkingSystem.GetPublicIPAddressAsync();
            _ = publicIPAddress.Should().NotBeNull();
            this.TestOutputHelper.WriteLine($"Public IPAddress: {publicIPAddress}");
        }

        /// <summary>
        /// networking services gateway ip address information for local address as an asynchronous operation.
        /// </summary>
        [Fact]
        public void NetworkServices_GatewayIPAddressInfoFromLocalIPAddressSubnet()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.GatewayPhysicalAddress))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.GatewayPhysicalAddress} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            _ = this.TestNetworkServices.PreferredLocalNetworkAddress.Should().NotBeNull();
            CoreIPAddressSubnet ipAddressSubnet = this.TestNetworkServices.PreferredLocalNetworkAddress!.IPAddressSubnet;

            this.TestOutputHelper.WriteLine($"IP Address subnet: {ipAddressSubnet}");

            _ = ipAddressSubnet.IsNullOrNone().Should().BeFalse();

            ICoreNetworkGatewayAddressInfo? gatewayIPAddressInfo = this.TestNetworkServices.GatewayIPAddressInfoFromLocalIPAddress(ipAddressSubnet.IPAddress);
            this.TestOutputHelper.WriteLine($"Gateway IPAddressInfo: {gatewayIPAddressInfo}");

            _ = gatewayIPAddressInfo.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkAddressInfo>();

            // Make sure Gateway IP is not the same as the preferred local address.
            _ = gatewayIPAddressInfo!.IPAddressSubnet.Should().NotBe(ipAddressSubnet);

            // Make sure Gateway and ip address subnets are the same.
            _ = gatewayIPAddressInfo.SubnetMask.Should().Be(ipAddressSubnet.SubnetMask);

            // Make sure ipAddressSubnet is routable on gateway
            _ = gatewayIPAddressInfo.IPAddressSubnet.CanGatewayRouteIPAddress(ipAddressSubnet.IPAddress).Should().BeTrue();

            _ = gatewayIPAddressInfo.IPAddressSubnet.IsNullOrNone().Should().BeFalse();
            _ = gatewayIPAddressInfo.SubnetMask.IsNullOrNone().Should().BeFalse();
            _ = gatewayIPAddressInfo.IPAddress.IsNullOrNone().Should().BeFalse();

            _ = gatewayIPAddressInfo.PhysicalAddress.IsNullOrNone().Should().BeFalse();
        }
    }
}
