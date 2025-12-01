// Assembly         : NetworkVisor.Platform.Test.Shared.Messaging.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreCommandProcessorIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Transactions;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NetworkVisor.Core.Messaging.Database;
using NetworkVisor.Core.Messaging.Inbox;
using NetworkVisor.Core.Messaging.Outbox;
using NetworkVisor.Core.Messaging.Services.CommandProcessor;
using NetworkVisor.Core.Messaging.Tables;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Commands;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Handlers;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Replies;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Requests;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base;
using NetworkVisor.Platform.Test.TestCase;
using Paramore.Brighter;
using Xunit;
using IDispatcher = Paramore.Brighter.ServiceActivator.IDispatcher;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Processor
{
    /// <summary>
    /// Class CoreCommandProcessorIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreCommandProcessorIntegrationTests))]

    public class CoreCommandProcessorIntegrationTests : CoreCommandTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreCommandProcessorIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreCommandProcessorIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public static bool RunTestCommands => false;

        [Fact]
        public void CommandDispatchService_TestCommandProcessor_Ctor()
        {
            this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();
            this.TestCommandProcessor.ProducersConfiguration.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IAmProducersConfiguration>();

            this.TestCommandProcessor.SqliteOutbox.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<ICoreSqliteOutbox>();

            this.TestCommandProcessor.ServiceActivatorConsumerOptions.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IAmConsumerOptions>();

            this.TestCommandProcessor.InboxConfiguration.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<InboxConfiguration>();

            this.TestCommandProcessor.SqliteInbox.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<ICoreSqliteInbox>();

            this.TestCommandProcessor.SqliteInbox.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IAmAnInboxAsync>();

            this.TestCommandProcessor.MessagingDatabase.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<ICoreMessagingDatabase>();
        }

        [Fact]
        public void CommandDispatchService_IsRunning()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
        }

        [Fact]
        public void CommandDispatchService_Dispatcher()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.Dispatcher.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IDispatcher>();
        }

        [Fact]
        public void CommandProcessor_IAmACommandProcessor()
        {
            IAmACommandProcessor commandProcessor = this.TestCaseServiceProvider.GetRequiredService<IAmACommandProcessor>();
            _ = commandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IAmACommandProcessor>();
        }

        [Fact]
        public void CommandProcessor_TestCase()
        {
            ICoreCommandProcessor? commandProcessor = this.TestCaseServiceProvider.GetRequiredService<ICoreCommandProcessor>();
            _ = commandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IAmACommandProcessor>();
            _ = commandProcessor.WrappedCommandProcessor.Should().BeSameAs(this.TestCommandProcessor.WrappedCommandProcessor);
        }

        /// <summary>
        /// Tests the asynchronous sending of a <see cref="TestCommand"/> using the <see cref="ICoreTestCommandProcessor"/>.
        /// </summary>
        /// <remarks>
        /// This test ensures that the <see cref="ICoreTestCommandProcessor"/> is properly initialized and capable of sending
        /// a <see cref="TestCommand"/> instance.
        /// Send() and SendAsync() - Sends a Command to one Caller Handler on the Internal Bus.
        /// Brighter follows Command-Query separation, and a Command does not have return value.
        /// So CommandDispatcher.SendAsync() does not return anything.
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation of the test.</returns>
        /// <exception cref="System.NullReferenceException">
        /// Thrown if <see cref="CoreCommandTestCaseBase.TestCommandProcessor"/> is null.
        /// </exception>
        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandProcessor_InternalBus_SendCommandAsync()
        {
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            await this.TestCommandProcessor!.SendAsync(new TestCommand(TestCommandName))!;
            this.DelayAndValidateTestOutput(0).Should().BeTrue();
        }

        /// <summary>
        /// Tests the ability of the <see cref="ICoreTestCommandProcessor"/> to publish a <see cref="TestEvent"/> asynchronously.
        /// </summary>
        /// <remarks>
        /// This test ensures that the <see cref="ICoreTestCommandProcessor"/> is properly initialized and running,
        /// and verifies that it can successfully publish a <see cref="TestEvent"/> instance.
        /// Broadcasts an Event to zero or more Caller Handlers on the Internal Bus.
        /// An Event is a fact, often the results of work that has been done.
        /// It is not atypical to raise an event to indicate the results of a Command having been actioned.
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the <see cref="ICoreTestCommandProcessor"/> is not properly initialized or running.
        /// </exception>
        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandProcessor_InternalBus_PublishEventAsync()
        {
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            var testEvent = new TestEvent(TestCommandName);

            await this.TestCommandProcessor!.PublishAsync(testEvent);
            this.DelayAndValidateTestOutput().Should().BeTrue();
        }

        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandProcessor_InternalBus_PublishRequestAsync()
        {
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            var testRequest = new TestCall(TestCommandName);

            await this.TestCommandProcessor!.PublishAsync(testRequest);
            this.DelayAndValidateTestOutput().Should().BeTrue();
        }

        /// <summary>
        /// Tests the asynchronous posting of a <see cref="TestCall"/> using the <see cref="ICoreTestCommandProcessor"/>.
        /// </summary>
        /// <remarks>
        /// This test verifies that the <see cref="ICoreTestCommandProcessor"/> is properly initialized and functional.
        /// It ensures that the <see cref="CoreTestClassBase.TestCommandDispatchService"/> is running and that the processor can handle
        /// a <see cref="TestCall"/> by posting it asynchronously.
        /// Immediately posts a Command or Event to another process via the external Bus.
        /// This is a one-step approach to dispatching a message via middleware. Use it if you do not need transactional messaging.
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation of the test.</returns>
        /// <exception cref="System.InvalidOperationException">
        /// Thrown if the <see cref="CoreTestClassBase.TestCommandDispatchService"/> is not running or the <see cref="CoreCommandTestCaseBase.TestCommandProcessor"/> is null.
        /// </exception>
        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandProcessor_ExternalBus_PostRequestAsync()
        {
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            // blocking call
            var testRequest = new TestCall(TestCommandName);

            await this.TestCommandProcessor!.PostAsync(testRequest);
            await this.TestCommandProcessor.ClearOutboxAsync([testRequest.Id]);

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            Message? deliveredMessage = await this.TestCommandProcessor.WaitForMessageDeliveryToOutboxAsync(testRequest.Id, cts.Token);
            _ = deliveredMessage.Should().NotBeNull();
            _ = deliveredMessage.IsEmpty.Should().BeFalse();

            (await this.TestCommandProcessor.WaitForInboxMessageDelivery<TestReply, TestReplyHandlerAsync>(testRequest.ReplyAddress.CorrelationId, cts.Token)).Should().BeTrue();
            this.TestOutputHelper.Output.Contains(TestCommandNameResult).Should().BeTrue();
        }

        /// <summary>
        /// Tests the <c>DepositPostAsync{TRequest}</c> method by posting a test request to the external bus
        /// and verifying the behavior of the command processor.
        /// </summary>
        /// <remarks>
        /// Puts one or many Command(s) or Event(s) in the Outbox for later delivery.
        /// This test ensures that the <see cref="ICoreTestCommandProcessor"/> is properly initialized and running,
        /// and that it can handle a test request by posting it to the external bus and clearing the outbox.
        /// </remarks>
        /// <returns>A task that represents the asynchronous operation of the test.</returns>
        /// <exception cref="System.Exception">
        /// Thrown if the <see cref="ICoreTestCommandProcessor"/> is not initialized or the test fails.
        /// </exception>
        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandProcessor_ExternalBus_DepositPostRequestAsync()
        {
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            // blocking call
            var testRequest = new TestCall(TestCommandName);

            string requestID = await this.TestCommandProcessor!.DepositPostAsync(testRequest);

            // Clear the Caller from the Outbox
            await this.TestCommandProcessor.ClearOutboxAsync([requestID]);

            CancellationTokenSource cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            Message? deliveredMessage = await this.TestCommandProcessor.WaitForMessageDeliveryToOutboxAsync(requestID, cts.Token);
            _ = deliveredMessage.Should().NotBeNull();
            _ = deliveredMessage.IsEmpty.Should().BeFalse();

            (await this.TestCommandProcessor.WaitForInboxMessageDelivery<TestReply, TestReplyHandlerAsync>(testRequest.ReplyAddress.CorrelationId, cts.Token)).Should().BeTrue();
            this.TestOutputHelper.Output.Contains(TestCommandNameResult).Should().BeTrue();
        }
    }
}
