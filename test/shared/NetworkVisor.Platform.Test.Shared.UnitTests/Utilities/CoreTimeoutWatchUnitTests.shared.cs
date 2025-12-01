// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreTimeoutWatchUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Assembly Extensions Unit Tests.</summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Core.Utilities;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.Utilities
{
    /// <summary>
    /// Class CoreTimeoutWatchUnitTests. Assembly Extensions Unit Tests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreTimeoutWatchUnitTests))]

    public class CoreTimeoutWatchUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreTimeoutWatchUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreTimeoutWatchUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method TimeoutWatch_Ctor.
        /// </summary>
        [Fact]
        public void TimeoutWatch_Ctor()
        {
            var timeOutWatch = CoreTimeoutWatch.StartNew(100);
            timeOutWatch.IsTimeElapsed.Should().BeFalse();
            this.TestDelay(150, this.TestCaseLogger).Should().BeTrue();

            timeOutWatch.IsTimeElapsed.Should().BeTrue();
        }

        /// <summary>
        /// Defines the test method TimeoutWatch_TimeSpan.
        /// </summary>
        [Fact]
        public void TimeoutWatch_TimeSpan()
        {
            var timeOutWatch = CoreTimeoutWatch.StartNew(new TimeSpan(0, 0, 0, 0, 200));
            timeOutWatch.IsTimeElapsed.Should().BeFalse();
            this.TestDelay(300, this.TestCaseLogger).Should().BeTrue();

            timeOutWatch.IsTimeElapsed.Should().BeTrue();
        }
    }
}
