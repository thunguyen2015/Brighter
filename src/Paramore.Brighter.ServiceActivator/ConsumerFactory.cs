﻿#region Licence
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
using Paramore.Brighter.Observability;

namespace Paramore.Brighter.ServiceActivator
{
    internal class ConsumerFactory<TRequest> : IConsumerFactory where TRequest : class, IRequest
    {
        private readonly IAmACommandProcessorProvider _commandProcessorProvider;
        private readonly IAmAMessageMapperRegistry _messageMapperRegistry;
        private readonly Subscription _subscription;
        private readonly IAmAMessageTransformerFactory _messageTransformerFactory;
        private readonly IAmARequestContextFactory _requestContextFactory;
        private readonly IAmABrighterTracer _tracer;
        private readonly InstrumentationOptions _instrumentationOptions;
        private readonly ConsumerName _consumerName;
        private readonly IAmAMessageMapperRegistryAsync _messageMapperRegistryAsync;
        private readonly IAmAMessageTransformerFactoryAsync _messageTransformerFactoryAsync;

        public ConsumerFactory(
            IAmACommandProcessorProvider commandProcessorProvider,
            Subscription subscription,
            IAmAMessageMapperRegistry messageMapperRegistry,
            IAmAMessageTransformerFactory messageTransformerFactory,
            IAmARequestContextFactory requestContextFactory,
            IAmABrighterTracer tracer,
            InstrumentationOptions instrumentationOptions = InstrumentationOptions.All)
        {
            _commandProcessorProvider = commandProcessorProvider;
            _messageMapperRegistry = messageMapperRegistry;
            _subscription = subscription;
            _messageTransformerFactory = messageTransformerFactory;
            _requestContextFactory = requestContextFactory;
            _tracer = tracer;
            _instrumentationOptions = instrumentationOptions;
            _consumerName = new ConsumerName($"{_subscription.Name}-{Guid.NewGuid()}");
        }
        
        public ConsumerFactory(
            IAmACommandProcessorProvider commandProcessorProvider,
            Subscription subscription,
            IAmAMessageMapperRegistryAsync messageMapperRegistryAsync,
            IAmAMessageTransformerFactoryAsync messageTransformerFactoryAsync,
            IAmARequestContextFactory requestContextFactory,
            IAmABrighterTracer tracer,
            InstrumentationOptions instrumentationOptions = InstrumentationOptions.All)
        {
            _commandProcessorProvider = commandProcessorProvider;
            _messageMapperRegistryAsync = messageMapperRegistryAsync;
            _subscription = subscription;
            _messageTransformerFactoryAsync = messageTransformerFactoryAsync;
            _requestContextFactory = requestContextFactory;
            _tracer = tracer;
            _instrumentationOptions = instrumentationOptions;
            _consumerName = new ConsumerName($"{_subscription.Name}-{Guid.NewGuid()}");
        }

        public Consumer Create()
        {
            if (_subscription.RunAsync)
                return CreateAsync();
            else
                return CreateBlocking();
        }

        private Consumer CreateBlocking()
        {
            var channel = _subscription.ChannelFactory.CreateChannel(_subscription);
            var messagePump = new MessagePumpBlocking<TRequest>(_commandProcessorProvider, _messageMapperRegistry, 
                _messageTransformerFactory, _requestContextFactory, _tracer, _instrumentationOptions)
            {
                Channel = channel,
                TimeOut = _subscription.TimeOut,
                RequeueCount = _subscription.RequeueCount,
                RequeueDelay = _subscription.RequeueDelay,
                UnacceptableMessageLimit = _subscription.UnacceptableMessageLimit
            };

            return new Consumer(_consumerName, _subscription, channel, messagePump);
        }

        private Consumer CreateAsync()
        {
            var channel = _subscription.ChannelFactory.CreateChannel(_subscription);
            var messagePump = new MessagePumpAsync<TRequest>(_commandProcessorProvider, _messageMapperRegistryAsync, 
                _messageTransformerFactoryAsync, _requestContextFactory, _tracer, _instrumentationOptions)
            {
                Channel = channel,
                TimeOut = _subscription.TimeOut,
                RequeueCount = _subscription.RequeueCount,
                RequeueDelay = _subscription.RequeueDelay,
                UnacceptableMessageLimit = _subscription.UnacceptableMessageLimit,
                EmptyChannelDelay = _subscription.EmptyChannelDelay,
                ChannelFailureDelay = _subscription.ChannelFailureDelay
            };

            return new Consumer(_consumerName, _subscription, channel, messagePump);
        }
    }
}
