// // ***********************************************************************
// <copyright file="CoreSchedulingIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
#if NV_USE_HANGFIRE
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using FluentAssertions;
using Hangfire;
using NetworkVisor.Core.Scheduling.Services;
using NetworkVisor.Core.Scheduling.Services.JobStates;
using NetworkVisor.Core.Scheduling.Services.Monitoring;
using NetworkVisor.Core.Test.TestCase;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Scheduling
{
    /// <summary>
    /// Integration tests for CoreHangfireSchedulingBackgroundService
    /// These tests use the actual running service instance and require Hangfire to be properly configured.
    /// </summary>
    [PlatformTrait(typeof(CoreSchedulingIntegrationTests))]
    public class CoreSchedulingIntegrationTests : CoreSchedulingTestCaseBase
    {
        public CoreSchedulingIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task SchedulingIntegration_Create_ExecutesSynchronousJob()
        {
            // Arrange
            string testJobId = this.CreateTestJobId();
            var enqueuedJobState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason());

            // Act
            string createdJobId = this.TestSchedulingService.Create<TestJob>(j => j.DoWork(testJobId), enqueuedJobState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            this.TestOutputHelper.WriteLine($"Created job with ID: {createdJobId} at {enqueuedJobState.EnqueuedAt.ToLocalTime()}");

            // Get job details to verify final state
            ICoreSchedulingJobInfo jobInfo = await this.ValidateSchedulingJobInfoAsync(testJobId, createdJobId, [CoreJobStateConstants.SucceededStateName]);
            jobInfo.Should().NotBeNull("because job info should be available");
        }

        [Fact]
        public async Task SchedulingIntegration_Create_ExecutesAsyncJob()
        {
            // Arrange
            string testJobId = this.CreateTestJobId();
            var enqueuedJobState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason());

            // Act
            string createdJobId = this.TestSchedulingService.Create<TestJob>(j => j.DoWorkAsync(testJobId), enqueuedJobState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            this.TestOutputHelper.WriteLine($"Created async job with ID: {createdJobId} at {enqueuedJobState.EnqueuedAt.ToLocalTime()}");

            // Get job details to verify final state
            ICoreSchedulingJobInfo jobInfo = await this.ValidateSchedulingJobInfoAsync(testJobId, createdJobId, [CoreJobStateConstants.SucceededStateName]);
            jobInfo.Should().NotBeNull("because job info should be available");
        }

        [Fact]
        public async Task SchedulingIntegration_Create_ExecutesAsyncJobTwice()
        {
            // Arrange
            string testJobId = this.CreateTestJobId();
            var enqueuedJobState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason());

            // Act
            string createdJobId = this.TestSchedulingService.Create<TestJob>(j => j.DoWorkAsync(testJobId), enqueuedJobState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            this.TestOutputHelper.WriteLine($"Created async job with ID: {createdJobId} at {enqueuedJobState.EnqueuedAt}");

            // Get job details to verify final state
            ICoreSchedulingJobInfo jobInfo = await this.ValidateSchedulingJobInfoAsync(testJobId, createdJobId, [CoreJobStateConstants.SucceededStateName]);
            jobInfo.Should().NotBeNull("because job info should be available");

            string testJobId2 = this.CreateTestJobId(false);
            var enqueuedJobState2 = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason());
            string createdJobId2 = this.TestSchedulingService.Create<TestJob>(j => j.DoWorkAsync(testJobId2), enqueuedJobState2, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            this.TestOutputHelper.WriteLine($"\nCreated second async job with ID: {createdJobId2} at {enqueuedJobState2.EnqueuedAt}");

            // Wait for 60 seconds to ensure both jobs have time to complete
            ICoreSchedulingJobInfo jobInfo2 = await this.ValidateSchedulingJobInfoAsync(testJobId2, createdJobId2, [CoreJobStateConstants.SucceededStateName], true, CoreJobStateConstants.TestQueue, TimeSpan.FromSeconds(60));
            jobInfo2.Should().NotBeNull("because job info should be available");
        }

        [Fact]
        public async Task SchedulingIntegration_Create_HandlesJobException()
        {
            // Arrange
            string testJobId = this.CreateTestJobId();

            // Act
            string createdJobId = this.TestSchedulingService.Create<TestJob>(
                j => j.ThrowException(testJobId),
                new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason()),
                this.GetTestJobParameters(),
                CoreJobStateConstants.TestQueue);

            this.TestOutputHelper.WriteLine($"Created exception job with ID: {createdJobId}");

            // Validate job info with increased timeout to allow for exception handling
            ICoreSchedulingJobInfo jobInfo = await this.ValidateSchedulingJobInfoAsync(testJobId, createdJobId, [CoreJobStateConstants.FailedStateName], false, CoreJobStateConstants.TestQueue, TimeSpan.FromSeconds(60));
            _ = jobInfo.Exception.Should().NotBeNull("because exception information should be available");
        }

        [Fact]
        public async Task SchedulingIntegration_Create_HandlesJobException_WithRetries()
        {
            // Arrange
            string testJobId = this.CreateTestJobId();

            // Act
            string createdJobId = this.TestSchedulingService.Create<TestJob>(
                j => j.ThrowExceptionWithRetries(testJobId),
                new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason()),
                this.GetTestJobParameters(),
                CoreJobStateConstants.TestQueue);

            this.TestOutputHelper.WriteLine($"Created exception job with ID: {createdJobId}");

            ICoreSchedulingJobInfo jobInfo = await this.ValidateSchedulingJobInfoAsync(testJobId, createdJobId, [CoreJobStateConstants.ScheduledStateName], false);
            _ = jobInfo.Exception.Should().BeNull("because exception information isn't available until after a retry.");
        }

        [Fact]
        public async Task SchedulingIntegration_ChangeState_ChangesJobState()
        {
            // Arrange
            string testJobId = this.CreateTestJobId();

            string createdJobId = this.TestSchedulingService.Create<TestJob>(
                j => j.DoWork(testJobId),
                new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), this.CreateTestReason()),
                this.GetTestJobParameters(),
                CoreJobStateConstants.TestQueue);

            this._createdJobIds.Add(createdJobId);
            this.TestOutputHelper.WriteLine($"Created scheduled job with ID: {createdJobId}");

            // Verify job is in scheduled state
            ICoreSchedulingJobInfo? initialJobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);
            _ = initialJobInfo.Should().NotBeNull();
            this.OutputSchedulingJobInfo(initialJobInfo, createdJobId, "Initial JobInfo");
            _ = initialJobInfo.CurrentState.Name.Should().Be(CoreJobStateConstants.ScheduledStateName);

            // Act
            bool success = this.TestSchedulingService.ChangeState(createdJobId, new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason()));

            // Assert
            _ = success.Should().BeTrue("because state change should succeed");

            // Verify state changed
            ICoreSchedulingJobInfo? updatedJobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);
            _ = updatedJobInfo.Should().NotBeNull();
            _ = updatedJobInfo.CurrentState.Name.Should().Be(CoreJobStateConstants.EnqueuedStateName);
            this.OutputSchedulingJobInfo(updatedJobInfo, createdJobId, "Updated JobInfo");
        }

        [Fact]
        public async Task SchedulingIntegration_JobExistsAsync_ReturnsCorrectResult()
        {
            // Arrange
            string testJobId = this.CreateTestJobId();
            string createdJobId = this.TestSchedulingService.Create<TestJob>(
                j => j.DoWork(testJobId),
                new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), this.CreateTestReason()),
                this.GetTestJobParameters(),
                CoreJobStateConstants.TestQueue);

            this._createdJobIds.Add(createdJobId);

            // Act
            bool exists = await this.TestSchedulingService.JobExistsAsync(createdJobId);
            bool nonExistentExists = await this.TestSchedulingService.JobExistsAsync(int.MaxValue.ToString());

            // Assert
            _ = exists.Should().BeTrue("because created job should exist");
            _ = nonExistentExists.Should().BeFalse("because non-existent job should not exist");

            ICoreSchedulingJobInfo? jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);
            this.OutputSchedulingJobInfo(jobInfo, createdJobId);
        }

        [Fact]
        public async Task SchedulingIntegration_GetJobStateAsync_ReturnsJobState()
        {
            // Arrange
            string testJobId = this.CreateTestJobId();
            string createdJobId = this.TestSchedulingService.Create<TestJob>(
                j => j.DoWork(testJobId),
                new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), this.CreateTestReason()),
                this.GetTestJobParameters(),
                CoreJobStateConstants.TestQueue);

            this._createdJobIds.Add(createdJobId);

            // Act
            ICoreJobState? jobState = await this.TestSchedulingService.GetJobStateAsync(createdJobId);

            // Assert
            _ = jobState.Should().NotBeNull("because job state should be available");
            _ = jobState.Name.Should().Be("Scheduled", "because job should be in scheduled state");

            ICoreSchedulingJobInfo? jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);
            this.OutputSchedulingJobInfo(jobInfo, createdJobId);
        }

        [Fact]
        public async Task SchedulingIntegration_GetJobInfoBatchAsync_ReturnsJobInfoForMultipleJobs()
        {
            // Arrange
            string testJobId1 = this.CreateTestJobId();
            string testJobId2 = this.CreateTestJobId(false);

            string createdJobId1 = this.TestSchedulingService.Create<TestJob>(
                j => j.DoWork(testJobId1),
                new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), this.CreateTestReason()),
                this.GetTestJobParameters(),
                CoreJobStateConstants.TestQueue);

            string createdJobId2 = this.TestSchedulingService.Create<TestJob>(
                j => j.DoWork(testJobId2),
                new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), this.CreateTestReason()),
                this.GetTestJobParameters(),
                CoreJobStateConstants.TestQueue);

            this._createdJobIds.Add(createdJobId1);
            this._createdJobIds.Add(createdJobId2);

            // Act
            IReadOnlyDictionary<string, ICoreSchedulingJobInfo> jobInfos = await this.TestSchedulingService.GetJobSchedulingInfoBatchAsync(new[] { createdJobId1, createdJobId2 });

            // Assert
            _ = jobInfos.Should().NotBeNull("because job info should be available");
            _ = jobInfos.Count.Should().Be(2, "because both jobs should have info");
            _ = jobInfos.Should().ContainKey(createdJobId1);
            _ = jobInfos.Should().ContainKey(createdJobId2);

            foreach (KeyValuePair<string, ICoreSchedulingJobInfo> jobInfo in jobInfos)
            {
                this.OutputSchedulingJobInfo(jobInfo.Value, jobInfo.Key);
            }
        }

        [Fact]
        public async Task SchedulingIntegration_AddOrUpdateRecurringJob_ExecutesRecurringJob()
        {
            // Arrange
            string recurringJobId = $"recurring-test-{Guid.NewGuid():N}";
            TestRecurringJob.ResetTracking();

            try
            {
                // Act
                this.TestSchedulingService.AddOrUpdateRecurringJob<TestRecurringJob>(
                    recurringJobId,
                    j => j.IncrementCounterAsync(recurringJobId),
                    "*/1 * * * * *", // Run every second
                    CoreJobStateConstants.TestQueue);

                // Trigger the job immediately
                _ = this.TestSchedulingService.TriggerRecurringJob(recurringJobId);

                // Wait for job to execute at least once
                bool executed = await this.WaitForRecurringJobExecutionAsync(recurringJobId);

                // Assert
                _ = executed.Should().BeTrue($"because recurring job should execute within {DefaultJobTimeout.TotalSeconds} seconds");
                _ = RecurringJobExecutionCounter.TryGetValue(recurringJobId, out int executionCount).Should().BeTrue();
                _ = executionCount.Should().BeGreaterThan(0, "because job should have executed at least once");

                // Optional: Wait longer to see multiple executions
                await Task.Delay(3000);

                _ = RecurringJobExecutionCounter.TryGetValue(recurringJobId, out executionCount).Should().BeTrue();
                this.TestOutputHelper.WriteLine($"Recurring job executed {executionCount} times");
            }
            finally
            {
                // Clean up
                this.TestSchedulingService.RemoveRecurringJob(recurringJobId);
            }
        }

        [Fact]
        public async Task SchedulingIntegration_RemoveRecurringJob_RemovesRecurringJobAsync()
        {
            // Arrange
            string recurringJobId = $"recurring-test-{Guid.NewGuid():N}";

            this.TestSchedulingService.AddOrUpdateRecurringJob<TestRecurringJob>(
                recurringJobId,
                j => j.IncrementCounterAsync(recurringJobId),
                "*/5 * * * *",
                CoreJobStateConstants.TestQueue);

            ICoreSchedulingRecurringJobInfo? recurringJobInfo = await this.TestSchedulingService.GetRecurringJobInfoAsync(recurringJobId);
            recurringJobInfo.Should().NotBeNull("because the recurring job should exist.");
            this.OutputRecurringJobInfo(recurringJobInfo, recurringJobId);

            // Act
            this.TestSchedulingService.RemoveRecurringJob(recurringJobId);

            // Assert - verification happens implicitly (no exception means success)
            // We can't easily verify removal since there's no API to check if a recurring job exists
            this.TestOutputHelper.WriteLine($"\nRecurring job {recurringJobId} removed successfully");

            (await this.TestSchedulingService.GetRecurringJobInfoAsync(recurringJobId))
                .Should().BeNull("because the recurring job should have been removed");
        }

        [Fact]
        public async Task SchedulingIntegration_BulkChangeStateAsync_ChangesMultipleJobStates()
        {
            // Arrange
            var jobIds = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                string testJobId = this.CreateTestJobId();

                string createdJobId = this.TestSchedulingService.Create<TestJob>(
                    j => j.DoWork(testJobId),
                    new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), this.CreateTestReason()),
                    this.GetTestJobParameters(),
                    CoreJobStateConstants.TestQueue);

                jobIds.Add(createdJobId);
                this._createdJobIds.Add(createdJobId);
            }

            // Act
            IReadOnlyDictionary<string, bool> results = await this.TestSchedulingService.BulkChangeStateAsync(
                jobIds,
                new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason()));

            // Assert
            _ = results.Should().NotBeNull();
            _ = results.Count.Should().Be(jobIds.Count);

            // Allow some time for the state changes to be processed
            this.TestDelay(100, this.TestCaseLogger);

            foreach (string jobId in jobIds)
            {
                _ = results.Should().ContainKey(jobId);
                _ = results[jobId].Should().BeTrue("because each job state change should succeed");

                ICoreSchedulingJobInfo? jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(jobId);
                _ = jobInfo.Should().NotBeNull();

                this.OutputSchedulingJobInfo(jobInfo, jobId);

                // Assert that the job state has changed to Enqueued, Processing, or Succeeded
                _ = jobInfo.CurrentState.Name.Should().BeOneOf([CoreJobStateConstants.EnqueuedStateName, CoreJobStateConstants.SucceededStateName, CoreJobStateConstants.ProcessingStateName], "because state should have changed to Enqueued, Processing, or Succeeded");
            }
        }

        [Fact]
        public async Task SchedulingIntegration_BulkDeleteJobsAsync_DeletesMultipleJobs()
        {
            // Arrange
            var jobIds = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                string testJobId = this.CreateTestJobId();
                string createdJobId = this.TestSchedulingService.Create<TestJob>(
                    j => j.DoWork(testJobId),
                    new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), this.CreateTestReason()),
                    this.GetTestJobParameters(),
                    CoreJobStateConstants.TestQueue);

                jobIds.Add(createdJobId);
                this._createdJobIds.Add(createdJobId);
            }

            // Act
            IReadOnlyDictionary<string, bool> results = await this.TestSchedulingService.BulkDeleteJobsAsync(jobIds);

            // Assert
            _ = results.Should().NotBeNull();
            _ = results.Count.Should().Be(jobIds.Count);

            foreach (string jobId in jobIds)
            {
                _ = results.Should().ContainKey(jobId);
                _ = results[jobId].Should().BeTrue("because each job deletion should succeed");

                ICoreSchedulingJobInfo? jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(jobId);

                _ = jobInfo.Should().NotBeNull();
                this.OutputSchedulingJobInfo(jobInfo, jobId);

                // Assert that the job state has changed to Deleted
                _ = jobInfo.CurrentState.Name.Should().Be("Deleted", "because state should have changed to Deleted");
            }
        }

        [Fact]
        public async Task SchedulingIntegration_SucceededJobs()
        {
            const int jobCount = 5;
            var jobIds = new List<string>();

            // Act - Create multiple jobs concurrently
            Task<string>[] enqueueTasks = Enumerable.Range(0, jobCount)
                .Select(i => Task.Run(async () =>
                {
                    // Arrange
                    string testJobId = this.CreateTestJobId();
                    var enqueuedJobState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason());

                    // Act
                    string createdJobId = this.TestSchedulingService.Create<TestJob>(j => j.DoWorkAsync(testJobId), enqueuedJobState, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

                    this.TestOutputHelper.WriteLine($"Created async job with ID: {createdJobId} at {enqueuedJobState.EnqueuedAt.ToLocalTime()}");

                    // Get job details to verify final state
                    ICoreSchedulingJobInfo jobInfo = await this.ValidateSchedulingJobInfoAsync(testJobId, createdJobId, [CoreJobStateConstants.SucceededStateName]);
                    jobInfo.Should().NotBeNull("because job info should be available");

                    return createdJobId;
                }))
                .ToArray();

            string[] enqueueResults = await Task.WhenAll(enqueueTasks);
            jobIds.AddRange(enqueueResults);

            IReadOnlyList<ICoreSchedulingJobInfo> succeededJobs = await this.TestSchedulingService.SucceededJobs(0, 100);

            foreach (ICoreSchedulingJobInfo succeededJob in succeededJobs)
            {
                this.TestOutputHelper.WriteLine($"Succeeded job: {succeededJob.JobId}");
            }
        }

        [Fact]
        public async Task SchedulingIntegration_GetJobStatisticsAsync_ReturnsStatistics()
        {
            // Act
            ICoreSchedulingJobStatistics statistics = await this.TestSchedulingService.GetJobStatisticsAsync();

            // Assert
            _ = statistics.Should().NotBeNull("because statistics should be available");
            _ = statistics.TotalJobs.Should().BeGreaterThanOrEqualTo(0);
            _ = statistics.JobsByState.Should().NotBeNull();
            _ = statistics.JobsByQueue.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Total jobs: {statistics.TotalJobs}");
            this.TestOutputHelper.WriteLine($"Jobs by state: {string.Join(", ", statistics.JobsByState.Select(kv => $"{kv.Key}: {kv.Value}"))}");
            this.TestOutputHelper.WriteLine($"Jobs by queue: {string.Join(", ", statistics.JobsByQueue.Select(kv => $"{kv.Key}: {kv.Value}"))}");
        }

        [Fact]
        public async Task SchedulingIntegration_GetAvailableQueuesAsync_ReturnsQueues()
        {
            // Arrange - Create a job with our test queue
            string jobId = Guid.NewGuid().ToString("N");
            string createdJobId = this.TestSchedulingService.Create<TestJob>(
                j => j.DoWork(jobId),
                new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, this.CreateTestReason()),
                this.GetTestJobParameters(),
                CoreJobStateConstants.TestQueue);

            this._createdJobIds.Add(createdJobId);

            // Act
            IReadOnlyCollection<string> queues = await this.TestSchedulingService.GetAvailableQueuesAsync();

            // Assert
            _ = queues.Should().NotBeNull("because queues should be available");
            _ = queues.Should().Contain(CoreJobStateConstants.TestQueue, "because our test queue should be available");

            this.TestOutputHelper.WriteLine($"Available queues: {string.Join(", ", queues)}");
        }
    }
}
#endif
