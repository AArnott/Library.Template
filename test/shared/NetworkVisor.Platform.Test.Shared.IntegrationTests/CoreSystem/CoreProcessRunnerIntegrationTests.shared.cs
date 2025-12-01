// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreProcessRunnerIntegrationTests.shared.cs" company="Network Visor">
//     Copyright (c) Network Visor. All rights reserved.
//     Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Diagnostics.CodeAnalysis;
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

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.CoreSystem
{
    /// <summary>
    /// Class ProcessRunnerIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreProcessRunnerIntegrationTests))]

    public class CoreProcessRunnerIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreProcessRunnerIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreProcessRunnerIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void ProcessRunnerIntegration_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        /// <summary>
        /// Defines the test ProcessRunner_RunProcessAsync.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task ProcessRunner_RunProcessAsync()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.RunProcess))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.RunProcess} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestOperatingSystem.IsIOS.Should().BeTrue();
                return;
            }

            var processRunner = new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);

            ICoreTaskResult<string?> taskResult = this.TestOperatingSystem.IsWindowsPlatform
                ? await processRunner.RunProcessAsync(Environment.GetEnvironmentVariable("ComSpec")!, new string[] { "/c echo TestResult" }, new TimeSpan(0, 0, 1, 0))
                : await processRunner.RunProcessAsync("echo", new string[] { "TestResult" }, new TimeSpan(0, 0, 1, 0));

            taskResult.Should().NotBeNull();
            taskResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();
            taskResult.IsException.Should().BeFalse();
            taskResult.Result.Should().NotBeNullOrWhiteSpace();
            this.TestOutputHelper.WriteLine($"Output: [{taskResult.Result}]");
            taskResult.Result.Should().Contain("TestResult");
            taskResult.IsTimedOutOrCanceled.Should().BeFalse();
        }

        /// <summary>
        /// Defines the test ProcessRunner_RunProcessAsync_TimeOut_After2Seconds.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task ProcessRunner_RunProcessAsync_TimeOut_After2Seconds()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.RunProcess))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.RunProcess} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestOperatingSystem.IsIOS.Should().BeTrue();
                return;
            }

            var processRunner = new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);
            ICoreTaskResult<string?> taskResult = this.TestOperatingSystem.IsWindowsPlatform
                ? await processRunner.RunProcessAsync(Environment.GetEnvironmentVariable("ComSpec")!, new string[] { "/c ping -n 50 127.0.0.1" }, new TimeSpan(0, 0, 0, 2))
                : await processRunner.RunProcessAsync("sleep", new string[] { "100" }, new TimeSpan(0, 0, 0, 2));

            taskResult.Should().NotBeNull();
            taskResult.IsCompleted.Should().BeFalse();
            taskResult.IsTimedOut.Should().BeTrue();
            taskResult.IsCanceled.Should().BeFalse();
            taskResult.IsTimedOutOrCanceled.Should().BeTrue();

            // taskResult!.IsCompletedSuccessfullyWithLogging(this.TestLogger).Should().BeFalse();
            this.TestOutputHelper.WriteLine($"Output: [{taskResult.Result}]");
            taskResult.Result.Should().BeNull();

            if (taskResult.IsException)
            {
                taskResult.Exception.Should().BeOfType<TaskCanceledException>();
            }
        }

        /// <summary>
        /// Defines the test ProcessRunner_RunProcessAsync_NoOutput_Throws.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task ProcessRunner_RunProcessAsync_NoOutput_Throws()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.RunProcess))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.RunProcess} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestOperatingSystem.IsIOS.Should().BeTrue();
                return;
            }

            ICoreTaskResult<string?>? taskResult = null;
            var processRunner = new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);

            try
            {
                taskResult = this.TestOperatingSystem.IsWindowsPlatform
                    ? await processRunner.RunProcessAsync(Environment.GetEnvironmentVariable("ComSpec")!, new string[] { "/c dir > NUL" }, new TimeSpan(0, 0, 0, 20), CancellationToken.None, true)
                    : await processRunner.RunProcessAsync("echo", new string[] { string.Empty }, new TimeSpan(0, 0, 0, 20), CancellationToken.None, true);
            }
            catch (Xunit.Sdk.XunitException)
            {
                throw;
            }
            catch (Exception e)
            {
                e.Should().BeOfType<InvalidDataException>();
            }

            taskResult.Should().BeNull();
        }

        /// <summary>
        /// Defines the test ProcessRunner_RunProcessAsync_NoOutput.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task ProcessRunner_RunProcessAsync_NoOutput()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.RunProcess))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.RunProcess} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestOperatingSystem.IsIOS.Should().BeTrue();
                return;
            }

            var processRunner = new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);
            ICoreTaskResult<string?> taskResult = this.TestOperatingSystem.IsWindowsPlatform
                ? await processRunner.RunProcessAsync(Environment.GetEnvironmentVariable("ComSpec")!, new string[] { "/c dir > NUL" }, new TimeSpan(0, 0, 1, 0))
                : await processRunner.RunProcessAsync("sleep", new string[] { "1" }, new TimeSpan(0, 0, 0, 20));

            taskResult.Should().NotBeNull();
            taskResult.IsCompleted.Should().BeTrue();
            taskResult.IsTimedOut.Should().BeFalse();
            taskResult.IsCanceled.Should().BeFalse();
            taskResult.IsTimedOutOrCanceled.Should().BeFalse();
            taskResult.IsException.Should().BeFalse();
            taskResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();
            taskResult.Result.Should().NotBeNull();

            this.TestOutputHelper.WriteLine($"Output: [{taskResult.Result}]");
            taskResult.Result!.Trim().Should().BeEmpty();
        }

        /// <summary>
        /// Defines the test ProcessRunner_RunProcessAsync_TimeOut_Null.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task ProcessRunner_RunProcessAsync_TimeOut_Null()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.RunProcess))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.RunProcess} is not available on {this.TestClassType.GetTraitOperatingSystem()} ({this.TestOperatingSystem.OSVersionWithPlatform})");
                this.TestOperatingSystem.IsIOS.Should().BeTrue();
                return;
            }

            var processRunner = new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);
            ICoreTaskResult<string?> taskResult = this.TestOperatingSystem.IsWindowsPlatform
                ? await processRunner.RunProcessAsync(Environment.GetEnvironmentVariable("ComSpec")!, new string[] { "/c echo TestResult" }, null)
                : await processRunner.RunProcessAsync("echo", new string[] { "TestResult" }, null);

            taskResult.Should().NotBeNull();
            taskResult.IsCompletedSuccessfullyWithLogging(this.TestCaseLogger).Should().BeTrue();
            taskResult.Result.Should().NotBeNullOrWhiteSpace();
            this.TestOutputHelper.WriteLine($"Output: [{taskResult.Result}]");
            taskResult.Result.Should().Contain("TestResult");
            taskResult.IsTimedOutOrCanceled.Should().BeFalse();
        }
    }
}
