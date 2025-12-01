// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreAsyncContextUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Context;
using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Context
{
    /// <summary>
    /// Class CoreAsyncContextUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreAsyncContextUnitTests))]

    public class CoreAsyncContextUnitTests : CoreTestCaseBase
    {
        // Implementation detail: the default capacity.
        private const int DefaultCapacity = 8;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreAsyncContextUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreAsyncContextUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreAsyncContext_StaysOnSameThread()
        {
            int testThread = Thread.CurrentThread.ManagedThreadId;
            int contextThread = CoreAsyncContext.Run(() => Thread.CurrentThread.ManagedThreadId);
            Assert.Equal(testThread, contextThread);
        }

        [Fact]
        public void Run_AsyncVoid_BlocksUntilCompletion()
        {
            bool resumed = false;
            CoreAsyncContext.Run((Action)(async () =>
            {
                await Task.Yield();
                resumed = true;
            }));
            Assert.True(resumed);
        }

        [Fact]
        public void Run_FuncThatCallsAsyncVoid_BlocksUntilCompletion()
        {
            bool resumed = false;
            int result = CoreAsyncContext.Run((Func<int>)(() =>
            {
                async void AsyncVoid()
                {
                    await Task.Yield();
                    resumed = true;
                }

                AsyncVoid();
                return 13;
            }));
            Assert.True(resumed);
            Assert.Equal(13, result);
        }

        [Fact]
        public void Run_AsyncTask_BlocksUntilCompletion()
        {
            bool resumed = false;
            CoreAsyncContext.Run(async () =>
            {
                await Task.Yield();
                resumed = true;
            });
            Assert.True(resumed);
        }

        [Fact]
        public void Run_AsyncTaskWithResult_BlocksUntilCompletion()
        {
            bool resumed = false;
            int result = CoreAsyncContext.Run(async () =>
            {
                await Task.Yield();
                resumed = true;
                return 17;
            });
            Assert.True(resumed);
            Assert.Equal(17, result);
        }

        [Fact]
        public void Current_WithoutCoreAsyncContext_IsNull()
        {
            Assert.Null(CoreAsyncContext.Current);
        }

        [Fact]
        public void Current_FromCoreAsyncContext_IsCoreAsyncContext()
        {
            CoreAsyncContext? observedContext = null;
            var context = new CoreAsyncContext();
            context.Factory.Run(() =>
            {
                observedContext = CoreAsyncContext.Current;
            });

            context.Execute();

            Assert.Same(context, observedContext);
        }

        [Fact]
        public void SynchronizationContextCurrent_FromCoreAsyncContext_IsCoreAsyncContextSynchronizationContext()
        {
            SynchronizationContext? observedContext = null;
            var context = new CoreAsyncContext();
            context.Factory.Run(() =>
            {
                observedContext = SynchronizationContext.Current;
            });

            context.Execute();

            Assert.Same(context.SynchronizationContext, observedContext);
        }

        [Fact]
        public void TaskSchedulerCurrent_FromCoreAsyncContext_IsThreadPoolTaskScheduler()
        {
            TaskScheduler? observedScheduler = null;
            var context = new CoreAsyncContext();
            context.Factory.Run(() =>
            {
                observedScheduler = TaskScheduler.Current;
            });

            context.Execute();

            Assert.Same(TaskScheduler.Default, observedScheduler);
        }

        [Fact]
        public void TaskScheduler_MaximumConcurrency_IsOne()
        {
            var context = new CoreAsyncContext();
            Assert.Equal(1, context.Scheduler.MaximumConcurrencyLevel);
        }

        [Fact]
        public void Run_PropagatesException()
        {
            static void Test()
            {
                CoreAsyncContext.Run(() => { throw new NotImplementedException(); });
            }

            CoreAsyncAssert.Throws<NotImplementedException>(Test, allowDerivedTypes: false);
        }

        [Fact]
        public void Run_Async_PropagatesException()
        {
            static void Test()
            {
                CoreAsyncContext.Run(async () =>
            {
                await Task.Yield();
                throw new NotImplementedException();
            });
            }

            CoreAsyncAssert.Throws<NotImplementedException>(Test, allowDerivedTypes: false);
        }

        [Fact]
        public void SynchronizationContextPost_PropagatesException()
        {
            static void Test()
            {
                CoreAsyncContext.Run(async () =>
            {
                SynchronizationContext.Current!.Post(
                    _ =>
                {
                    throw new NotImplementedException();
                },
                    null);
                await Task.Yield();
            });
            }

            CoreAsyncAssert.Throws<NotImplementedException>(Test, allowDerivedTypes: false);
        }

        [Fact]
        public async Task SynchronizationContext_Send_ExecutesSynchronously()
        {
            using var thread = new CoreAsyncContextThread();
            SynchronizationContext? synchronizationContext = await thread.Factory.Run(() => SynchronizationContext.Current);
            int value = 0;
            synchronizationContext?.Send(_ => { value = 13; }, null);
            Assert.Equal(13, value);
        }

        [Fact]
        public async Task SynchronizationContext_Send_ExecutesInlineIfNecessary()
        {
            using var thread = new CoreAsyncContextThread();
            int value = 0;
            await thread.Factory.Run(() =>
            {
                SynchronizationContext.Current?.Send(_ => { value = 13; }, null);
                Assert.Equal(13, value);
            });
            Assert.Equal(13, value);
        }

        [Fact]
        public void Task_AfterExecute_NeverRuns()
        {
            int value = 0;
            var context = new CoreAsyncContext();
            context.Factory.Run(() => { value = 1; });
            context.Execute();

            Task task = context.Factory.Run(() => { value = 2; });

            task.ContinueWith(_ => { throw new Exception("Should not run"); }, TaskScheduler.Default);
            Assert.Equal(1, value);
        }

        [Fact]
        public void SynchronizationContext_IsEqualToCopyOfItself()
        {
            SynchronizationContext? synchronizationContext1 = CoreAsyncContext.Run(() => SynchronizationContext.Current);
            SynchronizationContext synchronizationContext2 = synchronizationContext1!.CreateCopy();
            Assert.Equal(synchronizationContext1.GetHashCode(), synchronizationContext2.GetHashCode());
            Assert.True(synchronizationContext1.Equals(synchronizationContext2));
            Assert.False(synchronizationContext1.Equals(new SynchronizationContext()));
        }

        [Fact]
        public void Id_IsEqualToTaskSchedulerId()
        {
            var context = new CoreAsyncContext();
            Assert.Equal(context.Scheduler.Id, context.Id);
        }
    }
}
