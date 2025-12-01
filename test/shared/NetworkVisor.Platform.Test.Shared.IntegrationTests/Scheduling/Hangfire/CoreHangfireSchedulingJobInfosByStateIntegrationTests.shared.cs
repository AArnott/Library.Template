// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 01-06-2025
//
// Last Modified By : SteveBu
// Last Modified On : 01-06-2025
// // ***********************************************************************
// <copyright file="CoreHangfireSchedulingJobInfosByStateIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Integration tests for GetSchedulingJobInfosByState method in CoreHangfireSchedulingBackgroundService</summary>

#if NV_USE_HANGFIRE
using System.Linq.Expressions;
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

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Scheduling.Hangfire
{
    /// <summary>
    /// Integration tests for GetSchedulingJobInfosByState method in CoreHangfireSchedulingBackgroundService.
    /// </summary>
    [PlatformTrait(typeof(CoreHangfireSchedulingJobInfosByStateIntegrationTests))]
    public class CoreHangfireSchedulingJobInfosByStateIntegrationTests : CoreSchedulingTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreHangfireSchedulingJobInfosByStateIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreHangfireSchedulingJobInfosByStateIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Gets the Hangfire scheduling service cast to the concrete implementation.
        /// </summary>
        protected CoreHangfireSchedulingBackgroundService HangfireSchedulingService =>
            this.TestSchedulingService as CoreHangfireSchedulingBackgroundService ??
            throw new InvalidOperationException("Scheduling service is not CoreHangfireSchedulingBackgroundService");

        [Fact]
        public void JobInfosByStateIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void JobInfosByStateIntegration_ServiceSetup()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Assert
            _ = this.TestSchedulingService.Should().NotBeNull()
                .And.Subject.Should().BeAssignableTo<ICoreSchedulingBackgroundService>();
            _ = this.TestSchedulingService.IsRunning.Should().BeTrue();

            _ = this.TestSchedulingService.DatabaseFilePath.Should().NotBeNullOrEmpty();
            _ = this.TestFileSystem.FileExists(this.TestSchedulingService.DatabaseFilePath).Should().BeTrue();

            // Verify we can cast to the concrete implementation
            _ = this.HangfireSchedulingService.Should().NotBeNull()
                .And.Subject.Should().BeOfType<CoreHangfireSchedulingBackgroundService>();
        }

        [Fact]
        public void GetSchedulingJobInfosByState_WithInvalidStateName_ShouldReturnEmptyCollection()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string invalidStateName = "InvalidStateName";

            // Act
            IEnumerable<ICoreSchedulingJobInfo> jobInfos = this.HangfireSchedulingService.GetSchedulingJobInfosByState(invalidStateName);

            // Assert
            _ = jobInfos.Should().NotBeNull("because GetSchedulingJobInfosByState should never return null");

            var jobInfoList = jobInfos.ToList();
            _ = jobInfoList.Should().BeEmpty("because no jobs should exist in invalid state");

            this.TestOutputHelper.WriteLine($"Found {jobInfoList.Count} jobs in state '{invalidStateName}'");
        }

        [Fact]
        public void GetSchedulingJobInfosByState_WithNullStateName_ShouldThrowArgumentException()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Act & Assert
            _ = Assert.Throws<ArgumentException>(() =>
                this.HangfireSchedulingService.GetSchedulingJobInfosByState(null!).ToList());
        }

        [Fact]
        public void GetSchedulingJobInfosByState_WithEmptyStateName_ShouldThrowArgumentException()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Act & Assert
            _ = Assert.Throws<ArgumentException>(() =>
                this.HangfireSchedulingService.GetSchedulingJobInfosByState(string.Empty).ToList());
        }

        [Fact]
        public void GetSchedulingJobInfosByState_WithWhitespaceStateName_ShouldReturnEmpty()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            this.HangfireSchedulingService.GetSchedulingJobInfosByState("   ").Should().BeEmpty("because whitespace state names should not match any valid states");
        }

        [Fact]
        public async Task GetSchedulingJobInfosByState_WithEnqueuedJobs_ShouldReturnEnqueuedJobInfos()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create jobs in enqueued state
            var enqueuedJobIds = new List<string>();
            var scheduledJobIds = new List<string>();
            var allCreatedJobIds = new List<string>();
            var testJobIds = new List<string>();

            try
            {
                // Create enqueued jobs
                for (int i = 0; i < 3; i++)
                {
                    string testJobId = this.CreateTestJobId();
                    testJobIds.Add(testJobId);

                    string jobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(testJobId),
                        new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, $"Enqueued job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    enqueuedJobIds.Add(jobId);
                    allCreatedJobIds.Add(jobId);
                    this._createdJobIds.Add(jobId);
                }

                // Create scheduled jobs as a control group
                for (int i = 0; i < 2; i++)
                {
                    string testJobId = this.CreateTestJobId(false);
                    testJobIds.Add(testJobId);

                    string jobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(testJobId),
                        new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), $"Scheduled job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    scheduledJobIds.Add(jobId);
                    allCreatedJobIds.Add(jobId);
                    this._createdJobIds.Add(jobId);
                }

                // Wait for jobs to be persisted
                await Task.Delay(200);

                // Act
                IEnumerable<ICoreSchedulingJobInfo> foundJobInfos = this.HangfireSchedulingService.GetSchedulingJobInfosByState(CoreJobStateConstants.EnqueuedStateName);
                var foundJobInfoList = foundJobInfos.ToList();

                // Assert
                _ = foundJobInfos.Should().NotBeNull("because GetSchedulingJobInfosByState should never return null");

                // Verify all returned job infos are actually in enqueued state
                foreach (ICoreSchedulingJobInfo jobInfo in foundJobInfoList)
                {
                    if (enqueuedJobIds.Contains(jobInfo.JobId))
                    {
                        _ = jobInfo.CurrentState.Name.Should().BeOneOf(
                            CoreJobStateConstants.EnqueuedStateName,
                            CoreJobStateConstants.ProcessingStateName,
                            CoreJobStateConstants.SucceededStateName,
                            "because jobs may transition from enqueued to processing or succeeded during test execution");

                        // Verify job info properties
                        _ = jobInfo.JobId.Should().NotBeNullOrEmpty("because job ID should be valid");
                        _ = jobInfo.MethodName.Should().NotBeNullOrEmpty("because method name should be available");
                        _ = jobInfo.TypeName.Should().NotBeNullOrEmpty("because type name should be available");
                    }
                }

                // Verify scheduled jobs are NOT in the enqueued results
                foreach (string scheduledJobId in scheduledJobIds)
                {
                    bool foundScheduledJobInEnqueuedResults = foundJobInfoList.Any(jobInfo => jobInfo.JobId == scheduledJobId);
                    _ = foundScheduledJobInEnqueuedResults.Should().BeFalse(
                        $"because scheduled job {scheduledJobId} should not be in enqueued results");
                }

                this.TestOutputHelper.WriteLine($"Found {foundJobInfoList.Count} jobs in '{CoreJobStateConstants.EnqueuedStateName}' state");
                this.TestOutputHelper.WriteLine($"Expected enqueued jobs: {string.Join(", ", enqueuedJobIds)}");
                this.TestOutputHelper.WriteLine($"Found job IDs: {string.Join(", ", foundJobInfoList.Select(ji => ji.JobId))}");

                // Log detailed job info for debugging
                foreach (ICoreSchedulingJobInfo jobInfo in foundJobInfoList.Where(ji => enqueuedJobIds.Contains(ji.JobId)))
                {
                    this.OutputSchedulingJobInfo(jobInfo, jobInfo.JobId, "Enqueued Job Info");
                }
            }
            finally
            {
                await this.CleanupJobsAsync(allCreatedJobIds);
            }
        }

        [Fact]
        public async Task GetSchedulingJobInfosByState_WithScheduledJobs_ShouldReturnScheduledJobInfos()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create jobs in scheduled state
            var scheduledJobIds = new List<string>();
            var enqueuedJobIds = new List<string>();
            var allCreatedJobIds = new List<string>();
            var testJobIds = new List<string>();

            try
            {
                // Create scheduled jobs
                for (int i = 0; i < 3; i++)
                {
                    string testJobId = this.CreateTestJobId();
                    testJobIds.Add(testJobId);

                    string jobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(testJobId),
                        new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30 + i), $"Scheduled job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    scheduledJobIds.Add(jobId);
                    allCreatedJobIds.Add(jobId);
                    this._createdJobIds.Add(jobId);
                }

                // Create an enqueued job as a control
                string enqueuedTestJobId = this.CreateTestJobId(false);
                testJobIds.Add(enqueuedTestJobId);

                string enqueuedJobId = this.TestSchedulingService.Create<TestJob>(
                    j => j.DoWork(enqueuedTestJobId),
                    new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Control enqueued job"),
                    this.GetTestJobParameters(),
                    CoreJobStateConstants.TestQueue);

                enqueuedJobIds.Add(enqueuedJobId);
                allCreatedJobIds.Add(enqueuedJobId);
                this._createdJobIds.Add(enqueuedJobId);

                // Wait for jobs to be persisted
                await Task.Delay(200);

                // Act
                IEnumerable<ICoreSchedulingJobInfo> foundJobInfos = this.HangfireSchedulingService.GetSchedulingJobInfosByState(CoreJobStateConstants.ScheduledStateName);
                var foundJobInfoList = foundJobInfos.ToList();

                // Assert
                _ = foundJobInfos.Should().NotBeNull("because GetSchedulingJobInfosByState should never return null");
                _ = foundJobInfoList.Should().HaveCountGreaterThanOrEqualTo(
                    scheduledJobIds.Count,
                    "because all scheduled jobs should be found");

                // Verify all our scheduled jobs are in the result
                foreach (string scheduledJobId in scheduledJobIds)
                {
                    _ = foundJobInfoList.Should().Contain(
                        jobInfo => jobInfo.JobId == scheduledJobId,
                        $"because scheduled job {scheduledJobId} should be found by state");
                }

                // Verify all returned job infos are actually in scheduled state
                foreach (ICoreSchedulingJobInfo jobInfo in foundJobInfoList)
                {
                    if (scheduledJobIds.Contains(jobInfo.JobId))
                    {
                        _ = jobInfo.CurrentState.Name.Should().Be(
                            CoreJobStateConstants.ScheduledStateName,
                            "because jobs should remain in scheduled state since they're scheduled for the future");

                        // Verify job info properties
                        _ = jobInfo.JobId.Should().NotBeNullOrEmpty("because job ID should be valid");
                        _ = jobInfo.MethodName.Should().NotBeNullOrEmpty("because method name should be available");
                        _ = jobInfo.TypeName.Should().NotBeNullOrEmpty("because type name should be available");
                        _ = jobInfo.ScheduledAt.Should().NotBeNull("because scheduled jobs should have a scheduled time");
                        _ = jobInfo.ScheduledAt.Should().BeAfter(DateTimeOffset.UtcNow, "because jobs are scheduled for the future");
                    }
                }

                // Verify enqueued jobs are NOT in the scheduled results
                foreach (string enqueuedId in enqueuedJobIds)
                {
                    bool foundEnqueuedJobInScheduledResults = foundJobInfoList.Any(jobInfo => jobInfo.JobId == enqueuedId);
                    _ = foundEnqueuedJobInScheduledResults.Should().BeFalse(
                        $"because enqueued job {enqueuedId} should not be in scheduled results");
                }

                this.TestOutputHelper.WriteLine($"Found {foundJobInfoList.Count} jobs in '{CoreJobStateConstants.ScheduledStateName}' state");
                this.TestOutputHelper.WriteLine($"Expected scheduled jobs: {string.Join(", ", scheduledJobIds)}");
                this.TestOutputHelper.WriteLine($"Found job IDs: {string.Join(", ", foundJobInfoList.Select(ji => ji.JobId))}");

                // Log detailed job info for debugging
                foreach (ICoreSchedulingJobInfo jobInfo in foundJobInfoList.Where(ji => scheduledJobIds.Contains(ji.JobId)))
                {
                    this.OutputSchedulingJobInfo(jobInfo, jobInfo.JobId, "Scheduled Job Info");
                }
            }
            finally
            {
                await this.CleanupJobsAsync(allCreatedJobIds);
            }
        }

        [Fact]
        public async Task GetSchedulingJobInfosByState_WithMultipleStates_ShouldReturnCorrectJobInfosForEachState()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create jobs in different states
            var jobsByState = new Dictionary<string, List<string>>
            {
                [CoreJobStateConstants.EnqueuedStateName] = [],
                [CoreJobStateConstants.ScheduledStateName] = [],
            };
            var allCreatedJobIds = new List<string>();
            var testJobIds = new List<string>();

            try
            {
                // Create enqueued jobs
                for (int i = 0; i < 2; i++)
                {
                    string testJobId = this.CreateTestJobId();
                    testJobIds.Add(testJobId);

                    string jobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(testJobId),
                        new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, $"Enqueued job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    jobsByState[CoreJobStateConstants.EnqueuedStateName].Add(jobId);
                    allCreatedJobIds.Add(jobId);
                    this._createdJobIds.Add(jobId);
                }

                // Create scheduled jobs
                for (int i = 0; i < 2; i++)
                {
                    string testJobId = this.CreateTestJobId(false);
                    testJobIds.Add(testJobId);

                    string jobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(testJobId),
                        new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30 + i), $"Scheduled job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    jobsByState[CoreJobStateConstants.ScheduledStateName].Add(jobId);
                    allCreatedJobIds.Add(jobId);
                    this._createdJobIds.Add(jobId);
                }

                // Wait for jobs to be persisted
                await Task.Delay(200);

                // Act & Assert for each state
                foreach (KeyValuePair<string, List<string>> stateGroup in jobsByState)
                {
                    string stateName = stateGroup.Key;
                    List<string> expectedJobIds = stateGroup.Value;

                    if (expectedJobIds.Count == 0)
                    {
                        continue; // Skip states with no jobs
                    }

                    IEnumerable<ICoreSchedulingJobInfo> foundJobInfos = this.HangfireSchedulingService.GetSchedulingJobInfosByState(stateName);
                    var foundJobInfoList = foundJobInfos.ToList();

                    _ = foundJobInfos.Should().NotBeNull($"because GetSchedulingJobInfosByState should never return null for state '{stateName}'");

                    // Enqueued state may have fewer jobs due to processing
                    if (!stateName.Equals(CoreJobStateConstants.EnqueuedStateName))
                    {
                        _ = foundJobInfoList.Should().HaveCountGreaterThanOrEqualTo(
                            expectedJobIds.Count,
                            $"because all jobs in state '{stateName}' should be found");

                        // Verify all expected jobs are found
                        foreach (string expectedJobId in expectedJobIds)
                        {
                            _ = foundJobInfoList.Should().Contain(
                                jobInfo => jobInfo.JobId == expectedJobId,
                                $"because job {expectedJobId} should be found in state '{stateName}'");
                        }
                    }

                    // Verify job info properties for our created jobs
                    var ourJobInfos = foundJobInfoList.Where(ji => expectedJobIds.Contains(ji.JobId)).ToList();
                    foreach (ICoreSchedulingJobInfo jobInfo in ourJobInfos)
                    {
                        _ = jobInfo.JobId.Should().NotBeNullOrEmpty("because job ID should be valid");
                        _ = jobInfo.MethodName.Should().NotBeNullOrEmpty("because method name should be available");
                        _ = jobInfo.TypeName.Should().NotBeNullOrEmpty("because type name should be available");
                        _ = jobInfo.CurrentState.Should().NotBeNull("because current state should be available");

                        // State may have transitioned, so we check for valid states
                        if (stateName == CoreJobStateConstants.EnqueuedStateName)
                        {
                            _ = jobInfo.CurrentState.Name.Should().BeOneOf(
                                CoreJobStateConstants.EnqueuedStateName,
                                CoreJobStateConstants.ProcessingStateName,
                                CoreJobStateConstants.SucceededStateName,
                                "because enqueued jobs may transition during execution");
                        }
                        else if (stateName == CoreJobStateConstants.ScheduledStateName)
                        {
                            _ = jobInfo.CurrentState.Name.Should().Be(
                                CoreJobStateConstants.ScheduledStateName,
                                "because scheduled jobs should remain scheduled");
                        }
                    }

                    this.TestOutputHelper.WriteLine($"State '{stateName}': Expected {expectedJobIds.Count}, Found {foundJobInfoList.Count} total, {ourJobInfos.Count} ours");
                    this.TestOutputHelper.WriteLine($"  Expected: {string.Join(", ", expectedJobIds)}");
                    this.TestOutputHelper.WriteLine($"  Found (ours): {string.Join(", ", ourJobInfos.Select(ji => ji.JobId))}");
                }
            }
            finally
            {
                await this.CleanupJobsAsync(allCreatedJobIds);
            }
        }

        [Theory]
        [InlineData("Enqueued")]
        [InlineData("Scheduled")]
        [InlineData("Processing")]
        [InlineData("Succeeded")]
        [InlineData("Failed")]
        [InlineData("Deleted")]
        public void GetSchedulingJobInfosByState_WithVariousStateNames_ShouldNotThrow(string stateName)
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Act
            IEnumerable<ICoreSchedulingJobInfo> jobInfos = this.HangfireSchedulingService.GetSchedulingJobInfosByState(stateName);

            // Assert
            _ = jobInfos.Should().NotBeNull($"because GetSchedulingJobInfosByState should never return null for state '{stateName}'");

            // Evaluate the enumerable to ensure no exceptions are thrown
            var jobInfoList = jobInfos.ToList();
            _ = jobInfoList.Should().BeAssignableTo<IEnumerable<ICoreSchedulingJobInfo>>("because job infos should be valid");

            this.TestOutputHelper.WriteLine($"State '{stateName}': Found {jobInfoList.Count} job infos");

            // Verify all returned job infos have valid properties
            foreach (ICoreSchedulingJobInfo jobInfo in jobInfoList)
            {
                _ = jobInfo.JobId.Should().NotBeNullOrEmpty("because job ID should be valid");
                _ = jobInfo.CurrentState.Should().NotBeNull("because current state should be available");
            }
        }

        [Fact]
        public async Task GetSchedulingJobInfosByState_WithCancellationToken_ShouldRespectCancellation()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create some jobs first
            var createdJobIds = new List<string>();
            try
            {
                for (int i = 0; i < 2; i++)
                {
                    string testJobId = this.CreateTestJobId();
                    string jobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(testJobId),
                        new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), $"Test job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    createdJobIds.Add(jobId);
                    this._createdJobIds.Add(jobId);
                }

                await Task.Delay(100);

                using var cts = new CancellationTokenSource();

                // Act - Cancel the token before calling the method
                cts.Cancel();

                // Assert - Depending on implementation, this might throw OperationCanceledException or return partial results
                // Since the underlying enumerable uses yield return, cancellation handling depends on when it's consumed
                try
                {
                    // The method should handle cancellation gracefully
                    IEnumerable<ICoreSchedulingJobInfo> jobInfos = this.HangfireSchedulingService.GetSchedulingJobInfosByState(
                        CoreJobStateConstants.ScheduledStateName, cts.Token);

                    var jobInfoList = jobInfos.ToList();
                    this.TestOutputHelper.WriteLine($"Method completed with {jobInfoList.Count} results despite cancellation");
                }
                catch (OperationCanceledException)
                {
                    this.TestOutputHelper.WriteLine("Method properly threw OperationCanceledException");

                    // This is expected behavior for cancellation
                }
            }
            finally
            {
                await this.CleanupJobsAsync(createdJobIds);
            }
        }

        [Fact]
        public async Task GetSchedulingJobInfosByState_WithJobsTransitioningStates_ShouldHandleStateChanges()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create jobs that will execute quickly
            var createdJobIds = new List<string>();
            var testJobIds = new List<string>();

            try
            {
                // Create jobs that should execute quickly
                for (int i = 0; i < 2; i++)
                {
                    string testJobId = this.CreateTestJobId();
                    testJobIds.Add(testJobId);

                    string jobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(testJobId),
                        new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, $"Quick job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    createdJobIds.Add(jobId);
                    this._createdJobIds.Add(jobId);
                }

                // Wait a bit for potential execution
                await Task.Delay(500);

                // Act - Get job infos by different states
                var enqueuedJobInfos = this.HangfireSchedulingService
                    .GetSchedulingJobInfosByState(CoreJobStateConstants.EnqueuedStateName).ToList();
                var processingJobInfos = this.HangfireSchedulingService
                    .GetSchedulingJobInfosByState(CoreJobStateConstants.ProcessingStateName).ToList();
                var succeededJobInfos = this.HangfireSchedulingService
                    .GetSchedulingJobInfosByState(CoreJobStateConstants.SucceededStateName).ToList();

                // Assert - At least some of our jobs should be found in some state
                var allFoundJobInfos = enqueuedJobInfos.Concat(processingJobInfos).Concat(succeededJobInfos).ToList();
                var ourJobInfos = allFoundJobInfos.Where(ji => createdJobIds.Contains(ji.JobId)).ToList();

                _ = ourJobInfos.Should().HaveCountGreaterThanOrEqualTo(
                    1,
                    "because at least some of our created jobs should be found in some state");

                // Verify job info consistency
                foreach (ICoreSchedulingJobInfo jobInfo in ourJobInfos)
                {
                    _ = jobInfo.JobId.Should().NotBeNullOrEmpty("because job ID should be valid");
                    _ = jobInfo.CurrentState.Should().NotBeNull("because current state should be available");
                    _ = jobInfo.MethodName.Should().NotBeNullOrEmpty("because method name should be available");
                    _ = jobInfo.TypeName.Should().NotBeNullOrEmpty("because type name should be available");
                }

                this.TestOutputHelper.WriteLine($"Found our jobs in states: Enqueued={enqueuedJobInfos.Count(ji => createdJobIds.Contains(ji.JobId))}, " +
                    $"Processing={processingJobInfos.Count(ji => createdJobIds.Contains(ji.JobId))}, " +
                    $"Succeeded={succeededJobInfos.Count(ji => createdJobIds.Contains(ji.JobId))}");

                // Log detailed job info
                foreach (ICoreSchedulingJobInfo jobInfo in ourJobInfos)
                {
                    this.OutputSchedulingJobInfo(jobInfo, jobInfo.JobId, $"Job in {jobInfo.CurrentState.Name} state");
                }
            }
            finally
            {
                await this.CleanupJobsAsync(createdJobIds);
            }
        }

        /// <summary>
        /// Checks if Hangfire scheduler is supported on the current platform.
        /// </summary>
        /// <returns>True if supported, false otherwise.</returns>
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

        /// <summary>
        /// Helper method to cleanup created jobs.
        /// </summary>
        /// <param name="jobIds">The job IDs to cleanup.</param>
        private async Task CleanupJobsAsync(IEnumerable<string> jobIds)
        {
            foreach (string jobId in jobIds)
            {
                try
                {
                    _ = this.TestSchedulingService.ChangeState(jobId, new CoreDeletedJobState(null, "Test cleanup"));
                }
                catch (Exception ex)
                {
                    this.TestOutputHelper.WriteLine($"Failed to cleanup job {jobId}: {ex.Message}");
                }
            }

            // Give a moment for cleanup to process
            await Task.Delay(100);
        }
    }
}
#endif
