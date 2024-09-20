﻿#region Licence
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
using System.Collections.Generic;
using System.Transactions;
using FluentAssertions;
using Microsoft.Extensions.Time.Testing;
using Paramore.Brighter.Core.Tests.CommandProcessors.TestDoubles;
using Paramore.Brighter.Core.Tests.TestHelpers;
using Paramore.Brighter.Observability;
using Polly;
using Polly.CircuitBreaker;
using Polly.Registry;
using Polly.Retry;
using Xunit;

namespace Paramore.Brighter.Core.Tests.CommandProcessors.Post
{
    [Collection("CommandProcessor")]
    public class CommandProcessorPostMissingMessageTransformerTestsAsync : IDisposable
    {
        private readonly MyCommand _myCommand = new();
        private readonly InMemoryOutbox _outbox;
        private Exception _exception;
        private readonly MessageMapperRegistry _messageMapperRegistry;
        private readonly ProducerRegistry _producerRegistry;
        private readonly PolicyRegistry _policyRegistry;
        private readonly IAmABrighterTracer _tracer;

        public CommandProcessorPostMissingMessageTransformerTestsAsync()
        {
            _myCommand.Value = "Hello World";

            var timeProvider = new FakeTimeProvider();
            _tracer = new BrighterTracer(timeProvider);
            _outbox = new InMemoryOutbox(timeProvider) {Tracer = _tracer};

            _messageMapperRegistry = new MessageMapperRegistry(
                new SimpleMessageMapperFactory((_) => new MyCommandMessageMapper()),
                null);
            _messageMapperRegistry.Register<MyCommand, MyCommandMessageMapper>();

            RetryPolicy retryPolicy = Policy
                .Handle<Exception>()
                .Retry();

            CircuitBreakerPolicy circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreaker(1, TimeSpan.FromMilliseconds(1));

            var routingKey = new RoutingKey("MyTopic");
            
            _producerRegistry = new ProducerRegistry(new Dictionary<RoutingKey, IAmAMessageProducer>
            {
                {
                    routingKey, new InMemoryProducer(new InternalBus(), new FakeTimeProvider()){ 
                    Publication =
                    {
                        Topic = routingKey, RequestType = typeof(MyCommand)
                    }
                }},
            });

            _policyRegistry = new PolicyRegistry { { CommandProcessor.RETRYPOLICY, retryPolicy }, { CommandProcessor.CIRCUITBREAKER, circuitBreakerPolicy } };
         }

        [Fact]
        public void When_Creating_A_Command_Processor_Without_Message_Transformer_Async()
        {                                             
            _exception = Catch.Exception(() => new ExternalBusService<Message, CommittableTransaction>(
                _producerRegistry, 
                _policyRegistry,
                _messageMapperRegistry,
                new EmptyMessageTransformerFactory(),
                null,
                _tracer,
                _outbox)
            );               

            _exception.Should().BeOfType<ConfigurationException>();
        }

        public void Dispose()
        {
            CommandProcessor.ClearServiceBus();
        }
    }
}
