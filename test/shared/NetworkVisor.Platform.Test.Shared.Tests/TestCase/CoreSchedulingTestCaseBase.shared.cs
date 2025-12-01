// Assembly         : NetworkVisor.Platform.Test.Shared
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 10-05-2024
// // ***********************************************************************
// <copyright file="CoreSchedulingTestCaseBase.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;
using FluentAssertions;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Networking.Monitoring;
using NetworkVisor.Core.Scheduling.Services;
using NetworkVisor.Core.Scheduling.Services.JobStates;
using NetworkVisor.Core.Scheduling.Services.Monitoring;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Fixtures;
using NetworkVisor.Platform.Test.Fixtures;
using Xunit;
#if NV_USE_HANGFIRE
using Hangfire;
#endif

namespace NetworkVisor.Platform.Test.TestCase
{
    public abstract class CoreSchedulingTestCaseBase : CoreTestClassBase, IClassFixture<CoreTestClassFixture>
    {
        // Static collections to track job execution across test runs
        protected static readonly ConcurrentDictionary<string, bool> JobExecutionTracker = new();
        protected static readonly ConcurrentDictionary<string, int> RecurringJobExecutionCounter = new();
        protected static readonly ConcurrentDictionary<string, string> JobResultsTracker = new();

        /// <summary>
        /// Default timeout for job execution in tests.
        /// </summary>
        protected static readonly TimeSpan DefaultJobTimeout = TimeSpan.FromSeconds(30);

        protected readonly List<string> _createdJobIds = [];

        protected JsonSerializerOptions _jsonOptions;

        private bool disposedValue;

        protected CoreSchedulingTestCaseBase(ICoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            // Get the already running instance from TestNetworkServices
            this.TestSchedulingService = this.TestNetworkServices.SchedulingBackgroundService;

            // Get the already running instance from TestNetworkServices
            this.TestNetworkDeviceMonitoringService = this.TestNetworkServices.NetworkDeviceMonitoringService;

            this._jsonOptions = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider);

#if NV_USE_HANGFIRE
            // Verify the Scheduling service is available
            _ = this.TestSchedulingService.Should().NotBeNull("because the scheduling service should not be null");
            _ = this.TestSchedulingService.IsRunning.Should().BeTrue("because the scheduling service should be running");
            _ = this.TestSchedulingService.DatabaseFilePath.Should().NotBeNullOrEmpty("because the scheduling service should have a valid database path");
            _ = this.TestFileSystem.FileExists(this.TestSchedulingService.DatabaseFilePath).Should().BeTrue("because the scheduling service database file should exist");
            this.TestOutputHelper.WriteLine($"Using scheduling service with database at: {this.TestSchedulingService.DatabaseFilePath}\n");

            _ = this.TestNetworkDeviceMonitoringService.Should().NotBeNull("because the network device monitoring service should not be null");
#endif
        }

        protected ICoreSchedulingBackgroundService TestSchedulingService { get; }

        protected ICoreNetworkDeviceMonitoringService TestNetworkDeviceMonitoringService { get; }

        protected bool CleanupJobs { get; set; } = true;

        protected string HangFireDatabasePath => this.TestSchedulingService.DatabaseFilePath;

        protected string CreateTestReason(string? reason = null)
        {
            return $"Test_{this.TestDisplayName}{": " + reason}";
        }

        protected IDictionary<string, object>? GetTestJobParameters()
        {
            return null;
        }

        /// <summary>
        /// Creates a queue of state names representing the transition order of job states in the scheduling process.
        /// </summary>
        /// <returns>
        /// A <see cref="Queue{T}"/> containing state names in the following order:
        /// <list type="number">
        /// <item><description><see cref="CoreJobStateConstants.ScheduledStateName"/></description></item>
        /// <item><description><see cref="CoreJobStateConstants.ProcessingStateName"/></description></item>
        /// <item><description><see cref="CoreJobStateConstants.SucceededStateName"/></description></item>
        /// <item><description><see cref="CoreJobStateConstants.DeletedStateName"/></description></item>
        /// </list>
        /// </returns>
        protected Queue<string> CreateStateNameTransitionQueue() => new Queue<string>([CoreJobStateConstants.ScheduledStateName, CoreJobStateConstants.ProcessingStateName, CoreJobStateConstants.SucceededStateName, CoreJobStateConstants.DeletedStateName]);

