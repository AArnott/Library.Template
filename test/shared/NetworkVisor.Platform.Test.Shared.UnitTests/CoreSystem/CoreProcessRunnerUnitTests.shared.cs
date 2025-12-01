// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.UnitTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreProcessRunnerUnitTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using FluentAssertions;
using NetworkVisor.Core.Async;
using NetworkVisor.Core.CoreSystem;
using NetworkVisor.Core.Logging.Interfaces;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.ProcessRunner;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.UnitTests.CoreSystem
{
    /// <summary>
    /// Class CoreProcessRunnerUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreProcessRunnerUnitTests))]

    public class CoreProcessRunnerUnitTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreProcessRunnerUnitTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreProcessRunnerUnitTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        /// <summary>
        /// Defines the test method ProcessRunner_RunProcessAsync_Empty_Path.
        /// </summary>
        [Fact]
        public async Task ProcessRunner_RunProcessAsync_Empty_Path()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.RunProcess))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.RunProcess} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestOperatingSystem.IsIOS.Should().BeTrue();
                return;
            }

            ICoreProcessRunner processRunner = new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);
            ICoreTaskResult<string?> taskResult = await processRunner.RunProcessAsync(string.Empty, new string[1], null);

            taskResult.Should().NotBeNull();
            taskResult.IsCompletedSuccessfully.Should().BeFalse();
            taskResult.IsException.Should().BeTrue();
            taskResult.Result.Should().BeNullOrEmpty();
            taskResult.Exception.Should().NotBeNull().And.Subject.Should().BeOfType<InvalidOperationException>();
        }

        /// <summary>
        /// Defines the test method ProcessRunner_RunProcessAsync_Null_Args.
        /// </summary>
        [Fact]
        public async Task ProcessRunner_RunProcessAsync_Null_Args()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.RunProcess))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.RunProcess} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestOperatingSystem.IsIOS.Should().BeTrue();
                return;
            }

            ICoreProcessRunner processRunner = new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);
            Func<Task> act = () => processRunner.RunProcessAsync(string.Empty, null!, null);
            (await act.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("args");
        }

        /// <summary>
        /// Defines the test method ProcessRunner_RunProcessAsync_Null_Path.
        /// </summary>
        [Fact]
        public async Task ProcessRunner_RunProcessAsync_Null_Path()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.RunProcess))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.RunProcess} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");

                this.TestOperatingSystem.IsIOS.Should().BeTrue();
                return;
            }

            ICoreProcessRunner processRunner = new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            Func<Task> act = () => processRunner.RunProcessAsync(null, new string[1], null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
            _ = (await act.Should().ThrowAsync<ArgumentNullException>()).And.ParamName.Should().Be("path");
        }

        private ICoreProcessRunner CreateProcessRunner(ICoreTestCaseLogger testCaseLogger)
        {
            return new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);
        }
    }
}
