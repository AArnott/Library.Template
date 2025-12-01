// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreTaskFactoryExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Tasks
{
    /// <summary>
    /// Class CoreTaskFactoryExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreTaskFactoryExtensionsUnitTests))]

    public class CoreTaskFactoryExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTaskFactoryExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTaskFactoryExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task RunAction_WithFactoryScheduler_UsesFactoryScheduler()
        {
            TaskScheduler? scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            var factory = new TaskFactory(scheduler);
            TaskScheduler? result = null;

            Task? task = factory.Run(() =>
            {
                result = TaskScheduler.Current;
            });
            await task;

            Assert.Same(scheduler, result);
            Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
        }

        [Fact]
        public async Task RunAction_WithCurrentScheduler_UsesDefaultScheduler()
        {
            TaskScheduler? scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            var testFactory = new TaskFactory(scheduler);
            Task? task = null;
            TaskScheduler? result = null;

            await testFactory.StartNew(async () =>
            {
                Assert.Same(scheduler, TaskScheduler.Current);
                Assert.Null(Task.Factory.Scheduler);
                task = Task.Factory.Run(() =>
                {
                    result = TaskScheduler.Current;
                });
                await task;
            }).Unwrap();

            Assert.Same(TaskScheduler.Default, result);
            Assert.True((task!.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
        }

        [Fact]
        public async Task RunFunc_WithFactoryScheduler_UsesFactoryScheduler()
        {
            TaskScheduler? scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            var factory = new TaskFactory(scheduler);

            Task<TaskScheduler>? task = factory.Run(() => TaskScheduler.Current);
            TaskScheduler? result = await task;

            Assert.Same(scheduler, result);
            Assert.True((task.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
        }

        [Fact]
        public async Task RunFunc_WithCurrentScheduler_UsesDefaultScheduler()
        {
            TaskScheduler? scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            var testFactory = new TaskFactory(scheduler);
            Task<TaskScheduler>? task = null;
            TaskScheduler? result = null;

            await testFactory.StartNew(async () =>
            {
                Assert.Same(scheduler, TaskScheduler.Current);
                Assert.Null(Task.Factory.Scheduler);
                task = Task.Factory.Run(() => TaskScheduler.Current);
                result = await task;
            }).Unwrap();

            Assert.Same(TaskScheduler.Default, result);
            Assert.True((task!.CreationOptions & TaskCreationOptions.DenyChildAttach) == TaskCreationOptions.DenyChildAttach);
        }

        [Fact]
        public async Task RunAsyncAction_WithFactoryScheduler_UsesFactoryScheduler()
        {
            TaskScheduler? scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            var factory = new TaskFactory(scheduler);
            TaskScheduler? result = null;
            TaskScheduler? resultAfterAwait = null;

            Task? task = factory.Run(async () =>
            {
                result = TaskScheduler.Current;
                await Task.Yield();
                resultAfterAwait = TaskScheduler.Current;
            });
            await task;

            Assert.Same(scheduler, result);
            Assert.Same(scheduler, resultAfterAwait);
        }

        [Fact]
        public async Task RunAsyncAction_WithCurrentScheduler_UsesDefaultScheduler()
        {
            TaskScheduler? scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            var testFactory = new TaskFactory(scheduler);
            TaskScheduler? result = null;
            TaskScheduler? resultAfterAwait = null;

            await testFactory.StartNew(async () =>
            {
                Assert.Same(scheduler, TaskScheduler.Current);
                Assert.Null(Task.Factory.Scheduler);
                await Task.Factory.Run(async () =>
                {
                    result = TaskScheduler.Current;
                    await Task.Yield();
                    resultAfterAwait = TaskScheduler.Current;
                });
            }).Unwrap();

            Assert.Same(TaskScheduler.Default, result);
            Assert.Same(TaskScheduler.Default, resultAfterAwait);
        }

        [Fact]
        public async Task RunAsyncFunc_WithFactoryScheduler_UsesFactoryScheduler()
        {
            TaskScheduler? scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            var factory = new TaskFactory(scheduler);
            TaskScheduler? result = null;

            TaskScheduler? resultAfterAwait = await factory.Run(async () =>
            {
                result = TaskScheduler.Current;
                await Task.Yield();
                return TaskScheduler.Current;
            });

            Assert.Same(scheduler, result);
            Assert.Same(scheduler, resultAfterAwait);
        }

        [Fact]
        public async Task RunAsyncFunc_WithCurrentScheduler_UsesDefaultScheduler()
        {
            TaskScheduler? scheduler = new ConcurrentExclusiveSchedulerPair().ExclusiveScheduler;
            var testFactory = new TaskFactory(scheduler);
            TaskScheduler? result = null;
            TaskScheduler? resultAfterAwait = null;

            await testFactory.StartNew(async () =>
            {
                Assert.Same(scheduler, TaskScheduler.Current);
                Assert.Null(Task.Factory.Scheduler);
                resultAfterAwait = await Task.Factory.Run(async () =>
                {
                    result = TaskScheduler.Current;
                    await Task.Yield();
                    return TaskScheduler.Current;
                });
            }).Unwrap();

            Assert.Same(TaskScheduler.Default, result);
            Assert.Same(TaskScheduler.Default, resultAfterAwait);
        }
    }
}
