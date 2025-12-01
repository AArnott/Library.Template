// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 01-05-2025
//
// Last Modified By : SteveBu
// Last Modified On : 01-05-2025
// // ***********************************************************************
// <copyright file="CoreJobStateConverterIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Integration tests for CoreSchedulingJobStateConverter</summary>

#if NV_USE_HANGFIRE
using FluentAssertions;
using Hangfire;
using Hangfire.States;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Scheduling.Services;
using NetworkVisor.Core.Scheduling.Services.Converters.Hangfire;
using NetworkVisor.Core.Scheduling.Services.JobStates;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Scheduling.Services.Converters.Hangfire
{
    /// <summary>
    /// Integration tests for CoreSchedulingJobStateConverter.
    /// </summary>
    [PlatformTrait(typeof(CoreSchedulingJobStateConverterIntegrationTests))]
    public class CoreSchedulingJobStateConverterIntegrationTests : CoreCommandTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSchedulingJobStateConverterIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSchedulingJobStateConverterIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        protected ICoreSchedulingBackgroundService TestSchedulingService =>
            this.TestNetworkServices.SchedulingBackgroundService;

        [Fact]
        public void ConverterIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public void ConverterIntegration_ServiceSetup()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Assert
            _ = this.TestMessagingDatabase.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreMessagingDatabase>();
            _ = this.TestSchedulingService.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreSchedulingBackgroundService>();
            _ = this.TestSchedulingService.IsRunning.Should().BeTrue();

            _ = this.TestSchedulingService.DatabaseFilePath.Should().NotBeNullOrEmpty();
            _ = this.TestFileSystem.FileExists(this.TestSchedulingService.DatabaseFilePath).Should().BeTrue();
        }

        [Fact]
        public void Integration_RealJobStateConversion_EnqueuedState_ShouldRoundTripCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            var originalCoreState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Integration test enqueued job");

            // Act - Convert to Hangfire state
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalCoreState);

            // Verify Hangfire state properties
            _ = hangfireState.Should().BeOfType<EnqueuedState>();
            var enqueuedState = (EnqueuedState)hangfireState;
            _ = enqueuedState.Queue.Should().Be(CoreJobStateConstants.TestQueue);
            _ = enqueuedState.Reason.Should().Be("Integration test enqueued job");

            // Act - Convert back to Core state
            ICoreJobState roundTripCoreState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert - Verify round trip integrity
            _ = roundTripCoreState.Should().BeOfType<CoreEnqueuedJobState>();
            var resultState = (CoreEnqueuedJobState)roundTripCoreState;
            _ = resultState.Queue.Should().Be(originalCoreState.Queue);
            _ = resultState.Reason.Should().Be(originalCoreState.Reason);
            _ = resultState.Name.Should().Be(originalCoreState.Name);

            this.TestOutputHelper.WriteLine($"Successfully round-tripped CoreEnqueuedJobState: Queue={resultState.Queue}, Reason={resultState.Reason}");
        }

        [Fact]
        public void Integration_RealJobStateConversion_ScheduledState_ShouldRoundTripCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            DateTime scheduleTime = DateTime.UtcNow.AddMinutes(15).TruncateToSeconds();
            var originalCoreState = new CoreScheduledJobState(scheduleTime, "Integration test scheduled job");

            // Act - Convert to Hangfire state
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalCoreState);

            // Verify Hangfire state properties
            _ = hangfireState.Should().BeOfType<ScheduledState>();
            var scheduledState = (ScheduledState)hangfireState;
            _ = scheduledState.EnqueueAt.Should().Be(scheduleTime);
            _ = scheduledState.Reason.Should().Be("Integration test scheduled job");

            // Act - Convert back to Core state
            ICoreJobState roundTripCoreState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert - Verify round trip integrity
            _ = roundTripCoreState.Should().BeOfType<CoreScheduledJobState>();
            var resultState = (CoreScheduledJobState)roundTripCoreState;
            _ = resultState.EnqueueAt.Should().Be(originalCoreState.EnqueueAt);
            _ = resultState.Reason.Should().Be(originalCoreState.Reason);
            _ = resultState.Name.Should().Be(originalCoreState.Name);

            this.TestOutputHelper.WriteLine($"Successfully round-tripped CoreScheduledJobState: EnqueueAt={resultState.EnqueueAt}, Reason={resultState.Reason}");
        }

        [Fact]
        public void Integration_RealJobStateConversion_ProcessingState_ShouldRoundTripCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string serverId = Environment.MachineName + "-integration";
            string workerId = "worker-integration-1";
            var originalCoreState = new CoreProcessingJobState(serverId, workerId, "Integration test processing job");

            // Act - Convert to Hangfire state
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalCoreState);

            // Verify Hangfire state properties
            _ = hangfireState.Should().BeOfType<HangfireProcessingState>();
            var processingState = (HangfireProcessingState)hangfireState;
            _ = processingState.ServerId.Should().Be(serverId);
            _ = processingState.WorkerId.Should().Be(workerId);

            // Act - Convert back to Core state
            ICoreJobState roundTripCoreState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert - Verify round trip integrity
            _ = roundTripCoreState.Should().BeOfType<CoreProcessingJobState>();
            var resultState = (CoreProcessingJobState)roundTripCoreState;
            _ = resultState.ServerId.Should().Be(originalCoreState.ServerId);
            _ = resultState.WorkerId.Should().Be(originalCoreState.WorkerId);
            _ = resultState.Reason.Should().Be(originalCoreState.Reason);
            _ = resultState.Name.Should().Be(originalCoreState.Name);

            this.TestOutputHelper.WriteLine($"Successfully round-tripped CoreProcessingJobState: ServerId={resultState.ServerId}, WorkerId={resultState.WorkerId}");
        }

        [Fact]
        public void Integration_RealJobStateConversion_SucceededState_ShouldRoundTripCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            var result = new { Message = "Integration test completed", Count = 42 };
            long latency = 2500L;
            long performanceDuration = 1800L;
            var originalCoreState = new CoreSucceededJobState(result, latency, performanceDuration, "Integration test succeeded");

            // Act - Convert to Hangfire state
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalCoreState);

            // Verify Hangfire state properties
            _ = hangfireState.Should().BeOfType<SucceededState>();
            var succeededState = (SucceededState)hangfireState;
            _ = succeededState.Result.Should().Be(result);
            _ = succeededState.Latency.Should().Be(latency);
            _ = succeededState.PerformanceDuration.Should().Be(performanceDuration);
            _ = succeededState.Reason.Should().Be("Integration test succeeded");

            // Act - Convert back to Core state
            ICoreJobState roundTripCoreState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert - Verify round trip integrity
            _ = roundTripCoreState.Should().BeOfType<CoreSucceededJobState>();
            var resultState = (CoreSucceededJobState)roundTripCoreState;
            _ = resultState.Result.Should().Be(originalCoreState.Result);
            _ = resultState.Latency.Should().Be(originalCoreState.Latency);
            _ = resultState.PerformanceDuration.Should().Be(originalCoreState.PerformanceDuration);
            _ = resultState.Reason.Should().Be(originalCoreState.Reason);
            _ = resultState.Name.Should().Be(originalCoreState.Name);

            this.TestOutputHelper.WriteLine($"Successfully round-tripped CoreSucceededJobState: Result={resultState.Result}, Latency={resultState.Latency}ms");
        }

        [Fact]
        public void Integration_RealJobStateConversion_FailedState_ShouldRoundTripCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            var exception = new InvalidOperationException("Integration test failure exception");
            string serverId = Environment.MachineName + "-integration";
            var originalCoreState = new CoreFailedJobState(exception, serverId, "Integration test failed");

            // Act - Convert to Hangfire state
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalCoreState);

            // Verify Hangfire state properties
            _ = hangfireState.Should().BeOfType<FailedState>();
            var failedState = (FailedState)hangfireState;
            _ = failedState.Exception.Should().BeSameAs(exception);
            _ = failedState.ServerId.Should().Be(serverId);
            _ = failedState.Reason.Should().Be("Integration test failed");

            // Act - Convert back to Core state
            ICoreJobState roundTripCoreState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert - Verify round trip integrity
            _ = roundTripCoreState.Should().BeOfType<CoreFailedJobState>();
            var resultState = (CoreFailedJobState)roundTripCoreState;
            _ = resultState.Exception.Should().BeSameAs(originalCoreState.Exception);
            _ = resultState.ServerId.Should().Be(originalCoreState.ServerId);
            _ = resultState.Reason.Should().Be(originalCoreState.Reason);
            _ = resultState.Name.Should().Be(originalCoreState.Name);

            this.TestOutputHelper.WriteLine($"Successfully round-tripped CoreFailedJobState: Exception={resultState.Exception?.Message}, ServerId={resultState.ServerId}");
        }

        [Fact]
        public void Integration_RealJobStateConversion_DeletedState_ShouldRoundTripCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            var exception = new OperationCanceledException("Integration test job was cancelled");
            var originalCoreState = new CoreDeletedJobState(exception, "Integration test deleted");

            // Act - Convert to Hangfire state
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalCoreState);

            // Verify Hangfire state properties
            _ = hangfireState.Should().BeOfType<DeletedState>();
            var deletedState = (DeletedState)hangfireState;
            _ = deletedState.Reason.Should().Be("Integration test deleted");
            _ = deletedState.ExceptionInfo.Should().NotBeNull();

            // Act - Convert back to Core state
            ICoreJobState roundTripCoreState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert - Verify round trip integrity
            _ = roundTripCoreState.Should().BeOfType<CoreDeletedJobState>();
            var resultState = (CoreDeletedJobState)roundTripCoreState;
            _ = resultState.Exception.Should().NotBeNull();
            _ = resultState.Exception!.Message.Should().Be(originalCoreState.Exception!.Message);
            _ = resultState.Reason.Should().Be(originalCoreState.Reason);
            _ = resultState.Name.Should().Be(originalCoreState.Name);

            this.TestOutputHelper.WriteLine($"Successfully round-tripped CoreDeletedJobState: Exception={resultState.Exception?.Message}, Reason={resultState.Reason}");
        }

        [Fact]
        public void Integration_RealJobStateConversion_AwaitingState_ShouldRoundTripCorrectly()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Arrange
            string parentJobId = "integration-parent-job-123";
            var nextState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Continuation after parent");
            CoreJobContinuationOptions options = CoreJobContinuationOptions.OnAnyFinishedState;
            var expiration = TimeSpan.FromHours(4);
            var originalCoreState = new CoreAwaitingJobState(parentJobId, nextState, options, expiration, "Integration test awaiting");

            // Act - Convert to Hangfire state
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalCoreState);

            // Verify Hangfire state properties
            _ = hangfireState.Should().BeOfType<AwaitingState>();
            var awaitingState = (AwaitingState)hangfireState;
            _ = awaitingState.ParentId.Should().Be(parentJobId);
            _ = awaitingState.Reason.Should().Be("Integration test awaiting");

            // Act - Convert back to Core state
            ICoreJobState roundTripCoreState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert - Verify round trip integrity
            _ = roundTripCoreState.Should().BeOfType<CoreAwaitingJobState>();
            var resultState = (CoreAwaitingJobState)roundTripCoreState;
            _ = resultState.ParentId.Should().Be(originalCoreState.ParentId);
            _ = resultState.Reason.Should().Be(originalCoreState.Reason);
            _ = resultState.Options.Should().Be(originalCoreState.Options);
            _ = resultState.Expiration.Should().Be(originalCoreState.Expiration);
            _ = resultState.Name.Should().Be(originalCoreState.Name);

            // Verify nested state round trip
            _ = resultState.NextState.Should().BeOfType<CoreEnqueuedJobState>();
            var nextStateResult = (CoreEnqueuedJobState)resultState.NextState;
            _ = nextStateResult.Queue.Should().Be(nextState.Queue);
            _ = nextStateResult.Reason.Should().Be(nextState.Reason);

            this.TestOutputHelper.WriteLine($"Successfully round-tripped CoreAwaitingJobState: ParentId={resultState.ParentId}, Options={resultState.Options}");
        }

        [Fact]
        public async Task Integration_JobWorkflow_StateTransitions_ShouldMaintainDataIntegrity()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Test a complete job workflow with multiple state transitions
            var jobStates = new List<ICoreJobState>();
            var hangfireStates = new List<IState>();
            var roundTripStates = new List<ICoreJobState>();

            try
            {
                // 1. Create job in enqueued state
                var enqueuedState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Job enqueued for workflow test");
                jobStates.Add(enqueuedState);

                IState hangfireEnqueued = CoreSchedulingJobStateConverter.ToHangfireState(enqueuedState);
                hangfireStates.Add(hangfireEnqueued);

                ICoreJobState roundTripEnqueued = CoreSchedulingJobStateConverter.FromHangfireState(hangfireEnqueued);
                roundTripStates.Add(roundTripEnqueued);

                // 2. Transition to processing state
                var processingState = new CoreProcessingJobState("workflow-server", "workflow-worker-1", "Job picked up for processing");
                jobStates.Add(processingState);

                IState hangfireProcessing = CoreSchedulingJobStateConverter.ToHangfireState(processingState);
                hangfireStates.Add(hangfireProcessing);

                ICoreJobState roundTripProcessing = CoreSchedulingJobStateConverter.FromHangfireState(hangfireProcessing);
                roundTripStates.Add(roundTripProcessing);

                // 3. Complete successfully
                var succeededState = new CoreSucceededJobState("Workflow completed successfully", 3000, 2500, "Job completed");
                jobStates.Add(succeededState);

                IState hangfireSucceeded = CoreSchedulingJobStateConverter.ToHangfireState(succeededState);
                hangfireStates.Add(hangfireSucceeded);

                ICoreJobState roundTripSucceeded = CoreSchedulingJobStateConverter.FromHangfireState(hangfireSucceeded);
                roundTripStates.Add(roundTripSucceeded);

                // Verify all conversions maintained data integrity
                for (int i = 0; i < jobStates.Count; i++)
                {
                    ICoreJobState original = jobStates[i];
                    ICoreJobState roundTrip = roundTripStates[i];

                    _ = roundTrip.Should().BeOfType(original.GetType());
                    _ = roundTrip.Name.Should().Be(original.Name);
                    _ = roundTrip.Reason.Should().Be(original.Reason);

                    this.TestOutputHelper.WriteLine($"Step {i + 1}: {original.GetType().Name} -> {hangfireStates[i].GetType().Name} -> {roundTrip.GetType().Name} ✓");
                }

                await Task.Delay(100); // Allow any background processing to complete

                this.TestOutputHelper.WriteLine("Workflow state transition test completed successfully");
            }
            catch (Exception ex)
            {
                this.TestOutputHelper.WriteLine($"Workflow test failed: {ex.Message}");
                throw;
            }
        }

        [Fact]
        public void Integration_LargeDataRoundTrip_ShouldMaintainDataIntegrity()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Test with larger data objects to ensure serialization integrity
            var largeResult = new
            {
                Id = Guid.NewGuid().ToString(),
                ProcessedItems = Enumerable.Range(1, 1000).ToArray(),
                Metadata = new Dictionary<string, object>
                {
                    ["ProcessingTime"] = TimeSpan.FromMinutes(5),
                    ["BatchSize"] = 1000,
                    ["Timestamp"] = DateTime.UtcNow,
                    ["Success"] = true,
                },
                Details = string.Join(", ", Enumerable.Range(1, 100).Select(i => $"Item-{i}")),
            };

            var originalState = new CoreSucceededJobState(largeResult, 5000, 4500, "Large data processing completed");

            // Act - Round trip conversion
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
            ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert - Verify data integrity
            _ = roundTripState.Should().BeOfType<CoreSucceededJobState>();
            var resultState = (CoreSucceededJobState)roundTripState;
            _ = resultState.Result.Should().Be(originalState.Result);
            _ = resultState.Latency.Should().Be(originalState.Latency);
            _ = resultState.PerformanceDuration.Should().Be(originalState.PerformanceDuration);
            _ = resultState.Reason.Should().Be(originalState.Reason);

            this.TestOutputHelper.WriteLine("Large data round trip test completed successfully");
        }

        [Fact]
        public void Integration_ExceptionDataPreservation_ShouldMaintainExceptionDetails()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            // Create a complex exception with nested exceptions and stack trace
            try
            {
                throw new InvalidOperationException(
                    "Outer exception",
                    new ArgumentException(
                        "Inner exception",
                        new FormatException("Deep inner exception")));
            }
            catch (Exception complexException)
            {
                var originalState = new CoreFailedJobState(complexException, "integration-server", "Complex exception test");

                // Act - Round trip conversion
                IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
                ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

                // Assert - Verify exception details are preserved
                _ = roundTripState.Should().BeOfType<CoreFailedJobState>();
                var resultState = (CoreFailedJobState)roundTripState;
                _ = resultState.Exception.Should().BeSameAs(originalState.Exception);
                _ = resultState.Exception.Message.Should().Be(complexException.Message);
                _ = resultState.Exception.InnerException.Should().NotBeNull();
                _ = resultState.Exception.InnerException!.Message.Should().Be("Inner exception");
                _ = resultState.ServerId.Should().Be(originalState.ServerId);
                _ = resultState.Reason.Should().Be(originalState.Reason);

                this.TestOutputHelper.WriteLine($"Exception preservation test completed: {resultState.Exception.Message}");
            }
        }

        [Fact]
        public void Integration_HighVolumeConversions_ShouldMaintainPerformance()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            const int conversionCount = 1000;
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            // Test high-volume conversions
            for (int i = 0; i < conversionCount; i++)
            {
                var coreState = new CoreEnqueuedJobState($"queue-{i % 10}", $"Job {i}");
                IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(coreState);
                ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

                // Quick validation
                _ = roundTripState.Should().BeOfType<CoreEnqueuedJobState>();
                _ = roundTripState.Name.Should().Be(coreState.Name);
            }

            stopwatch.Stop();
            double averageTime = stopwatch.ElapsedMilliseconds / (double)conversionCount;

            this.TestOutputHelper.WriteLine($"Processed {conversionCount} conversions in {stopwatch.ElapsedMilliseconds}ms (avg: {averageTime:F2}ms per conversion)");

            // Performance assertion - should be fast enough for production use
            _ = averageTime.Should().BeLessThan(1.0, "Conversions should be fast enough for production use");
        }

        [Fact]
        public async Task Integration_ConcurrentConversions_ShouldBeThreadSafe()
        {
            if (!this.IsHangfireSchedulerSupported())
            {
                return;
            }

            const int taskCount = 10;
            const int conversionsPerTask = 100;

            Task<int>[] tasks = Enumerable.Range(0, taskCount).Select(taskId => Task.Run(() =>
            {
                for (int i = 0; i < conversionsPerTask; i++)
                {
                    var coreState = new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(i), $"Task {taskId} Job {i}");
                    IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(coreState);
                    ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

                    // Validate round trip
                    _ = roundTripState.Should().BeOfType<CoreScheduledJobState>();
                    var resultState = (CoreScheduledJobState)roundTripState;
                    _ = resultState.Reason.Should().Be(coreState.Reason);
                }

                return taskId;
            })).ToArray();

            int[] results = await Task.WhenAll(tasks);

            // Assert all tasks completed successfully
            _ = results.Should().HaveCount(taskCount);
            _ = results.Should().BeEquivalentTo(Enumerable.Range(0, taskCount));

            this.TestOutputHelper.WriteLine($"Concurrent conversion test completed: {taskCount} tasks × {conversionsPerTask} conversions = {taskCount * conversionsPerTask} total conversions");
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
