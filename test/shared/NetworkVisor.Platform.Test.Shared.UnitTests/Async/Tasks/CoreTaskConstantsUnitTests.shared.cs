// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreTaskConstantsUnitTests.shared.cs" company="Network Visor">
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
    /// Class CoreTaskConstantsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreTaskConstantsUnitTests))]

    public class CoreTaskConstantsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTaskConstantsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTaskConstantsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public async Task BooleanTrue_IsCompletedWithValueOfTrue()
        {
            Task<bool>? task = TaskConstants.BooleanTrue;
            Assert.True(task.IsCompleted);
            Assert.True(await task);
        }

        [Fact]
        public void BooleanTrue_IsCached()
        {
            Task<bool>? task1 = TaskConstants.BooleanTrue;
            Task<bool>? task2 = TaskConstants.BooleanTrue;
            Assert.Same(task1, task2);
        }

        [Fact]
        public async Task BooleanFalse_IsCompletedWithValueOfFalse()
        {
            Task<bool>? task = TaskConstants.BooleanFalse;
            Assert.True(task.IsCompleted);
            Assert.False(await task);
        }

        [Fact]
        public void BooleanFalse_IsCached()
        {
            Task<bool>? task1 = TaskConstants.BooleanFalse;
            Task<bool>? task2 = TaskConstants.BooleanFalse;
            Assert.Same(task1, task2);
        }

        [Fact]
        public async Task Int32Zero_IsCompletedWithValueOfZero()
        {
            Task<int>? task = TaskConstants.Int32Zero;
            Assert.True(task.IsCompleted);
            Assert.Equal(0, await task);
        }

        [Fact]
        public void Int32Zero_IsCached()
        {
            Task<int>? task1 = TaskConstants.Int32Zero;
            Task<int>? task2 = TaskConstants.Int32Zero;
            Assert.Same(task1, task2);
        }

        [Fact]
        public async Task Int32NegativeOne_IsCompletedWithValueOfNegativeOne()
        {
            Task<int>? task = TaskConstants.Int32NegativeOne;
            Assert.True(task.IsCompleted);
            Assert.Equal(-1, await task);
        }

        [Fact]
        public void Int32NegativeOne_IsCached()
        {
            Task<int>? task1 = TaskConstants.Int32NegativeOne;
            Task<int>? task2 = TaskConstants.Int32NegativeOne;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void Completed_IsCompleted()
        {
            Task? task = TaskConstants.Completed;
            Assert.True(task.IsCompleted);
        }

        [Fact]
        public void Completed_IsCached()
        {
            Task? task1 = TaskConstants.Completed;
            Task? task2 = TaskConstants.Completed;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void Canceled_IsCanceled()
        {
            Task? task = TaskConstants.Canceled;
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void Canceled_IsCached()
        {
            Task? task1 = TaskConstants.Canceled;
            Task? task2 = TaskConstants.Canceled;
            Assert.Same(task1, task2);
        }

        [Fact]
        public async Task Default_ReferenceType_IsCompletedWithValueOfNull()
        {
            Task<object>? task = TaskConstants<object>.Default;
            Assert.True(task.IsCompleted);
            Assert.Null(await task);
        }

        [Fact]
        public async Task Default_ValueType_IsCompletedWithValueOfZero()
        {
            Task<byte>? task = TaskConstants<byte>.Default;
            Assert.True(task.IsCompleted);
            Assert.Equal(0, await task);
        }

        [Fact]
        public void Default_IsCached()
        {
            Task<object>? task1 = TaskConstants<object>.Default;
            Task<object>? task2 = TaskConstants<object>.Default;
            Assert.Same(task1, task2);
        }

        [Fact]
        public void CanceledOfT_IsCanceled()
        {
            Task<object>? task = TaskConstants<object>.Canceled;
            Assert.True(task.IsCanceled);
        }

        [Fact]
        public void CanceledOfT_IsCached()
        {
            Task<object>? task1 = TaskConstants<object>.Canceled;
            Task<object>? task2 = TaskConstants<object>.Canceled;
            Assert.Same(task1, task2);
        }
    }
}
