// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreTaskCompletionSourceExtensionsUnitTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Tasks
{
    /// <summary>
    /// Class CoreTaskCompletionSourceExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreTaskCompletionSourceExtensionsUnitTests))]

    public class CoreTaskCompletionSourceExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTaskCompletionSourceExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTaskCompletionSourceExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task TryCompleteFromCompletedTaskTResult_PropagatesResult()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
            int result = await tcs.Task;
            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task TryCompleteFromCompletedTaskTResult_WithDifferentTResult_PropagatesResult()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.TryCompleteFromCompletedTask(TaskConstants.Int32NegativeOne);
            object result = await tcs.Task;
            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task TryCompleteFromCompletedTaskTResult_PropagatesCancellation()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TryCompleteFromCompletedTask(TaskConstants<int>.Canceled);
            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(() => tcs.Task);
        }

        [Fact]
        public async Task TryCompleteFromCompletedTaskTResult_PropagatesException()
        {
            var source = new TaskCompletionSource<int>();
            source.TrySetException(new NotImplementedException());

            var tcs = new TaskCompletionSource<int>();
            tcs.TryCompleteFromCompletedTask(source.Task);
            await CoreAsyncAssert.ThrowsAsync<NotImplementedException>(() => tcs.Task);
        }

        [Fact]
        public async Task TryCompleteFromCompletedTask_PropagatesResult()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TryCompleteFromCompletedTask(TaskConstants.Completed, () => -1);
            int result = await tcs.Task;
            Assert.Equal(-1, result);
        }

        [Fact]
        public async Task TryCompleteFromCompletedTask_PropagatesCancellation()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TryCompleteFromCompletedTask(TaskConstants.Canceled, () => -1);
            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(() => tcs.Task);
        }

        [Fact]
        public async Task TryCompleteFromCompletedTask_PropagatesException()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TryCompleteFromCompletedTask(Task.FromException(new NotImplementedException()), () => -1);
            await CoreAsyncAssert.ThrowsAsync<NotImplementedException>(() => tcs.Task);
        }

        [Fact]
        public async Task CreateAsyncTaskSource_PermitsCompletingTask()
        {
            TaskCompletionSource<object> tcs = TaskCompletionSourceExtensions.CreateAsyncTaskSource<object>();
            tcs.SetResult(null!);

            await tcs.Task;
        }
    }
}
