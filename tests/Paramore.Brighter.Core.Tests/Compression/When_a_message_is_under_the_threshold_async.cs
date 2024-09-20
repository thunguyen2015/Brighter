﻿using System;
using System.IO.Compression;
using System.Threading.Tasks;
using FluentAssertions;
using Paramore.Brighter.Transforms.Transformers;
using Xunit;

namespace Paramore.Brighter.Core.Tests.Compression;

public class AsyncSmallPayloadNotCompressedTests
{
    private readonly CompressPayloadTransformerAsync _transformer;
    private readonly Message _message;
    private readonly RoutingKey _topic = new("test_topic");
    private const ushort GZIP_LEAD_BYTES = 0x8b1f;
    
    
    public AsyncSmallPayloadNotCompressedTests()
    {
        _transformer = new CompressPayloadTransformerAsync();
        _transformer.InitializeWrapFromAttributeParams(CompressionMethod.GZip, CompressionLevel.Optimal, 5);

        string body = "small message";
        _message = new Message(
            new MessageHeader(Guid.NewGuid().ToString(), _topic, MessageType.MT_EVENT, 
                timeStamp:DateTime.UtcNow, contentType: MessageBody.APPLICATION_JSON
            ),
            new MessageBody(body, MessageBody.APPLICATION_JSON, CharacterEncoding.UTF8)
        );      
    }
    
    [Fact]
    public async Task When_a_message_is_under_the_threshold()
    {
        var uncompressedMessage = await _transformer.WrapAsync(_message, new Publication{Topic = new RoutingKey(_topic)});

        //look for gzip in the bytes
        uncompressedMessage.Body.ContentType.Should().Be(MessageBody.APPLICATION_JSON);
        uncompressedMessage.Body.Value.Should().Be(_message.Body.Value);
    }
}