        /// <summary>
        /// Waits for a recurring job to execute at least once within a specified timeout period.
        /// </summary>
        /// <param name="recurringJobId">The identifier of the recurring job to monitor.</param>
        /// <param name="timeout">The maximum duration to wait for the recurring job to execute.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result is <see langword="true"/> if the recurring job executed at least once within the timeout period; otherwise, <see langword="false"/>.
        /// </returns>
        /// <remarks>
        /// This method checks the execution status of a recurring job by monitoring the <see cref="RecurringJobExecutionCounter"/>.
        /// If the job executes successfully within the timeout period, the method returns <see langword="true"/>. Otherwise, it returns <see langword="false"/>.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="recurringJobId"/> is <see langword="null"/> or empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if <paramref name="timeout"/> is less than or equal to <see cref="TimeSpan.Zero"/>.</exception>
        protected async Task<bool> WaitForRecurringJobExecutionAsync(string recurringJobId, TimeSpan? timeout = null)
        {
            timeout ??= DefaultJobTimeout;
            DateTime startTime = DateTime.Now;
            this.TestOutputHelper.WriteLine($"\nStarting recurring job {recurringJobId} at {startTime}");

            while (DateTime.Now - startTime < timeout)
            {
                if (RecurringJobExecutionCounter.TryGetValue(recurringJobId, out int count) && count > 0)
                {
                    this.TestOutputHelper.WriteLine($"Completed recurring job {recurringJobId} successfully at {DateTime.Now} with execution time of {DateTime.Now.Subtract(startTime)}");

                    // Wait for the job result to be set
                    await Task.Delay(100).ConfigureAwait(false);
                    return true;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }

            return false;
        }

        protected string CreateTestJobId(bool resetTracking = true)
        {
            if (resetTracking)
            {
                TestJob.ResetTracking();
            }

            return Guid.NewGuid().ToString("N");
        }

        protected async Task<ICoreSchedulingJobInfo> ValidateSchedulingJobInfoAsync(string testJobId, string createdJobId, string[] expectedStateNames, bool isFinalState = true, string? queue = CoreJobStateConstants.TestQueue, TimeSpan? expectedTimeout = null)
        {
            expectedTimeout ??= DefaultJobTimeout;
            this._createdJobIds.Add(createdJobId);

            DateTime startTime = DateTime.Now;
            this.TestOutputHelper.WriteLine($"\nStarting job {testJobId} at {startTime}");
            ICoreSchedulingJobInfo? jobInfo = null;
            int retry = 0;

            while (DateTime.Now - startTime < expectedTimeout)
            {
                if (JobExecutionTracker.TryGetValue(testJobId, out bool executed) && executed)
                {
                    this.TestOutputHelper.WriteLine($"Completed job {testJobId} successfully at {DateTime.Now} with execution time of {DateTime.Now.Subtract(startTime)}");

                    // Wait for the job result to be set
                    await Task.Delay(100).ConfigureAwait(false);

                    while (retry++ < CoreJobStateConstants.DefaultMaxRetryCount && DateTime.Now - startTime < expectedTimeout)
                    {
                        jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);

                        if (jobInfo is not null && expectedStateNames.Contains(jobInfo.CurrentState.Name, StringComparer.InvariantCultureIgnoreCase))
                        {
                            break;
                        }

                        this.TestOutputHelper.WriteLine($"Job {testJobId} is not in final state ({jobInfo?.CurrentState.Name ?? "JobInfo is null"}), retrying... ({retry} of {CoreJobStateConstants.DefaultMaxRetryCount})");

                        // Wait 1 second before retrying
                        await Task.Delay((retry * 100) + 900).ConfigureAwait(false);
                    }

                    _ = jobInfo.Should().NotBeNull("because job info should be available");
                    this.ValidateSchedulingJobInfo(jobInfo, retry, testJobId, createdJobId, expectedStateNames, isFinalState, queue, expectedTimeout);

                    return jobInfo;
                }

                await Task.Delay(100).ConfigureAwait(false);
            }

            // On timeout, get the latest job info to output
            jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(createdJobId);

            if (jobInfo is not null)
            {
                if (expectedStateNames.Contains(jobInfo.CurrentState.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    this.ValidateSchedulingJobInfo(jobInfo, retry, testJobId, createdJobId, expectedStateNames, isFinalState, queue, expectedTimeout);

                    return jobInfo;
                }
            }

            throw new TimeoutException($"Job {testJobId} timed out after {expectedTimeout}");
        }

        protected void ValidateSchedulingJobInfo(ICoreSchedulingJobInfo? jobInfo, int retry, string testJobId, string createdJobId, string[] expectedStateNames, bool isFinalState = true, string? queue = CoreJobStateConstants.TestQueue, TimeSpan? expectedTimeout = null)
        {
            _ = jobInfo.Should().NotBeNull("because job info should be available");

            this.OutputSchedulingJobInfo(jobInfo, createdJobId);

            retry.Should().BeLessThan(CoreJobStateConstants.DefaultMaxRetryCount, "because job should reach final state within retry limit");

            _ = jobInfo.MethodName.Should().NotBeNullOrEmpty("because job method name should be available");
            _ = jobInfo.TypeName.Should().NotBeNullOrEmpty("because job type name should be available");
            _ = jobInfo.Arguments.Should().NotBeNullOrEmpty("because job arguments should be available");

            // Validate CreateAt timestamp is in UTC.
            _ = jobInfo.CreatedAt.Offset.Should().Be(TimeSpan.Zero, "because job creation time should be in UTC");
            _ = jobInfo.CreatedAt.Should()
                .BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(45), "because job creation time should be close to now");

            // Validate UpdatedAt timestamp is in UTC.
            _ = jobInfo.UpdatedAt.Should().NotBeNull("because job updated time should not be null");
            _ = jobInfo.UpdatedAt?.Offset.Should().Be(TimeSpan.Zero, "because job updated time should be in UTC");
            _ = jobInfo.UpdatedAt.Should()
                .BeCloseTo(jobInfo.CreatedAt, TimeSpan.FromSeconds(45), "because job updated time should be close to CreatedAt");

            _ = jobInfo.RetryCount.Should().Be(0, "because job should not have retried");
            _ = jobInfo.MaxRetryCount.Should().Be(CoreJobStateConstants.DefaultMaxRetryCount, "because job should have the default retry limit");

            _ = jobInfo.JobId.Should().Be(createdJobId, "because job ID should match");
            _ = jobInfo.Queue.Should().Be(queue, "because job should be retryable if it has not failed");

            _ = jobInfo.CurrentState.Name.Should().BeOneOf(expectedStateNames, "because job should be the one of the expected states");
            _ = jobInfo.IsInFinalState.Should().Be(isFinalState, "because job should be in a final state");
        }

