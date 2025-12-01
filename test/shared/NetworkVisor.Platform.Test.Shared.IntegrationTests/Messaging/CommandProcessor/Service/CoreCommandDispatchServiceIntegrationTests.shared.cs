// Assembly         : NetworkVisor.Platform.Test.Shared.Messaging.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreCommandDispatchServiceIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Text.Json;
using System.Text.Json.Serialization;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NetworkVisor.Core.Extensions;
using NetworkVisor.Core.Messaging.Inbox;
using NetworkVisor.Core.Messaging.Services.CommandProcessor;
using NetworkVisor.Core.Serialization;
using NetworkVisor.Core.Test.Traits;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Commands;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Handlers;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Replies;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Requests;
using NetworkVisor.Platform.Test.Fixtures;
using NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Base;
using Paramore.Brighter;
using Paramore.Brighter.ServiceActivator;
using Paramore.Brighter.ServiceActivator.Status;
using Xunit;
using IDispatcher = Paramore.Brighter.ServiceActivator.IDispatcher;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Service
{
    /// <summary>
    /// Class CoreCommandDispatchServiceIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreCommandDispatchServiceIntegrationTests))]

    public class CoreCommandDispatchServiceIntegrationTests : CoreCommandTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreCommandDispatchServiceIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreCommandDispatchServiceIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        public static bool RunTestCommands => false;

        [Fact]
        public void SchedulingIntegration_Ctor()
        {
            _ = this.TestCommandProcessor.ProducersConfiguration.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IAmProducersConfiguration>();

            _ = this.TestCommandProcessor.ServiceActivatorConsumerOptions.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<IAmConsumerOptions>();

            _ = this.TestCommandProcessor.InboxConfiguration.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<InboxConfiguration>();

            _ = this.TestCommandProcessor.SqliteInbox.Should().NotBeNull().And.Subject.Should()
                .BeAssignableTo<ICoreSqliteInbox>();
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
        public void CommandDispatchService_Dispatcher_Output()
        {
            _ = this.TestCommandDispatchService.Should().NotBeNull();
            _ = this.TestCommandDispatchService!.Dispatcher.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IDispatcher>();

            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider);

            // Configure options to handle unsupported types
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.Converters.Add(new CoreBrighterMessageTypeFuncConverter());

            string jsonString = JsonSerializer.Serialize(this.TestCommandDispatchService!.Dispatcher, typeof(IDispatcher), options);
            this.TestOutputHelper.WriteLine($"\n{nameof(this.TestCommandDispatchService.Dispatcher).CenterTitle()}\n{jsonString}");

            foreach (DispatcherStateItem state in this.TestCommandDispatchService.Dispatcher.GetState())
            {
                this.TestOutputHelper.WriteLine(
                    $"\n{state.Name.CenterTitle()}\n{JsonSerializer.Serialize(state, options)}");
            }
        }

        [Fact]
        public void CommandDispatchService_Dispatcher_HostName()
        {
            _ = this.TestCommandDispatchService!.Dispatcher.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IDispatcher>();
            this.TestOutputHelper.WriteLine($"{this.TestCommandDispatchService.Dispatcher.HostName}");

            // Compare hostname with the local network agent host name.
            _ = this.TestCommandDispatchService.Dispatcher.HostName.ToString().Should().Be(this.TestNetworkServices.LocalNetworkAgent.NetworkAgentHostName);
        }

        [Fact]
        public void CommandDispatchService_Dispatcher_Consumers_Output()
        {
            JsonSerializerOptions options = CoreDefaultJsonSerializerOptions.GetDefaultJsonSerializerOptions(CoreSerializationFormatFlags.JsonFormatted, this.TestCaseServiceProvider);
            _ = this.TestCommandDispatchService!.Dispatcher.Should().NotBeNull().And.Subject.Should().BeAssignableTo<IDispatcher>();

            // Configure options to handle unsupported types
            options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.Converters.Add(new CoreBrighterMessageTypeFuncConverter());

            foreach (IAmAConsumer consumer in this.TestCommandDispatchService!.Dispatcher.Consumers)
            {
                string jsonString = JsonSerializer.Serialize(consumer, typeof(IAmAConsumer), options);
                this.TestOutputHelper.WriteLine($"\n{consumer.Name.ToString().CenterTitle()}\n{jsonString}");
            }
        }

        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandDispatchService_CommandProcessor_InternalBus_SendCommandAsync()
        {
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            await this.TestCommandProcessor!.SendAsync(new TestCommand(TestCommandName))!;
            _ = this.DelayAndValidateTestOutput(0).Should().BeTrue();
        }

        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandDispatchService_CommandProcessor_InternalBus_PublishRequestAsync()
        {
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            var testRequest = new TestCall(TestCommandName);

            await this.TestCommandProcessor!.PublishAsync(testRequest);
            _ = this.DelayAndValidateTestOutput().Should().BeTrue();
        }

        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandDispatchService_CommandProcessor_InternalBus_PublishEventAsync()
        {
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            var testEvent = new TestEvent(TestCommandName);

            await this.TestCommandProcessor!.PublishAsync(testEvent);
            _ = this.DelayAndValidateTestOutput().Should().BeTrue();
        }

        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandDispatchService_CommandProcessor_ExternalBus_PostRequestAsync()
        {
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();

            // blocking call
            var testRequest = new TestCall(TestCommandName);

            await this.TestCommandProcessor!.PostAsync(testRequest);
            await this.TestCommandProcessor.ClearOutboxAsync([testRequest.Id]);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(DefaultLongDelayInMilliseconds));

            Message? deliveredMessage = await this.TestCommandProcessor.WaitForMessageDeliveryToOutboxAsync(testRequest.Id, cts.Token);
            _ = deliveredMessage.Should().NotBeNull();
            _ = deliveredMessage.IsEmpty.Should().BeFalse();

            _ = (await this.TestCommandProcessor.WaitForInboxMessageDelivery<TestReply, TestReplyHandlerAsync>(testRequest.ReplyAddress.CorrelationId, cts.Token)).Should().BeTrue();
            _ = this.TestOutputHelper.Output.Contains(TestCommandNameResult).Should().BeTrue();
        }

        [Fact(SkipUnless = nameof(RunTestCommands), Skip = SkipReason)]
        public async Task CommandDispatchService_CommandProcessor_ExternalBus_DepositPostRequestAsync()
        {
            _ = this.TestCommandDispatchService!.IsRunning.Should().BeTrue();
            _ = this.TestCommandProcessor.Should().NotBeNull().And.Subject.Should().BeAssignableTo<ICoreTestCommandProcessor>();
            IAmATransactionConnectionProvider transactionProvider = this.TestCaseServiceProvider.GetRequiredService<IAmATransactionConnectionProvider>();

            // blocking call
            var testRequest = new TestCall(TestCommandName);

            string requestID = await this.TestCommandProcessor!.DepositPostAsync(testRequest, transactionProvider);

            // Clear the Caller from the Outbox
            await this.TestCommandProcessor.ClearOutboxAsync([requestID]);

            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromMilliseconds(DefaultLongDelayInMilliseconds));

            await transactionProvider.CommitAsync(cts.Token);

            Message? deliveredMessage = await this.TestCommandProcessor.WaitForMessageDeliveryToOutboxAsync(requestID, cts.Token);

            _ = deliveredMessage.Should().NotBeNull();
            _ = deliveredMessage.IsEmpty.Should().BeFalse();

            _ = (await this.TestCommandProcessor.WaitForInboxMessageDelivery<TestReply, TestReplyHandlerAsync>(testRequest.ReplyAddress.CorrelationId, cts.Token)).Should().BeTrue();
            _ = this.TestOutputHelper.Output.Contains(TestCommandNameResult).Should().BeTrue();
        }
    }
}
