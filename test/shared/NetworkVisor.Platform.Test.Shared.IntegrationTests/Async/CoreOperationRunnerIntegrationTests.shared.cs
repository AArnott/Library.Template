// ***********************************************************************
// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// ***********************************************************************
// <copyright file="CoreOperationRunnerIntegrationTests.shared.cs" company="Network Visor">
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
using NetworkVisor.Core.Networking.Async;
using NetworkVisor.Core.Networking.Types;
using NetworkVisor.Core.Test.Extensions;
using NetworkVisor.Core.Test.Logging.Loggers;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Networking.ProcessRunner;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Async
{
    /// <summary>
    /// Class OperationRunnerUnitTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreOperationRunnerIntegrationTests))]

    public class CoreOperationRunnerIntegrationTests : CoreTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreOperationRunnerIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreOperationRunnerIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void CoreOperationRunnerUnit_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        /// <summary>
        /// Defines the test ProcessRunner_RetryOperationIfNeededAsync_RetryOnNoOutput.
        /// </summary>
        [Fact]
        [ExcludeFromCodeCoverage]
        public async Task ProcessRunner_RetryOperationIfNeededAsync_RetryOnNoOutput()
        {
            if (!this.TestNetworkingSystem.IsServiceSupported(CoreNetworkServiceTypes.RunProcess))
            {
                this.TestOutputHelper.WriteLine($"{CoreNetworkServiceTypes.RunProcess} is not available on {this.TestClassType.GetTraitOperatingSystem()}");
                this.TestOperatingSystem.IsIOS.Should().BeTrue();
                return;
            }

            var operationRunner = new CoreOperationRunner(this.TestCaseServiceProvider, this.TestCaseLogger);
            var processRunner = new CoreProcessRunner(this.TestCaseServiceProvider, this.TestFileSystem, this.TestCaseLogger);

            ICoreTaskResult<string?> taskResult = this.TestOperatingSystem.IsWindowsPlatform
                ? await operationRunner.RetryOperationIfNeededAsync(
                    () => processRunner.RunProcessAsync(Environment.GetEnvironmentVariable("ComSpec")!, new string[] { "/c dir > NUL" }, new TimeSpan(0, 0, 0, 20), CancellationToken.None, true),
                    operationRunner.RetryExceptWhenCancelled,
                    2,
                    new TimeSpan(0, 0, 0, 2),
                    CancellationToken.None)
                : await operationRunner.RetryOperationIfNeededAsync(
                    () => processRunner.RunProcessAsync("echo", new string[] { string.Empty }, new TimeSpan(0, 0, 0, 20), CancellationToken.None, true),
                    operationRunner.RetryExceptWhenCancelled,
                    2,
                    new TimeSpan(0, 0, 0, 2),
                    CancellationToken.None);
            taskResult.Should().NotBeNull();
            taskResult.IsCompletedSuccessfully.Should().BeFalse();
            taskResult.IsException.Should().BeTrue();
            taskResult.Exception.Should().NotBeNull().And.BeOfType<InvalidDataException>();
            taskResult.Exception!.Message.Should().EndWith(" produced no output.");
            taskResult.IsCompletedSuccessfully.Should().BeFalse();
        }
    }
}
