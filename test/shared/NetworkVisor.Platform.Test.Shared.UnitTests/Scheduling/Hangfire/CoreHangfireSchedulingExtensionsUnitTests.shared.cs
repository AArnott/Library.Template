// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 01-05-2025
//
// Last Modified By : SteveBu
// Last Modified On : 01-05-2025
// // ***********************************************************************
// <copyright file="CoreHangfireSchedulingExtensionsUnitTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Unit tests for CoreSchedulingBackgroundServiceExtensions</summary>

using System.Linq.Expressions;
using System.Text;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.CoreApp.Settings;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Logging.Loggable;
using NetworkVisor.Core.Logging.LogProperty;
using NetworkVisor.Core.Networking.Connectivity;
using NetworkVisor.Core.Networking.Hosting;
using NetworkVisor.Core.Networking.Interfaces;
using NetworkVisor.Core.Scheduling.Services;
using NetworkVisor.Core.Scheduling.Services.JobStates;
using NetworkVisor.Core.Scheduling.Services.Monitoring;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Scheduling.Hangfire
{
    /// <summary>
    /// Unit tests for CoreSchedulingBackgroundServiceExtensions.
    /// </summary>
    [PlatformTrait(typeof(CoreSchedulingBackgroundServiceExtensionsUnitTests))]
    public class CoreSchedulingBackgroundServiceExtensionsUnitTests : CoreSchedulingTestCaseBase
    {
        private readonly TestSchedulingBackgroundService _testService;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSchedulingBackgroundServiceExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSchedulingBackgroundServiceExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            this._testService = new TestSchedulingBackgroundService();
        }

        [Fact]
        public void CoreSchedulingBackgroundServiceExtensions_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);

            _ = this._testService.DatabaseFilePath.Should().Be("test-database.db");
        }

        [Fact]
        public void TriggerRecurringJob_WithValidJobId_ShouldReturnJobId()
        {
            // Arrange
            const string recurringJobId = "test-recurring-job";

            // Act
            string result = this._testService.TriggerRecurringJob(recurringJobId);

            // Assert
            _ = result.Should().Be("triggered-job-id");
            _ = this._testService.TriggerRecurringJobCalls.Should().HaveCount(1);
            _ = this._testService.TriggerRecurringJobCalls[0].Should().Be(recurringJobId);
        }

        [Fact]
        public void TriggerRecurringJob_WithEmptyJobId_ShouldReturnJobId()
        {
            // Arrange
            const string recurringJobId = "";

            // Act
            string result = this._testService.TriggerRecurringJob(recurringJobId);

            // Assert
            _ = result.Should().Be("triggered-job-id");
            _ = this._testService.TriggerRecurringJobCalls.Should().HaveCount(1);
            _ = this._testService.TriggerRecurringJobCalls[0].Should().Be(recurringJobId);
        }

        [Fact]
        public void TriggerRecurringJob_WithNullJobId_ShouldReturnJobId()
        {
            // Arrange
            string? recurringJobId = null;

            // Act
            string result = this._testService.TriggerRecurringJob(recurringJobId!);

            // Assert
            _ = result.Should().Be("triggered-job-id");
            _ = this._testService.TriggerRecurringJobCalls.Should().HaveCount(1);
            _ = this._testService.TriggerRecurringJobCalls[0].Should().BeNull();
        }

        [Fact]
        public void TriggerRecurringJob_MultipleCalls_ShouldTrackAllCalls()
        {
            // Arrange
            string[] jobIds = new[] { "job-1", "job-2", "job-3" };

            // Act
            string[] results = jobIds.Select(this._testService.TriggerRecurringJob).ToArray();

            // Assert
            _ = results.Should().HaveCount(3);
            _ = results.Should().OnlyContain(r => r == "triggered-job-id");
            _ = this._testService.TriggerRecurringJobCalls.Should().HaveCount(3);
            _ = this._testService.TriggerRecurringJobCalls.Should().BeEquivalentTo(jobIds);
        }

        [Theory]
        [InlineData("daily-backup")]
        [InlineData("weekly-report")]
        [InlineData("monthly-cleanup")]
        [InlineData("hourly-sync")]
        public void TriggerRecurringJob_WithVariousJobIds_ShouldHandleCorrectly(string recurringJobId)
        {
            // Act
            string result = this._testService.TriggerRecurringJob(recurringJobId);

            // Assert
            _ = result.Should().Be("triggered-job-id");
            _ = this._testService.TriggerRecurringJobCalls.Should().HaveCount(1);
            _ = this._testService.TriggerRecurringJobCalls[0].Should().Be(recurringJobId);
        }

        [Fact]
        public void Enqueue_WithInstanceMethod_ShouldCallCreateWithEnqueuedState()
        {
            // Arrange
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("test");

            // Act
            string result = this._testService.Enqueue(methodCall, this.GetTestJobParameters(), CoreJobStateConstants.TestQueue);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().Be(CoreJobStateConstants.TestQueue);
            _ = call.State.Should().BeOfType<CoreEnqueuedJobState>();
        }

        [Fact]
        public void Enqueue_WithInstanceMethodAndQueue_ShouldCallCreateWithQueue()
        {
            // Arrange
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("test");
            const string queue = "test-queue";

            // Act
            string result = this._testService.Enqueue(methodCall, this.GetTestJobParameters(), queue);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().Be(queue);
            _ = call.State.Should().BeOfType<CoreEnqueuedJobState>();
        }

        [Fact]
        public void Enqueue_WithStaticMethod_ShouldCallCreateWithEnqueuedState()
        {
            // Arrange
            Expression<Action> methodCall = () => TestJobClass.StaticTestMethod("test");

            // Act
            string result = this._testService.Enqueue(methodCall);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeOfType<CoreEnqueuedJobState>();
        }

        [Fact]
        public void Enqueue_WithAsyncMethod_ShouldCallCreateWithEnqueuedState()
        {
            // Arrange
            Expression<Func<Task>> methodCall = () => TestJobClass.StaticAsyncTestMethod("test");

            // Act
            string result = this._testService.Enqueue(methodCall);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeOfType<CoreEnqueuedJobState>();
        }

        [Fact]
        public void Enqueue_WithAsyncInstanceMethod_ShouldCallCreateWithEnqueuedState()
        {
            // Arrange
            Expression<Func<TestJobClass, Task>> methodCall = x => x.TestAsyncMethod("test");

            // Act
            string result = this._testService.Enqueue(methodCall);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeOfType<CoreEnqueuedJobState>();
        }

        [Fact]
        public void Enqueue_WithNullService_ShouldThrowArgumentNullException()
        {
            // Arrange
            ICoreSchedulingBackgroundService? service = null;
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("test");

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => service!.Enqueue(methodCall));
            _ = exception.ParamName.Should().Be("service");
        }

        [Fact]
        public void Schedule_WithDelay_ShouldCallCreateWithScheduledState()
        {
            // Arrange
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("test");
            var delay = TimeSpan.FromMinutes(5);

            // Act
            string result = this._testService.Schedule(methodCall, delay);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeOfType<CoreScheduledJobState>();
        }

        [Fact]
        public void Schedule_WithEnqueueAt_ShouldCallCreateWithScheduledState()
        {
            // Arrange
            Expression<Action> methodCall = () => TestJobClass.StaticTestMethod("test");
            DateTimeOffset enqueueAt = DateTimeOffset.UtcNow.AddMinutes(10);

            // Act
            string result = this._testService.Schedule(methodCall, enqueueAt);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeOfType<CoreScheduledJobState>();
        }

        [Fact]
        public void Schedule_WithQueueAndDelay_ShouldCallCreateWithQueueAndScheduledState()
        {
            // Arrange
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("test");
            var delay = TimeSpan.FromMinutes(5);
            const string queue = "scheduled-queue";

            // Act
            string result = this._testService.Schedule(methodCall, delay, this.GetTestJobParameters(), queue);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().Be(queue);
            _ = call.State.Should().BeOfType<CoreScheduledJobState>();
        }

        [Fact]
        public void Schedule_WithAsyncMethod_ShouldCallCreateWithScheduledState()
        {
            // Arrange
            Expression<Func<Task>> methodCall = () => TestJobClass.StaticAsyncTestMethod("test");
            var delay = TimeSpan.FromMinutes(5);

            // Act
            string result = this._testService.Schedule(methodCall, delay);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeOfType<CoreScheduledJobState>();
        }

        [Fact]
        public void Schedule_WithAsyncInstanceMethod_ShouldCallCreateWithScheduledState()
        {
            // Arrange
            Expression<Func<TestJobClass, Task>> methodCall = x => x.TestAsyncMethod("test");
            DateTimeOffset enqueueAt = DateTimeOffset.UtcNow.AddMinutes(10);

            // Act
            string result = this._testService.Schedule(methodCall, enqueueAt);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeOfType<CoreScheduledJobState>();
        }

        [Fact]
        public void Schedule_WithNullService_ShouldThrowArgumentNullException()
        {
            // Arrange
            ICoreSchedulingBackgroundService? service = null;
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("test");
            var delay = TimeSpan.FromMinutes(5);

            // Act & Assert
            ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => service!.Schedule(methodCall, delay));
            _ = exception.ParamName.Should().Be("service");
        }

        [Fact]
        public void ChangeState_ShouldCallServiceChangeStateWithNullFromState()
        {
            // Arrange
            const string jobId = "test-job-id";
            var state = new CoreEnqueuedJobState(CoreJobStateConstants.TestQueue, "test reason");

            // Act
            bool result = this._testService.ChangeState(jobId, state);

            // Assert
            _ = result.Should().BeTrue();
            _ = this._testService.ChangeStateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.ChangeStateCall call = this._testService.ChangeStateCalls[0];
            _ = call.JobId.Should().Be(jobId);
            _ = call.State.Should().BeSameAs(state);
            _ = call.FromState.Should().BeNull();
        }

        [Fact]
        public void Delete_ShouldCallChangeStateWithDeletedState()
        {
            // Arrange
            const string jobId = "test-job-id";

            // Act
            bool result = this._testService.Delete(jobId);

            // Assert
            _ = result.Should().BeTrue();
            _ = this._testService.ChangeStateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.ChangeStateCall call = this._testService.ChangeStateCalls[0];
            _ = call.JobId.Should().Be(jobId);
            _ = call.State.Should().BeOfType<CoreDeletedJobState>();
            _ = call.FromState.Should().BeNull();
        }

        [Fact]
        public void Delete_WithFromState_ShouldCallChangeStateWithDeletedStateAndFromState()
        {
            // Arrange
            const string jobId = "test-job-id";
            const string fromState = "Processing";

            // Act
            bool result = this._testService.Delete(jobId, fromState);

            // Assert
            _ = result.Should().BeTrue();
            _ = this._testService.ChangeStateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.ChangeStateCall call = this._testService.ChangeStateCalls[0];
            _ = call.JobId.Should().Be(jobId);
            _ = call.State.Should().BeOfType<CoreDeletedJobState>();
            _ = call.FromState.Should().Be(fromState);
        }

        [Fact]
        public void Requeue_ShouldCallChangeStateWithEnqueuedState()
        {
            // Arrange
            const string jobId = "test-job-id";

            // Act
            bool result = this._testService.Requeue(jobId);

            // Assert
            _ = result.Should().BeTrue();
            _ = this._testService.ChangeStateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.ChangeStateCall call = this._testService.ChangeStateCalls[0];
            _ = call.JobId.Should().Be(jobId);
            _ = call.State.Should().BeOfType<CoreEnqueuedJobState>();
            _ = call.FromState.Should().BeNull();
        }

        [Fact]
        public void Reschedule_WithDelay_ShouldCallChangeStateWithScheduledState()
        {
            // Arrange
            const string jobId = "test-job-id";
            var delay = TimeSpan.FromMinutes(10);

            // Act
            bool result = this._testService.Reschedule(jobId, delay);

            // Assert
            _ = result.Should().BeTrue();
            _ = this._testService.ChangeStateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.ChangeStateCall call = this._testService.ChangeStateCalls[0];
            _ = call.JobId.Should().Be(jobId);
            _ = call.State.Should().BeOfType<CoreScheduledJobState>();
            _ = call.FromState.Should().BeNull();
        }

        [Fact]
        public void Reschedule_WithEnqueueAt_ShouldCallChangeStateWithScheduledState()
        {
            // Arrange
            const string jobId = "test-job-id";
            DateTimeOffset enqueueAt = DateTimeOffset.UtcNow.AddMinutes(15);

            // Act
            bool result = this._testService.Reschedule(jobId, enqueueAt);

            // Assert
            _ = result.Should().BeTrue();
            _ = this._testService.ChangeStateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.ChangeStateCall call = this._testService.ChangeStateCalls[0];
            _ = call.JobId.Should().Be(jobId);
            _ = call.State.Should().BeOfType<CoreScheduledJobState>();
            _ = call.FromState.Should().BeNull();
        }

        [Fact]
        public void ContinueJobWith_SimpleInstanceMethod_ShouldCallCreateWithAwaitingState()
        {
            // Arrange
            const string parentId = "parent-job-id";
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("continuation");

            // Act
            string result = this._testService.ContinueJobWith(parentId, methodCall);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeOfType<CoreAwaitingJobState>();

            var awaitingState = (CoreAwaitingJobState)call.State;
            _ = awaitingState.Data["ParentId"].Should().Be(parentId);
            _ = awaitingState.Data["Options"].Should().Be(CoreJobContinuationOptions.OnlyOnSucceededState.ConvertToValueString());
        }

        [Fact]
        public void ContinueJobWith_WithNextStateAndOptions_ShouldCallCreateWithSpecifiedState()
        {
            // Arrange
            const string parentId = "parent-job-id";
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("continuation");
            var nextState = new CoreScheduledJobState(TimeSpan.FromMinutes(5));
            CoreJobContinuationOptions options = CoreJobContinuationOptions.OnAnyFinishedState;

            // Act
            string result = this._testService.ContinueJobWith(parentId, methodCall, nextState, options);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.State.Should().BeOfType<CoreAwaitingJobState>();

            var awaitingState = (CoreAwaitingJobState)call.State;
            _ = awaitingState.Data["ParentId"].Should().Be(parentId);
            _ = awaitingState.Data["Options"].Should().Be(options.ConvertToValueString());
        }

        [Fact]
        public void ContinueJobWith_WithQueue_ShouldCallCreateWithQueue()
        {
            // Arrange
            const string parentId = "parent-job-id";
            const string queue = "continuation-queue";
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("continuation");

            // Act
            string result = this._testService.ContinueJobWith(parentId, methodCall, this.GetTestJobParameters(), queue);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().Be(queue);
            _ = call.State.Should().BeOfType<CoreAwaitingJobState>();
        }

        [Fact]
        public void ContinueJobWith_StaticMethod_ShouldCallCreateWithAwaitingState()
        {
            // Arrange
            const string parentId = "parent-job-id";
            Expression<Action> methodCall = () => TestJobClass.StaticTestMethod("continuation");

            // Act
            string result = this._testService.ContinueJobWith(parentId, methodCall);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.State.Should().BeOfType<CoreAwaitingJobState>();
        }

        [Fact]
        public void ContinueJobWith_AsyncMethod_ShouldCallCreateWithAwaitingState()
        {
            // Arrange
            const string parentId = "parent-job-id";
            Expression<Func<Task>> methodCall = () => TestJobClass.StaticAsyncTestMethod("continuation");

            // Act
            string result = this._testService.ContinueJobWith(parentId, methodCall);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.State.Should().BeOfType<CoreAwaitingJobState>();
        }

        [Fact]
        public void ContinueJobWith_AsyncInstanceMethod_ShouldCallCreateWithAwaitingState()
        {
            // Arrange
            const string parentId = "parent-job-id";
            Expression<Func<TestJobClass, Task>> methodCall = x => x.TestAsyncMethod("continuation");

            // Act
            string result = this._testService.ContinueJobWith(parentId, methodCall);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.State.Should().BeOfType<CoreAwaitingJobState>();
        }

        [Fact]
        public void Create_WithStaticAction_ShouldCallCreateWithEmptyQueue()
        {
            // Arrange
            Expression<Action> methodCall = () => TestJobClass.StaticTestMethod("test");
            var state = new CoreProcessingJobState("server-1", "worker-1");

            // Act
            string result = this._testService.Create(methodCall, state);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeSameAs(state);
        }

        [Fact]
        public void Create_WithAsyncMethod_ShouldCallCreateWithEmptyQueue()
        {
            // Arrange
            Expression<Func<Task>> methodCall = () => TestJobClass.StaticAsyncTestMethod("test");
            var state = new CoreSucceededJobState("Task completed", 1000, 900);

            // Act
            string result = this._testService.Create(methodCall, state);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeSameAs(state);
        }

        [Fact]
        public void Create_WithInstanceMethod_ShouldCallCreateWithEmptyQueue()
        {
            // Arrange
            Expression<Action<TestJobClass>> methodCall = x => x.TestMethod("test");
            var state = new CoreFailedJobState(new InvalidOperationException("Test exception"));

            // Act
            string result = this._testService.Create(methodCall, state);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeSameAs(state);
        }

        [Fact]
        public void Create_WithAsyncInstanceMethod_ShouldCallCreateWithEmptyQueue()
        {
            // Arrange
            Expression<Func<TestJobClass, Task>> methodCall = x => x.TestAsyncMethod("test");
            var state = new CoreDeletedJobState(null, "Job cancelled by user");

            // Act
            string result = this._testService.Create(methodCall, state);

            // Assert
            _ = result.Should().Be("test-job-id");
            _ = this._testService.CreateCalls.Should().HaveCount(1);
            TestSchedulingBackgroundService.CreateCall call = this._testService.CreateCalls[0];
            _ = call.Queue.Should().BeNull();
            _ = call.State.Should().BeSameAs(state);
        }

        [Theory]
        [InlineData("Enqueue")]
        [InlineData("Schedule")]
        [InlineData("ChangeState")]
        [InlineData("Delete")]
        [InlineData("Requeue")]
        [InlineData("Reschedule")]
        [InlineData("ContinueJobWith")]
        public void ExtensionMethods_WithNullService_ShouldThrowArgumentNullException(string methodName)
        {
            // Arrange
            ICoreSchedulingBackgroundService? service = null;

            // Act & Assert
            ArgumentNullException exception = methodName switch
            {
                "Enqueue" => Assert.Throws<ArgumentNullException>(() => service!.Enqueue((Expression<Action<TestJobClass>>)(x => x.TestMethod("test")))),
                "Schedule" => Assert.Throws<ArgumentNullException>(() => service!.Schedule((Expression<Action<TestJobClass>>)(x => x.TestMethod("test")), TimeSpan.FromMinutes(5))),
                "ChangeState" => Assert.Throws<ArgumentNullException>(() => service!.ChangeState("job-id", new CoreEnqueuedJobState())),
                "Delete" => Assert.Throws<ArgumentNullException>(() => service!.Delete("job-id")),
                "Requeue" => Assert.Throws<ArgumentNullException>(() => service!.Requeue("job-id")),
                "Reschedule" => Assert.Throws<ArgumentNullException>(() => service!.Reschedule("job-id", TimeSpan.FromMinutes(5))),
                "ContinueJobWith" => Assert.Throws<ArgumentNullException>(() => service!.ContinueJobWith("parent-id", (Expression<Action<TestJobClass>>)(x => x.TestMethod("test")))),
                _ => throw new ArgumentException($"Unknown method: {methodName}"),
            };

            _ = exception.ParamName.Should().Be("service");
        }

        /// <summary>
        /// Test job class for extension method testing.
        /// </summary>
        public class TestJobClass
        {
            public static void StaticTestMethod(string parameter)
            {
                Console.WriteLine($"Static test job executed with parameter: {parameter}");
            }

            public static async Task StaticAsyncTestMethod(string parameter)
            {
                Console.WriteLine($"Static async test job executed with parameter: {parameter}");
                await Task.Delay(100);
            }

            public void TestMethod(string parameter)
            {
                Console.WriteLine($"Test job executed with parameter: {parameter}");
            }

            public async Task TestAsyncMethod(string parameter)
            {
                Console.WriteLine($"Test async job executed with parameter: {parameter}");
                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Test implementation of ICoreSchedulingBackgroundService for unit testing.
        /// </summary>
        public class TestSchedulingBackgroundService : ICoreSchedulingBackgroundService
        {
            // Placeholder implementations for other interface members
            public ICoreFileSystem FileSystem => throw new NotImplementedException();

            public ICoreAppSettings AppSettings => throw new NotImplementedException();

            public bool IsRunning => true;

            public CoreServiceHostStatus ServiceHostStatus => CoreServiceHostStatus.Foreground;

            ICoreFileSystem ICoreHostedService.FileSystem => throw new NotImplementedException();

            ICoreAppSettings ICoreHostedService.AppSettings => throw new NotImplementedException();

            public IServiceProvider ServiceProvider => throw new NotImplementedException();

            public ICoreLogger Logger { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            CoreServiceHostStatus ICoreServiceHostStatus.ServiceHostStatus => throw new NotImplementedException();

            public bool IsInitialized => throw new NotImplementedException();

            public bool IsBackground => throw new NotImplementedException();

            public bool IsForeground => throw new NotImplementedException();

            public List<CreateCall> CreateCalls { get; } = [];

            public List<ChangeStateCall> ChangeStateCalls { get; } = [];

            public List<RecurringJobCall> RecurringJobCalls { get; } = [];

            public List<string?> TriggerRecurringJobCalls { get; } = [];

            public string DatabaseFilePath { get; } = "test-database.db";

            /// <inheritdoc/>
            public string Create(Expression<Action> methodCall, ICoreJobState state, IDictionary<string, object>? parameters = null, string? queue = null)
            {
                this.CreateCalls.Add(new CreateCall(methodCall.ToString(), state, queue));
                return "test-job-id";
            }

            /// <inheritdoc/>
            public string Create<T>(Expression<Action<T>> methodCall, ICoreJobState state, IDictionary<string, object>? parameters = null, string? queue = null)
            {
                this.CreateCalls.Add(new CreateCall(methodCall.ToString(), state, queue));
                return "test-job-id";
            }

            /// <inheritdoc/>
            public string Create<T>(Expression<Func<T, Task>> methodCall, ICoreJobState state, IDictionary<string, object>? parameters = null, string? queue = null)
            {
                this.CreateCalls.Add(new CreateCall(methodCall.ToString(), state, queue));
                return "test-job-id";
            }

            /// <inheritdoc/>
            public string Create(Expression<Func<Task>> methodCall, ICoreJobState state, IDictionary<string, object>? parameters = null, string? queue = null)
            {
                this.CreateCalls.Add(new CreateCall(methodCall.ToString(), state, queue));
                return "test-job-id";
            }

            /// <inheritdoc/>
            public bool ChangeState(string jobId, ICoreJobState state, string? fromState)
            {
                this.ChangeStateCalls.Add(new ChangeStateCall(jobId, state, fromState));
                return true;
            }

            /// <inheritdoc/>
            public string TriggerRecurringJob(string recurringJobId)
            {
                this.TriggerRecurringJobCalls.Add(recurringJobId);
                return "triggered-job-id";
            }

            /// <inheritdoc/>
            public void AddOrUpdateRecurringJob<T>(string recurringJobId, Expression<Func<T, Task>> methodCall, string cronExpression, string? queue = null)
            {
                this.RecurringJobCalls.Add(new RecurringJobCall(recurringJobId, methodCall.ToString(), cronExpression, queue));
            }

            /// <inheritdoc/>
            public void AddOrUpdateRecurringJob(string recurringJobId, Expression<Func<Task>> methodCall, string cronExpression, string? queue = null)
            {
                this.RecurringJobCalls.Add(new RecurringJobCall(recurringJobId, methodCall.ToString(), cronExpression, queue));
            }

            /// <inheritdoc/>
            public void AddOrUpdateRecurringJob<T>(string recurringJobId, Expression<Action<T>> methodCall, string cronExpression, string? queue = null)
            {
                this.RecurringJobCalls.Add(new RecurringJobCall(recurringJobId, methodCall.ToString(), cronExpression, queue));
            }

            /// <inheritdoc/>
            public void AddOrUpdateRecurringJob(string recurringJobId, Expression<Action> methodCall, string cronExpression, string? queue = null)
            {
                this.RecurringJobCalls.Add(new RecurringJobCall(recurringJobId, methodCall.ToString(), cronExpression, queue));
            }

            /// <inheritdoc/>
            public void RemoveRecurringJob(string recurringJobId)
            {
                this.RecurringJobCalls.Add(new RecurringJobCall(recurringJobId, "REMOVE", string.Empty, null));
            }

            /// <inheritdoc/>
            public Task<ICoreSchedulingJobInfo?> GetSchedulingJobInfoAsync(string jobId, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IReadOnlyDictionary<string, ICoreSchedulingJobInfo>> GetJobSchedulingInfoBatchAsync(IEnumerable<string> jobIds, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public IEnumerable<string> FindJobIDsByState(string stateName)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public IEnumerable<string> GetAllJobIDs()
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public IEnumerable<ICoreSchedulingJobInfo> GetSchedulingJobInfos(IEnumerable<string> jobIds, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public IEnumerable<ICoreSchedulingJobInfo> GetSchedulingJobInfosByState(string stateName, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<long> GetFilteredJobCountAsync(Func<ICoreSchedulingJobInfo, bool> filter, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public long GetTotalJobCount()
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<bool> JobExistsAsync(string jobId, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<ICoreJobState?> GetJobStateAsync(string jobId, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IReadOnlyList<ICoreSchedulingJobStateHistory>> GetJobStateHistoryAsync(string jobId, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<ICoreSchedulingJobStatistics> GetJobStatisticsAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<long> GetJobCountByStateAsync(string stateName, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<long> GetJobCountByQueueAsync(string queueName, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IReadOnlyCollection<string>> GetAvailableQueuesAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IReadOnlyCollection<string>> GetAvailableJobStatesAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<ICoreSchedulingRecurringJobInfo?> GetRecurringJobInfoAsync(string recurringJobId, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IReadOnlyList<ICoreSchedulingRecurringJobInfo>> EnumerateRecurringJobsAsync(CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<bool> RecurringJobExistsAsync(string recurringJobId, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IReadOnlyDictionary<string, bool>> BulkChangeStateAsync(IEnumerable<string> jobIds, ICoreJobState newState, string? fromState = null, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IReadOnlyDictionary<string, bool>> BulkDeleteJobsAsync(IEnumerable<string> jobIds, string? fromState = null, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IReadOnlyDictionary<string, bool>> BulkRequeueJobsAsync(IEnumerable<string> jobIds, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            /// <inheritdoc/>
            public Task<IReadOnlyList<ICoreSchedulingJobInfo>> SucceededJobs(int from, int count)
            {
                throw new NotImplementedException();
            }

            // Minimal implementations for interface compliance

            /// <inheritdoc/>
            public Task<bool> RestartService(CoreNetworkChangeEvent networkChangeEvent, ICoreNetworkInterface preferredNetworkInterface, CancellationToken cancellationToken = default)
                => Task.FromResult(true);

            /// <inheritdoc/>
            public void NetworkingSystem_NetworkConnectivityChanged(object? sender, ICoreNetworkConnectivityEvent networkConnectivityEvent)
            {
            }

            /// <inheritdoc/>
            public void Dispose()
            {
            }

            /// <inheritdoc/>
            public void Start()
            {
            }

            /// <inheritdoc/>
            public void Stop()
            {
            }

            /// <inheritdoc/>
            public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

            /// <inheritdoc/>
            public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

            /// <inheritdoc/>
            public string ToString(ICoreLogPropertyListFormatter logPropertyListFormatter)
            {
                return string.Empty;
            }

            /// <inheritdoc/>
            public string ToString(CoreLoggableFormatFlags loggableFormat, LogLevel? logLevel = null)
            {
                return string.Empty;
            }

            /// <inheritdoc/>
            public StringBuilder FormatLogString(StringBuilder? sb, ICoreLogPropertyListFormatter logPropertyListFormatter)
            {
                return new StringBuilder();
            }

            /// <inheritdoc/>
            public IEnumerable<ICoreLogPropertyLevel> GetLogPropertyListLevel(ICoreLogPropertyListFormatter logPropertyListFormatter)
            {
                return [];
            }

            // Additional required implementations would go here...
            // For brevity, only implementing what's needed for the extension method tests
            public record CreateCall(string MethodCall, ICoreJobState State, string? Queue);

            public record ChangeStateCall(string JobId, ICoreJobState State, string? FromState);

            public record RecurringJobCall(string RecurringJobId, string MethodCall, string CronExpression, string? Queue);
        }
    }
}
