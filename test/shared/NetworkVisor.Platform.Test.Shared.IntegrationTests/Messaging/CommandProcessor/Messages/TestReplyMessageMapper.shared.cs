// Assembly         : Test.Shared.MulticastDns
// Author           : SteveBu
// Created          : 04-11-2020
//
// Last Modified By : SteveBu
// Last Modified On : 04-12-2020
// // ***********************************************************************
// <copyright file="TestReplyMessageMapper.shared.cs" company="Network Visor">
//      Copyright (c) Network Visor. All rights reserved.
//      Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>
// // ***********************************************************************
// <summary></summary>

using System.Net;
using NetworkVisor.Core.Messaging.Mappers.Base;
using NetworkVisor.Platform.MulticastDns.Shared.Test.Replies;
using Paramore.Brighter;

namespace NetworkVisor.Platform.MulticastDns.Shared.Test.Messages
{
    /// <summary>
    /// Provides functionality to map <see cref="TestReply"/> objects to <see cref="Message"/> objects and vice versa.
    /// </summary>
    /// <remarks>
    /// This class extends the <see cref="CoreMessageMapperAsyncBase{TRequest}"/> base class, specifically for handling
    /// <see cref="TestReply"/> objects. It facilitates the conversion between domain-specific reply objects and
    /// messaging infrastructure objects, enabling seamless communication within the system.
    /// </remarks>
    public class TestReplyMessageMapper : CoreMessageMapperAsyncBase<TestReply>
    {
        /// <inheritdoc/>
        public override Task<Message> MapToMessageAsync(TestReply reply, Publication publication, CancellationToken cancellationToken = default)
        {
            if (reply is null)
            {
                throw new ArgumentNullException(nameof(reply));
            }

            if (publication is null)
            {
                throw new ArgumentNullException(nameof(publication));
            }

            var header = new MessageHeader(
                messageId: reply.Id,
                topic: reply.SendersAddress.Topic,
                messageType: MessageType.MT_COMMAND,
                correlationId: reply.SendersAddress.CorrelationId);

            return Task.FromResult(new Message(header, new MessageBody(reply.MessageBody.ToJsonString<TestReplyBody>())));
        }

        /// <inheritdoc/>
        public override Task<TestReply> MapToRequestAsync(Message replyMessage, CancellationToken cancellationToken = default)
        {
            if (replyMessage is null)
            {
                throw new ArgumentNullException(nameof(replyMessage));
            }

            TestReplyBody? replyBody = TestReplyBody.GetMessageBodyFromJson(replyMessage.Body.Value);

            if (replyBody is null)
            {
                throw new ArgumentNullException(nameof(replyBody), $"Failed to deserialize {nameof(TestReplyBody)} from message body.");
            }

            return Task.FromResult(new TestReply(new ReplyAddress(topic: replyMessage.Header.ReplyTo ?? RoutingKey.Empty, correlationId: replyMessage.Header.CorrelationId), replyBody));
        }
    }
}
