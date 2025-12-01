// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreSynchronousTaskExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************

using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Async.Tasks.Synchronous;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Tasks
{
    /// <summary>
    /// Class CoreSynchronousTaskExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreSynchronousTaskExtensionsUnitTests))]

    public class CoreSynchronousTaskExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreSynchronousTaskExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreSynchronousTaskExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void WaitAndUnwrapException_Completed_DoesNotBlock()
        {
            TaskConstants.Completed.WaitAndUnwrapException();
        }

        [Fact]
        public void WaitAndUnwrapException_Faulted_UnwrapsException()
        {
            var task = Task.Run(() => { throw new NotImplementedException(); });
            CoreAsyncAssert.Throws<NotImplementedException>(task.WaitAndUnwrapException);
        }

        [Fact]
        public void WaitAndUnwrapExceptionWithCT_Completed_DoesNotBlock()
        {
            using var cts = new CancellationTokenSource();
            TaskConstants.Completed.WaitAndUnwrapException(cts.Token);
        }

        [Fact]
        public void WaitAndUnwrapExceptionWithCT_Faulted_UnwrapsException()
        {
            using var cts = new CancellationTokenSource();
            var task = Task.Run(() => { throw new NotImplementedException(); });
            CoreAsyncAssert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException(cts.Token));
        }

        [Fact]
        public void WaitAndUnwrapExceptionWithCT_CancellationTokenCancelled_Cancels()
        {
            var tcs = new TaskCompletionSource<object>();
            Task task = tcs.Task;
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            CoreAsyncAssert.Throws<OperationCanceledException>(() => task.WaitAndUnwrapException(cts.Token));
        }

        [Fact]
        public void WaitAndUnwrapExceptionResult_Completed_DoesNotBlock()
        {
            TaskConstants.Int32Zero.WaitAndUnwrapException();
        }

        [Fact]
        public void WaitAndUnwrapExceptionResult_Faulted_UnwrapsException()
        {
            Task<int> task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            CoreAsyncAssert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException(), allowDerivedTypes: false);
        }

        [Fact]
        public void WaitAndUnwrapExceptionResultWithCT_Completed_DoesNotBlock()
        {
            using var cts = new CancellationTokenSource();
            TaskConstants.Int32Zero.WaitAndUnwrapException(cts.Token);
        }

        [Fact]
        public void WaitAndUnwrapExceptionResultWithCT_Faulted_UnwrapsException()
        {
            using var cts = new CancellationTokenSource();
            Task<int> task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            CoreAsyncAssert.Throws<NotImplementedException>(() => task.WaitAndUnwrapException(cts.Token), allowDerivedTypes: false);
        }

        [Fact]
        public void WaitAndUnwrapExceptionResultWithCT_CancellationTokenCancelled_Cancels()
        {
            var tcs = new TaskCompletionSource<int>();
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            CoreAsyncAssert.Throws<OperationCanceledException>(() => tcs.Task.WaitAndUnwrapException(cts.Token));
        }

        [Fact]
        public void WaitWithoutException_Completed_DoesNotBlock()
        {
            TaskConstants.Completed.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutException_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants.Canceled.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutException_Faulted_DoesNotBlockOrThrow()
        {
            var task = Task.Run(() => { throw new NotImplementedException(); });
            task.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutExceptionResult_Completed_DoesNotBlock()
        {
            TaskConstants.Int32Zero.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutExceptionResult_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants<int>.Canceled.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutExceptionResult_Faulted_DoesNotBlockOrThrow()
        {
            Task<int> task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            task.WaitWithoutException();
        }

        [Fact]
        public void WaitWithoutExceptionWithCancellationToken_Completed_DoesNotBlock()
        {
            TaskConstants.Completed.WaitWithoutException(CancellationToken.None);
        }

        [Fact]
        public void WaitWithoutExceptionWithCancellationToken_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants.Canceled.WaitWithoutException(CancellationToken.None);
        }

        [Fact]
        public void WaitWithoutExceptionWithCancellationToken_Faulted_DoesNotBlockOrThrow()
        {
            var task = Task.Run(() => { throw new NotImplementedException(); });
            task.WaitWithoutException(CancellationToken.None);
        }

        [Fact]
        public void WaitWithoutExceptionResultWithCancellationToken_Completed_DoesNotBlock()
        {
            TaskConstants.Int32Zero.WaitWithoutException(CancellationToken.None);
        }

        [Fact]
        public void WaitWithoutExceptionResultWithCancellationToken_Canceled_DoesNotBlockOrThrow()
        {
            TaskConstants<int>.Canceled.WaitWithoutException(CancellationToken.None);
        }

        [Fact]
        public void WaitWithoutExceptionResultWithCancellationToken_Faulted_DoesNotBlockOrThrow()
        {
            Task<int> task = Task.Run((Func<int>)(() => { throw new NotImplementedException(); }));
            task.WaitWithoutException(CancellationToken.None);
        }

        [Fact]
        public void WaitWithoutExceptionWithCancellationToken_CanceledToken_DoesNotBlockButThrowsException()
        {
            Task task = new TaskCompletionSource<object>().Task;
            using var cts = new CancellationTokenSource();
            cts.Cancel();
            CoreAsyncAssert.Throws<OperationCanceledException>(() => task.WaitWithoutException(cts.Token));
        }

        [Fact]
        public async Task WaitWithoutExceptionWithCancellationToken_TokenCanceled_ThrowsException()
        {
            Task sourceTask = new TaskCompletionSource<object>().Task;
            using var cts = new CancellationTokenSource();
            var task = Task.Run(() => sourceTask.WaitWithoutException(cts.Token));
#pragma warning disable xUnit1031
            bool result = task.Wait(500);
#pragma warning restore xUnit1031
            Assert.False(result);
            cts.Cancel();
            await CoreAsyncAssert.ThrowsAsync<OperationCanceledException>(() => task);
        }
    }
}
