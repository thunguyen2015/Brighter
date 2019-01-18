#region Licence
/* The MIT License (MIT)
Copyright © 2015 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

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
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using Paramore.Brighter.Tests.CommandProcessors.TestDoubles;
using Polly;
using Xunit;

namespace Paramore.Brighter.Tests.CommandProcessors
{
    public class CommandProcessorPostBoxClearAsyncTests : IDisposable
    {
        private readonly CommandProcessor _commandProcessor;
        private readonly Message _message;
        private readonly FakeMessageStore _fakeMessageStore;
        private readonly FakeMessageProducer _fakeMessageProducer;

        public CommandProcessorPostBoxClearAsyncTests()
        {
            var myCommand = new MyCommand{ Value = "Hello World"};

            _fakeMessageStore = new FakeMessageStore();
            _fakeMessageProducer = new FakeMessageProducer();

            _message = new Message(
                new MessageHeader(myCommand.Id, "MyCommand", MessageType.MT_COMMAND),
                new MessageBody(JsonConvert.SerializeObject(myCommand))
                );

            var messageMapperRegistry = new MessageMapperRegistry(new SimpleMessageMapperFactory((_) => new MyCommandMessageMapper()));
            messageMapperRegistry.Register<MyCommand, MyCommandMessageMapper>();

            var retryPolicy = Policy
                .Handle<Exception>()
                .RetryAsync();

            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(1));

            _commandProcessor = new CommandProcessor(
                new InMemoryRequestContextFactory(),
                new PolicyRegistry { { CommandProcessor.RETRYPOLICYASYNC, retryPolicy }, { CommandProcessor.CIRCUITBREAKERASYNC, circuitBreakerPolicy } },
                messageMapperRegistry,
                (IAmAMessageStoreAsync<Message>)_fakeMessageStore,
                (IAmAMessageProducerAsync)_fakeMessageProducer);
        }

        [Fact]
        public async Task When_Clearing_The_PostBox_On_The_Command_Processor_Async()
        {
            _fakeMessageStore.Add(_message);
            
            await _commandProcessor.ClearPostBoxAsync(new []{_message.Id});

            //_should_send_a_message_via_the_messaging_gateway
            _fakeMessageProducer.MessageWasSent.Should().BeTrue();

            var sentMessage = _fakeMessageProducer.SentMessages.FirstOrDefault();
            sentMessage.Should().NotBe(null);
            sentMessage.Id.Should().Be(_message.Id);
            sentMessage.Header.Topic.Should().Be(_message.Header.Topic);
            sentMessage.Body.Value.Should().Be(_message.Body.Value);
        }

        public void Dispose()
        {
            _commandProcessor.Dispose();
       }
    }
}
