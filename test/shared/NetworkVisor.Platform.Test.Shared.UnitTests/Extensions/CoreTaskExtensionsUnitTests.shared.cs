// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-13-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-13-2020
// ***********************************************************************
// <copyright file="CoreTaskExtensionsUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics;
using System.Globalization;
using FluentAssertions;
using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Extensions
{
    /// <summary>
    /// Class CoreTaskExtensionsUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreTaskExtensionsUnitTests))]

    public class CoreTaskExtensionsUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTaskExtensionsUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTaskExtensionsUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreTaskExtensionsUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Unit);
        }

        /// <summary>
        /// Defines the test method TaskExtensions_Cancel_Immediately.
        /// </summary>
        [Fact]
        public async Task TaskExtensions_Cancel_Immediately()
        {
            // Wait for 5 seconds
            var tasks = new List<Task>
            {
                Task.Delay(5000),
            };

            using var cts = new CancellationTokenSource();
            cts.Cancel();

            Task result = tasks.WhenAnyWaitAsync(cts.Token, this.TestCaseLogger);

            var stopWatch = Stopwatch.StartNew();

            Func<Task> fx = () => result;

            await fx.Should().ThrowAsync<TaskCanceledException>();

            stopWatch.Elapsed.Milliseconds.Should().BeLessThan(4000);

            result.Status.Should().Be(TaskStatus.Canceled);
        }

        /// <summary>
        /// Defines the test method TaskExtensions_Empty.
        /// </summary>
        [Fact]
        public async Task TaskExtensions_Empty()
        {
            var tasks = new List<Task>();

            await tasks.WhenAllWaitAsync(CancellationToken.None, this.TestCaseLogger);
        }

        /// <summary>
        /// Defines the test method TaskExtensions_Throws.
        /// </summary>
        [Fact]
        public async Task TaskExtensions_Throws()
        {
            var tasks = new List<Task>
            {
                Task.Run(() => throw new InvalidOperationException()),
            };

            Task result = tasks.WhenAllWaitAsync(CancellationToken.None, this.TestCaseLogger);

            Func<Task> fx = () => result;

            await fx.Should().ThrowAsync<InvalidOperationException>();

            // await result;
            result.Status.Should().Be(TaskStatus.Faulted);
        }

        /// <summary>
        /// Defines the test method TaskExtensions_Wait.
        /// </summary>
        [Fact]
        public async Task TaskExtensions_Wait()
        {
            var tasks = new List<Task>
            {
                Task.Delay(300),
                Task.Delay(100),
            };

            var stopWatch = Stopwatch.StartNew();

            // We want to wait on the current thread.
            await tasks.WhenAllWaitAsync(CancellationToken.None, this.TestCaseLogger);

            stopWatch.Stop();

            stopWatch.ElapsedMilliseconds.Should().BeGreaterThanOrEqualTo(250);
            this.TestOutputHelper.WriteLine(stopWatch.ElapsedMilliseconds.ToString(CultureInfo.InvariantCulture));
        }
    }
}
