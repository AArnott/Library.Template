// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestRequestMessageMapper.shared.cs" company="Network Visor">
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
    /// Provides functionality to map <see cref="TestCall"/> objects to <see cref="Message"/> objects and vice versa.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="CoreMessageMapperAsyncBase{TRequest}"/> base class, specifically for handling
    /// <see cref="TestCall"/> instances. It facilitates the conversion between the domain-specific request objects
    /// and messaging infrastructure objects used within the NetworkVisor platform.
    /// </remarks>
    /// <seealso cref="CoreMessageMapperAsyncBase{TRequest}"/>
    /// <seealso cref="TestCall"/>
    /// <seealso cref="Message"/>
    public class TestRequestMessageMapper : CoreMessageMapperAsyncBase<TestCall>
    {
        /// <inheritdoc/>
        public override Task<Message> MapToMessageAsync(TestCall call, Publication publication, CancellationToken cancellationToken = default)
        {
            if (call is null)
            {
                throw new ArgumentNullException(nameof(call));
            }

            if (publication is null)
            {
                throw new ArgumentNullException(nameof(publication));
            }

            var header = new MessageHeader(
                messageId: call.Id,
                topic: publication.Topic ?? RoutingKey.Empty,
                messageType: call.RequestToMessageType(),
                correlationId: call.ReplyAddress.CorrelationId,
                replyTo: call.ReplyAddress.Topic);

            var testRequestBody = new TestCallBody(call.Name);
            var body = new MessageBody(testRequestBody.ToJsonString<TestCallBody>());
            var message = new Message(header, body);
            return Task.FromResult(message);
        }

        /// <inheritdoc/>
        public override Task<TestCall> MapToRequestAsync(Message message, CancellationToken cancellationToken = default)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var replyAddress = new CoreReplyAddress(topic: message.Header.ReplyTo ?? RoutingKey.Empty, correlationId: message.Header.CorrelationId);
            var request = new TestCall(TestCallBody.GetMessageBodyFromJson(message.Body.Value)!, replyAddress);

            return Task.FromResult(request);
        }
    }
}
