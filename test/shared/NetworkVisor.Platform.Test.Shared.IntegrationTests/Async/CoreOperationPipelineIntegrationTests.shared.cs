// Assembly         : NetworkVisor.Platform.Test.Shared.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-11-2020
// // ***********************************************************************
// <copyright file="CoreOperationPipelineIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using FluentAssertions;
using NetworkVisor.Core.Async.Tasks;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.TestCase;
using Polly;
using Polly.RateLimiting;
using Polly.Retry;
using Polly.Timeout;
using Xunit;

namespace NetworkVisor.Platform.Test.Shared.IntegrationTests.Async
{
    /// <summary>
    /// Class CoreOperationPipelineIntegrationTests.
    /// Implements the <see cref="CoreTestClassBase" />.
    /// </summary>
    /// <seealso cref="CoreTestClassBase" />
    [PlatformTrait(typeof(CoreOperationPipelineIntegrationTests))]

    public class CoreOperationPipelineIntegrationTests : CoreTestCaseBase
    {
        private const int DefaultMaxRetryAttempts = 3;
        private RetryStrategyOptions _defaultRetryOptions;

        /// <summary>
        /// Initializes a new instance of the <see cref="CoreOperationPipelineIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreOperationPipelineIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
            // For advanced control over the retry behavior, including the number of attempts,
            // delay between retries, and the types of exceptions to handle.
            this._defaultRetryOptions = new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,  // Adds a random factor to the delay
                MaxRetryAttempts = DefaultMaxRetryAttempts,
                Delay = TimeSpan.FromMilliseconds(100),
            };
        }

        [Fact]
        public void CoreOperationPipelineIntegrationTests_ValidateTestClass()
        {
            this.ValidateTestClass(TraitOperatingSystem.Core, TraitTestType.Integration);
        }

        [Fact]
        public async Task CoreOperationPipelineIntegration_Polly_Timeout()
        {
            // Creating a new resilience pipeline
            ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
                .AddRetry(this._defaultRetryOptions)
                .AddTimeout(TimeSpan.FromSeconds(1))
                .AddConcurrencyLimiter(1)
                .Build();

            await pipeline.Invoking(s => s.ExecuteAsync(async token =>
                {
                    // Wait for 10 seconds
                    await Task.Delay(10000, token).ConfigureAwait(false);
                })
                .AsTask()).Should().ThrowAsync<TimeoutRejectedException>();
        }

        [Fact]
        public async Task CoreOperationPipelineIntegration_Polly_UserCancel()
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            cts.CancelAfter(TimeSpan.FromSeconds(1));

            // Creating a new resilience pipeline
            ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
                .AddRetry(this._defaultRetryOptions)
                .Build();

            await pipeline.Invoking(s => s.ExecuteAsync(
                async token =>
                        {
                            await Task.Delay(3000, token).ConfigureAwait(false);
                        },
                cts.Token)
            .AsTask()).Should().ThrowAsync<Exception>()
                .Where(ex => ex is TaskCanceledException || ex is OperationCanceledException);
        }

        [Fact]
        public async Task CoreOperationPipelineIntegration_Polly_Retry_Failure()
        {
            // Creating a new resilience pipeline
            ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
                .AddRetry(this._defaultRetryOptions)
                .AddTimeout(TimeSpan.FromSeconds(1))
                .AddConcurrencyLimiter(1)
                .Build();

            int retries = 0;

            await pipeline.Invoking(s => s.ExecuteAsync(token =>
                {
                    retries++;
                    throw new InvalidOperationException("Test invalid operation exception.");
                })
                .AsTask()).Should().ThrowAsync<InvalidOperationException>();

            // Verify the original failure plus DefaultMaxRetryAttempts
            retries.Should().Be(DefaultMaxRetryAttempts + 1);
        }

        [Fact]
        public async Task CoreOperationPipelineIntegration_Polly_Retry_Callback()
        {
            RetryStrategyOptions retryWithCallback =
                this._defaultRetryOptions = new RetryStrategyOptions
                {
                    ShouldHandle = new PredicateBuilder().Handle<InvalidOperationException>(),
                    OnRetry = args =>
                    {
                        this.TestOutputHelper.WriteLine($"Retry Attempt: {args.AttemptNumber}");
                        return default;
                    },
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true,  // Adds a random factor to the delay
                    MaxRetryAttempts = DefaultMaxRetryAttempts,
                    Delay = TimeSpan.FromMilliseconds(100),
                };

            // Creating a new resilience pipeline
            ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
                .AddRetry(retryWithCallback)
                .AddTimeout(TimeSpan.FromSeconds(5))
                .Build();

            int retries = 0;

            await pipeline.Invoking(s => s.ExecuteAsync(token =>
                {
                    retries++;
                    throw new InvalidOperationException("Test invalid operation exception.");
                })
                .AsTask()).Should().ThrowAsync<InvalidOperationException>();

            // Verify the original failure plus DefaultMaxRetryAttempts
            retries.Should().Be(DefaultMaxRetryAttempts + 1);
        }

        [Fact]
        public async Task CoreOperationPipelineIntegration_Polly_ConcurrencyLimiting()
        {
            // Creating a new resilience pipeline
            ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
                .AddRetry(this._defaultRetryOptions)
                .AddConcurrencyLimiter(1, 1)
                .Build();

            var tasks = new List<Task>
            {
                pipeline.ExecuteAsync(
                    async token =>
                    {
                        await Task.Delay(1000, token).ConfigureAwait(false);
                    }).AsTask(),

                pipeline.ExecuteAsync(
                    async token =>
                    {
                        await Task.Delay(2000, token).ConfigureAwait(false);
                    }).AsTask(),
            };

            await tasks.WhenAll();
        }

        [Fact]
        public async Task CoreOperationPipelineIntegration_Polly_ConcurrencyLimiter_Rejected()
        {
            // Creating a new resilience pipeline
            ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
                .AddRetry(this._defaultRetryOptions)
                .AddConcurrencyLimiter(1, 0)
                .Build();

            var tasks = new List<Task>
            {
                pipeline.ExecuteAsync(
                    async token =>
                    {
                        await Task.Delay(1000, token).ConfigureAwait(false);
                    }).AsTask(),

                pipeline.ExecuteAsync(
                    async token =>
                    {
                        await Task.Delay(2000, token).ConfigureAwait(false);
                    }).AsTask(),
            };

            await Assert.ThrowsAsync<RateLimiterRejectedException>(tasks.WhenAll);
        }
    }
}
