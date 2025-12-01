// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-25-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-27-2020
// ***********************************************************************
// <copyright file="CoreNoopDisposableUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>
//      Forked from https://github.com/StephenCleary/AsyncEx.
//      Original idea by Stephen Toub: http://blogs.msdn.com/b/pfxteam/archive/2012/02/11/10266930.aspx.
// </summary>
// ***********************************************************************
#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER

using System;
using System.Threading.Tasks;
using NetworkVisor.Core.Async.Coordination;
using NetworkVisor.Core.Async.Disposables;
using NetworkVisor.Core.Async.Tasks.Interop;
using NetworkVisor.Core.Async.Tasks.Synchronous;
using NetworkVisor.Core.Disposables;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Async;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Async.Disposables
{
    /// <summary>
    /// Class CoreNoopDisposableUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreNoopDisposableUnitTests))]

    public class CoreNoopDisposableUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreNoopDisposableUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreNoopDisposableUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void Instance_IsSingleton()
        {
            Assert.Same(NoopDisposable.Instance, NoopDisposable.Instance);
        }

        [Fact]
        public void Dispose_MultipleTimes_DoesNothing()
        {
            NoopDisposable.Instance.Dispose();
            NoopDisposable.Instance.Dispose();
        }

        [Fact]
        public async Task DisposeAsync_MultipleTimes_DoesNothing()
        {
            await NoopDisposable.Instance.DisposeAsync();
            await NoopDisposable.Instance.DisposeAsync();
        }
    }
}
#endif
