// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 01-05-2025
//
// Last Modified By : SteveBu
// Last Modified On : 01-05-2025
// // ***********************************************************************
// <copyright file="CoreJobStateConverterUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Unit tests for CoreSchedulingJobStateConverter</summary>

#if NV_USE_HANGFIRE
using System.Globalization;
using FluentAssertions;
using Hangfire;
using Hangfire.States;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Scheduling.Services;
using NetworkVisor.Core.Scheduling.Services.Converters.Hangfire;
using NetworkVisor.Core.Scheduling.Services.JobStates;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Scheduling.Services.Converters.Hangfire
{
    /// <summary>
    /// Unit tests for CoreSchedulingJobStateConverter.
    /// </summary>
    [PlatformTrait(typeof(CoreSchedulingJobStateConverterUnitTests))]
    public class CoreSchedulingJobStateConverterUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSchedulingJobStateConverterUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSchedulingJobStateConverterUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreJobStateConverter_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        [Fact]
        public void ToHangfireState_WithNullCoreJobState_ShouldThrowArgumentNullException()
        {
            // Arrange
            ICoreJobState? coreJobState = null;

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => CoreSchedulingJobStateConverter.ToHangfireState(coreJobState!));
            _ = exception.ParamName.Should().Be("coreJobState");
        }

        [Fact]
        public void ToHangfireState_WithCoreEnqueuedJobState_ShouldReturnEnqueuedState()
        {
            // Arrange
            var coreState = new CoreEnqueuedJobState("critical", "Test enqueue reason");

            // Act
            IState result = CoreSchedulingJobStateConverter.ToHangfireState(coreState);

            // Assert
            _ = result.Should().BeOfType<EnqueuedState>();
            var enqueuedState = (EnqueuedState)result;
            _ = enqueuedState.Queue.Should().Be("critical");
            _ = enqueuedState.Reason.Should().Be("Test enqueue reason");
        }

        [Fact]
        public void ToHangfireState_WithCoreScheduledJobState_ShouldReturnScheduledState()
        {
            // Arrange
            DateTime enqueueAt = DateTime.UtcNow.AddMinutes(30);
            var coreState = new CoreScheduledJobState(enqueueAt, "Test schedule reason");

            // Act
            IState result = CoreSchedulingJobStateConverter.ToHangfireState(coreState);

            // Assert
            _ = result.Should().BeOfType<ScheduledState>();
            var scheduledState = (ScheduledState)result;
            _ = scheduledState.EnqueueAt.Should().Be(enqueueAt);
            _ = scheduledState.Reason.Should().Be("Test schedule reason");
        }

        [Fact]
        public void ToHangfireState_WithCoreProcessingJobState_ShouldReturnHangfireProcessingState()
        {
            // Arrange
            var coreState = new CoreProcessingJobState("server-1", "worker-2", "Test processing reason");

            // Act
            IState result = CoreSchedulingJobStateConverter.ToHangfireState(coreState);

            // Assert
            _ = result.Should().BeOfType<HangfireProcessingState>();
            var processingState = (HangfireProcessingState)result;
            _ = processingState.ServerId.Should().Be("server-1");
            _ = processingState.WorkerId.Should().Be("worker-2");
        }

        [Fact]
        public void ToHangfireState_WithCoreSucceededJobState_ShouldReturnSucceededState()
        {
            // Arrange
            string testResult = "Test result";
            var coreState = new CoreSucceededJobState(testResult, 1000, 500, "Test success reason");

            // Act
            IState result = CoreSchedulingJobStateConverter.ToHangfireState(coreState);

            // Assert
            _ = result.Should().BeOfType<SucceededState>();
            var succeededState = (SucceededState)result;
            _ = succeededState.Result.Should().Be(testResult);
            _ = succeededState.Latency.Should().Be(1000);
            _ = succeededState.PerformanceDuration.Should().Be(500);
            _ = succeededState.Reason.Should().Be("Test success reason");
        }

        [Fact]
        public void ToHangfireState_WithCoreFailedJobState_ShouldReturnFailedState()
        {
            // Arrange
            var testException = new InvalidOperationException("Test exception");
            var coreState = new CoreFailedJobState(testException, "server-1", "Test failure reason");

            // Act
            IState result = CoreSchedulingJobStateConverter.ToHangfireState(coreState);

            // Assert
            _ = result.Should().BeOfType<FailedState>();
            var failedState = (FailedState)result;
            _ = failedState.Exception.Should().BeSameAs(testException);
            _ = failedState.ServerId.Should().Be("server-1");
            _ = failedState.Reason.Should().Be("Test failure reason");
        }

        [Fact]
        public void ToHangfireState_WithCoreDeletedJobState_ShouldReturnDeletedState()
        {
            // Arrange
            var testException = new InvalidOperationException("Test deletion exception");
            var coreState = new CoreDeletedJobState(testException, "Test deletion reason");

            // Act
            IState result = CoreSchedulingJobStateConverter.ToHangfireState(coreState);

            // Assert
            _ = result.Should().BeOfType<DeletedState>();
            var deletedState = (DeletedState)result;
            _ = deletedState.Reason.Should().Be("Test deletion reason");
            _ = deletedState.ExceptionInfo.Should().NotBeNull();
        }

        [Fact]
        public void ToHangfireState_WithCoreDeletedJobStateNoException_ShouldReturnDeletedState()
        {
            // Arrange
            var coreState = new CoreDeletedJobState(null, "Test deletion reason");

            // Act
            IState result = CoreSchedulingJobStateConverter.ToHangfireState(coreState);

            // Assert
            _ = result.Should().BeOfType<DeletedState>();
            var deletedState = (DeletedState)result;
            _ = deletedState.Reason.Should().Be("Test deletion reason");
        }

        [Fact]
        public void ToHangfireState_WithCoreAwaitingJobState_ShouldReturnAwaitingState()
        {
            // Arrange
            var nextState = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Next state");
            var expiration = TimeSpan.FromHours(1);
            var coreState = new CoreAwaitingJobState("parent-job-id", nextState, CoreJobContinuationOptions.OnlyOnSucceededState, expiration, "Test awaiting reason");

            // Act
            IState result = CoreSchedulingJobStateConverter.ToHangfireState(coreState);

            // Assert
            _ = result.Should().BeOfType<AwaitingState>();
            var awaitingState = (AwaitingState)result;
            _ = awaitingState.ParentId.Should().Be("parent-job-id");
            _ = awaitingState.Reason.Should().Be("Test awaiting reason");
        }

        [Fact]
        public void ToHangfireState_WithUnsupportedJobState_ShouldThrowNotSupportedException()
        {
            // Arrange
            var unsupportedState = new UnsupportedJobState();

            // Act & Assert
            NotSupportedException exception = Assert.Throws<NotSupportedException>(() => CoreSchedulingJobStateConverter.ToHangfireState(unsupportedState));
            _ = exception.Message.Should().Contain("UnsupportedJobState");
            _ = exception.Message.Should().Contain("not supported");
        }

        [Fact]
        public void FromHangfireState_WithNullHangfireState_ShouldThrowArgumentNullException()
        {
            // Arrange
            IState? hangfireState = null;

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => CoreSchedulingJobStateConverter.FromHangfireState(hangfireState!));
            _ = exception.ParamName.Should().Be("hangfireState");
        }

        [Fact]
        public void FromHangfireState_WithEnqueuedState_ShouldReturnCoreEnqueuedJobState()
        {
            // Arrange
            var hangfireState = new EnqueuedState("critical") { Reason = "Test enqueue reason" };

            // Act
            ICoreJobState result = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = result.Should().BeOfType<CoreEnqueuedJobState>();
            var coreState = (CoreEnqueuedJobState)result;
            _ = coreState.Queue.Should().Be("critical");
            _ = coreState.Reason.Should().Be("Test enqueue reason");
        }

        [Fact]
        public void FromHangfireState_WithScheduledState_ShouldReturnCoreScheduledJobState()
        {
            // Arrange
            DateTime enqueueAt = DateTime.UtcNow.AddMinutes(30);
            var hangfireState = new ScheduledState(enqueueAt) { Reason = "Test schedule reason" };

            // Act
            ICoreJobState result = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = result.Should().BeOfType<CoreScheduledJobState>();
            var coreState = (CoreScheduledJobState)result;
            _ = coreState.EnqueueAt.Should().Be(enqueueAt);
            _ = coreState.Reason.Should().Be("Test schedule reason");
        }

        [Fact]
        public void FromHangfireState_WithProcessingState_ShouldReturnCoreProcessingJobState()
        {
            // Arrange
            var hangfireState = new HangfireProcessingState("server-1", "worker-2") { Reason = "Test processing reason" };

            // Act
            ICoreJobState result = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = result.Should().BeOfType<CoreProcessingJobState>();
            var coreState = (CoreProcessingJobState)result;
            _ = coreState.ServerId.Should().Be("server-1");
            _ = coreState.WorkerId.Should().Be("worker-2");
            _ = coreState.Reason.Should().Be("Test processing reason");
        }

        [Fact]
        public void FromHangfireState_WithSucceededState_ShouldReturnCoreSucceededJobState()
        {
            // Arrange
            string testResult = "Test result";
            var hangfireState = new SucceededState(testResult, 1000, 500) { Reason = "Test success reason" };

            // Act
            ICoreJobState result = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = result.Should().BeOfType<CoreSucceededJobState>();
            var coreState = (CoreSucceededJobState)result;
            _ = coreState.Result.Should().Be(testResult);
            _ = coreState.Latency.Should().Be(1000);
            _ = coreState.PerformanceDuration.Should().Be(500);
            _ = coreState.Reason.Should().Be("Test success reason");
        }

        [Fact]
        public void FromHangfireState_WithFailedState_ShouldReturnCoreFailedJobState()
        {
            // Arrange
            var testException = new InvalidOperationException("Test exception");
            var hangfireState = new FailedState(testException, "server-1") { Reason = "Test failure reason" };

            // Act
            ICoreJobState result = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = result.Should().BeOfType<CoreFailedJobState>();
            var coreState = (CoreFailedJobState)result;
            _ = coreState.Exception.Should().BeSameAs(testException);
            _ = coreState.ServerId.Should().Be("server-1");
            _ = coreState.Reason.Should().Be("Test failure reason");
        }

        [Fact]
        public void FromHangfireState_WithDeletedState_ShouldReturnCoreDeletedJobState()
        {
            // Arrange
            var testException = new InvalidOperationException("Test deletion exception");
            var hangfireState = new DeletedState(new ExceptionInfo(testException)) { Reason = "Test deletion reason" };

            // Act
            ICoreJobState result = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = result.Should().BeOfType<CoreDeletedJobState>();
            var coreState = (CoreDeletedJobState)result;
            _ = coreState.Exception.Should().NotBeNull();
            _ = coreState.Exception!.Message.Should().Be("Test deletion exception");
            _ = coreState.Reason.Should().Be("Test deletion reason");
        }

        [Fact]
        public void FromHangfireState_WithAwaitingState_ShouldReturnCoreAwaitingJobState()
        {
            // Arrange
            var nextState = new EnqueuedState(CoreJobStateConstants.TestQueue);
            var hangfireState = new AwaitingState("parent-job-id", nextState, JobContinuationOptions.OnlyOnSucceededState, TimeSpan.FromHours(1))
            {
                Reason = "Test awaiting reason",
            };

            // Act
            ICoreJobState result = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = result.Should().BeOfType<CoreAwaitingJobState>();
            var coreState = (CoreAwaitingJobState)result;
            _ = coreState.ParentId.Should().Be("parent-job-id");
            _ = coreState.Reason.Should().Be("Test awaiting reason");
            _ = coreState.NextState.Should().BeOfType<CoreEnqueuedJobState>();
        }

        [Fact]
        public void FromHangfireState_WithUnsupportedState_ShouldThrowNotSupportedException()
        {
            // Arrange
            var unsupportedState = new UnsupportedHangfireState();

            // Act & Assert
            NotSupportedException exception = Assert.Throws<NotSupportedException>(() => CoreSchedulingJobStateConverter.FromHangfireState(unsupportedState));
            _ = exception.Message.Should().Contain("UnsupportedHangfireState");
            _ = exception.Message.Should().Contain("not supported");
        }

        [Fact]
        public void RoundTrip_CoreEnqueuedJobState_ShouldPreserveAllProperties()
        {
            // Arrange
            var originalState = new CoreEnqueuedJobState("critical", "Original reason");

            // Act - Convert to Hangfire and back
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
            ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = roundTripState.Should().BeOfType<CoreEnqueuedJobState>();
            var resultState = (CoreEnqueuedJobState)roundTripState;
            _ = resultState.Queue.Should().Be(originalState.Queue);
            _ = resultState.Reason.Should().Be(originalState.Reason);
            _ = resultState.Name.Should().Be(originalState.Name);
        }

        [Fact]
        public void RoundTrip_CoreScheduledJobState_ShouldPreserveAllProperties()
        {
            // Arrange
            DateTime enqueueAt = DateTime.UtcNow.AddMinutes(30).TruncateToSeconds(); // Truncate to avoid precision loss
            var originalState = new CoreScheduledJobState(enqueueAt, "Original schedule reason");

            // Act - Convert to Hangfire and back
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
            ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = roundTripState.Should().BeOfType<CoreScheduledJobState>();
            var resultState = (CoreScheduledJobState)roundTripState;
            _ = resultState.EnqueueAt.Should().Be(originalState.EnqueueAt);
            _ = resultState.Reason.Should().Be(originalState.Reason);
            _ = resultState.Name.Should().Be(originalState.Name);
        }

        [Fact]
        public void RoundTrip_CoreProcessingJobState_ShouldPreserveAllProperties()
        {
            // Arrange
            var originalState = new CoreProcessingJobState("server-1", "worker-2", "Original processing reason");

            // Act - Convert to Hangfire and back
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
            ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = roundTripState.Should().BeOfType<CoreProcessingJobState>();
            var resultState = (CoreProcessingJobState)roundTripState;
            _ = resultState.ServerId.Should().Be(originalState.ServerId);
            _ = resultState.WorkerId.Should().Be(originalState.WorkerId);
            _ = resultState.Reason.Should().Be(originalState.Reason);
            _ = resultState.Name.Should().Be(originalState.Name);
        }

        [Fact]
        public void RoundTrip_CoreSucceededJobState_ShouldPreserveAllProperties()
        {
            // Arrange
            string testResult = "Test result object";
            var originalState = new CoreSucceededJobState(testResult, 1500, 750, "Original success reason");

            // Act - Convert to Hangfire and back
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
            ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = roundTripState.Should().BeOfType<CoreSucceededJobState>();
            var resultState = (CoreSucceededJobState)roundTripState;
            _ = resultState.Result.Should().Be(originalState.Result);
            _ = resultState.Latency.Should().Be(originalState.Latency);
            _ = resultState.PerformanceDuration.Should().Be(originalState.PerformanceDuration);
            _ = resultState.Reason.Should().Be(originalState.Reason);
            _ = resultState.Name.Should().Be(originalState.Name);
        }

        [Fact]
        public void RoundTrip_CoreFailedJobState_ShouldPreserveAllProperties()
        {
            // Arrange
            var testException = new InvalidOperationException("Test exception message");
            var originalState = new CoreFailedJobState(testException, "server-1", "Original failure reason");

            // Act - Convert to Hangfire and back
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
            ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = roundTripState.Should().BeOfType<CoreFailedJobState>();
            var resultState = (CoreFailedJobState)roundTripState;
            _ = resultState.Exception.Should().BeSameAs(originalState.Exception);
            _ = resultState.ServerId.Should().Be(originalState.ServerId);
            _ = resultState.Reason.Should().Be(originalState.Reason);
            _ = resultState.Name.Should().Be(originalState.Name);
        }

        [Fact]
        public void RoundTrip_CoreDeletedJobState_WithException_ShouldPreserveProperties()
        {
            // Arrange
            var testException = new InvalidOperationException("Test deletion exception");
            var originalState = new CoreDeletedJobState(testException, "Original deletion reason");

            // Act - Convert to Hangfire and back
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
            ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = roundTripState.Should().BeOfType<CoreDeletedJobState>();
            var resultState = (CoreDeletedJobState)roundTripState;
            _ = resultState.Exception.Should().NotBeNull();
            _ = resultState.Exception!.Message.Should().Be(originalState.Exception!.Message);
            _ = resultState.Reason.Should().Be(originalState.Reason);
            _ = resultState.Name.Should().Be(originalState.Name);
        }

        [Fact]
        public void RoundTrip_CoreDeletedJobState_WithoutException_ShouldPreserveProperties()
        {
            // Arrange
            var originalState = new CoreDeletedJobState(null, "Original deletion reason");

            // Act - Convert to Hangfire and back
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
            ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = roundTripState.Should().BeOfType<CoreDeletedJobState>();
            var resultState = (CoreDeletedJobState)roundTripState;
            _ = resultState.Reason.Should().Be(originalState.Reason);
            _ = resultState.Name.Should().Be(originalState.Name);
        }

        [Fact]
        public void RoundTrip_CoreAwaitingJobState_ShouldPreserveAllProperties()
        {
            // Arrange
            var nextState = new CoreEnqueuedJobState("high-priority", "Next state");
            var expiration = TimeSpan.FromHours(2);
            var originalState = new CoreAwaitingJobState("parent-job-id", nextState, CoreJobContinuationOptions.OnAnyFinishedState, expiration, "Original awaiting reason");

            // Act - Convert to Hangfire and back
            IState hangfireState = CoreSchedulingJobStateConverter.ToHangfireState(originalState);
            ICoreJobState roundTripState = CoreSchedulingJobStateConverter.FromHangfireState(hangfireState);

            // Assert
            _ = roundTripState.Should().BeOfType<CoreAwaitingJobState>();
            var resultState = (CoreAwaitingJobState)roundTripState;
            _ = resultState.ParentId.Should().Be(originalState.ParentId);
            _ = resultState.Reason.Should().Be(originalState.Reason);
            _ = resultState.Options.Should().Be(originalState.Options);
            _ = resultState.Expiration.Should().Be(originalState.Expiration);
            _ = resultState.Name.Should().Be(originalState.Name);

            // Verify nested state round trip
            _ = resultState.NextState.Should().BeOfType<CoreEnqueuedJobState>();
            var nextStateResult = (CoreEnqueuedJobState)resultState.NextState;
            _ = nextStateResult.Queue.Should().Be(nextState.Queue);
            _ = nextStateResult.Reason.Should().Be(nextState.Reason);
        }

        [Fact]
        public void CanConvertToHangfire_WithNullState_ShouldReturnFalse()
        {
            // Arrange
            ICoreJobState? state = null;

            // Act
            bool result = CoreSchedulingJobStateConverter.CanConvertToHangfire(state!);

            // Assert
            _ = result.Should().BeFalse();
        }

        [Theory]
        [InlineData(typeof(CoreEnqueuedJobState))]
        [InlineData(typeof(CoreScheduledJobState))]
        [InlineData(typeof(CoreProcessingJobState))]
        [InlineData(typeof(CoreSucceededJobState))]
        [InlineData(typeof(CoreFailedJobState))]
        [InlineData(typeof(CoreDeletedJobState))]
        [InlineData(typeof(CoreAwaitingJobState))]
        public void CanConvertToHangfire_WithSupportedStates_ShouldReturnTrue(Type stateType)
        {
            // Arrange
            ICoreJobState state = CreateCoreJobStateInstance(stateType);

            // Act
            bool result = CoreSchedulingJobStateConverter.CanConvertToHangfire(state);

            // Assert
            _ = result.Should().BeTrue();
        }

        [Fact]
        public void CanConvertToHangfire_WithUnsupportedState_ShouldReturnFalse()
        {
            // Arrange
            var unsupportedState = new UnsupportedJobState();

            // Act
            bool result = CoreSchedulingJobStateConverter.CanConvertToHangfire(unsupportedState);

            // Assert
            _ = result.Should().BeFalse();
        }

        [Fact]
        public void CanConvertFromHangfire_WithNullState_ShouldReturnFalse()
        {
            // Arrange
            IState? state = null;

            // Act
            bool result = CoreSchedulingJobStateConverter.CanConvertFromHangfire(state!);

            // Assert
            _ = result.Should().BeFalse();
        }

        [Theory]
        [InlineData(typeof(EnqueuedState))]
        [InlineData(typeof(ScheduledState))]
        [InlineData(typeof(ProcessingState))]
        [InlineData(typeof(SucceededState))]
        [InlineData(typeof(FailedState))]
        [InlineData(typeof(DeletedState))]
        [InlineData(typeof(AwaitingState))]
        public void CanConvertFromHangfire_WithSupportedStates_ShouldReturnTrue(Type stateType)
        {
            // Arrange
            IState state = CreateHangfireStateInstance(stateType);

            // Act
            bool result = CoreSchedulingJobStateConverter.CanConvertFromHangfire(state);

            // Assert
            _ = result.Should().BeTrue();
        }

        [Fact]
        public void CanConvertFromHangfire_WithUnsupportedState_ShouldReturnFalse()
        {
            // Arrange
            var unsupportedState = new UnsupportedHangfireState();

            // Act
            bool result = CoreSchedulingJobStateConverter.CanConvertFromHangfire(unsupportedState);

            // Assert
            _ = result.Should().BeFalse();
        }

        [Fact]
        public void ToHangfireState_WithCoreProcessingJobState_NullServerIdAndWorkerId_ShouldUseDefaults()
        {
            // Arrange
            var coreState = new CoreProcessingJobState(null, null, "Test reason");

            // Act
            IState result = CoreSchedulingJobStateConverter.ToHangfireState(coreState);

            // Assert
            _ = result.Should().BeOfType<HangfireProcessingState>();
            var processingState = (HangfireProcessingState)result;
            _ = processingState.ServerId.Should().Be(Environment.MachineName);
            _ = processingState.WorkerId.Should().Be("1");
        }

        [Fact]
        public void ToHangfireState_WithCoreFailedJobState_NullException_ShouldCreateDefaultException()
        {
            // Act
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new CoreFailedJobState(null!, "server-1", "Test failure"));

            // Assert
            exception.ParamName.Should().Be("exception");
        }

        [Fact]
        public void ToHangfireState_WithCoreFailedJobState_NullExceptionAndReason_ShouldCreateDefaultException()
        {
            // Act
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new CoreFailedJobState(null!, "server-1", null));

            // Assert
            exception.ParamName.Should().Be("exception");
        }

        private static ICoreJobState CreateCoreJobStateInstance(Type stateType)
        {
            return stateType.Name switch
            {
                nameof(CoreEnqueuedJobState) => new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "Test reason"),
                nameof(CoreScheduledJobState) => new CoreScheduledJobState(DateTime.UtcNow.AddMinutes(5), "Test reason"),
                nameof(CoreProcessingJobState) => new CoreProcessingJobState("server", "worker", "Test reason"),
                nameof(CoreSucceededJobState) => new CoreSucceededJobState("result", 100, 50, "Test reason"),
                nameof(CoreFailedJobState) => new CoreFailedJobState(new Exception("test"), "server", "Test reason"),
                nameof(CoreDeletedJobState) => new CoreDeletedJobState(null, "Test reason"),
                nameof(CoreAwaitingJobState) => new CoreAwaitingJobState("parent", new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue), CoreJobContinuationOptions.OnlyOnSucceededState, null, "Test reason"),
                _ => throw new ArgumentException($"Unsupported state type: {stateType.Name}"),
            };
        }

        private static IState CreateHangfireStateInstance(Type stateType)
        {
            return stateType.Name switch
            {
                nameof(EnqueuedState) => new EnqueuedState("default") { Reason = "Test reason" },
                nameof(ScheduledState) => new ScheduledState(DateTime.UtcNow.AddMinutes(5)) { Reason = "Test reason" },
                nameof(ProcessingState) => new HangfireProcessingState("server", "worker") { Reason = "Test reason" },
                nameof(SucceededState) => new SucceededState("result", 100, 50) { Reason = "Test reason" },
                nameof(FailedState) => new FailedState(new Exception("test"), "server") { Reason = "Test reason" },
                nameof(DeletedState) => new DeletedState() { Reason = "Test reason" },
                nameof(AwaitingState) => new AwaitingState("parent", new EnqueuedState(CoreJobStateConstants.TestQueue), JobContinuationOptions.OnlyOnSucceededState, TimeSpan.FromHours(1)) { Reason = "Test reason" },
                _ => throw new ArgumentException($"Unsupported state type: {stateType.Name}"),
            };
        }

        /// <summary>
        /// Test class representing an unsupported job state for testing error handling.
        /// </summary>
        public class UnsupportedJobState : ICoreJobState
        {
            public string Name => "Unsupported";

            public string? Reason => "Test unsupported state";

            public bool IsFinal => true;

            public bool IgnoreJobLoadException => false;

            public IReadOnlyDictionary<string, string> Data => new Dictionary<string, string>();
        }

        /// <summary>
        /// Test class representing an unsupported Hangfire state for testing error handling.
        /// </summary>
        public class UnsupportedHangfireState : IState
        {
            public string Name => "UnsupportedHangfire";

            public string? Reason { get; set; } = "Test unsupported Hangfire state";

            public bool IsFinal => false;

            public bool IgnoreJobLoadException => false;

            public Dictionary<string, string> SerializeData() => [];
        }
    }
}
#endif
