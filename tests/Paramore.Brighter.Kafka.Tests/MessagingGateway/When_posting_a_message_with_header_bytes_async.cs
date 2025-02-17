﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;
using FluentAssertions;
using Paramore.Brighter.Kafka.Tests.TestDoubles;
using Paramore.Brighter.MessagingGateway.Kafka;
using Xunit;
using Xunit.Abstractions;

namespace Paramore.Brighter.Kafka.Tests.MessagingGateway;

[Trait("Category", "Kafka")]
[Collection("Kafka")]   //Kafka doesn't like multiple consumers of a partition
public class KafkaMessageProducerHeaderBytesSendTestsAsync : IAsyncDisposable, IDisposable
{
    private readonly ITestOutputHelper _output;
    private readonly string _queueName = Guid.NewGuid().ToString();
    private readonly string _topic = Guid.NewGuid().ToString();
    private readonly IAmAProducerRegistry _producerRegistry;
    private readonly IAmAMessageConsumerAsync _consumer;
    private readonly string _partitionKey = Guid.NewGuid().ToString();
    private readonly IAsyncSerializer<MyKafkaCommand> _serializer;
    private readonly IAsyncDeserializer<MyKafkaCommand> _deserializer;
    private readonly SerializationContext _serializationContext;

    public KafkaMessageProducerHeaderBytesSendTestsAsync(ITestOutputHelper output)
    {
        const string groupId = "Kafka Message Producer Header Bytes Send Test";
        _output = output;
        _producerRegistry = new KafkaProducerRegistryFactory(
            new KafkaMessagingGatewayConfiguration
            {
                Name = "Kafka Producer Send Test",
                BootStrapServers = new[] {"localhost:9092"}
            },
            new[] {new KafkaPublication
            {
                Topic = new RoutingKey(_topic),
                NumPartitions = 1,
                ReplicationFactor = 1,
                //These timeouts support running on a container using the same host as the tests,
                //your production values ought to be lower
                MessageTimeoutMs = 2000,
                RequestTimeoutMs = 2000,
                MakeChannels = OnMissingChannel.Create
            }}).CreateAsync().Result;

        _consumer = new KafkaMessageConsumerFactory(
                new KafkaMessagingGatewayConfiguration
                {
                    Name = "Kafka Consumer Test",
                    BootStrapServers = new[] { "localhost:9092" }
                })
            .CreateAsync(new KafkaSubscription<MyCommand>(
                    channelName: new ChannelName(_queueName),
                    routingKey: new RoutingKey(_topic),
                    groupId: groupId,
                    numOfPartitions: 1,
                    replicationFactor: 1,
                    messagePumpType: MessagePumpType.Reactor,
                    makeChannels: OnMissingChannel.Create
                ));

        var schemaRegistryConfig = new SchemaRegistryConfig { Url = "http://localhost:8081"};
        ISchemaRegistryClient schemaRegistryClient = new CachedSchemaRegistryClient(schemaRegistryConfig);

        _serializer = new JsonSerializer<MyKafkaCommand>(schemaRegistryClient, ConfluentJsonSerializationConfig.SerdesJsonSerializerConfig(),
            ConfluentJsonSerializationConfig.NJsonSchemaGeneratorSettings());
        _deserializer = new JsonDeserializer<MyKafkaCommand>();
        _serializationContext = new SerializationContext(MessageComponentType.Value, _topic);
    }

    [Fact]
    public async Task When_posting_a_message_via_the_messaging_gateway()
    {
        //arrange

        var myCommand = new MyKafkaCommand{ Value = "Hello World"};

        //use the serdes json serializer to write the message to the topic
        var body = await _serializer.SerializeAsync(myCommand, _serializationContext);

        //grab the schema id that was written to the message by the serializer
        var schemaId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(body.Skip(1).Take(4).ToArray()));

        var routingKey = new RoutingKey(_topic);

        var sent = new Message(
            new MessageHeader(Guid.NewGuid().ToString(), routingKey, MessageType.MT_COMMAND)
            {
                PartitionKey = _partitionKey
            },
            new MessageBody(body));

        //act

        await ((IAmAMessageProducerAsync)_producerRegistry.LookupAsyncBy(routingKey)).SendAsync(sent);

        var received = await GetMessageAsync();

        received.Body.Bytes.Length.Should().BeGreaterThan(5);

        var receivedSchemaId = IPAddress.NetworkToHostOrder(BitConverter.ToInt32(received.Body.Bytes.Skip(1).Take(4).ToArray()));

        var receivedCommand =await  _deserializer.DeserializeAsync(received.Body.Bytes, received.Body.Bytes is null, _serializationContext);

        //assert
        received.Header.MessageType.Should().Be(MessageType.MT_COMMAND);
        received.Header.PartitionKey.Should().Be(_partitionKey);
        received.Body.Bytes.Should().Equal(received.Body.Bytes);
        received.Body.Value.Should().Be(received.Body.Value);
        receivedSchemaId.Should().Be(schemaId);
        receivedCommand.Id.Should().Be(myCommand.Id);
        receivedCommand.Value.Should().Be(myCommand.Value);
    }

    private async Task<Message> GetMessageAsync()
    {
        Message[] messages = Array.Empty<Message>();
        int maxTries = 0;
        do
        {
            try
            {
                maxTries++;
                await Task.Delay(500); //Let topic propagate in the broker
                messages = await _consumer.ReceiveAsync(TimeSpan.FromMilliseconds(1000));

                if (messages[0].Header.MessageType != MessageType.MT_NONE)
                {
                    await _consumer.AcknowledgeAsync(messages[0]);
                    break;
                }

            }
            catch (ChannelFailureException cfx)
            {
                //Lots of reasons to be here as Kafka propagates a topic, or the test cluster is still initializing
                _output.WriteLine($" Failed to read from topic:{_topic} because {cfx.Message} attempt: {maxTries}");
            }

        } while (maxTries <= 3);

        if (messages[0].Header.MessageType == MessageType.MT_NONE)
            throw new Exception($"Failed to read from topic:{_topic} after {maxTries} attempts");

        return messages[0];
    }
    
    public void Dispose()
    {
        _producerRegistry?.Dispose();
        ((IAmAMessageConsumerSync)_consumer)?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        _producerRegistry.Dispose();
        await _consumer.DisposeAsync();
    }
}
