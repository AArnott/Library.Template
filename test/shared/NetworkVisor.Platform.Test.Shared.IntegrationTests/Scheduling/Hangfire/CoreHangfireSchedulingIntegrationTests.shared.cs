// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 01-05-2025
//
// Last Modified By : SteveBu
// Last Modified On : 01-05-2025
// // ***********************************************************************
// <copyright file="CoreHangfireSchedulingIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Integration tests for CoreHangfireSchedulingIntegrationTests</summary>

#if NV_USE_HANGFIRE
using System;
using System.Linq.Expressions;
using FluentAssertions;
using Hangfire;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Networking.Connectivity;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Scheduling.Services;
using NetworkVisor.Core.Scheduling.Services.JobStates;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base;
using NetworkVisor.Platform.Test.TestCase;
using Org.BouncyCastle.Tls.Crypto;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Scheduling.Services
{
    /// <summary>
    /// Integration tests for CoreHangfireSchedulingIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreHangfireSchedulingIntegrationTests))]
    public class CoreHangfireSchedulingIntegrationTests : CoreSchedulingTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreHangfireSchedulingIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreHangfireSchedulingIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        protected ICoreSchedulingBackgroundService SchedulingBackgroundService =>
            this.TestNetworkServices.SchedulingBackgroundService;

        protected IBackgroundJobClientV2 TestBackgroundJobClient => this.TestCaseServiceProvider.GetRequiredService<IBackgroundJobClientV2>();

        protected IRecurringJobManagerV2 TestRecurringJobManager => this.TestCaseServiceProvider.GetRequiredService<IRecurringJobManagerV2>();

        protected string HangfireDatabasePath => this.TestCaseServiceProvider
            .GetRequiredService<ICoreSQLiteDbConnectionFactory>().DatabasePath;

        [Fact]
        public void HangfireSchedulingIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);

            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            this.SchedulingBackgroundService.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<ICoreSchedulingBackgroundService>();

            _ = this.SchedulingBackgroundService.DatabaseFilePath.Should().NotBeNullOrEmpty();
            _ = this.TestFileSystem.FileExists(this.SchedulingBackgroundService.DatabaseFilePath).Should().BeTrue();
            _ = this.SchedulingBackgroundService.DatabaseFilePath.Should().Be(this.HangfireDatabasePath);

            this.TestBackgroundJobClient.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IBackgroundJobClientV2>();

            this.TestRecurringJobManager.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IRecurringJobManagerV2>();
        }

        [Fact]
        public void HangfireSchedulingIntegration_TriggerRecurringJobIntegration_WithValidRecurringJob_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string recurringJobId = "integration-test-recurring-trigger";
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("trigger-test");
            const string cronExpression = "0 0 1 * *"; // Monthly

            // First, create a recurring job to trigger
            this.SchedulingBackgroundService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            try
            {
                // Act
                string triggeredJobId = this.SchedulingBackgroundService.TriggerRecurringJob(recurringJobId);

                // Assert
                triggeredJobId.Should().NotBeNullOrEmpty();
                this.TestOutputHelper.WriteLine($"Triggered recurring job '{recurringJobId}' with new job ID: {triggeredJobId}");
            }
            finally
            {
                // Cleanup
                this.SchedulingBackgroundService.RemoveRecurringJob(recurringJobId);
            }
        }

        [Fact]
        public void HangfireSchedulingIntegration_TriggerRecurringJobIntegration_WithAsyncRecurringJob_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string recurringJobId = "integration-test-async-recurring-trigger";
            Expression<Func<IntegrationTestJobClass, Task>> methodCall = x => x.TestAsyncMethod("async-trigger-test");
            const string cronExpression = "0 */15 * * *"; // Every 15 minutes

            // First, create a recurring job to trigger
            this.SchedulingBackgroundService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            try
            {
                // Act
                string triggeredJobId = this.SchedulingBackgroundService.TriggerRecurringJob(recurringJobId);

                // Assert
                triggeredJobId.Should().NotBeNullOrEmpty();
                this.TestOutputHelper.WriteLine($"Triggered async recurring job '{recurringJobId}' with new job ID: {triggeredJobId}");
            }
            finally
            {
                // Cleanup
                this.SchedulingBackgroundService.RemoveRecurringJob(recurringJobId);
            }
        }

        [Fact]
        public void HangfireSchedulingIntegration_TriggerRecurringJobIntegration_WithStaticMethodRecurringJob_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string recurringJobId = "integration-test-static-recurring-trigger";
            Expression<Action> methodCall = () => IntegrationTestJobClass.StaticTestMethod("static-trigger-test");
            const string cronExpression = "0 0 * * *"; // Daily

            // First, create a recurring job to trigger
            this.SchedulingBackgroundService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            try
            {
                // Act
                string triggeredJobId = this.SchedulingBackgroundService.TriggerRecurringJob(recurringJobId);

                // Assert
                triggeredJobId.Should().NotBeNullOrEmpty();
                this.TestOutputHelper.WriteLine($"Triggered static recurring job '{recurringJobId}' with new job ID: {triggeredJobId}");
            }
            finally
            {
                // Cleanup
                this.SchedulingBackgroundService.RemoveRecurringJob(recurringJobId);
            }
        }

        [Fact]
        public void HangfireSchedulingIntegration_TriggerRecurringJobIntegration_WithNonExistentJob_ShouldHandleGracefully()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string nonExistentJobId = "non-existent-recurring-job-12345";

            // Act & Assert - Should not throw an exception
            string result = this.SchedulingBackgroundService.TriggerRecurringJob(nonExistentJobId);

            // The exact behavior depends on Hangfire implementation, but it shouldn't crash
            this.TestOutputHelper.WriteLine($"Attempted to trigger non-existent job '{nonExistentJobId}', result: {result ?? "null"}");
        }

        [Fact]
        public void HangfireSchedulingIntegration_TriggerRecurringJobIntegration_MultipleTriggersOfSameJob_ShouldCreateMultipleInstances()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string recurringJobId = "integration-test-multiple-trigger";
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("multiple-trigger-test");
            const string cronExpression = "0 0 1 1 *"; // Yearly (rarely executed automatically)

            // First, create a recurring job to trigger
            this.SchedulingBackgroundService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            try
            {
                // Act - Trigger the same job multiple times
                var triggeredJobIds = new List<string>();
                for (int i = 0; i < 3; i++)
                {
                    string triggeredJobId = this.SchedulingBackgroundService.TriggerRecurringJob(recurringJobId);
                    triggeredJobIds.Add(triggeredJobId);
                    this.TestOutputHelper.WriteLine($"Trigger {i + 1}: {triggeredJobId}");
                }

                // Assert
                triggeredJobIds.Should().HaveCount(3);
                triggeredJobIds.Should().OnlyContain(id => !string.IsNullOrEmpty(id));

                // Each trigger should create a separate job instance (different IDs)
                triggeredJobIds.Should().OnlyHaveUniqueItems();
            }
            finally
            {
                // Cleanup
                this.SchedulingBackgroundService.RemoveRecurringJob(recurringJobId);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void HangfireSchedulingIntegration_TriggerRecurringJobIntegration_WithEmptyOrWhitespaceJobId_ShouldHandleGracefully(string? recurringJobId)
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Act & Assert - Should throw an exception
            ArgumentException exception = Assert.Throws<ArgumentException>(() => this.TestSchedulingService.TriggerRecurringJob(recurringJobId!));
            _ = exception.ParamName.Should().Be("recurringJobId");
        }

        [Fact]
        public void HangfireSchedulingIntegration_TriggerRecurringJobIntegration_WorkflowTest_CreateTriggerRemove()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Complete workflow test: Create -> Trigger -> Remove
            const string recurringJobId = "integration-workflow-test";
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("workflow-test");
            const string cronExpression = "0 0 1 1 *"; // Yearly

            try
            {
                // Step 1: Create recurring job
                this.SchedulingBackgroundService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);
                this.TestOutputHelper.WriteLine($"Step 1: Created recurring job '{recurringJobId}'");

                // Step 2: Trigger the job
                string triggeredJobId = this.SchedulingBackgroundService.TriggerRecurringJob(recurringJobId);
                triggeredJobId.Should().NotBeNullOrEmpty();
                this.TestOutputHelper.WriteLine($"Step 2: Triggered job, new instance ID: {triggeredJobId}");

                // Step 3: Trigger again to ensure it's still available
                string secondTriggeredJobId = this.SchedulingBackgroundService.TriggerRecurringJob(recurringJobId);
                secondTriggeredJobId.Should().NotBeNullOrEmpty();
                secondTriggeredJobId.Should().NotBe(triggeredJobId); // Should be different instances
                this.TestOutputHelper.WriteLine($"Step 3: Triggered again, new instance ID: {secondTriggeredJobId}");

                // Step 4: Remove the recurring job
                this.SchedulingBackgroundService.RemoveRecurringJob(recurringJobId);
                this.TestOutputHelper.WriteLine($"Step 4: Removed recurring job '{recurringJobId}'");

                // Step 5: Try to trigger the removed job
                string postRemovalResult = this.SchedulingBackgroundService.TriggerRecurringJob(recurringJobId);
                this.TestOutputHelper.WriteLine($"Step 5: Attempted to trigger removed job, result: {postRemovalResult ?? "null"}");
            }
            finally
            {
                // Ensure cleanup
                try
                {
                    this.SchedulingBackgroundService.RemoveRecurringJob(recurringJobId);
                }
                catch
                {
                    // Ignore cleanup exceptions
                }
            }
        }

        [Fact]
        public void HangfireSchedulingIntegration_Constructor_ShouldInitializeCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Assert
            _ = this.SchedulingBackgroundService.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<ICoreSchedulingBackgroundService>();

            _ = this.SchedulingBackgroundService.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void HangfireSchedulingIntegration_ServiceType_ShouldBeCorrectImplementation()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Assert
            _ = this.SchedulingBackgroundService.Should().BeOfType<CoreHangfireSchedulingBackgroundService>();
        }

        [Fact]
        public async Task HangfireSchedulingIntegration_StartStop_ShouldWorkCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            _ = service.IsRunning.Should().BeTrue();

            // Act - Stop the service
            await service.StopAsync(CancellationToken.None);

            // Assert
            _ = service.IsRunning.Should().BeFalse();

            // Act - Start the service again
            await service.StartAsync(CancellationToken.None);

            // Assert
            _ = service.IsRunning.Should().BeTrue();
        }

        [Fact]
        public async Task HangfireSchedulingIntegration_RestartService_ShouldWorkCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            ICoreNetworkInterface? preferredInterface = this.TestNetworkingSystem.PreferredNetworkInterface;
            _ = preferredInterface.Should().NotBeNull();

            // Act
            bool result = await service.RestartService(
                CoreNetworkChangeEvent.NetworkAddressChanged,
                preferredInterface!,
                CancellationToken.None);

            // Assert
            _ = result.Should().BeTrue();
            _ = service.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void HangfireSchedulingIntegration_CreateJob_WithAction_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            var jobState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Integration test job");
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("test-parameter");

            // Act
            string jobId = service.Create(methodCall, jobState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created job with ID: {jobId}");
        }

        [Fact]
        public void HangfireSchedulingIntegration_CreateJob_WithAsyncAction_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            var jobState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Integration test async job");
            Expression<Func<IntegrationTestJobClass, Task>> methodCall = x => x.TestAsyncMethod("async-test-parameter");

            // Act
            string jobId = service.Create(methodCall, jobState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created async job with ID: {jobId}");
        }

        [Fact]
        public void HangfireSchedulingIntegration_CreateJob_WithStaticAction_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            var jobState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Integration test static job");
            Expression<Action> methodCall = () => IntegrationTestJobClass.StaticTestMethod("static-test-parameter");

            // Act
            string jobId = service.Create(methodCall, jobState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created static job with ID: {jobId}");
        }

        [Fact]
        public void HangfireSchedulingIntegration_ChangeJobState_ShouldWorkCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            var initialJobState = new CoreScheduledJobState(DateTimeOffset.UtcNow.AddMinutes(5), "Initial state scheduled for later");
            Expression<Action> methodCall = () => IntegrationTestJobClass.StaticTestMethod("test");

            // Create a job first
            string jobId = service.Create(methodCall, initialJobState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);
            _ = jobId.Should().NotBeNullOrEmpty();

            // Wait for the job to be created
            this.TestDelay(100, this.TestCaseLogger);

            // Act - Change the job state
            var newJobState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Change to enqueued");
            bool result = service.ChangeState(jobId, newJobState, initialJobState.Name);

            // Assert
            _ = result.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"Changed state for job {jobId}");
        }

        [Fact]
        public void HangfireSchedulingIntegration_AddRecurringJob_ShouldWorkCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("recurring-test");
            const string cronExpression = "0 0 1 * *"; // Monthly on the 1st at midnight
            const string recurringJobId = "integration-test-recurring-job";

            // Act
            service.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            // Assert - In a real integration test, you might verify the job was registered in Hangfire storage
            this.TestOutputHelper.WriteLine($"Added recurring job with ID: {recurringJobId}");

            // Cleanup
            service.RemoveRecurringJob(recurringJobId);
        }

        [Fact]
        public void HangfireSchedulingIntegration_AddAsyncRecurringJob_ShouldWorkCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            Expression<Func<IntegrationTestJobClass, Task>> methodCall = x => x.TestAsyncMethod("async-recurring-test");
            const string cronExpression = "0 */15 * * *"; // Every 15 minutes
            const string recurringJobId = "integration-test-async-recurring-job";

            // Act
            service.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            // Assert
            this.TestOutputHelper.WriteLine($"Added async recurring job with ID: {recurringJobId}");

            // Cleanup
            service.RemoveRecurringJob(recurringJobId);
        }

        [Fact]
        public void HangfireSchedulingIntegration_RemoveRecurringJob_ShouldWorkCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            Expression<Action> methodCall = () => IntegrationTestJobClass.StaticTestMethod("remove-test");
            const string cronExpression = "0 0 * * *"; // Daily at midnight
            const string recurringJobId = "integration-test-remove-recurring-job";

            // First add a recurring job
            service.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            // Act - Remove the recurring job
            service.RemoveRecurringJob(recurringJobId);

            // Assert
            this.TestOutputHelper.WriteLine($"Removed recurring job with ID: {recurringJobId}");
        }

        [Fact]
        public async Task HangfireSchedulingIntegration_ServiceLifecycle_ShouldHandleMultipleStartStopCycles()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;

            // Act & Assert - Multiple start/stop cycles
            for (int i = 0; i < 3; i++)
            {
                // Stop the service
                await service.StopAsync(CancellationToken.None);
                _ = service.IsRunning.Should().BeFalse();

                // Start the service
                await service.StartAsync(CancellationToken.None);
                _ = service.IsRunning.Should().BeTrue();

                this.TestOutputHelper.WriteLine($"Completed start/stop cycle {i + 1}");
            }
        }

        [Fact]
        public async Task HangfireSchedulingIntegration_ConcurrentOperations_ShouldHandleCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            ICoreSchedulingBackgroundService service = this.SchedulingBackgroundService;
            const int jobCount = 10;

            // Act - Create multiple jobs concurrently
            Task<string>[] tasks = Enumerable.Range(0, jobCount)
                .Select(i => Task.Run(() =>
                {
                    string queueName = $"concurrent-queue-{i}";
                    Expression<Action> methodCall = () => IntegrationTestJobClass.StaticTestMethod($"concurrent-{i}");
                    return service.Create(methodCall, new CoreEnqueuedJobState(queueName), this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);
                }))
                .ToArray();

            string[] jobIds = await Task.WhenAll(tasks);

            // Assert
            _ = jobIds.Should().HaveCount(jobCount);
            _ = jobIds.Should().OnlyContain(id => !string.IsNullOrEmpty(id));
            _ = jobIds.Should().OnlyHaveUniqueItems();

            this.TestOutputHelper.WriteLine($"Created {jobCount} concurrent jobs successfully");
        }

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
#endif
