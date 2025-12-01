// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 01-05-2025
//
// Last Modified By : SteveBu
// Last Modified On : 01-05-2025
// // ***********************************************************************
// <copyright file="CoreHangfireSchedulingExtensionsIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Integration tests for CoreSchedulingBackgroundServiceExtensions</summary>

using System.Linq.Expressions;
using System.Text.Json;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Scheduling.Services;
using NetworkVisor.Core.Scheduling.Services.JobStates;
using NetworkVisor.Core.Scheduling.Services.Monitoring;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;
using Xunit.Sdk;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Scheduling.Hangfire
{
    /// <summary>
    /// Integration tests for CoreSchedulingBackgroundServiceExtensions.
    /// </summary>
    [PlatformTrait(typeof(CoreSchedulingBackgroundServiceExtensionsIntegrationTests))]
    public class CoreSchedulingBackgroundServiceExtensionsIntegrationTests : CoreSchedulingTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSchedulingBackgroundServiceExtensionsIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSchedulingBackgroundServiceExtensionsIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void SchedulingExtensionsIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void SchedulingExtensionsIntegration_ServiceSetup()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Assert
            _ = this.TestSchedulingService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreSchedulingBackgroundService>();
            _ = this.TestSchedulingService.IsRunning.Should().BeTrue();

            _ = this.TestSchedulingService.DatabaseFilePath.Should().NotBeNullOrEmpty();
            _ = this.TestFileSystem.FileExists(this.TestSchedulingService.DatabaseFilePath).Should().BeTrue();
        }

        [Fact]
        public void TriggerRecurringJobIntegration_WithValidRecurringJob_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string recurringJobId = $"integration-test-recurring-trigger-{Guid.NewGuid().ToStringNoDashes()}";
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("trigger-test");
            const string cronExpression = "0 0 1 * *"; // Monthly

            // First, create a recurring job to trigger
            this.TestSchedulingService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            try
            {
                // Act
                string triggeredJobId = this.TestSchedulingService.TriggerRecurringJob(recurringJobId);

                // Assert
                _ = triggeredJobId.Should().NotBeNullOrEmpty();
                this.TestOutputHelper.WriteLine($"Triggered recurring job '{recurringJobId}' with new job ID: {triggeredJobId}");
            }
            finally
            {
                // Cleanup
                this.TestSchedulingService.RemoveRecurringJob(recurringJobId);
            }
        }

        [Fact]
        public void TriggerRecurringJobIntegration_WithAsyncRecurringJob_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string recurringJobId = $"integration-test-async-recurring-trigger-{Guid.NewGuid().ToStringNoDashes()}";
            Expression<Func<IntegrationTestJobClass, Task>> methodCall = x => x.TestAsyncMethod("async-trigger-test");
            const string cronExpression = "0 */15 * * *"; // Every 15 minutes

            // First, create a recurring job to trigger
            this.TestSchedulingService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            try
            {
                // Act
                string triggeredJobId = this.TestSchedulingService.TriggerRecurringJob(recurringJobId);

                // Assert
                _ = triggeredJobId.Should().NotBeNullOrEmpty();
                this.TestOutputHelper.WriteLine($"Triggered async recurring job '{recurringJobId}' with new job ID: {triggeredJobId}");
            }
            finally
            {
                // Cleanup
                this.TestSchedulingService.RemoveRecurringJob(recurringJobId);
            }
        }

        [Fact]
        public void TriggerRecurringJobIntegration_WithStaticMethodRecurringJob_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string recurringJobId = $"integration-test-static-recurring-trigger-{Guid.NewGuid().ToStringNoDashes()}";
            Expression<Action> methodCall = () => IntegrationTestJobClass.StaticTestMethod("static-trigger-test");
            const string cronExpression = "0 0 * * *"; // Daily

            // First, create a recurring job to trigger
            this.TestSchedulingService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            try
            {
                // Act
                string triggeredJobId = this.TestSchedulingService.TriggerRecurringJob(recurringJobId);

                // Assert
                _ = triggeredJobId.Should().NotBeNullOrEmpty();
                this.TestOutputHelper.WriteLine($"Triggered static recurring job '{recurringJobId}' with new job ID: {triggeredJobId}");
            }
            finally
            {
                // Cleanup
                this.TestSchedulingService.RemoveRecurringJob(recurringJobId);
            }
        }

        [Fact]
        public void TriggerRecurringJobIntegration_WithNonExistentJob_ShouldHandleGracefully()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string nonExistentJobId = "non-existent-recurring-job-12345";

            // Act & Assert - Should not throw an exception
            string result = this.TestSchedulingService.TriggerRecurringJob(nonExistentJobId);

            // The exact behavior depends on Hangfire implementation, but it shouldn't crash
            this.TestOutputHelper.WriteLine($"Attempted to trigger non-existent job '{nonExistentJobId}', result: {result ?? "null"}");
        }

        [Fact]
        public void TriggerRecurringJobIntegration_MultipleTriggersOfSameJob_ShouldCreateMultipleInstances()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string recurringJobId = $"integration-test-multiple-trigger-{Guid.NewGuid().ToStringNoDashes()}";
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("multiple-trigger-test");
            const string cronExpression = "0 0 1 1 *"; // Yearly (rarely executed automatically)

            // First, create a recurring job to trigger
            this.TestSchedulingService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);

            try
            {
                // Act - Trigger the same job multiple times
                var triggeredJobIds = new List<string>();

                for (int i = 0; i < 3; i++)
                {
                    string triggeredJobId = this.TestSchedulingService.TriggerRecurringJob(recurringJobId);
                    triggeredJobIds.Add(triggeredJobId);
                    this.TestOutputHelper.WriteLine($"Trigger {i + 1}: {triggeredJobId}");
                }

                // Assert
                _ = triggeredJobIds.Should().HaveCount(3);
                _ = triggeredJobIds.Should().OnlyContain(id => !string.IsNullOrEmpty(id));

                // Each trigger should create a separate job instance (different IDs)
                _ = triggeredJobIds.Should().OnlyHaveUniqueItems();
            }
            finally
            {
                // Cleanup
                this.TestSchedulingService.RemoveRecurringJob(recurringJobId);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void TriggerRecurringJobIntegration_WithEmptyOrWhitespaceJobId_ShouldThrowException(string? recurringJobId)
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
        public void TriggerRecurringJobIntegration_WorkflowTest_CreateTriggerRemove()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Complete workflow test: Create -> Trigger -> Remove
            string recurringJobId = $"integration-workflow-test-{Guid.NewGuid().ToStringNoDashes()}";
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("workflow-test");
            const string cronExpression = "0 0 1 1 *"; // Yearly

            try
            {
                // Step 1: Create recurring job
                this.TestSchedulingService.AddOrUpdateRecurringJob(recurringJobId, methodCall, cronExpression);
                this.TestOutputHelper.WriteLine($"Step 1: Created recurring job '{recurringJobId}'");

                // Step 2: Trigger the job
                string triggeredJobId = this.TestSchedulingService.TriggerRecurringJob(recurringJobId);
                _ = triggeredJobId.Should().NotBeNullOrEmpty();
                this.TestOutputHelper.WriteLine($"Step 2: Triggered job, new instance ID: {triggeredJobId}");

                // Step 3: Trigger again to ensure it's still available
                string secondTriggeredJobId = this.TestSchedulingService.TriggerRecurringJob(recurringJobId);
                _ = secondTriggeredJobId.Should().NotBeNullOrEmpty();
                _ = secondTriggeredJobId.Should().NotBe(triggeredJobId); // Should be different instances
                this.TestOutputHelper.WriteLine($"Step 3: Triggered again, new instance ID: {secondTriggeredJobId}");

                // Step 4: Remove the recurring job
                this.TestSchedulingService.RemoveRecurringJob(recurringJobId);
                this.TestOutputHelper.WriteLine($"Step 4: Removed recurring job '{recurringJobId}'");

                // Step 5: Try to trigger the removed job
                string postRemovalResult = this.TestSchedulingService.TriggerRecurringJob(recurringJobId);
                this.TestOutputHelper.WriteLine($"Step 5: Attempted to trigger removed job, result: {postRemovalResult ?? "null"}");
            }
            finally
            {
                // Ensure cleanup
                try
                {
                    this.TestSchedulingService.RemoveRecurringJob(recurringJobId);
                }
                catch
                {
                    // Ignore cleanup exceptions
                }
            }
        }

        [Fact]
        public void EnqueueIntegration_InstanceMethod_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("integration-test");

            // Act
            string jobId = this.TestSchedulingService.Enqueue(methodCall);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Enqueued job with ID: {jobId}");
        }

        [Fact]
        public void EnqueueIntegration_WithQueue_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string queue = "integration-test-queue";
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("queue-test");

            // Act
            string jobId = this.TestSchedulingService.Enqueue(methodCall, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Enqueued job with ID: {jobId} in queue: {queue}");
        }

        [Fact]
        public void EnqueueIntegration_StaticMethod_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action> methodCall = () => IntegrationTestJobClass.StaticTestMethod("static-integration-test");

            // Act
            string jobId = this.TestSchedulingService.Enqueue(methodCall);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Enqueued static job with ID: {jobId}");
        }

        [Fact]
        public void EnqueueIntegration_AsyncMethod_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Func<Task>> methodCall = () => IntegrationTestJobClass.StaticAsyncTestMethod("async-integration-test");

            // Act
            string jobId = this.TestSchedulingService.Enqueue(methodCall);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Enqueued async job with ID: {jobId}");
        }

        [Fact]
        public void EnqueueIntegration_AsyncInstanceMethod_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Func<IntegrationTestJobClass, Task>> methodCall = x => x.TestAsyncMethod("async-instance-test");

            // Act
            string jobId = this.TestSchedulingService.Enqueue(methodCall);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Enqueued async instance job with ID: {jobId}");
        }

        [Fact]
        public void ScheduleIntegration_WithDelay_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("scheduled-test");
            var delay = TimeSpan.FromSeconds(30);

            // Act
            string jobId = this.TestSchedulingService.Schedule(methodCall, delay);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Scheduled job with ID: {jobId} for {delay}");
        }

        [Fact]
        public void ScheduleIntegration_WithEnqueueAt_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action> methodCall = () => IntegrationTestJobClass.StaticTestMethod("scheduled-at-test");
            DateTimeOffset enqueueAt = DateTimeOffset.UtcNow.AddMinutes(1);

            // Act
            string jobId = this.TestSchedulingService.Schedule(methodCall, enqueueAt);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Scheduled job with ID: {jobId} for {enqueueAt}");
        }

        [Fact]
        public void ScheduleIntegration_WithQueueAndDelay_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string queue = "scheduled-queue";
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("queue-scheduled-test");
            var delay = TimeSpan.FromSeconds(45);

            // Act
            string jobId = this.TestSchedulingService.Schedule(methodCall, delay, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Scheduled job with ID: {jobId} in queue: {queue} for {delay}");
        }

        [Fact]
        public void ScheduleIntegration_AsyncMethod_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Func<Task>> methodCall = () => IntegrationTestJobClass.StaticAsyncTestMethod("async-scheduled-test");
            var delay = TimeSpan.FromSeconds(60);

            // Act
            string jobId = this.TestSchedulingService.Schedule(methodCall, delay);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Scheduled async job with ID: {jobId} for {delay}");
        }

        [Fact]
        public void ScheduleIntegration_AsyncInstanceMethodWithEnqueueAt_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Func<IntegrationTestJobClass, Task>> methodCall = x => x.TestAsyncMethod("async-instance-scheduled-test");
            DateTimeOffset enqueueAt = DateTimeOffset.UtcNow.AddMinutes(2);

            // Act
            string jobId = this.TestSchedulingService.Schedule(methodCall, enqueueAt);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Scheduled async instance job with ID: {jobId} for {enqueueAt}");
        }

        [Fact]
        public void StateManagementIntegration_ChangeState_ShouldSucceed()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("state-change-test");
            string jobId = this.TestSchedulingService.Enqueue(methodCall);
            var newState = new CoreScheduledJobState(TimeSpan.FromMinutes(5), "Changed to scheduled");

            // Act
            bool result = this.TestSchedulingService.ChangeState(jobId, newState);

            // Assert
            _ = result.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"Changed state for job {jobId}");
        }

        [Fact]
        public void StateManagementIntegration_Delete_ShouldSucceed()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("delete-test");
            string jobId = this.TestSchedulingService.Enqueue(methodCall);

            // Act
            bool result = this.TestSchedulingService.Delete(jobId);

            // Assert
            _ = result.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"Deleted job {jobId}");
        }

        [Fact]
        public void StateManagementIntegration_Requeue_ShouldSucceed()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("requeue-test");
            string jobId = this.TestSchedulingService.Schedule(methodCall, TimeSpan.FromMinutes(10));

            // Act
            bool result = this.TestSchedulingService.Requeue(jobId);

            // Assert
            _ = result.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"Requeued job {jobId}");
        }

        [Fact]
        public void StateManagementIntegration_RescheduleWithDelay_ShouldSucceed()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("reschedule-test");
            string jobId = this.TestSchedulingService.Enqueue(methodCall);
            var newDelay = TimeSpan.FromMinutes(15);

            // Act
            bool result = this.TestSchedulingService.Reschedule(jobId, newDelay);

            // Assert
            _ = result.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"Rescheduled job {jobId} for {newDelay}");
        }

        [Fact]
        public void StateManagementIntegration_RescheduleWithEnqueueAt_ShouldSucceed()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("reschedule-at-test");
            string jobId = this.TestSchedulingService.Enqueue(methodCall);
            DateTimeOffset enqueueAt = DateTimeOffset.UtcNow.AddMinutes(20);

            // Act
            bool result = this.TestSchedulingService.Reschedule(jobId, enqueueAt);

            // Assert
            _ = result.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"Rescheduled job {jobId} for {enqueueAt}");
        }

        [Fact]
        public void ContinuationIntegration_SimpleInstanceMethod_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> parentMethodCall = x => x.TestMethod("parent-job");
            string parentJobId = this.TestSchedulingService.Enqueue(parentMethodCall);

            Expression<Action<IntegrationTestJobClass>> continuationMethodCall = x => x.TestMethod("continuation-job");

            // Act
            string continuationJobId = this.TestSchedulingService.ContinueJobWith(parentJobId, continuationMethodCall);

            // Assert
            _ = continuationJobId.Should().NotBeNullOrEmpty();
            _ = continuationJobId.Should().NotBe(parentJobId);
            this.TestOutputHelper.WriteLine($"Created continuation job {continuationJobId} for parent {parentJobId}");
        }

        [Fact]
        public void ContinuationIntegration_WithQueue_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> parentMethodCall = x => x.TestMethod("parent-queue-job");
            string parentJobId = this.TestSchedulingService.Enqueue(parentMethodCall);

            const string continuationQueue = "continuation-queue";
            Expression<Action<IntegrationTestJobClass>> continuationMethodCall = x => x.TestMethod("continuation-queue-job");

            // Act
            string continuationJobId = this.TestSchedulingService.ContinueJobWith(parentJobId, continuationMethodCall, this.GetTestJobParameters(), continuationQueue);

            // Assert
            _ = continuationJobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created continuation job {continuationJobId} in queue {continuationQueue} for parent {parentJobId}");
        }

        [Fact]
        public void ContinuationIntegration_StaticMethod_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action> parentMethodCall = () => IntegrationTestJobClass.StaticTestMethod("parent-static-job");
            string parentJobId = this.TestSchedulingService.Enqueue(parentMethodCall);

            Expression<Action> continuationMethodCall = () => IntegrationTestJobClass.StaticTestMethod("continuation-static-job");

            // Act
            string continuationJobId = this.TestSchedulingService.ContinueJobWith(parentJobId, continuationMethodCall);

            // Assert
            _ = continuationJobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created static continuation job {continuationJobId} for parent {parentJobId}");
        }

        [Fact]
        public void ContinuationIntegration_AsyncMethod_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Func<Task>> parentMethodCall = () => IntegrationTestJobClass.StaticAsyncTestMethod("parent-async-job");
            string parentJobId = this.TestSchedulingService.Enqueue(parentMethodCall);

            Expression<Func<Task>> continuationMethodCall = () => IntegrationTestJobClass.StaticAsyncTestMethod("continuation-async-job");

            // Act
            string continuationJobId = this.TestSchedulingService.ContinueJobWith(parentJobId, continuationMethodCall);

            // Assert
            _ = continuationJobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created async continuation job {continuationJobId} for parent {parentJobId}");
        }

        [Fact]
        public void ContinuationIntegration_AsyncInstanceMethod_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Func<IntegrationTestJobClass, Task>> parentMethodCall = x => x.TestAsyncMethod("parent-async-instance-job");
            string parentJobId = this.TestSchedulingService.Enqueue(parentMethodCall);

            Expression<Func<IntegrationTestJobClass, Task>> continuationMethodCall = x => x.TestAsyncMethod("continuation-async-instance-job");

            // Act
            string continuationJobId = this.TestSchedulingService.ContinueJobWith(parentJobId, continuationMethodCall);

            // Assert
            _ = continuationJobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created async instance continuation job {continuationJobId} for parent {parentJobId}");
        }

        [Fact]
        public void ContinuationIntegration_WithCustomNextStateAndOptions_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> parentMethodCall = x => x.TestMethod("parent-custom-job");
            string parentJobId = this.TestSchedulingService.Enqueue(parentMethodCall);

            Expression<Action<IntegrationTestJobClass>> continuationMethodCall = x => x.TestMethod("continuation-custom-job");
            var nextState = new CoreScheduledJobState(TimeSpan.FromMinutes(5), "Custom scheduled continuation");
            CoreJobContinuationOptions options = CoreJobContinuationOptions.OnAnyFinishedState;

            // Act
            string continuationJobId = this.TestSchedulingService.ContinueJobWith(parentJobId, continuationMethodCall, nextState, options);

            // Assert
            _ = continuationJobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created custom continuation job {continuationJobId} for parent {parentJobId} with options {options}");
        }

        [Fact]
        public void CreateIntegration_WithCustomState_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action> methodCall = () => IntegrationTestJobClass.StaticTestMethod("custom-state-test");
            var customState = new CoreProcessingJobState("integration-server", "worker-1", "Integration test processing");

            // Act
            string jobId = this.TestSchedulingService.Create(methodCall, customState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created job with custom processing state: {jobId}");
        }

        [Fact]
        public void CreateIntegration_WithFailedState_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Func<Task>> methodCall = () => IntegrationTestJobClass.StaticAsyncTestMethod("failed-state-test");
            var failedException = new InvalidOperationException("Integration test failure");
            var failedState = new CoreFailedJobState(failedException, "Integration test failed state");

            // Act
            string jobId = this.TestSchedulingService.Create(methodCall, failedState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created job with failed state: {jobId}");
        }

        [Fact]
        public void CreateIntegration_WithSucceededState_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("succeeded-state-test");
            var succeededState = new CoreSucceededJobState("Integration test result", 5000, 4500, "Integration test succeeded");

            // Act
            string jobId = this.TestSchedulingService.Create(methodCall, succeededState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created job with succeeded state: {jobId}");
        }

        [Fact]
        public void CreateIntegration_WithDeletedState_ShouldReturnJobId()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Func<IntegrationTestJobClass, Task>> methodCall = x => x.TestAsyncMethod("deleted-state-test");
            var deletedState = new CoreDeletedJobState(null, "Integration test deleted state");

            // Act
            string jobId = this.TestSchedulingService.Create(methodCall, deletedState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"Created job with deleted state: {jobId}");
        }

        [Fact]
        public async Task ComprehensiveIntegration_JobLifecycle_ShouldWorkCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod("lifecycle-test");

            // Act & Assert - Create job
            string jobId = this.TestSchedulingService.Enqueue(methodCall);
            _ = jobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"1. Enqueued job: {jobId}");

            // Act & Assert - Schedule job
            var delay = TimeSpan.FromSeconds(30);
            bool rescheduleResult = this.TestSchedulingService.Reschedule(jobId, delay);
            _ = rescheduleResult.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"2. Rescheduled job {jobId} for {delay}");

            // Act & Assert - Requeue job
            bool requeueResult = this.TestSchedulingService.Requeue(jobId);
            _ = requeueResult.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"3. Requeued job {jobId}");

            // Act & Assert - Create continuation
            Expression<Action<IntegrationTestJobClass>> continuationCall = x => x.TestMethod("lifecycle-continuation");
            string continuationJobId = this.TestSchedulingService.ContinueJobWith(jobId, continuationCall);
            _ = continuationJobId.Should().NotBeNullOrEmpty();
            this.TestOutputHelper.WriteLine($"4. Created continuation job: {continuationJobId}");

            // Act & Assert - Delete jobs
            bool deleteContinuationResult = this.TestSchedulingService.Delete(continuationJobId);
            _ = deleteContinuationResult.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"5. Deleted continuation job {continuationJobId}");

            bool deleteResult = this.TestSchedulingService.Delete(jobId);
            _ = deleteResult.Should().BeTrue();
            this.TestOutputHelper.WriteLine($"6. Deleted main job {jobId}");

            // Allow some time for processing
            await Task.Delay(100);
        }

        [Fact]
        public async Task ComprehensiveIntegration_ConcurrentOperations_ShouldWorkCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            const int jobCount = 5;
            var jobIds = new List<string>();

            // Act - Create multiple jobs concurrently
            Task<string>[] enqueueTasks = Enumerable.Range(0, jobCount)
                .Select(i => Task.Run(() =>
                {
                    Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod($"concurrent-{i}");
                    return this.TestSchedulingService.Enqueue(methodCall);
                }))
                .ToArray();

            string[] enqueueResults = await Task.WhenAll(enqueueTasks);
            jobIds.AddRange(enqueueResults);

            // Act - Schedule multiple jobs concurrently
            Task<string>[] scheduleTasks = Enumerable.Range(0, jobCount)
                .Select(i => Task.Run(() =>
                {
                    Expression<Action<IntegrationTestJobClass>> methodCall = x => x.TestMethod($"scheduled-{i}");
                    return this.TestSchedulingService.Schedule(methodCall, TimeSpan.FromSeconds(30 + i));
                }))
                .ToArray();

            string[] scheduleResults = await Task.WhenAll(scheduleTasks);
            jobIds.AddRange(scheduleResults);

            // Assert
            _ = jobIds.Should().HaveCount(jobCount * 2);
            _ = jobIds.Should().OnlyContain(id => !string.IsNullOrEmpty(id));
            _ = jobIds.Should().OnlyHaveUniqueItems();

            this.TestOutputHelper.WriteLine($"Created {jobIds.Count} concurrent jobs successfully");

            // Cleanup - Delete all jobs
            foreach (string jobId in jobIds)
            {
                _ = this.TestSchedulingService.Delete(jobId);
            }
        }

        [Fact]
        public async Task Enqueue_WithActionT_ShouldCreateAndExecuteJob()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId();

            // Act
            string createdJobId = this.TestSchedulingService.Enqueue<TestJob>(
                job => job.DoWork(jobId),
                null,
                CoreJobStateConstants.TestQueue);

            // Assert
            _ = await this.ValidateSchedulingJobInfoAsync(
                jobId,
                createdJobId,
                [CoreJobStateConstants.SucceededStateName]);

            _ = JobExecutionTracker[jobId].Should().BeTrue("because the job should have executed");
        }

        [Fact]
        public async Task Enqueue_WithAction_ShouldCreateAndExecuteJob()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId();

            // Act
            string createdJobId = this.TestSchedulingService.Enqueue(
                () => TestJob.ResetTracking(),
                null,
                CoreJobStateConstants.TestQueue);

            // Assert
            ICoreSchedulingJobInfo? jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);
            _ = jobInfo.Should().NotBeNull();
            _ = jobInfo!.CurrentState.Name.Should().BeOneOf(CoreJobStateConstants.SucceededStateName, CoreJobStateConstants.EnqueuedStateName, CoreJobStateConstants.ProcessingStateName);
        }

        [Fact]
        public async Task Enqueue_WithFuncTask_ShouldCreateAndExecuteJob()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId();

            // Act - Use a static method instead of async lambda
            string createdJobId = this.TestSchedulingService.Enqueue(
                () => TestJob.SetJobCompleted(jobId), // Replace with call to static method
                null,
                CoreJobStateConstants.TestQueue);

            // Assert
            _ = await this.ValidateSchedulingJobInfoAsync(
                jobId,
                createdJobId,
                [CoreJobStateConstants.SucceededStateName]);
        }

        [Fact]
        public async Task Enqueue_WithFuncTTask_ShouldCreateAndExecuteJob()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId();

            // Act
            string createdJobId = this.TestSchedulingService.Enqueue<TestJob>(
                job => job.DoWorkAsync(jobId),
                null,
                CoreJobStateConstants.TestQueue);

            // Assert
            _ = await this.ValidateSchedulingJobInfoAsync(
                jobId,
                createdJobId,
                [CoreJobStateConstants.SucceededStateName]);
        }

        [Fact]
        public async Task Schedule_WithTimeSpanDelay_ShouldExecuteAfterDelay()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId();
            var delay = TimeSpan.FromSeconds(1);

            // Act
            string createdJobId = this.TestSchedulingService.Schedule<TestJob>(
                job => job.DoWork(jobId),
                delay,
                null,
                CoreJobStateConstants.TestQueue);

            // Assert
            _ = await this.ValidateSchedulingJobInfoAsync(
                jobId,
                createdJobId,
                [CoreJobStateConstants.SucceededStateName],
                true,
                CoreJobStateConstants.TestQueue,
                TimeSpan.FromSeconds(45));

            // Check that the job was initially in Scheduled state
            IReadOnlyList<Core.Scheduling.Services.Monitoring.ICoreSchedulingJobStateHistory> history = await this.TestSchedulingService.GetJobStateHistoryAsync(createdJobId);
            _ = history.Should().NotBeEmpty("because the job should have succeeded.");
            _ = history.Should().Contain(h => h.State.Name == CoreJobStateConstants.ScheduledStateName);
        }

        [Fact]
        public async Task Schedule_DisplayJobStateHistory()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId();
            var delay = TimeSpan.FromSeconds(1);

            // Act
            string createdJobId = this.TestSchedulingService.Schedule<TestJob>(
                job => job.DoWork(jobId),
                delay,
                null,
                CoreJobStateConstants.TestQueue);

            // Assert
            _ = await this.ValidateSchedulingJobInfoAsync(
                jobId,
                createdJobId,
                [CoreJobStateConstants.SucceededStateName]);

            this.TestSchedulingService.Delete(createdJobId).Should().BeTrue();

            // Check that the job was initially in Scheduled state
            IReadOnlyList<Core.Scheduling.Services.Monitoring.ICoreSchedulingJobStateHistory> history = await this.TestSchedulingService.GetJobStateHistoryAsync(createdJobId);
            _ = history.Should().NotBeEmpty("because the job should have succeeded.");

            Queue<string> stateNameOrder = this.CreateStateNameTransitionQueue();

            this.TestOutputHelper.WriteLine($"\n{$"Job state history for Job ({createdJobId}):".CenterTitle()}");
            foreach (ICoreSchedulingJobStateHistory item in history)
            {
                string json = JsonSerializer.Serialize(item, this._jsonOptions);

                this.TestOutputHelper.WriteLine($"{item.State.Name.CenterTitle()}\n{json}\n");
                item.State.Name.Should().Be(stateNameOrder.Dequeue(), "because the job should follow the expected state transition order.");
            }

            _ = history.Should().Contain(h => h.State.Name == CoreJobStateConstants.ScheduledStateName);
        }

        [Fact]
        public async Task Schedule_WithDateTimeOffset_ShouldExecuteAtSpecificTime()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId();
            DateTimeOffset executionTime = DateTimeOffset.UtcNow.AddSeconds(30);

            // Act
            string createdJobId = this.TestSchedulingService.Schedule<TestJob>(
                job => job.DoWork(jobId),
                executionTime,
                null,
                CoreJobStateConstants.TestQueue);

            // Assert
            // Verify the job was scheduled correctly
            Core.Scheduling.Services.Monitoring.ICoreSchedulingJobInfo? jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);
            jobInfo.Should().NotBeNull("because job info should be available");
            jobInfo.CurrentState.Name.Should().Be(CoreJobStateConstants.ScheduledStateName);
            _ = jobInfo!.ScheduledAt.Should().NotBeNull();

            // Allow 1-second margin for scheduling precision
            _ = jobInfo.ScheduledAt!.Value.Should().BeCloseTo(executionTime, TimeSpan.FromSeconds(30));
        }

        [Fact]
        public async Task ChangeState_ShouldModifyJobState()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId(false); // Don't reset tracking

            string createdJobId = this.TestSchedulingService.Schedule<TestJob>(
                job => job.DoWork(jobId),
                TimeSpan.FromSeconds(30), // Schedule far enough to change state before execution
                null,
                CoreJobStateConstants.TestQueue);

            // Wait briefly to ensure job is created
            await Task.Delay(500);

            // Act
            bool stateChanged = this.TestSchedulingService.ChangeState(
                createdJobId,
                new CoreEnqueuedJobState());

            // Assert
            _ = stateChanged.Should().BeTrue();

            // Wait for the job change to process
            this.TestDelay(500, this.TestCaseLogger);

            _ = await this.ValidateSchedulingJobInfoAsync(
                jobId,
                createdJobId,
                [CoreJobStateConstants.SucceededStateName]);
        }

        [Fact]
        public async Task Delete_ShouldChangeJobStateToDeleted()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId();

            string createdJobId = this.TestSchedulingService.Schedule<TestJob>(
                job => job.DoWork(jobId),
                TimeSpan.FromSeconds(30), // Schedule far enough to delete before execution
                null,
                CoreJobStateConstants.TestQueue);

            // Wait briefly to ensure job is created
            await Task.Delay(500);

            // Act
            bool deleted = this.TestSchedulingService.Delete(createdJobId);

            // Assert
            _ = deleted.Should().BeTrue();

            ICoreSchedulingJobInfo? jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);
            _ = jobInfo.Should().NotBeNull();
            _ = jobInfo!.CurrentState.Name.Should().Be(CoreJobStateConstants.DeletedStateName);
            _ = jobInfo.IsInFinalState.Should().BeTrue();

            // Job should not execute after deletion
            await Task.Delay(1000);
            _ = JobExecutionTracker.TryGetValue(jobId, out bool executed).Should().BeFalse();
        }

        [Fact]
        public async Task Requeue_ShouldChangeJobStateToEnqueued()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create a job that will fail
            string jobId = this.CreateTestJobId();

            string createdJobId = this.TestSchedulingService.Enqueue<TestJob>(
                job => job.ThrowException(jobId),
                null,
                CoreJobStateConstants.TestQueue);

            // Wait for the job to fail
            _ = await this.ValidateSchedulingJobInfoAsync(
                jobId,
                createdJobId,
                [CoreJobStateConstants.FailedStateName],
                false);

            // Act
            bool requeued = this.TestSchedulingService.Requeue(createdJobId);

            // Assert
            _ = requeued.Should().BeTrue();

            ICoreSchedulingJobInfo? requeuedJobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);
            _ = requeuedJobInfo.Should().NotBeNull();
            _ = requeuedJobInfo!.CurrentState.Name.Should().BeOneOf([CoreJobStateConstants.EnqueuedStateName, CoreJobStateConstants.FailedStateName], "because job should be enqueued or failed if run again");
        }

        [Fact]
        public async Task Reschedule_ShouldChangeJobTimingWithTimeSpan()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string jobId = this.CreateTestJobId();

            string createdJobId = this.TestSchedulingService.Enqueue<TestJob>(
                job => job.DoWork(jobId),
                null,
                CoreJobStateConstants.TestQueue);

            // Act - Reschedule with a delay
            bool rescheduled = this.TestSchedulingService.Reschedule(
                createdJobId,
                TimeSpan.FromSeconds(2));

            // Assert
            _ = rescheduled.Should().BeTrue();

            this.TestDelay(100, this.TestCaseLogger);

            ICoreSchedulingJobInfo? jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);
            _ = jobInfo.Should().NotBeNull();
            _ = jobInfo!.CurrentState.Name.Should().BeOneOf(CoreJobStateConstants.ScheduledStateName, CoreJobStateConstants.SucceededStateName, CoreJobStateConstants.ProcessingStateName);

            if (jobInfo.CurrentState.Name.Equals(CoreJobStateConstants.ScheduledStateName))
            {
                _ = jobInfo.ScheduledAt.Should().NotBeNull();
                _ = jobInfo.ScheduledAt!.Value.Should().BeAfter(DateTimeOffset.UtcNow);
            }

            // Wait for completion
            _ = await this.ValidateSchedulingJobInfoAsync(
                jobId,
                createdJobId,
                [CoreJobStateConstants.ScheduledStateName, CoreJobStateConstants.SucceededStateName]);
        }

        [Fact]
        public async Task ContinueJobWith_ShouldExecuteAfterParentJob()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string parentJobId = this.CreateTestJobId();
            string continuationJobId = Guid.NewGuid().ToString("N");

            // Create the parent job
            string createdParentJobId = this.TestSchedulingService.Enqueue<TestJob>(
                job => job.DoWork(parentJobId),
                null,
                CoreJobStateConstants.TestQueue);

            // Create the continuation job
            string createdContinuationJobId = this.TestSchedulingService.ContinueJobWith<TestJob>(
                createdParentJobId,
                job => job.DoWork(continuationJobId),
                null,
                CoreJobStateConstants.TestQueue);

            // Assert - Wait for parent job to complete
            _ = await this.ValidateSchedulingJobInfoAsync(
                parentJobId,
                createdParentJobId,
                [CoreJobStateConstants.SucceededStateName]);

            // Wait for continuation job to complete
            _ = await this.ValidateSchedulingJobInfoAsync(
                continuationJobId,
                createdContinuationJobId,
                [CoreJobStateConstants.SucceededStateName]);

            // Verify job states
            ICoreSchedulingJobInfo? parentJobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdParentJobId);
            _ = parentJobInfo!.CurrentState.Name.Should().Be(CoreJobStateConstants.SucceededStateName);

            ICoreSchedulingJobInfo? continuationJobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdContinuationJobId);
            _ = continuationJobInfo!.CurrentState.Name.Should().Be(CoreJobStateConstants.SucceededStateName);
        }

        [Fact]
        public async Task ContinueJobWith_WithFailedParentAndAnyState_ShouldExecute()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string parentJobId = this.CreateTestJobId();
            string continuationJobId = Guid.NewGuid().ToString("N");

            // Create the parent job that will fail
            string createdParentJobId = this.TestSchedulingService.Enqueue<TestJob>(
                job => job.ThrowException(parentJobId),
                null,
                CoreJobStateConstants.TestQueue);

            // Create the continuation job that runs regardless of parent state
            string createdContinuationJobId = this.TestSchedulingService.ContinueJobWith<TestJob>(
                createdParentJobId,
                job => job.DoWork(continuationJobId),
                new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue),
                CoreJobContinuationOptions.OnAnyFinishedState,
                null,
                CoreJobStateConstants.TestQueue);

            // Wait for parent job to complete (fail)
            _ = await this.ValidateSchedulingJobInfoAsync(
                parentJobId,
                createdParentJobId,
                [CoreJobStateConstants.FailedStateName],
                false);

            // Verify job states
            ICoreSchedulingJobInfo? parentJobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdParentJobId);
            _ = parentJobInfo!.CurrentState.Name.Should().Be(CoreJobStateConstants.FailedStateName);

            ICoreSchedulingJobInfo? continuationJobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdContinuationJobId);
            _ = continuationJobInfo!.CurrentState.Name.Should().Be(CoreJobStateConstants.AwaitingStateName);
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
