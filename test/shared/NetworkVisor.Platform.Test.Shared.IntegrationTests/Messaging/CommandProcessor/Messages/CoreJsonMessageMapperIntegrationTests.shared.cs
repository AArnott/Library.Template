// Assembly         : NetworkVisor.Platform.Test.Shared.Messaging.IntegrationTests
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="CoreJsonMessageMapperIntegrationTests.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net.Mime;
using System.Text.Json;
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
using NetworkVisor.Platform.Test.Shared.IntegrationTests.Message.Sqlite.TestDoubles;
using Paramore.Brighter;
using Paramore.Brighter.JsonConverters;
using Paramore.Brighter.MessageMappers;
using Paramore.Brighter.ServiceActivator;
using Paramore.Brighter.ServiceActivator.Status;
using Xunit;
using IDispatcher = Paramore.Brighter.ServiceActivator.IDispatcher;

namespace NetworkVisor.Platform.Test.Messaging.Shared.IntegrationTests.Messaging.CommandProcessor.Service
{
    /// <summary>
    /// Class CoreJsonMessageMapperIntegrationTests.
    /// </summary>
    [PlatformTrait(typeof(CoreJsonMessageMapperIntegrationTests))]

    public class CoreJsonMessageMapperIntegrationTests : CoreCommandTestCaseBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CoreJsonMessageMapperIntegrationTests"/> class.
        /// </summary>
        /// <param name="testClassFixture">The fixture for the test class, providing shared context for all tests in the class.</param>
        public CoreJsonMessageMapperIntegrationTests(CoreTestClassFixture testClassFixture)
            : base(testClassFixture)
        {
        }

        [Fact]
        public void JsonMessageMapperIntegration_MapToMessage()
        {
            var mapper = new JsonMessageMapper<MyCommand>();
            var command = new MyCommand { Value = Guid.NewGuid().ToString() };
            var publication = new Publication { Topic = new RoutingKey(Guid.NewGuid().ToString()) };
            Message message = mapper.MapToMessage(command, publication);

            message.Should().NotBeNull();
#if NET472_OR_GREATER
            message.Header.ContentType.Should().Be(new ContentType("application/json"));
#else
            message.Header.ContentType.Should().Be(new ContentType(MediaTypeNames.Application.Json));
#endif
            message.Header.MessageType.Should().Be(MessageType.MT_COMMAND);
            message.Header.Topic.Should().Be(publication.Topic);
            message.Id.Should().Be(command.Id);
            message.Body.Should().NotBeNull();

            MyCommand? body = JsonSerializer.Deserialize<MyCommand>(message.Body.Bytes, JsonSerialisationOptions.Options);
            body.Should().NotBeNull();
            body.Value.Should().Be(command.Value);
        }

        [Fact]
        public async Task JsonMessageMapperIntegration_MapToMessageAsync()
        {
            var mapper = new JsonMessageMapper<MyCommand>();
            var command = new MyCommand { Value = Guid.NewGuid().ToString() };
            var publication = new Publication { Topic = new RoutingKey(Guid.NewGuid().ToString()) };
            Message message = await mapper.MapToMessageAsync(command, publication);

            message.Should().NotBeNull();
#if NET472_OR_GREATER
            message.Header.ContentType.Should().Be(new ContentType("application/json"));
#else
            message.Header.ContentType.Should().Be(new ContentType(MediaTypeNames.Application.Json));
#endif
            message.Header.MessageType.Should().Be(MessageType.MT_COMMAND);
            message.Header.Topic.Should().Be(publication.Topic);
            message.Id.Should().Be(command.Id);
            message.Body.Should().NotBeNull();

            MyCommand? body = JsonSerializer.Deserialize<MyCommand>(message.Body.Bytes, JsonSerialisationOptions.Options);
            body.Should().NotBeNull();
            body.Value.Should().Be(command.Value);
        }
    }
}
