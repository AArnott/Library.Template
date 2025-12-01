// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestEventMessageMapper.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary>Provides functionality to map <see cref="TestCall"/> objects to <see cref="Message"/> objects and vice versa.</summary>

using NetworkVisor.Core.Messaging.Mappers.Base;
using NetworkVisor.Core.Messaging.ReplyAddresses.Base;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Requests;
using Paramore.Brighter;
using Paramore.Brighter.Extensions;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Messages
{
    /// <summary>
    /// Provides functionality to map between <see cref="TestEvent"/> objects and <see cref="Message"/> objects asynchronously.
    /// </summary>
    /// <remarks>
    /// This class extends <see cref="CoreMessageMapperAsyncBase{TestEvent}"/> and is specifically designed to handle
    /// the mapping of <see cref="TestEvent"/> instances to <see cref="Message"/> instances and vice versa.
    /// It ensures proper serialization and deserialization of the event data for message-based communication.
    /// </remarks>
    public class TestEventMessageMapper : CoreMessageMapperAsyncBase<TestEvent>
    {
        /// <inheritdoc/>
        public override Task<Message> MapToMessageAsync(TestEvent request, Publication publication, CancellationToken cancellationToken = default)
        {
            if (request is null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            if (publication is null)
            {
                throw new ArgumentNullException(nameof(publication));
            }

            var header = new MessageHeader(
                messageId: request.Id,
                topic: publication.Topic ?? RoutingKey.Empty,
                messageType: request.RequestToMessageType());

            var testRequestBody = new TestCallBody(request.Name);
            var body = new MessageBody(testRequestBody.ToJsonString<TestCallBody>());
            var message = new Message(header, body);
            return Task.FromResult(message);
        }

        /// <inheritdoc/>
        public override Task<TestEvent> MapToRequestAsync(Message message, CancellationToken cancellationToken = default)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var request = new TestEvent(message.Body.Value);

            return Task.FromResult(request);
        }
    }
}
