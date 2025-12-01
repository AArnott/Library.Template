// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-25-2020
// ***********************************************************************
// <copyright file="CoreUPnPBackgroundServiceIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>

using System.Collections.Immutable;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.Entities.Devices.UPnP;
using NetworkVisor.Core.Entities.Devices.UPnP.Notify;
using NetworkVisor.Core.Entities.Devices.UPnP.Search;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Services;
using NetworkVisor.Core.Networking.Services.UPnP.Message;
using NetworkVisor.Core.Networking.Services.UPnP.Message.Base;
using NetworkVisor.Core.Networking.Services.UPnP.Message.Device;
using NetworkVisor.Core.Networking.Services.UPnP.Message.Service;
using NetworkVisor.Core.Networking.Services.UPnP.Service;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Networking.UPnP
{
    /// <summary>
    /// Class CoreUPnPBackgroundServiceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreUPnPBackgroundServiceIntegrationTests))]

    public class CoreUPnPBackgroundServiceIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreUPnPBackgroundServiceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreUPnPBackgroundServiceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method Ctor.
        /// </summary>
        [Fact]
        public void UPnPBackgroundServiceIntegration_Ctor()
        {
            _ = this.TestOutputHelper.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestOutputHelper>();
            _ = this.TestNetworkingSystem.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkingSystem>();
            _ = this.TestNetworkingSystem.FileSystem.Should().BeSameAs(this.TestFileSystem);
            _ = this.TestNetworkingSystem.Logger.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreLogger>();

            _ = this.TestNetworkServices.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreNetworkServices>();
            _ = this.TestNetworkServices.NetworkingSystem.Should().BeSameAs(this.TestNetworkingSystem);
        }

        [Fact]
        public async Task UPnPBackgroundServiceIntegration_StartStop()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.UPnP))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.UPnP} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                return;
            }

            // using IDisposable globalLogLevel = this.BeginGlobalLogScope(LogLevel.Trace);
            var cts = new CancellationTokenSource();

            using CoreUPnPBackgroundService? uPnPBackgroundService = await this.StartUPnPBackgroundServiceAsync(cts, TimeSpan.FromSeconds(30), true, CoreTaskCacheStateFlags.NotInitialized);
            _ = uPnPBackgroundService.Should().NotBeNull();

            // Wait for 20 seconds, 5 seconds in CI.
            _ = this.TestDelay(TimeSpan.FromSeconds(CoreAppConstants.IsRunningInCI ? 5 : 30), this.TestCaseLogger, cts.Token);

            // Stop the UPnP background service and verify it stops correctly before processing results.
            _ = this.StopUPnPBackgroundService(uPnPBackgroundService!, true).Should().BeFalse();

            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.DefaultFormatted;

            this.TestOutputHelper.WriteLine($"Discovered {uPnPBackgroundService.DiscoveredUPnPDeviceNotifies.Count()} UPnP Notifications".CenterTitle());

            foreach (CoreUPnPDiscoveredNotifyEntity upnpResponse in uPnPBackgroundService.DiscoveredUPnPDeviceNotifies)
            {
                string json = JsonSerializer.Serialize(upnpResponse, typeof(CoreUPnPDiscoveredNotifyEntity), options);

                this.TestOutputHelper.WriteLine($"{json}");
            }

            var hashSet = new HashSet<string>();

            this.TestOutputHelper.WriteLine($"Discovered {uPnPBackgroundService.DiscoveredUPnPSearches.Count} UPnP Searches".CenterTitle());

            foreach (CoreUPnPDiscoveredSearchEntity upnpResponse in uPnPBackgroundService.DiscoveredUPnPSearches)
            {
                string lookupKey =
                    $"{upnpResponse.UPnPMessage.MessageTarget!} ({upnpResponse.UPnPMessage.Header?.MessageType ?? CoreUPnPMessageType.Unknown}, {upnpResponse.UPnPMessage.RemoteEndPoint?.Address ?? IPAddress.None}:{upnpResponse.UPnPMessage.RemoteEndPoint?.Port ?? 0})";

                if (string.IsNullOrEmpty(upnpResponse.UPnPMessage.MessageTarget?.Target) || !hashSet.Add(lookupKey))
                {
                    continue;
                }

                string json = JsonSerializer.Serialize(upnpResponse, typeof(CoreUPnPDiscoveredSearchEntity), options);

                this.TestOutputHelper.WriteLine($"Lookup Key: {lookupKey}\n{json}");
            }

            var groupedNotifies = uPnPBackgroundService.DiscoveredUPnPDeviceNotifies.ToImmutableList()
                .SelectMany(r => r.UPnPMessage.Properties)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(kvp => kvp.Value).ToImmutableSortedSet(),
                    StringComparer.InvariantCultureIgnoreCase);

            this.TestOutputHelper.WriteLine($"\n{"All Notify Properties".CenterTitle()}");
            foreach (KeyValuePair<string, ImmutableSortedSet<string>> kvp in groupedNotifies.OrderBy(item => item.Key))
            {
                this.TestOutputHelper.WriteLine($"{kvp.Key}:\n  {string.Join("\n  ", kvp.Value)}\n");
            }

            var groupedSearches = uPnPBackgroundService.DiscoveredUPnPSearches.ToImmutableList()
                .SelectMany(r => r.UPnPMessage.Properties)
                .GroupBy(kvp => kvp.Key)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(kvp => kvp.Value).ToImmutableSortedSet(),
                    StringComparer.InvariantCultureIgnoreCase);

            this.TestOutputHelper.WriteLine($"\n{"All Search Properties".CenterTitle()}");
            foreach (KeyValuePair<string, ImmutableSortedSet<string>> kvp in groupedSearches.OrderBy(item => item.Key))
            {
                this.TestOutputHelper.WriteLine($"{kvp.Key}:\n  {string.Join("\n  ", kvp.Value)}\n");
            }

            this.TestOutputHelper.WriteLine($"\n{"Unknown Devices".CenterTitle()}");
            foreach (CoreUPnPDiscoveredNotifyEntity discoveredNotifyEntity in uPnPBackgroundService.DiscoveredUPnPDeviceNotifies.Where(item => item.IsUnknownDevice))
            {
                string json = JsonSerializer.Serialize(discoveredNotifyEntity, typeof(CoreUPnPDiscoveredNotifyEntity), options);

                this.TestOutputHelper.WriteLine($"{json}");
            }

            this.TestOutputHelper.WriteLine($"\n{"Unknown Services".CenterTitle()}");
            foreach (CoreUPnPDiscoveredNotifyEntity discoveredNotifyEntity in uPnPBackgroundService.DiscoveredUPnPDeviceNotifies.Where(item => item.IsUnknownService))
            {
                string json = JsonSerializer.Serialize(discoveredNotifyEntity, typeof(CoreUPnPDiscoveredNotifyEntity), options);

                this.TestOutputHelper.WriteLine($"{json}");
            }
        }

        private async Task<CoreUPnPBackgroundService?> StartUPnPBackgroundServiceAsync(CancellationTokenSource cts, TimeSpan timeout, bool hookEvents, CoreTaskCacheStateFlags taskCacheStateFlags)
        {
            var uPnPBackgroundService = new CoreUPnPBackgroundService(this.TestCaseServiceProvider, this.TestNetworkServices, taskCacheStateFlags, this.TestCaseLogger);

            if (hookEvents)
            {
            }

            await uPnPBackgroundService.StartAsync(cts.Token);
            cts.CancelAfter(timeout);
            _ = uPnPBackgroundService.IsRunning.Should().BeTrue();

            return uPnPBackgroundService;
        }

        private bool StopUPnPBackgroundService(CoreUPnPBackgroundService uPnPBackgroundService, bool unhookEvents)
        {
            uPnPBackgroundService.Stop();

            _ = uPnPBackgroundService.IsRunning.Should().BeFalse();

            if (unhookEvents)
            {
            }

            return uPnPBackgroundService.IsRunning;
        }
    }
}
