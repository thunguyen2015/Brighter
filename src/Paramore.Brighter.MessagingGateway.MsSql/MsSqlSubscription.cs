﻿#region Licence
/* The MIT License (MIT)
Copyright © 2020 Ian Cooper <ian_hammond_cooper@yahoo.co.uk>

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

namespace Paramore.Brighter.MessagingGateway.MsSql
{
    public class MsSqlSubscription : Subscription
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class.
        /// </summary>
        /// <param name="dataType">Type of the data.</param>
        /// <param name="name">The name. Defaults to the data type's full name.</param>
        /// <param name="channelName">The channel name. Defaults to the data type's full name.</param>
        /// <param name="routingKey">The routing key. Defaults to the data type's full name.</param>
        /// <param name="bufferSize">The number of messages to buffer at any one time, also the number of messages to retrieve at once. Min of 1 Max of 10</param>
        /// <param name="noOfPerformers">The no of threads reading this channel.</param>
        /// <param name="timeOut">The timeout in milliseconds.</param>
        /// <param name="requeueCount">The number of times you want to requeue a message before dropping it.</param>
        /// <param name="requeueDelay">The delay the delivery of a requeue message. 0 is no delay. Defaults to 0</param>
        /// <param name="unacceptableMessageLimit">The number of unacceptable messages to handle, before stopping reading from the channel.</param>
        /// <param name="runAsync">Is this channel read asynchronously</param>
        /// <param name="channelFactory">The channel factory to create channels for Consumer.</param>
        /// <param name="makeChannels">Should we make channels if they don't exist, defaults to creating</param>
        /// <param name="emptyChannelDelay">How long to pause when a channel is empty in milliseconds</param>
        /// <param name="channelFailureDelay">How long to pause when there is a channel failure in milliseconds</param>
        public MsSqlSubscription(
            Type dataType, 
            SubscriptionName name = null, 
            ChannelName channelName = null, 
            RoutingKey routingKey = null, 
            int bufferSize = 1, 
            int noOfPerformers = 1, 
            TimeSpan? timeOut = null, 
            int requeueCount = -1, 
            TimeSpan? requeueDelay = null, 
            int unacceptableMessageLimit = 0, 
            bool runAsync = false, 
            IAmAChannelFactory channelFactory = null, 
            OnMissingChannel makeChannels = OnMissingChannel.Create,
            int emptyChannelDelay = 500,
            int channelFailureDelay = 1000) 
            : base(dataType, name, channelName, routingKey, bufferSize, noOfPerformers, timeOut, requeueCount, 
                requeueDelay, unacceptableMessageLimit, runAsync, channelFactory, makeChannels, 
                emptyChannelDelay, channelFailureDelay)
        {
        }
    }

    public class MsSqlSubscription<T> : MsSqlSubscription where T : IRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Subscription"/> class.
        /// </summary>
        /// <param name="name">The name. Defaults to the data type's full name.</param>
        /// <param name="channelName">The channel name. Defaults to the data type's full name.</param>
        /// <param name="routingKey">The routing key. Defaults to the data type's full name.</param>
        /// <param name="bufferSize">The number of messages to buffer at any one time, also the number of messages to retrieve at once. Min of 1 Max of 10</param>
        /// <param name="noOfPerformers">The no of threads reading this channel.</param>
        /// <param name="timeOut">The timeout to wait for messages</param>
        /// <param name="requeueCount">The number of times you want to requeue a message before dropping it.</param>
        /// <param name="requeueDelay">The Delay to the requeue of a message. 0 is no delay. Defaults to 0</param>
        /// <param name="unacceptableMessageLimit">The number of unacceptable messages to handle, before stopping reading from the channel.</param>
        /// <param name="runAsync">Is this channel read asynchronously</param>
        /// <param name="channelFactory">The channel factory to create channels for Consumer.</param>
        /// <param name="makeChannels">Should we make channels if they don't exist, defaults to creating</param>
        /// <param name="emptyChannelDelay">How long to pause when a channel is empty in milliseconds</param>
        /// <param name="channelFailureDelay">How long to pause when there is a channel failure in milliseconds</param>
        public MsSqlSubscription(
            SubscriptionName name = null, 
            ChannelName channelName = null, 
            RoutingKey routingKey = null, 
            int bufferSize = 1, 
            int noOfPerformers = 1, 
            TimeSpan? timeOut = null, 
            int requeueCount = -1, 
            TimeSpan? requeueDelay = null, 
            int unacceptableMessageLimit = 0, 
            bool runAsync = false, 
            IAmAChannelFactory channelFactory = null, 
            OnMissingChannel makeChannels = OnMissingChannel.Create,
            int emptyChannelDelay = 500,
            int channelFailureDelay = 1000) 
            : base(typeof(T), name, channelName, routingKey, bufferSize, noOfPerformers, timeOut, requeueCount, 
                requeueDelay, unacceptableMessageLimit, runAsync, channelFactory, makeChannels, emptyChannelDelay, channelFailureDelay)
        {
        }
       
    }
}
