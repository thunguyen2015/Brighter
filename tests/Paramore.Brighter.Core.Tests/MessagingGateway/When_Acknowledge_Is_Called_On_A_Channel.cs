#region Licence
/* The MIT License (MIT)
Copyright © 2014 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the “Software”), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE. */

#endregion

using System;
using Microsoft.Extensions.Time.Testing;
using Xunit;

namespace Paramore.Brighter.Core.Tests.MessagingGateway
{
    public class ChannelAcknowledgeTests
    {
        private readonly IAmAChannel _channel;
        private readonly InternalBus _bus = new();
        private readonly FakeTimeProvider _fakeTimeProvider = new();
        private readonly RoutingKey Topic = new("myTopic");
        private const string ChannelName = "myChannel";

        public ChannelAcknowledgeTests()
        {
            IAmAMessageConsumer gateway = new InMemoryMessageConsumer(new RoutingKey(Topic), _bus, _fakeTimeProvider, TimeSpan.FromMilliseconds(1000)); 

            _channel = new  Channel(new (ChannelName), new(Topic), gateway);

            var sentMessage = new Message(
                new MessageHeader(Guid.NewGuid().ToString(), Topic, MessageType.MT_EVENT),
                new MessageBody("a test body"));
            
            _bus.Enqueue(sentMessage);
        }

        [Fact]
        public void When_Acknowledge_Is_Called_On_A_Channel_Should_Be_Removed()
        {
            var receivedMessage = _channel.Receive(TimeSpan.FromMilliseconds(1000));
            _channel.Acknowledge(receivedMessage);
            
            _fakeTimeProvider.Advance(TimeSpan.FromSeconds(2)); //allow for message to timeout if not acked

        }
    }
}
