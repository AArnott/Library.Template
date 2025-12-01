// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreOperationRunnerUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Async;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async
{
    /// <summary>
    /// Class OperationRunnerUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreOperationRunnerUnitTests))]

    public class CoreOperationRunnerUnitTests : CoreTestCaseBase
    {
        private static readonly TimeSpan MinimumTaskDelay = new(0, 0, 0, 0, 100);       // .1 second
        private static readonly TimeSpan MinimumCancelDelay = new(0, 0, 0, 0, 100);       // .1 second
        private static readonly TimeSpan QuickCacheExpiration = new(0, 0, 0, 0, 200);       // .2 second
        private static readonly TimeSpan DefaultTaskDelay = new(0, 1, 0);   // 1 minute
        private static readonly TimeSpan LongTaskDelay = new(0, 2, 0);      // 2 minutes

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreOperationRunnerUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreOperationRunnerUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreOperationRunnerUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method OperationRunner_RunOperationAsync_NoDelay.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RunOperationAsync_NoDelay()
        {
            using var cts = new CancellationTokenSource();
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            // Wait for 1 second
            Task<ICoreTaskResult<bool>> taskOperation = operationRunner.TimeoutWaitAsync(new(0, 0, 0, 1, 0), cts.Token);

            taskOperation.Status.Should().Be(TaskStatus.WaitingForActivation);

            // Test a delay of 1 secs on a 60 sec timeout
            ICoreTaskResult<bool> taskOperationResult = await operationRunner.RunOperationAsync(
                taskOperation,
                new TimeSpan(0, 0, 60),
                cts.Token,
                false,
                this.TestCaseLogger);

            taskOperation.IsFaulted.Should().BeFalse();
            taskOperation.IsCanceled.Should().BeFalse();
            taskOperation.IsCompleted.Should().BeTrue();
            taskOperation.IsCompletedSuccessfully().Should().BeTrue();

            ICoreTaskResult<bool> taskResultOperation = await taskOperation;
            taskResultOperation.Should().NotBeNull();
            taskResultOperation.IsException.Should().BeFalse();
            taskResultOperation.IsCanceled.Should().BeFalse();
            taskResultOperation.IsCompleted.Should().BeTrue();
            taskResultOperation.IsCompletedSuccessfully.Should().BeTrue();
            taskResultOperation.Result.Should().BeTrue();

            taskOperationResult.IsCanceled.Should().BeFalse();
            taskOperationResult.IsTimedOut.Should().BeFalse();

            taskOperationResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();

            cts.Token.IsCancellationRequested.Should().BeFalse();
            cts.Cancel();
        }

        /// <summary>
        /// Defines the test method OperationRunner_RunOperationAsync_AlreadyCompleted.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RunOperationAsync_AlreadyCompleted()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            using var cts = new CancellationTokenSource();

            // Wait for .1 second
            Task<ICoreTaskResult<bool>> taskOperation = operationRunner.TimeoutWaitAsync(new(0, 0, 0, 0, 1), cts.Token);

            while (taskOperation.Status != TaskStatus.RanToCompletion)
            {
                await Task.Yield();
            }

            taskOperation.Status.Should().Be(TaskStatus.RanToCompletion);

            // Test a delay of .1 secs on a 30 sec timeout
            ICoreTaskResult<bool> taskOperationResult = await operationRunner.RunOperationAsync(
                taskOperation,
                new TimeSpan(0, 0, 30),
                cts.Token,
                false,
                this.TestCaseLogger);

            taskOperation.IsFaulted.Should().BeFalse();
            taskOperation.IsCanceled.Should().BeFalse();
            taskOperation.IsCompleted.Should().BeTrue();
            taskOperation.IsCompletedSuccessfully().Should().BeTrue();

            ICoreTaskResult<bool> taskResultOperation = await taskOperation;
            taskResultOperation.Should().NotBeNull();
            taskResultOperation.IsCompleted.Should().BeTrue();
            taskResultOperation.IsException.Should().BeFalse();
            taskResultOperation.IsCanceled.Should().BeFalse();
            taskResultOperation.IsCompletedSuccessfully.Should().BeTrue();
            taskResultOperation.Result.Should().BeTrue();

            taskOperationResult.IsCanceled.Should().BeFalse();
            taskOperationResult.IsTimedOut.Should().BeFalse();

            taskOperationResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();

            cts.Token.IsCancellationRequested.Should().BeFalse();
            cts.Cancel();
        }

        /// <summary>
        /// Defines the test method OperationRunner_RunOperationAsync_AlreadyCompleted_Cancel.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RunOperationAsync_AlreadyCompleted_Cancel()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            using var cts = new CancellationTokenSource();

            // Wait for 1 second
            Task<ICoreTaskResult<bool>> taskOperation = operationRunner.TimeoutWaitAsync(new(0, 0, 0, 1, 0), cts.Token);

            while (taskOperation.Status != TaskStatus.RanToCompletion)
            {
                await Task.Yield();
            }

            taskOperation.Status.Should().Be(TaskStatus.RanToCompletion);
            cts.Cancel();
            cts.Token.IsCancellationRequested.Should().BeTrue();

            // Test a delay of 1 sec on a 30 sec timeout
            ICoreTaskResult<bool> taskOperationResult = await operationRunner.RunOperationAsync(
                taskOperation,
                new TimeSpan(0, 0, 30),
                cts.Token,
                false,
                this.TestCaseLogger);

            taskOperation.IsFaulted.Should().BeFalse();
            taskOperation.IsCanceled.Should().BeFalse();
            taskOperation.IsCompleted.Should().BeTrue();
            taskOperation.IsCompletedSuccessfully().Should().BeTrue();

            ICoreTaskResult<bool> taskResultOperation = await taskOperation;
            taskResultOperation.Should().NotBeNull();
            taskResultOperation.IsException.Should().BeFalse();
            taskResultOperation.IsCanceled.Should().BeFalse();
            taskResultOperation.IsCompleted.Should().BeTrue();
            taskResultOperation.IsCompletedSuccessfully.Should().BeTrue();
            taskResultOperation.Result.Should().BeTrue();

            taskOperationResult.IsCanceled.Should().BeFalse();
            taskOperationResult.IsTimedOut.Should().BeFalse();

            taskOperationResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method OperationRunner_RunOperationAsync_MinimumDelay.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RunOperationAsync_MinimumDelay()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            using var cts = new CancellationTokenSource();

            // Wait for 4 seconds
            Task<ICoreTaskResult<bool>> taskOperation = operationRunner.TimeoutWaitAsync(new(0, 0, 0, 4, 0), cts.Token);

            taskOperation.Status.Should().Be(TaskStatus.WaitingForActivation);

            // Test an operation of 2 secs on a 45 sec timeout
            ICoreTaskResult<bool> taskOperationResult = await operationRunner.RunOperationAsync(
                taskOperation,
                new TimeSpan(0, 0, 45),
                cts.Token,
                false,
                this.TestCaseLogger);

            this.TestOutputHelper.WriteLine("Finished RunOperationAsync");
            this.TestOutputHelper.WriteLine($"Task Operation Status: {taskOperation.Status}");
            taskOperation.IsFaulted.Should().BeFalse();
            taskOperation.IsCanceled.Should().BeFalse();
            taskOperation.IsCompleted.Should().BeTrue();
            taskOperation.IsCompletedSuccessfully().Should().BeTrue();

            ICoreTaskResult<bool> taskResultOperation = await taskOperation;
            taskResultOperation.Should().NotBeNull();
            taskResultOperation.IsException.Should().BeFalse();
            taskResultOperation.IsCanceled.Should().BeFalse();
            taskResultOperation.IsCompleted.Should().BeTrue();
            taskResultOperation.IsCompletedSuccessfully.Should().BeTrue();
            taskResultOperation.Result.Should().BeTrue();

            taskOperationResult.IsCanceled.Should().BeFalse();
            taskOperationResult.IsTimedOut.Should().BeFalse();

            taskOperationResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();

            cts.Token.IsCancellationRequested.Should().BeFalse();
            cts.Cancel();
        }

        /// <summary>
        /// Defines the test method OperationRunner_RunOperationAsync_Timeout.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RunOperationAsync_Timeout()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            using CancellationTokenSource cts = new CancellationTokenSource();

            // Wait for 3 minutes
            Task<ICoreTaskResult<bool>> taskOperation = operationRunner.TimeoutWaitAsync(LongTaskDelay, cts.Token);

            taskOperation.Status.Should().Be(TaskStatus.WaitingForActivation);

            // Test a delay of 3 minutes on a 5 sec timeout
            ICoreTaskResult<bool> taskOperationResult = await operationRunner.RunOperationAsync(
                taskOperation,
                new TimeSpan(0, 0, 5),
                cts.Token,
                false,
                this.TestCaseLogger);

            taskOperation.IsCompletedSuccessfully().Should().BeFalse();

            taskOperation.IsFaulted.Should().BeFalse();
            taskOperation.IsCanceled.Should().BeFalse();
            taskOperation.IsCompleted.Should().BeFalse();
            taskOperation.IsCompletedSuccessfully().Should().BeFalse();

            taskOperationResult.IsCanceled.Should().BeFalse();
            taskOperationResult.IsTimedOut.Should().BeTrue();

            if (taskOperationResult.IsException)
            {
                this.TestOutputHelper.WriteLine("Timeout exception thrown");
                taskOperationResult.Exception.Should().BeOfType<TimeoutException>();
            }

            taskOperationResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeFalse();

            cts.Token.IsCancellationRequested.Should().BeFalse();
            cts.Cancel();
        }

        /// <summary>
        /// Defines the test method OperationRunner_RunOperationAsync_AsyncTask.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RunOperationAsync_AsyncTask()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            using CancellationTokenSource cts = new CancellationTokenSource();
            var task = Task.Run(async () =>
            {
                this.TestOutputHelper.WriteLine("Starting Task");
                await operationRunner.TimeoutWaitAsync(QuickCacheExpiration, cts.Token);
                this.TestOutputHelper.WriteLine("Finishing Task");
            });

            task.Status.Should().Be(TaskStatus.WaitingForActivation);

            ICoreTaskResult taskResult = await operationRunner.RunOperationAsync(
                task,
                new TimeSpan(0, 0, 0, 120),
                cts.Token,
                false,
                this.TestCaseLogger);

            this.TestOutputHelper.WriteLine($"Task Status: {task.Status}");
            task.IsCanceled.Should().BeFalse();
            task.IsFaulted.Should().BeFalse();
            task.IsCompleted.Should().BeTrue();
            task.IsCompletedSuccessfully().Should().BeTrue();

            taskResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();
            taskResult.IsTimedOut.Should().BeFalse();
            taskResult.IsCanceled.Should().BeFalse();

            cts.Token.IsCancellationRequested.Should().BeFalse();
            cts.Cancel();
        }

        /// <summary>
        /// Defines the test method OperationRunner_RunOperationWithTimeoutAsync_Null.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RunOperationWithTimeoutAsync_Null()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            Func<Task<ICoreTaskResult>> fx = async () =>
                await operationRunner.RunOperationAsync(null!, new TimeSpan(0, 0, 0, 5), CancellationToken.None, false, this.TestCaseLogger);

            (await fx.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("operationTask");
        }

        /// <summary>
        /// Defines the test method OperationRunner_RetryOperationIfNeededAsync.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task OperationRunner_RetryOperationIfNeededAsync()
        {
            int attempts = 1;
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            Task task = operationRunner
                .RetryOperationIfNeededAsync(
                    () =>
                    {
                        this.TestOutputHelper.WriteLine("Task Succeeded.");

                        return Task.FromResult<ICoreTaskResult<bool>>(new CoreTaskResult<bool>(true, true));
                    },
                    (e, logger) =>
                    {
                        e.Message.Should().Be("Invalidate operation");
                        this.TestOutputHelper.WriteLine($"Retrying Attempt#{attempts++}");
                        return true;
                    },
                    3,
                    new TimeSpan(100),
                    CancellationToken.None);

            await task;
            task.IsCompletedSuccessfully().Should().BeTrue();

            attempts.Should().Be(1);
        }

        /// <summary>
        /// Defines the test method OperationRunner_RetryOperationIfNeededAsync_Null.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task OperationRunner_RetryOperationIfNeededAsync_Null()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            Func<Task<ICoreTaskResult<bool>>> fx = async () =>
                await operationRunner
                    .RetryOperationIfNeededAsync<bool>(null!, (e, logger) => true, 3, new TimeSpan(100), CancellationToken.None);

            (await fx.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("operation");
        }

        /// <summary>
        /// Defines the test method OperationRunner_RetryOperationIfNeededAsync_Func_Null.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task OperationRunner_RetryOperationIfNeededAsync_Func_Null()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            Func<Task<ICoreTaskResult<int>>> fx = async () =>
                await operationRunner
                    .RetryOperationIfNeededAsync<int>(null!, (e, logger) => true, 3, new TimeSpan(100), CancellationToken.None);

            (await fx.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("operation");
        }

        /// <summary>
        /// Defines the test method OperationRunner_RetryOperationIfNeededAsync_Fail_Throw.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RetryOperationIfNeededAsync_Fail_Throw()
        {
            int attempts = 1;
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            ICoreTaskResult<bool> taskResult = await operationRunner
                .RetryOperationIfNeededAsync<bool>(
                    () => throw new InvalidOperationException("Invalidate operation"),
                    (e, logger) =>
                    {
                        e.Message.Should().Be("Invalidate operation");
                        this.TestOutputHelper.WriteLine($"Retrying Attempt#{attempts++}");
                        return true;
                    },
                    3,
                    new TimeSpan(100),
                    CancellationToken.None);

            taskResult.Should().NotBeNull();
            taskResult.IsCompletedSuccessfully.Should().BeFalse();
            taskResult.IsException.Should().BeTrue();
            taskResult.Exception.Should().NotBeNull().And.BeOfType<InvalidOperationException>();
            taskResult.Exception!.Message.Should().Be("Invalidate operation");
            taskResult.Result.Should().BeFalse();
            attempts.Should().Be(3);
        }

        /// <summary>
        /// Defines the test method OperationRunner_RetryOperationIfNeededAsync_Fail_Default.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RetryOperationIfNeededAsync_Fail_Default()
        {
            int attempts = 1;
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            Task task = operationRunner
                .RetryOperationIfNeededAsync<bool>(
                    () => throw new InvalidOperationException("Invalidate operation"),
                    (e, logger) =>
                    {
                        e.Message.Should().Be("Invalidate operation");
                        this.TestOutputHelper.WriteLine($"Retrying Attempt#{attempts++}");
                        return true;
                    },
                    3,
                    new TimeSpan(100),
                    CancellationToken.None);

            await task;
            task.IsCompletedSuccessfully().Should().BeTrue();
            attempts.Should().Be(3);
        }

        /// <summary>
        /// Defines the test method OperationRunner_RetryOperationIfNeededAsync_Fail_Succeed.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RetryOperationIfNeededAsync_Fail_Succeed()
        {
            int attempts = 1;
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            Task task = operationRunner
                .RetryOperationIfNeededAsync<bool>(
                    () => throw new InvalidOperationException("Invalidate operation"),
                    (e, logger) =>
                    {
                        e.Message.Should().Be("Invalidate operation");
                        this.TestOutputHelper.WriteLine($"Retrying Attempt#{attempts}");
                        return attempts++ == 2;
                    },
                    3,
                    new TimeSpan(100),
                    CancellationToken.None);

            await task;
            task.IsCompletedSuccessfully().Should().BeTrue();
            attempts.Should().Be(2);
        }

        /// <summary>
        /// Defines the test method OperationRunner_RunOperationsAsync.
        /// </summary>
        [Fact]
        public async Task OperationRunner_RunOperationsAsync()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();
            IList<string> sources = ["a", "b", "c", "d", "e", "f", "g", "h"];

            IList<string> results = await operationRunner.RunOperationsAsync(
                sources,
                async item =>
                    {
                        await Task.Run(() => this.TestOutputHelper.WriteLine(item));
                        return item;
                    },
                CancellationToken.None);

            results.Should().BeEquivalentTo(sources);
        }

        /// <summary>
        /// Defines the test method OperationRunner_TimeoutWaitAsync_Timeout.
        /// </summary>
        [Fact]
        public async Task OperationRunner_TimeoutWaitAsync_Timeout()
        {
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            ICoreTaskResult<bool> taskResult = await operationRunner.TimeoutWaitAsync(MinimumTaskDelay);

            taskResult.IsCompleted.Should().BeTrue();
            taskResult.IsTimedOut.Should().BeFalse();
            taskResult.IsCanceled.Should().BeFalse();
            taskResult.IsCompletedSuccessfully.Should().BeTrue();
            taskResult.IsTimedOutOrCanceled.Should().BeFalse();

            taskResult.Should().NotBeNull();
            taskResult.Result.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method OperationRunner_TimeoutWaitAsync_Cancel.
        /// </summary>
        [Fact]
        public async Task OperationRunner_TimeoutWaitAsync_Cancel()
        {
            using var cts = new CancellationTokenSource();
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            Task<ICoreTaskResult<bool>> taskTimeout = operationRunner.TimeoutWaitAsync(DefaultTaskDelay, cts.Token);

            taskTimeout.Status.Should().Be(TaskStatus.WaitingForActivation);
            taskTimeout.IsCompleted.Should().BeFalse();
            taskTimeout.IsCanceled.Should().BeFalse();
            taskTimeout.IsCompletedSuccessfully().Should().BeFalse();

            cts.Cancel();

            ICoreTaskResult<bool> taskResult = await taskTimeout;

            taskResult.IsCompleted.Should().BeFalse();
            taskResult.IsTimedOut.Should().BeFalse();
            taskResult.IsCanceled.Should().BeTrue();
            taskResult.IsCompletedSuccessfully.Should().BeFalse();
            taskResult.IsTimedOutOrCanceled.Should().BeTrue();

            taskResult.Should().NotBeNull();
            taskResult.Result.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test method OperationRunner_TimeoutWaitAsync_Cancel_Delay.
        /// </summary>
        [Fact]
        public async Task OperationRunner_TimeoutWaitAsync_Cancel_Delay()
        {
            using var cts = new CancellationTokenSource();
            ICoreOperationRunner operationRunner = this.CreateOperationRunner();

            Task<ICoreTaskResult<bool>> taskTimeout = operationRunner.TimeoutWaitAsync(DefaultTaskDelay, cts.Token);

            taskTimeout.Status.Should().Be(TaskStatus.WaitingForActivation);
            taskTimeout.IsCompleted.Should().BeFalse();
            taskTimeout.IsCanceled.Should().BeFalse();
            taskTimeout.IsCompletedSuccessfully().Should().BeFalse();

            cts.CancelAfter(MinimumCancelDelay);

            ICoreTaskResult<bool> taskResult = await taskTimeout;

            taskResult.IsCompleted.Should().BeFalse();
            taskResult.IsTimedOut.Should().BeFalse();
            taskResult.IsCanceled.Should().BeTrue();
            taskResult.IsCompletedSuccessfully.Should().BeFalse();
            taskResult.IsTimedOutOrCanceled.Should().BeTrue();

            taskResult.Should().NotBeNull();
            taskResult.Result.Should().BeFalse();
        }

        private ICoreOperationRunner CreateOperationRunner()
        {
            return new CoreOperationRunner(this.TestCaseServiceProvider, this.TestCaseLogger);
        }
    }
}