        protected void OutputSchedulingJobInfo(ICoreSchedulingJobInfo? jobInfo, string createdJobId, string? titlePrefix = null)
        {
            if (jobInfo is not null)
            {
                titlePrefix ??= "Job Info";
                string json = JsonSerializer.Serialize(jobInfo, this._jsonOptions);

                this.TestOutputHelper.WriteLine($"\n{$"{titlePrefix} ID: {createdJobId}".CenterTitle()}\n{json}");
            }
        }

        protected void OutputRecurringJobInfo(ICoreSchedulingRecurringJobInfo? recurringJobInfo, string createdJobId, string? titlePrefix = null)
        {
            if (recurringJobInfo is not null)
            {
                titlePrefix ??= "Recurring Job Info";
                string json = JsonSerializer.Serialize(recurringJobInfo, this._jsonOptions);

                this.TestOutputHelper.WriteLine($"\n{$"{titlePrefix} ID: {createdJobId}".CenterTitle()}\n{json}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposedValue)
            {
                try
                {
                    if (disposing)
                    {
                        if (this.CleanupJobs && this._createdJobIds.Count > 0)
                        {
                            try
                            {
                                this.TestOutputHelper.WriteLine($"\nCleaning up {this._createdJobIds.Count} test jobs...");
                                _ = this.TestSchedulingService.BulkDeleteJobsAsync(this._createdJobIds).GetAwaiter().GetResult();
                                this._createdJobIds.Clear();
                            }
                            catch (Exception ex)
                            {
                                this.TestOutputHelper.WriteLine($"Error during test cleanup: {ex.Message}");
                            }
                        }
                    }
                }
                finally
                {
                    this.disposedValue = true;
                    base.Dispose(disposing);
                }
            }
        }

        /// <summary>
        /// Test job class for integration tests.
        /// </summary>
        public class TestJob
        {
            /// <summary>
            /// Marks the specified job as completed by updating the job execution tracker.
            /// </summary>
            /// <param name="jobId">The unique identifier of the job to be marked as completed.</param>
            /// <returns>A completed <see cref="Task"/> representing the asynchronous operation.</returns>
            public static Task SetJobCompleted(string jobId)
            {
                JobExecutionTracker[jobId] = true;
                return Task.CompletedTask;
            }

            public static void ResetTracking()
            {
                JobExecutionTracker.Clear();
                JobResultsTracker.Clear();
            }

            public void DoWork(string jobId)
            {
                JobExecutionTracker[jobId] = true;
                JobResultsTracker[jobId] = $"Completed at {DateTime.Now}";
            }

            public Task DoWorkAsync(string jobId)
            {
                JobExecutionTracker[jobId] = true;
                JobResultsTracker[jobId] = $"Async completed at {DateTime.Now}";
                return Task.CompletedTask;
            }

#if NV_USE_HANGFIRE
            [AutomaticRetry(Attempts = 0)]
#endif
            public void ThrowException(string jobId)
            {
                JobExecutionTracker[jobId] = true;
                throw new InvalidOperationException($"Test exception for job {jobId}");
            }

            public void ThrowExceptionWithRetries(string jobId)
            {
                JobExecutionTracker[jobId] = true;
                throw new InvalidOperationException($"Retrying test exception for job {jobId}");
            }
        }

        /// <summary>
        /// Test job class for recurring jobs.
        /// </summary>
        public class TestRecurringJob
        {
            public static void ResetTracking()
            {
                RecurringJobExecutionCounter.Clear();
            }

            public Task IncrementCounterAsync(string id)
            {
                if (!RecurringJobExecutionCounter.ContainsKey(id))
                {
                    RecurringJobExecutionCounter[id] = 0;
                }

                RecurringJobExecutionCounter[id]++;
                return Task.CompletedTask;
            }
        }

        /// <summary>
        /// Test job class for integration testing.
        /// </summary>
        public class IntegrationTestJobClass
        {
            public static void StaticTestMethod(string parameter)
            {
                Console.WriteLine($"Integration test static job executed with parameter: {parameter}");
            }

            public static async Task StaticAsyncTestMethod(string parameter)
            {
                Console.WriteLine($"Integration test static async job executed with parameter: {parameter}");
                await Task.Delay(50); // Shorter delay for integration tests
            }

            public void TestMethod(string parameter)
            {
                Console.WriteLine($"Integration test job executed with parameter: {parameter}");
            }

            public async Task TestAsyncMethod(string parameter)
            {
                Console.WriteLine($"Integration test async job executed with parameter: {parameter}");
                await Task.Delay(50); // Shorter delay for integration tests
            }
        }
    }
}
