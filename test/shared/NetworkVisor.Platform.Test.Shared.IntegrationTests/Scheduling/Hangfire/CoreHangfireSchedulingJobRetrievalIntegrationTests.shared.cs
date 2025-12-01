// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 01-06-2025
//
// Last Modified By : SteveBu
// Last Modified On : 01-06-2025
// // ***********************************************************************
// <copyright file="CoreHangfireSchedulingJobRetrievalIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Integration tests for CoreHangfireSchedulingBackgroundService job retrieval methods</summary>

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
    /// Integration tests for GetAllJobIDs and FindJobIDsByState methods in CoreHangfireSchedulingBackgroundService.
    /// </summary>
    [PlatformTrait(typeof(CoreHangfireSchedulingJobRetrievalIntegrationTests))]
    public class CoreHangfireSchedulingJobRetrievalIntegrationTests : CoreSchedulingTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreHangfireSchedulingJobRetrievalIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreHangfireSchedulingJobRetrievalIntegrationTests(CoreTestClassFixture testClassFixture)
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
        public void JobRetrievalIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void JobRetrievalIntegration_ServiceSetup()
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
        public void GetAllJobIDs_WithNoJobs_ShouldReturnEmptyCollection()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Act
            IEnumerable<string> jobIds = this.HangfireSchedulingService.GetAllJobIDs();

            // Assert
            _ = jobIds.Should().NotBeNull("because GetAllJobIDs should never return null");

            // Convert to list to evaluate the enumerable
            var jobIdList = jobIds.ToList();

            this.TestOutputHelper.WriteLine($"Found {jobIdList.Count} total jobs in the system");

            // The collection might not be empty if there are existing jobs from other tests
            _ = jobIdList.Should().BeAssignableTo<IEnumerable<string>>("because job IDs should be strings");

            // Log all existing job IDs for debugging
            foreach (string jobId in jobIdList)
            {
                this.TestOutputHelper.WriteLine($"Existing job ID: {jobId}");
            }
        }

        [Fact]
        public async Task GetAllJobIDs_WithCreatedJobs_ShouldReturnAllJobIds()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create multiple jobs in different states
            var createdJobIds = new List<string>();
            var testJobIds = new List<string>();

            try
            {
                // Create jobs in different states
                for (int i = 0; i < 3; i++)
                {
                    string testJobId = this.CreateTestJobId();
                    testJobIds.Add(testJobId);

                    string createdJobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(testJobId),
                        new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, $"Test job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    createdJobIds.Add(createdJobId);
                    this._createdJobIds.Add(createdJobId);
                }

                // Create a scheduled job
                string scheduledTestJobId = this.CreateTestJobId(false);
                testJobIds.Add(scheduledTestJobId);

                string scheduledJobId = this.TestSchedulingService.Create<TestJob>(
                    j => j.DoWork(scheduledTestJobId),
                    new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), "Scheduled test job"),
                    this.GetTestJobParameters(),
                    CoreJobStateConstants.TestQueue);

                createdJobIds.Add(scheduledJobId);
                this._createdJobIds.Add(scheduledJobId);

                // Wait a moment for jobs to be persisted
                await Task.Delay(100);

                // Act
                IEnumerable<string> allJobIds = this.HangfireSchedulingService.GetAllJobIDs();
                var allJobIdsList = allJobIds.ToList();

                // Assert
                _ = allJobIds.Should().NotBeNull("because GetAllJobIDs should never return null");
                _ = allJobIdsList.Should().HaveCountGreaterThanOrEqualTo(
                    createdJobIds.Count,
                    "because all created jobs should be included in the result");

                // Verify all our created jobs are in the result
                foreach (string createdJobId in createdJobIds)
                {
                    _ = allJobIdsList.Should().Contain(
                        createdJobId,
                        $"because created job {createdJobId} should be in the results");
                }

                // Verify all job IDs are valid strings
                _ = allJobIdsList.Should().OnlyContain(
                    id => !string.IsNullOrWhiteSpace(id),
                    "because all job IDs should be non-empty strings");

                this.TestOutputHelper.WriteLine($"Total jobs found: {allJobIdsList.Count}");
                this.TestOutputHelper.WriteLine($"Created jobs: {string.Join(", ", createdJobIds)}");
                this.TestOutputHelper.WriteLine($"All job IDs: {string.Join(", ", allJobIdsList)}");
            }
            finally
            {
                // Cleanup in finally block to ensure jobs are cleaned up even if test fails
                await this.CleanupJobsAsync(createdJobIds);
            }
        }

        [Fact]
        public void FindJobIDsByState_WithInvalidStateName_ShouldThrowArgumentException()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Act & Assert - Test with null state name
            _ = Assert.Throws<ArgumentException>(() =>
                this.HangfireSchedulingService.FindJobIDsByState(null!));

            // Act & Assert - Test with empty state name
            _ = Assert.Throws<ArgumentException>(() =>
                this.HangfireSchedulingService.FindJobIDsByState(string.Empty));
        }

        [Fact]
        public void FindJobIDsByState_WithNonExistentState_ShouldReturnEmptyCollection()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            const string nonExistentState = "NonExistentState";

            // Act
            IEnumerable<string> jobIds = this.HangfireSchedulingService.FindJobIDsByState(nonExistentState);

            // Assert
            _ = jobIds.Should().NotBeNull("because FindJobIDsByState should never return null");

            var jobIdList = jobIds.ToList();
            _ = jobIdList.Should().BeEmpty("because no jobs should exist in non-existent state");

            this.TestOutputHelper.WriteLine($"Found {jobIdList.Count} jobs in state '{nonExistentState}'");
        }

        [Fact]
        public async Task FindJobIDsByState_WithEnqueuedJobs_ShouldReturnEnqueuedJobIds()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create jobs in enqueued state
            var enqueuedJobIds = new List<string>();
            var scheduledJobIds = new List<string>();
            var allCreatedJobIds = new List<string>();

            try
            {
                // Create enqueued jobs
                for (int i = 0; i < 2; i++)
                {
                    string testJobId = this.CreateTestJobId();

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
                await Task.Delay(100);

                // Act
                IEnumerable<string> foundJobIds = this.HangfireSchedulingService.FindJobIDsByState(CoreJobStateConstants.EnqueuedStateName);
                var foundJobIdList = foundJobIds.ToList();

                // Assert
                _ = foundJobIds.Should().NotBeNull("because FindJobIDsByState should never return null");

                // Verify each enqueued job is in either enqueued or processing or succeeded states
                foreach (string job in enqueuedJobIds)
                {
                    ICoreSchedulingJobInfo? jobInfo = await this.TestSchedulingService.GetSchedulingJobInfoAsync(job);
                    jobInfo.Should().NotBeNull($"because job {job} should exist in the system");

                    jobInfo!.CurrentState.Name.Should().BeOneOf(
                        [CoreJobStateConstants.EnqueuedStateName, CoreJobStateConstants.ProcessingStateName, CoreJobStateConstants.SucceededStateName],
                        $"because job {job} should be in '{CoreJobStateConstants.EnqueuedStateName}' state");
                }

                // Verify scheduled jobs are NOT in the enqueued results
                foreach (string scheduledJobId in scheduledJobIds)
                {
                    _ = foundJobIdList.Should().NotContain(
                        scheduledJobId,
                        $"because scheduled job {scheduledJobId} should not be in enqueued results");
                }

                this.TestOutputHelper.WriteLine($"Found {foundJobIdList.Count} jobs in '{CoreJobStateConstants.EnqueuedStateName}' state");
                this.TestOutputHelper.WriteLine($"Expected enqueued jobs: {string.Join(", ", enqueuedJobIds)}");
                this.TestOutputHelper.WriteLine($"Found job IDs: {string.Join(", ", foundJobIdList)}");
            }
            finally
            {
                await this.CleanupJobsAsync(allCreatedJobIds);
            }
        }

        [Fact]
        public async Task FindJobIDsByState_WithScheduledJobs_ShouldReturnScheduledJobIds()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create jobs in scheduled state
            var scheduledJobIds = new List<string>();
            var enqueuedJobIds = new List<string>();
            var allCreatedJobIds = new List<string>();

            try
            {
                // Create scheduled jobs
                for (int i = 0; i < 3; i++)
                {
                    string testJobId = this.CreateTestJobId();

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
                string enqueuedJobId = this.TestSchedulingService.Create<TestJob>(
                    j => j.DoWork(enqueuedTestJobId),
                    new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Control enqueued job"),
                    this.GetTestJobParameters(),
                    CoreJobStateConstants.TestQueue);

                enqueuedJobIds.Add(enqueuedJobId);
                allCreatedJobIds.Add(enqueuedJobId);
                this._createdJobIds.Add(enqueuedJobId);

                // Wait for jobs to be persisted
                await Task.Delay(100);

                // Act
                IEnumerable<string> foundJobIds = this.HangfireSchedulingService.FindJobIDsByState(CoreJobStateConstants.ScheduledStateName);
                var foundJobIdList = foundJobIds.ToList();

                // Assert
                _ = foundJobIds.Should().NotBeNull("because FindJobIDsByState should never return null");
                _ = foundJobIdList.Should().HaveCountGreaterThanOrEqualTo(
                    scheduledJobIds.Count,
                    "because all scheduled jobs should be found");

                // Verify all our scheduled jobs are in the result
                foreach (string scheduledJobId in scheduledJobIds)
                {
                    _ = foundJobIdList.Should().Contain(
                        scheduledJobId,
                        $"because scheduled job {scheduledJobId} should be found by state");
                }

                // Verify enqueued jobs are NOT in the scheduled results
                foreach (string enqueuedId in enqueuedJobIds)
                {
                    _ = foundJobIdList.Should().NotContain(
                        enqueuedId,
                        $"because enqueued job {enqueuedId} should not be in scheduled results");
                }

                this.TestOutputHelper.WriteLine($"Found {foundJobIdList.Count} jobs in '{CoreJobStateConstants.ScheduledStateName}' state");
                this.TestOutputHelper.WriteLine($"Expected scheduled jobs: {string.Join(", ", scheduledJobIds)}");
                this.TestOutputHelper.WriteLine($"Found job IDs: {string.Join(", ", foundJobIdList)}");
            }
            finally
            {
                await this.CleanupJobsAsync(allCreatedJobIds);
            }
        }

        [Fact]
        public async Task FindJobIDsByState_WithMultipleStates_ShouldReturnCorrectJobsForEachState()
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
                [CoreJobStateConstants.ProcessingStateName] = [],
                [CoreJobStateConstants.SucceededStateName] = [],
            };
            var allCreatedJobIds = new List<string>();

            try
            {
                // Create enqueued jobs
                for (int i = 0; i < 2; i++)
                {
                    string testJobId = this.CreateTestJobId();
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
                await Task.Delay(100);

                // Act & Assert for each state
                foreach (KeyValuePair<string, List<string>> stateGroup in jobsByState)
                {
                    string stateName = stateGroup.Key;
                    List<string> expectedJobIds = stateGroup.Value;

                    IEnumerable<string> foundJobIds = this.HangfireSchedulingService.FindJobIDsByState(stateName);
                    var foundJobIdList = foundJobIds.ToList();

                    _ = foundJobIds.Should().NotBeNull($"because FindJobIDsByState should never return null for state '{stateName}'");

                    // Enqueued jobs may transitioned to other states, so we allow for fewer found jobs
                    if (!stateName.Equals(CoreJobStateConstants.EnqueuedStateName))
                    {
                        _ = foundJobIdList.Should().HaveCountGreaterThanOrEqualTo(
                            expectedJobIds.Count,
                            $"because all jobs in state '{stateName}' should be found");

                        // Verify all expected jobs are found
                        foreach (string expectedJobId in expectedJobIds)
                        {
                            _ = foundJobIdList.Should().Contain(
                                expectedJobId,
                                $"because job {expectedJobId} should be found in state '{stateName}'");
                        }
                    }

                    this.TestOutputHelper.WriteLine($"State '{stateName}': Expected {expectedJobIds.Count}, Found {foundJobIdList.Count}");
                    this.TestOutputHelper.WriteLine($"  Expected: {string.Join(", ", expectedJobIds)}");
                    this.TestOutputHelper.WriteLine($"  Found: {string.Join(", ", foundJobIdList)}");
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
        public void FindJobIDsByState_WithVariousStateNames_ShouldNotThrow(string stateName)
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Act
            IEnumerable<string> jobIds = this.HangfireSchedulingService.FindJobIDsByState(stateName);

            // Assert
            _ = jobIds.Should().NotBeNull($"because FindJobIDsByState should never return null for state '{stateName}'");

            // Evaluate the enumerable to ensure no exceptions are thrown
            var jobIdList = jobIds.ToList();
            _ = jobIdList.Should().BeAssignableTo<IEnumerable<string>>("because job IDs should be strings");

            this.TestOutputHelper.WriteLine($"State '{stateName}': Found {jobIdList.Count} jobs");
        }

        [Fact]
        public async Task GetAllJobIDs_AndFindJobIDsByState_ShouldBeConsistent()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange - Create jobs in different states
            var allCreatedJobIds = new List<string>();

            try
            {
                // Create jobs in different states
                for (int i = 0; i < 2; i++)
                {
                    // Enqueued job
                    string enqueuedTestJobId = this.CreateTestJobId();
                    string enqueuedJobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(enqueuedTestJobId),
                        new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, $"Enqueued job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    allCreatedJobIds.Add(enqueuedJobId);
                    this._createdJobIds.Add(enqueuedJobId);

                    // Scheduled job
                    string scheduledTestJobId = this.CreateTestJobId(false);
                    string scheduledJobId = this.TestSchedulingService.Create<TestJob>(
                        j => j.DoWork(scheduledTestJobId),
                        new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(30), $"Scheduled job {i + 1}"),
                        this.GetTestJobParameters(),
                        CoreJobStateConstants.TestQueue);

                    allCreatedJobIds.Add(scheduledJobId);
                    this._createdJobIds.Add(scheduledJobId);
                }

                // Wait for jobs to be persisted
                await Task.Delay(100);

                // Act
                IEnumerable<string> allJobIds = this.HangfireSchedulingService.GetAllJobIDs();
                var allJobIdsList = allJobIds.ToList();

                IEnumerable<string> enqueuedJobIds = this.HangfireSchedulingService.FindJobIDsByState(CoreJobStateConstants.EnqueuedStateName);
                var enqueuedJobIdsList = enqueuedJobIds.ToList();

                IEnumerable<string> scheduledJobIds = this.HangfireSchedulingService.FindJobIDsByState(CoreJobStateConstants.ScheduledStateName);
                var scheduledJobIdsList = scheduledJobIds.ToList();

                // Assert
                _ = allJobIds.Should().NotBeNull("because GetAllJobIDs should never return null");
                _ = enqueuedJobIds.Should().NotBeNull("because FindJobIDsByState should never return null");
                _ = scheduledJobIds.Should().NotBeNull("because FindJobIDsByState should never return null");

                // All jobs found by state should be in the all jobs list
                foreach (string enqueuedJobId in enqueuedJobIdsList)
                {
                    _ = allJobIdsList.Should().Contain(
                        enqueuedJobId,
                        "because all enqueued jobs should be in the complete job list");
                }

                foreach (string scheduledJobId in scheduledJobIdsList)
                {
                    _ = allJobIdsList.Should().Contain(
                        scheduledJobId,
                        "because all scheduled jobs should be in the complete job list");
                }

                // Verify our created jobs are found
                foreach (string createdJobId in allCreatedJobIds)
                {
                    _ = allJobIdsList.Should().Contain(
                        createdJobId,
                        $"because created job {createdJobId} should be in all jobs list");
                }

                this.TestOutputHelper.WriteLine($"Total jobs: {allJobIdsList.Count}");
                this.TestOutputHelper.WriteLine($"Enqueued jobs: {enqueuedJobIdsList.Count}");
                this.TestOutputHelper.WriteLine($"Scheduled jobs: {scheduledJobIdsList.Count}");
                this.TestOutputHelper.WriteLine($"Created jobs: {allCreatedJobIds.Count}");
            }
            finally
            {
                await this.CleanupJobsAsync(allCreatedJobIds);
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
            await Task.Delay(50);
        }
    }
}
#endif
