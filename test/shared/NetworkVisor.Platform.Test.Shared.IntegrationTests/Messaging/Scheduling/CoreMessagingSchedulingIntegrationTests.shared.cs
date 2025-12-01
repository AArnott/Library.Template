// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-26-2020
// // ***********************************************************************
// <copyright file="CoreMessagingSchedulingIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
#if NV_USE_HANGFIRE_MESSAGING
using Hangfire;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using Hangfire.Storage.SQLite;
using NetworkVisor.Core.Scheduling.Services;
#endif
using System.Net.NetworkInformation;
using System.Reactive;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Networking.Extensions;
using NetworkVisor.Core.Networking.Services.Ping.Commands;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Messaging.Scheduling
{
    /// <summary>
    /// Class CoreSchedulingIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreMessagingSchedulingIntegrationTests))]

    public class CoreMessagingSchedulingIntegrationTests : CoreCommandTestCaseBase
    {
        private JsonSerializerOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreMessagingSchedulingIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreMessagingSchedulingIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider);
        }

        [Fact]
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        public async Task MessagingSchedulingIntegration_Ctor()
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            _ = this.TestMessagingDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMessagingDatabase>();
            _ = this.TestMessagingDatabasePath.Should().NotBeNull();
            _ = this.TestFileSystem.FileExists(this.TestMessagingDatabasePath).Should().BeTrue();

#if NV_USE_HANGFIRE_MESSAGING

            this.TestOperatingSystem.IsWPF.Should().BeFalse("Hangfire is not supported in WPF applications.");

            this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.HangfireScheduler)
                .Should().BeTrue($"Hangfire Scheduler is not supported on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

            this.TestNetworkServices.SchedulingBackgroundService.Should().NotBeNull()
                .And.Subject.Should().BeAssignableTo<ICoreSchedulingBackgroundService>();

            this.TestNetworkServices.SchedulingBackgroundService.IsRunning.Should().BeTrue();

            IBackgroundJobClientV2 backgroundJobScheduler = this.TestCaseServiceProvider.GetRequiredService<IBackgroundJobClientV2>();
            _ = backgroundJobScheduler.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IBackgroundJobClientV2>();

            IRecurringJobManagerV2 recurringJobManager = this.TestCaseServiceProvider.GetRequiredService<IRecurringJobManagerV2>();
            _ = recurringJobManager.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IRecurringJobManagerV2>();

            IMonitoringApi? monitoringApi = backgroundJobScheduler.Storage.GetMonitoringApi();
            monitoringApi.Should().NotBeNull("Hangfire Monitoring API should be available when Hangfire is configured correctly.");

            var pingCommand = new CorePingCommand(CoreIPAddressExtensions.GetRandomPublicServerAddress());
            string requestID = await this.TestCommandProcessor.SendAsync(TimeSpan.FromSeconds(1), pingCommand);
            requestID.Should().NotBeEmpty();

            this.TestOutputHelper.WriteLine($"{"Servers".CenterTitle()}\n");

            // Print out list of servers for debugging purposes.
            foreach (ServerDto server in monitoringApi.Servers())
            {
                server.Should().NotBeNull();
                string json = JsonSerializer.Serialize(server, this._options);

                this.TestOutputHelper.WriteLine($"{server.Name.CenterTitle()}\n{json}\n");
            }

            this.TestOutputHelper.WriteLine($"{"Job Details".CenterTitle()}\n");

            // Print out list of job details for debugging purposes.
            foreach (KeyValuePair<string, ScheduledJobDto> job in monitoringApi.ScheduledJobs(0, int.MaxValue))
            {
                job.Should().NotBeNull();
                this.TestOutputHelper.WriteLine($"ID: {job.Key}");
                this.TestOutputHelper.WriteLine($"MethodName: {job.Value.Job.Method.Name}");
                this.TestOutputHelper.WriteLine($"EnqueueAt: {job.Value.EnqueueAt}");
                this.TestOutputHelper.WriteLine($"ScheduledAt: {job.Value.ScheduledAt}");

                if (job.Value.StateData is not null)
                {
                    this.TestOutputHelper.WriteLine($"StateData [{string.Join(", ", job.Value.StateData)}]");
                }
            }
#endif
        }

#if NV_USE_HANGFIRE_MESSAGING
        [Fact]
        public async Task MessagingSchedulingIntegration_HangFire_Schedule_PingCommandAsync()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            IBackgroundJobClientV2 backgroundJobScheduler = this.TestCaseServiceProvider.GetRequiredService<IBackgroundJobClientV2>();
            _ = backgroundJobScheduler.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IBackgroundJobClientV2>();

            IRecurringJobManagerV2 recurringJobManager = this.TestCaseServiceProvider.GetRequiredService<IRecurringJobManagerV2>();
            _ = recurringJobManager.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IRecurringJobManagerV2>();

            var pingCommand = new CorePingCommand(CoreIPAddressExtensions.GetRandomPublicServerAddress());
            var requestID = await this.TestCommandProcessor.SendAsync(TimeSpan.FromSeconds(1), pingCommand);
            requestID.Should().NotBeEmpty();

            this.TestDelay(20000, this.TestCaseLogger);
        }
#endif

        protected bool IsHangfireSchedulerSupported()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.HangfireScheduler))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.HangfireScheduler} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
#if (NV_USE_HANGFIRE || NV_USE_HANGFIRE_MESSAGING) && !NET472_OR_GREATER && !NETSTANDARD2_0_OR_GREATER
                throw new InvalidOperationException("Hangfire Scheduler should only be disabled on NET472");
#else
                return false;
#endif
            }

            return true;
        }
    }
}
